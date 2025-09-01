#!/bin/bash

echo "?? Starting Pricing API with Docker"
echo "=================================="

# Build and start the containers
echo "?? Building and starting containers..."
docker-compose up --build -d

echo "? Waiting for services to be ready..."
sleep 15

echo "???  Running database migrations..."
docker-compose exec pricing-api dotnet ef database update --project /src/Pricing.Infrastructure --startup-project /src/Pricing.Api

echo "?? Seeding the database with sample data..."
echo "You can seed the database by calling: curl -X POST http://localhost:5000/dev/seed"

echo ""
echo "? Application is ready!"
echo "?? API URL: http://localhost:5000"
echo "?? Swagger UI: http://localhost:5000"
echo "???  SQL Server: localhost:1433 (sa/YourStrong@Passw0rd)"
echo ""
echo "To stop the application, run: docker-compose down"