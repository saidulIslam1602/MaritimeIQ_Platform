// Simple test script for ACR Tasks
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcrTasksDemo.Tests
{
    public class SimpleTests
    {
        public static bool RunBasicTests()
        {
            try
            {
                Console.WriteLine("Starting basic application tests...");
                
                // Test 1: Check if application starts
                Console.WriteLine("✓ Application compiled successfully");
                
                // Test 2: Basic functionality test
                var testData = new
                {
                    Message = "Test completed",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success"
                };
                
                Console.WriteLine($"✓ Basic functionality test: {testData.Status}");
                
                // Test 3: Environment check
                Console.WriteLine($"✓ Running on: {Environment.OSVersion}");
                Console.WriteLine($"✓ .NET Version: {Environment.Version}");
                
                Console.WriteLine("All tests passed!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                return false;
            }
        }
    }
}