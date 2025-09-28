#!/bin/bash

# Maritime Platform - Research Group Deployment Script
# This script automates the deployment process for research groups

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
COMPOSE_FILE="docker-compose.research.yml"
ENV_FILE=".env.research"

# Functions
print_header() {
    echo -e "${BLUE}"
    echo "=================================================="
    echo "  Maritime Platform - Research Deployment"
    echo "=================================================="
    echo -e "${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
    exit 1
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker Desktop first."
    fi
    
    # Check if Docker Compose is available
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not available. Please install Docker Compose."
    fi
    
    # Check if .NET is installed (for local development)
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version)
        print_success "Found .NET version: $DOTNET_VERSION"
    else
        print_warning ".NET not found. Docker deployment will be used."
    fi
    
    print_success "Prerequisites check completed"
}

setup_environment() {
    print_info "Setting up environment configuration..."
    
    # Copy environment template if .env doesn't exist
    if [ ! -f "$ENV_FILE" ]; then
        if [ -f ".env.research.template" ]; then
            cp ".env.research.template" "$ENV_FILE"
            print_success "Created $ENV_FILE from template"
            print_warning "Please edit $ENV_FILE with your research group settings"
        else
            print_error "Environment template not found"
        fi
    else
        print_success "Environment file already exists"
    fi
    
    # Create data directories
    mkdir -p research-data logs sql-scripts monitoring
    print_success "Created necessary directories"
}

build_application() {
    print_info "Building Maritime Platform..."
    
    # Build Docker image
    if docker build -f Dockerfile.research -t maritime-platform:research .; then
        print_success "Docker image built successfully"
    else
        print_error "Failed to build Docker image"
    fi
}

deploy_services() {
    print_info "Deploying services with Docker Compose..."
    
    # Create network if it doesn't exist
    docker network create maritime-network 2>/dev/null || true
    
    # Start services
    if docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d; then
        print_success "Services deployed successfully"
    else
        print_error "Failed to deploy services"
    fi
    
    # Wait for services to be ready
    print_info "Waiting for services to be ready..."
    sleep 30
    
    # Check service health
    check_service_health
}

check_service_health() {
    print_info "Checking service health..."
    
    # Check Maritime Platform
    if curl -f http://localhost:8080/health >/dev/null 2>&1; then
        print_success "Maritime Platform is healthy"
    else
        print_warning "Maritime Platform health check failed"
    fi
    
    # Check database
    if docker-compose -f "$COMPOSE_FILE" exec -T db sqlcmd -S localhost -U sa -P "ResearchPass123!" -Q "SELECT 1" >/dev/null 2>&1; then
        print_success "Database is healthy"
    else
        print_warning "Database health check failed"
    fi
}

show_access_info() {
    print_info "Deployment completed! Access your services:"
    echo ""
    echo "🌐 Maritime Platform:    http://localhost:8080"
    echo "🗄️  Database Admin:      http://localhost:8081"
    echo "📊 Monitoring Dashboard: http://localhost:3000 (admin/research123)"
    echo "💾 File Storage:         http://localhost:9001 (research/research123)"
    echo ""
    echo "📁 Data directory:       ./research-data"
    echo "📝 Logs directory:       ./logs"
    echo ""
    print_success "Maritime Platform is ready for your research group!"
}

stop_services() {
    print_info "Stopping Maritime Platform services..."
    docker-compose -f "$COMPOSE_FILE" down
    print_success "Services stopped"
}

cleanup_deployment() {
    print_info "Cleaning up deployment..."
    docker-compose -f "$COMPOSE_FILE" down -v
    docker rmi maritime-platform:research 2>/dev/null || true
    print_success "Cleanup completed"
}

show_help() {
    echo "Maritime Platform - Research Group Deployment Script"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  deploy    - Deploy the complete platform (default)"
    echo "  stop      - Stop all services"
    echo "  restart   - Restart all services"
    echo "  cleanup   - Stop services and remove data"
    echo "  status    - Show service status"
    echo "  logs      - Show service logs"
    echo "  help      - Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 deploy     # Deploy the platform"
    echo "  $0 stop       # Stop the platform"
    echo "  $0 status     # Check status"
}

show_status() {
    print_info "Service Status:"
    docker-compose -f "$COMPOSE_FILE" ps
}

show_logs() {
    print_info "Service Logs:"
    docker-compose -f "$COMPOSE_FILE" logs --tail=50 -f
}

# Main execution
main() {
    print_header
    
    case "${1:-deploy}" in
        "deploy")
            check_prerequisites
            setup_environment
            build_application
            deploy_services
            show_access_info
            ;;
        "stop")
            stop_services
            ;;
        "restart")
            stop_services
            sleep 5
            deploy_services
            show_access_info
            ;;
        "cleanup")
            cleanup_deployment
            ;;
        "status")
            show_status
            ;;
        "logs")
            show_logs
            ;;
        "help"|"-h"|"--help")
            show_help
            ;;
        *)
            print_error "Unknown command: $1. Use 'help' for available commands."
            ;;
    esac
}

# Run main function with all arguments
main "$@"