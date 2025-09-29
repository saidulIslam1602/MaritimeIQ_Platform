using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Functions
{
    /// <summary>
    /// Passenger Notification Functions - Handles boarding notifications, delays,
    /// Northern Lights alerts, and personalized passenger communications
    /// </summary>
    public class PassengerNotificationFunction
    {
        private readonly ILogger<PassengerNotificationFunction> _logger;

        public PassengerNotificationFunction(ILogger<PassengerNotificationFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Service Bus triggered function for passenger boarding notifications
        /// Triggered when passengers need to be notified about boarding procedures
        /// </summary>
        [Function("ProcessBoardingNotifications")]
        public async Task ProcessBoardingNotifications(
            [ServiceBusTrigger("boarding-notifications", Connection = "ServiceBusConnectionString")] string message,
            FunctionContext context)
        {
            _logger.LogInformation("Processing boarding notification request");

            try
            {
                var notification = JsonSerializer.Deserialize<BoardingNotification>(message);
                if (notification != null)
                {
                    await ProcessBoardingNotification(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing boarding notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Timer triggered function for Northern Lights alerts
        /// Runs every 30 minutes during aurora season to check viewing conditions
        /// </summary>
        [Function("CheckNorthernLightsConditions")]
        public async Task CheckNorthernLightsConditions(
            [TimerTrigger("0 */30 * * * *")] TimerInfo timer,
            FunctionContext context)
        {
            _logger.LogInformation("Checking Northern Lights conditions for passenger alerts");

            var auroraConditions = await GetCurrentAuroraConditions();
            var activeVoyages = await GetActiveVoyages();

            foreach (var voyage in activeVoyages)
            {
                if (ShouldSendAuroraAlert(auroraConditions, voyage))
                {
                    await SendNorthernLightsAlert(voyage, auroraConditions);
                }
            }

            _logger.LogInformation($"Aurora monitoring completed - {activeVoyages.Count()} voyages checked");
        }

        /// <summary>
        /// Event triggered function for delay notifications
        /// Processes vessel delay information and notifies affected passengers
        /// </summary>
        [Function("ProcessDelayNotifications")]
        public async Task ProcessDelayNotifications(
            [EventHubTrigger("vessel-delays", Connection = "EventHubConnectionString")] string[] events,
            FunctionContext context)
        {
            _logger.LogInformation($"Processing {events.Length} delay notifications");

            foreach (string eventData in events)
            {
                try
                {
                    var delayInfo = JsonSerializer.Deserialize<DelayNotification>(eventData);
                    if (delayInfo != null)
                    {
                        await ProcessDelayNotification(delayInfo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing delay notification: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// HTTP triggered function for sending personalized passenger communications
        /// </summary>
        [Function("SendPersonalizedCommunication")]
        public async Task<HttpResponseData> SendPersonalizedCommunication(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("Processing personalized communication request");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var commRequest = JsonSerializer.Deserialize<PersonalizedCommunication>(requestBody);

                if (commRequest != null)
                {
                    var result = await ProcessPersonalizedCommunication(commRequest);
                    await response.WriteStringAsync(JsonSerializer.Serialize(result));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in personalized communication: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Error = ex.Message }));
            }

            return response;
        }

        /// <summary>
        /// Timer function for daily voyage updates
        /// Sends daily itinerary and activity updates to passengers
        /// </summary>
        [Function("SendDailyVoyageUpdates")]
        public async Task SendDailyVoyageUpdates(
            [TimerTrigger("0 0 7 * * *")] TimerInfo timer, // Daily at 07:00
            FunctionContext context)
        {
            _logger.LogInformation("Sending daily voyage updates to passengers");

            var activeVoyages = await GetActiveVoyages();
            var updatesSent = 0;

            foreach (var voyage in activeVoyages)
            {
                var dailyUpdate = await CreateDailyUpdate(voyage);
                var passengers = await GetVoyagePassengers(voyage.VoyageId);

                foreach (var passenger in passengers)
                {
                    await SendDailyUpdateToPassenger(passenger, dailyUpdate);
                    updatesSent++;
                }
            }

            _logger.LogInformation($"Daily voyage updates sent to {updatesSent} passengers across {activeVoyages.Count()} voyages");
        }

        /// <summary>
        /// Port arrival notification function
        /// Notifies passengers about upcoming port arrivals and shore excursions
        /// </summary>
        [Function("SendPortArrivalNotifications")]
        public async Task SendPortArrivalNotifications(
            [TimerTrigger("0 */15 * * * *")] TimerInfo timer, // Every 15 minutes
            FunctionContext context)
        {
            _logger.LogInformation("Checking for upcoming port arrivals");

            var activeVoyages = await GetActiveVoyages();
            var notificationsSent = 0;

            foreach (var voyage in activeVoyages)
            {
                var upcomingPorts = await GetUpcomingPortArrivals(voyage.VoyageId);
                
                foreach (var portArrival in upcomingPorts)
                {
                    if (ShouldSendPortNotification(portArrival))
                    {
                        await SendPortArrivalNotification(voyage, portArrival);
                        notificationsSent++;
                    }
                }
            }

            _logger.LogInformation($"Port arrival notifications processed - {notificationsSent} notifications sent");
        }

        // Core processing methods
        private async Task ProcessBoardingNotification(BoardingNotification notification)
        {
            _logger.LogInformation($"Processing boarding notification for voyage {notification.VoyageId}");

            var passengers = await GetVoyagePassengers(notification.VoyageId);
            var vessel = await GetVesselInfo(notification.VesselId);
            
            var boardingInfo = new
            {
                VoyageId = notification.VoyageId,
                VesselName = vessel?.Name,
                DeparturePort = notification.DeparturePort,
                BoardingTime = notification.BoardingTime,
                DepartureTime = notification.DepartureTime,
                BoardingGate = notification.BoardingGate,
                ImportantNotices = new[]
                {
                    "Please have your boarding pass and ID ready",
                    "Boarding begins 2 hours before departure",
                    "All luggage must be checked 30 minutes before departure",
                    "Car loading begins 1 hour before departure"
                },
                WeatherUpdate = await GetPortWeather(notification.DeparturePort),
                ContactInfo = new
                {
                    GuestServices = "+47 810 30 000",
                    Emergency = "112",
                    PortOffice = notification.PortContactNumber
                }
            };

            // Send to all passengers
            foreach (var passenger in passengers)
            {
                await SendBoardingNotificationToPassenger(passenger, boardingInfo);
            }

            // Send SMS alerts for urgent boarding
            if (notification.IsUrgent)
            {
                await SendUrgentBoardingAlerts(passengers, boardingInfo);
            }

            _logger.LogInformation($"Boarding notifications sent to {passengers.Count()} passengers");
        }

        private async Task<object> GetCurrentAuroraConditions()
        {
            await Task.Delay(100);
            
            return new
            {
                KpIndex = 6.8,
                SolarActivity = "High",
                CloudCover = 15, // Percentage
                MoonPhase = "New Moon",
                Visibility = "Excellent",
                OptimalViewingTime = new { Start = "22:00", End = "02:00" },
                Confidence = 0.94,
                UpdateTime = DateTime.UtcNow,
                ViewingProbability = new
                {
                    Tonight = 95,
                    Tomorrow = 87,
                    NextWeek = 72
                },
                BestViewingLocations = new[]
                {
                    new { Port = "Tromsø", Probability = 98, ArrivalTime = DateTime.UtcNow.AddHours(4) },
                    new { Port = "Alta", Probability = 94, ArrivalTime = DateTime.UtcNow.AddHours(12) },
                    new { Port = "Kirkenes", Probability = 92, ArrivalTime = DateTime.UtcNow.AddHours(18) }
                }
            };
        }

        private async Task<IEnumerable<VoyageInfo>> GetActiveVoyages()
        {
            await Task.Delay(80);
            
            return new[]
            {
                new VoyageInfo 
                { 
                    VoyageId = "HAV-001", 
                    VesselId = 1, 
                    VesselName = "MS Nordic Aurora", 
                    Route = "Bergen-Kirkenes",
                    CurrentPosition = new VoyagePosition { Latitude = 67.28m, Longitude = 14.40m, NearestPort = "Bodø" },
                    PassengerCount = 487,
                    DepartureDate = DateTime.UtcNow.AddDays(-2)
                },
                new VoyageInfo 
                { 
                    VoyageId = "HAV-002", 
                    VesselId = 2, 
                    VesselName = "MS Arctic Explorer", 
                    Route = "Kirkenes-Bergen",
                    CurrentPosition = new VoyagePosition { Latitude = 69.65m, Longitude = 18.96m, NearestPort = "Tromsø" },
                    PassengerCount = 521,
                    DepartureDate = DateTime.UtcNow.AddDays(-1)
                }
            };
        }

        private bool ShouldSendAuroraAlert(object auroraConditions, VoyageInfo voyage)
        {
            var kpIndex = ((dynamic)auroraConditions).KpIndex;
            var cloudCover = ((dynamic)auroraConditions).CloudCover;
            var vesselLatitude = voyage.CurrentPosition?.Latitude ?? 0m;

            // Send alert if KP index > 5, low cloud cover, and vessel north of 65°
            return kpIndex > 5.0 && cloudCover < 30 && vesselLatitude > 65;
        }

        private async Task SendNorthernLightsAlert(VoyageInfo voyage, object auroraConditions)
        {
            _logger.LogInformation($"Sending Northern Lights alert to passengers on {voyage.VesselName}");

            var alert = new
            {
                AlertType = "NORTHERN_LIGHTS_VIEWING",
                VoyageId = voyage.VoyageId,
                VesselName = voyage.VesselName,
                AlertTime = DateTime.UtcNow,
                Conditions = auroraConditions,
                ViewingInstructions = new
                {
                    BestDecks = new[] { "Deck 7 - Aurora Deck", "Deck 6 - Panorama Lounge" },
                    ViewingTips = new[]
                    {
                        "Dress warmly - temperatures around -10°C",
                        "Allow 20 minutes for eyes to adjust to darkness", 
                        "Aurora activity typically peaks between 22:00-02:00",
                        "Photography tips available from our naturalist guides"
                    },
                    Services = new
                    {
                        HotDrinks = "Complimentary hot chocolate and coffee on deck",
                        Blankets = "Warm blankets available",
                        WakeUpService = "Free wake-up calls if aurora activity increases",
                        Photography = "Professional photography workshop at 21:30"
                    }
                },
                Forecast = new
                {
                    Tonight = "Excellent viewing conditions - KP index 6.8",
                    Duration = "Expected to be visible for 4-6 hours",
                    Intensity = "High - expect green curtains with possible pink/purple colors"
                }
            };

            var passengers = await GetVoyagePassengers(voyage.VoyageId);
            
            foreach (var passenger in passengers)
            {
                await SendAuroraAlertToPassenger(passenger, alert);
            }

            // Also announce over ship's PA system
            await TriggerShipAnnouncement(voyage.VesselId ?? 0, "Northern Lights are now visible from the upper decks!");

            _logger.LogInformation($"Aurora alert sent to {passengers.Count()} passengers");
        }

        private async Task ProcessDelayNotification(DelayNotification delayInfo)
        {
            _logger.LogInformation($"Processing delay notification - {delayInfo.DelayMinutes} minutes for voyage {delayInfo.VoyageId}");

            var passengers = await GetVoyagePassengers(delayInfo.VoyageId);
            var vessel = await GetVesselInfo(delayInfo.VesselId);

            var delayAlert = new
            {
                DelayType = delayInfo.DelayType,
                VoyageId = delayInfo.VoyageId,
                VesselName = vessel?.Name,
                DelayMinutes = delayInfo.DelayMinutes,
                Reason = delayInfo.Reason,
                NewArrivalTime = delayInfo.OriginalArrivalTime.AddMinutes(delayInfo.DelayMinutes),
                CompensationInfo = await CalculateCompensation(delayInfo),
                AlternativeServices = await GetAlternativeServices(delayInfo),
                ApologyMessage = GenerateApologyMessage(delayInfo),
                ContactInfo = new
                {
                    GuestServices = "+47 810 30 000",
                    Email = "guestservices@maritimeiq.com",
                    OnboardAssistance = "Visit Guest Services Desk on Deck 5"
                }
            };

            foreach (var passenger in passengers)
            {
                await SendDelayNotificationToPassenger(passenger, delayAlert);
            }

            // For significant delays, also send SMS
            if (delayInfo.DelayMinutes > 60)
            {
                await SendUrgentDelayAlerts(passengers, delayAlert);
            }

            _logger.LogInformation($"Delay notifications sent to {passengers.Count()} passengers");
        }

        private async Task<object> ProcessPersonalizedCommunication(PersonalizedCommunication request)
        {
            _logger.LogInformation($"Processing personalized communication for passenger {request.PassengerId}");

            var passenger = await GetPassengerInfo(request.PassengerId);
            if (passenger == null)
            {
                return new { Error = "Passenger not found", PassengerId = request.PassengerId };
            }

            var personalization = await GeneratePersonalization(passenger, request);

            var communication = new
            {
                CommunicationId = Guid.NewGuid(),
                PassengerId = request.PassengerId,
                MessageType = request.MessageType,
                PersonalizationLevel = ((dynamic)personalization).Level,
                Content = await GeneratePersonalizedContent(passenger, request, personalization),
                DeliveryMethods = request.DeliveryMethods,
                Language = passenger.PreferredLanguage ?? "en",
                Timestamp = DateTime.UtcNow,
                Status = "PREPARED"
            };

            // Send via requested delivery methods
            var deliveryResults = new List<object>();
            
            foreach (var method in request.DeliveryMethods)
            {
                var result = await DeliverMessage(communication, method, passenger);
                deliveryResults.Add(result);
            }

            return new
            {
                Communication = communication,
                DeliveryResults = deliveryResults,
                OverallStatus = deliveryResults.All(r => ((dynamic)r).Success) ? "SUCCESS" : "PARTIAL_SUCCESS"
            };
        }

        private async Task<object> CreateDailyUpdate(VoyageInfo voyage)
        {
            var weather = await GetCurrentWeather(voyage.CurrentPosition ?? throw new ArgumentNullException(nameof(voyage.CurrentPosition))); // Ensure position is not null
            var todaysPorts = await getTodaysPortSchedule(voyage.VoyageId);
            var activities = await getTodaysActivities(voyage.VoyageId);
            var auroraForecast = await GetAuroraForecast();

            return new
            {
                Date = DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy"),
                VoyageId = voyage.VoyageId,
                VesselName = voyage.VesselName,
                DayOfVoyage = voyage.DepartureDate.HasValue
                    ? (DateTime.UtcNow - voyage.DepartureDate.Value).Days + 1
                    : 1,
                CurrentLocation = $"Near {voyage.CurrentPosition?.NearestPort ?? "current route"}",
                
                TodaysSchedule = new
                {
                    Ports = todaysPorts,
                    Activities = activities,
                    Dining = new
                    {
                        BreakfastBuffet = "07:00-10:00 - Arctic Menu Restaurant",
                        Lunch = "12:00-14:30 - Coastal cuisine featuring local seafood",
                        Dinner = "18:00-21:00 - Three-course Norwegian dinner",
                        SpecialTonight = "Traditional fish soup and Northern Norwegian delicacies"
                    }
                },
                
                WeatherUpdate = weather,
                
                NorthernLightsOutlook = auroraForecast,
                
                TomorrowsHighlights = await GetTomorrowsHighlights(voyage.VoyageId),
                
                SafetyReminder = "Please remember to check weather conditions before going on deck and use handrails in rough seas.",
                
                DidYouKnow = GetDailyMaritimeFact(),
                
                ContactInfo = new
                {
                    GuestServices = "Deck 5 - Open 24/7",
                    MedicalCenter = "Deck 4 - Staffed by qualified physician",
                    Naturalist = "Available for questions about wildlife and Northern Lights"
                }
            };
        }

        // Helper methods
        private async Task<IEnumerable<PassengerInfo>> GetVoyagePassengers(string voyageId)
        {
            await Task.Delay(60);

            return new[]
            {
                new PassengerInfo
                {
                    PassengerId = 1,
                    Name = "Erik Nordahl",
                    Email = "erik.nordahl@email.no",
                    Phone = "+47 98765432",
                    PreferredLanguage = "no",
                    CabinNumber = "A-205",
                    Nationality = "Norwegian"
                },
                new PassengerInfo
                {
                    PassengerId = 2,
                    Name = "Sarah Mitchell",
                    Email = "sarah.mitchell@email.com",
                    Phone = "+44 7123456789",
                    PreferredLanguage = "en",
                    CabinNumber = "B-112",
                    Nationality = "British"
                }
            };
        }

        private async Task SendBoardingNotificationToPassenger(PassengerInfo passenger, object boardingInfo)
        {
            await Task.Delay(30);
            _logger.LogInformation($"Boarding notification sent to {passenger.Name} via email and SMS");
        }

        private async Task SendAuroraAlertToPassenger(PassengerInfo passenger, object alert)
        {
            await Task.Delay(25);
            _logger.LogInformation($"Aurora alert sent to {passenger.Name}");
        }

        private async Task SendDelayNotificationToPassenger(PassengerInfo passenger, object delayAlert)
        {
            await Task.Delay(35);
            _logger.LogInformation($"Delay notification sent to {passenger.Name}");
        }

        private async Task<object> CalculateCompensation(DelayNotification delayInfo)
        {
            await Task.Delay(40);
            
            return delayInfo.DelayMinutes switch
            {
                >= 180 => new { Type = "Significant", Amount = "50% cabin refund + dining credit", Reason = "Major delay compensation" },
                >= 120 => new { Type = "Moderate", Amount = "25% cabin refund", Reason = "Extended delay compensation" },
                >= 60 => new { Type = "Minor", Amount = "Dining credit €50", Reason = "Delay inconvenience" },
                _ => new { Type = "Information", Amount = "No compensation required", Reason = "Minor operational delay" }
            };
        }

        private string GenerateApologyMessage(DelayNotification delayInfo)
        {
            return $"We sincerely apologize for the {delayInfo.DelayMinutes}-minute delay. {delayInfo.Reason}. " +
                   "Our crew is working to minimize any further inconvenience. Thank you for your understanding.";
        }

        private string GetDailyMaritimeFact()
        {
            var facts = new[]
            {
                "The Norwegian coastal route covers 2,500 kilometers and visits 34 ports in 11 days.",
                "MaritimeIQ's hybrid vessels can run on battery power alone for up to 4 hours.",
                "The Northern Lights are visible up to 200 nights per year in Northern Norway.",
                "Norwegian coastal vessels have been operating this route for over 125 years.",
                "The midnight sun is visible for 76 consecutive days in Kirkenes during summer."
            };

            return facts[DateTime.UtcNow.DayOfYear % facts.Length];
        }

        private async Task<VesselInfo> GetVesselInfo(int vesselId)
        {
            await Task.Delay(40);
            
            return vesselId switch
            {
                1 => new VesselInfo { Name = "MS Nordic Aurora", Capacity = 640, CrewCount = 55 },
                2 => new VesselInfo { Name = "MS Arctic Explorer", Capacity = 640, CrewCount = 55 },
                3 => new VesselInfo { Name = "MS Coastal Voyager", Capacity = 640, CrewCount = 55 },
                4 => new VesselInfo { Name = "MS Nordic Spirit", Capacity = 640, CrewCount = 55 },
                _ => new VesselInfo { Name = "Unknown Vessel", Capacity = 0, CrewCount = 0 }
            };
        }

        private async Task<object> GetCurrentWeather(object position)
        {
            await Task.Delay(50);
            
            return new
            {
                Temperature = "-8°C",
                WindSpeed = "15 knots SW", 
                Visibility = "12 km",
                SeaState = "Moderate (2-3m waves)",
                Conditions = "Partly cloudy with good aurora viewing potential"
            };
        }

        private async Task<object> GetPortWeather(string portName)
        {
            await Task.Delay(45);
            
            return new
            {
                Port = portName,
                Temperature = "-5°C",
                WindSpeed = "12 knots",
                Conditions = "Clear skies",
                Visibility = "Excellent"
            };
        }

        private async Task TriggerShipAnnouncement(int vesselId, string message)
        {
            await Task.Delay(20);
            _logger.LogInformation($"Ship announcement triggered on vessel {vesselId}: {message}");
        }

        private async Task SendUrgentBoardingAlerts(IEnumerable<PassengerInfo> passengers, object boardingInfo)
        {
            await Task.Delay(40);
            _logger.LogInformation($"Urgent boarding SMS alerts sent to {passengers.Count()} passengers");
        }

        private async Task SendUrgentDelayAlerts(IEnumerable<PassengerInfo> passengers, object delayAlert)
        {
            await Task.Delay(35);
            _logger.LogInformation($"Urgent delay SMS alerts sent to {passengers.Count()} passengers");
        }

        private async Task<object> GeneratePersonalization(PassengerInfo passenger, PersonalizedCommunication request)
        {
            await Task.Delay(60);
            
            return new
            {
                Level = "HIGH",
                Factors = new[]
                {
                    $"Language preference: {passenger.PreferredLanguage}",
                    $"Nationality: {passenger.Nationality}",
                    $"Cabin type: {passenger.CabinNumber?.Substring(0, 1)}",
                    "Previous voyage history available"
                }
            };
        }

        private async Task<object> GeneratePersonalizedContent(PassengerInfo passenger, PersonalizedCommunication request, object personalization)
        {
            await Task.Delay(80);
            
            return new
            {
                Greeting = $"Dear {passenger.Name}",
                Content = request.Content,
                PersonalizedElements = new[]
                {
                    "Content translated to preferred language",
                    "Cultural references for nationality",
                    "Cabin-specific information included",
                    "Previous voyage preferences considered"
                },
                CallToAction = request.CallToAction,
                Signature = "The MaritimeIQ Platform Team"
            };
        }

        private async Task<object> DeliverMessage(object communication, string method, PassengerInfo passenger)
        {
            await Task.Delay(50);
            
            return new
            {
                Method = method,
                Recipient = method == "email" ? passenger.Email : passenger.Phone,
                Success = true,
                DeliveryTime = DateTime.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }

        private async Task<PassengerInfo> GetPassengerInfo(int passengerId)
        {
            await Task.Delay(30);
            
            return new PassengerInfo
            {
                PassengerId = passengerId,
                Name = "Sample Passenger",
                Email = "passenger@email.com",
                PreferredLanguage = "en",
                Nationality = "International"
            };
        }

        private async Task SendDailyUpdateToPassenger(PassengerInfo passenger, object dailyUpdate)
        {
            await Task.Delay(25);
            _logger.LogInformation($"Daily update sent to {passenger.Name}");
        }

        private async Task<IEnumerable<object>> GetUpcomingPortArrivals(string voyageId)
        {
            await Task.Delay(70);

            return new[]
            {
                new { Port = "Tromsø", ArrivalTime = DateTime.UtcNow.AddHours(2), StayDuration = 180 },
                new { Port = "Hammerfest", ArrivalTime = DateTime.UtcNow.AddHours(8), StayDuration = 90 }
            };
        }

        private bool ShouldSendPortNotification(object portArrival)
        {
            var arrivalTime = ((dynamic)portArrival).ArrivalTime;
            var hoursUntilArrival = (arrivalTime - DateTime.UtcNow).TotalHours;
            
            return hoursUntilArrival <= 2 && hoursUntilArrival > 1.5; // Send 1.5-2 hours before arrival
        }

        private async Task SendPortArrivalNotification(VoyageInfo voyage, object portArrival)
        {
            await Task.Delay(45);
            _logger.LogInformation($"Port arrival notification sent for {((dynamic)portArrival).Port}");
        }

        private async Task<object[]> getTodaysPortSchedule(string voyageId)
        {
            await Task.Delay(40);
            return new object[]
            {
                new { Port = "Tromsø", Arrival = "14:30", Departure = "17:30", Highlights = "Arctic Cathedral, Northern Lights center" }
            };
        }

        private async Task<object[]> getTodaysActivities(string voyageId)
        {
            await Task.Delay(35);
            return new object[]
            {
                new { Time = "10:00", Activity = "Naturalist presentation: Arctic Wildlife", Location = "Conference Center" },
                new { Time = "15:00", Activity = "Shore excursion: Tromsø city tour", Location = "Meet at reception" },
                new { Time = "21:30", Activity = "Northern Lights photography workshop", Location = "Deck 7" }
            };
        }

        private async Task<object> GetAuroraForecast()
        {
            await Task.Delay(30);
            return new
            {
                Tonight = "Excellent - KP index 6.2",
                Tomorrow = "Good - KP index 4.8",
                Recommendation = "Best viewing from 22:00-02:00 tonight"
            };
        }

        private async Task<object[]> GetTomorrowsHighlights(string voyageId)
        {
            await Task.Delay(25);
            return new object[]
            {
                new { Event = "Hammerfest arrival", Time = "08:00", Description = "World's northernmost town" },
                new { Event = "King crab tasting", Time = "19:00", Description = "Fresh from Arctic waters" }
            };
        }

        private async Task<object> GetAlternativeServices(DelayNotification delayInfo)
        {
            await Task.Delay(30);
            return new
            {
                ExtendedDining = "Restaurant hours extended",
                PortAssistance = "Additional shore excursion time provided",
                Compensation = "Complimentary refreshments available"
            };
        }
    }

    // Data models
    public class BoardingNotification
    {
        public string VoyageId { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public string DeparturePort { get; set; } = string.Empty;
        public DateTime BoardingTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public string BoardingGate { get; set; } = string.Empty;
        public string PortContactNumber { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
    }

    public class DelayNotification
    {
        public string VoyageId { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public string DelayType { get; set; } = string.Empty;
        public int DelayMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime OriginalArrivalTime { get; set; }
    }

    // All models are now in SharedModels.cs
}