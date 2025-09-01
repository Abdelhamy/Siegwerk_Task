# Pricing API System

A comprehensive .NET 9 Web API for managing product pricing across multiple suppliers with advanced features including best price calculation, CSV bulk import, and currency conversion support.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns:

```
├── Pricing.Api/                # API Layer (Controllers/Endpoints)
├── Pricing.Application/         # Application Layer (Use Cases/Handlers)
├── Pricing.Domain/             # Domain Layer (Entities/Value Objects)
├── Pricing.Infrastructure/     # Infrastructure Layer (Data Access/External Services)
└── Pricing.Application.Tests/  # Unit & Integration Tests
```

## 🚀 Features

### Core Functionality
- **Best Price Calculation**: Find optimal prices with intelligent tie-breaking
- **Price List Management**: CRUD operations for supplier price lists
- **Multi-Currency Support**: USD, EUR, EGP with conversion capabilities
- **Advanced Filtering**: Filter by SKU, supplier, date, currency, and quantity
- **Pagination & Sorting**: Efficient data retrieval with customizable sorting

### Bulk Operations
- **CSV Import**: Bulk upload price lists with comprehensive validation
- **Template Download**: Get properly formatted CSV templates
- **Validation Engine**: Row-by-row validation with detailed error reporting
- **Overlap Detection**: Prevent conflicting date ranges for same supplier/SKU

### Technical Features
- **Clean Architecture**: Separation of concerns with CQRS pattern
- **Entity Framework Core**: Code-first approach with SQL Server
- **Minimal APIs**: Modern .NET 9 endpoint routing
- **Comprehensive Logging**: Structured logging with Serilog-style patterns
- **OpenAPI/Swagger**: Complete API documentation
- **Health Checks**: Built-in health monitoring
- **Comprehensive Testing**: Unit and integration tests with high coverage

## 🛠️ Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9**: ORM for data access
- **SQL Server**: Primary database
- **OpenAPI/Swagger**: API documentation
- **CORS**: Cross-origin resource sharing support
- **xUnit**: Testing framework
- **Moq**: Mocking framework for unit tests
- **FluentAssertions**: Fluent test assertions

## 📋 Prerequisites

- .NET 9 SDK (for local development)
- **Docker Desktop** (for containerized deployment)
- SQL Server (LocalDB/Express/Full) - *for local development only*
- Visual Studio 2022 or VS Code
- Git

## 🚀 Getting Started

You can run this application in two ways: **locally** or with **Docker** (recommended for simplicity).

### Option 1: 🐳 Docker Deployment (Recommended)

This is the easiest way to run the application. Docker will handle all dependencies including SQL Server.

#### Prerequisites for Docker
- Docker Desktop installed and running
- Git

#### Steps
1. **Clone the Repository**
```bash
git clone https://github.com/Abdelhamy/Siegwerk_Task
cd Siegwerk_Task
```

2. **Run with Docker**

**On Windows:**
```bash
start-docker.bat
```

**On Linux/Mac:**
```bash
chmod +x start-docker.sh
./start-docker.sh
```

**Or manually:**
```bash
docker-compose up --build
```

3. **Access the Application**
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000
- **SQL Server**: localhost:1433 (sa/YourStrong@Passw0rd)

4. **Seed Sample Data**
```bash
curl -X POST "http://localhost:5000/dev/seed"
```

5. **Stop the Application**
```bash
docker-compose down
```

### Option 2: 💻 Local Development

For development and debugging, you can run the application locally.

#### Prerequisites for Local Development
- .NET 9 SDK
- SQL Server (LocalDB/Express/Full)

#### Steps

1. **Clone the Repository**
```bash
git clone https://github.com/Abdelhamy/Siegwerk_Task
cd Siegwerk_Task
```

2. **Database Setup**
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PricingDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

3. **Database Migration**
```bash
dotnet ef database update --project Pricing.Infrastructure --startup-project Pricing.Api
```

4. **Seed Sample Data (Optional)**
```bash
# Start the application first, then:
curl -X POST "https://localhost:7001/dev/seed"
```

5. **Run the Application**
```bash
dotnet run --project Pricing.Api
```

The API will be available at:
- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5000`
- **Swagger UI**: `https://localhost:7001` (root URL)

## 📖 API Documentation

### Core Endpoints

#### 🔍 Get Best Price
```http
GET /pricing/best?sku=SKU-1001&qty=10&currency=USD&date=2025-01-15
```

**Response:**
```json
{
  "sku": "SKU-1001",
  "qty": 10,
  "currency": "USD",
  "unitPrice": 24.50,
  "total": 245.00,
  "supplierId": 1,
  "supplierName": "ACME Corporation",
  "supplierPreferred": true,
  "supplierLeadTimeDays": 2,
  "reason": "Lowest unit price with preferred supplier status"
}
```

#### 📋 List Price Entries
```http
GET /pricing/prices?sku=SKU-1001&quantity=5&validOn=2025-01-15&currency=USD&page=1&pageSize=10
```

**Response:**
```json
{
  "prices": [...],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "filtering": {
    "hasFilters": true,
    "appliedFilters": {
      "sku": "SKU-1001",
      "quantity": 5,
      "validOn": "2025-01-15",
      "currency": "USD"
    }
  }
}
```

#### 📤 Upload CSV Price List
```http
POST /pricing/prices/upload-csv
Content-Type: multipart/form-data

file: [CSV file]
```

#### 📥 Download CSV Template
```http
GET /pricing/prices/csv-template
```

### Development Endpoints

#### 🗃️ Seed Database
```http
POST /dev/seed
```

## 📊 CSV Import Format

### Required Columns
| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| SupplierId | int | ✅ | Supplier identifier | 1 |
| Sku | string | ✅ | Product SKU | SKU-1001 |
| ValidFrom | date | ✅ | Start date (yyyy-MM-dd) | 2025-01-01 |
| ValidTo | date | ❌ | End date (yyyy-MM-dd) | 2025-12-31 |
| Currency | string | ✅ | Currency code | USD |
| PricePerUom | decimal | ✅ | Unit price | 25.50 |
| MinQty | int | ✅ | Minimum quantity | 10 |

### Example CSV
```csv
SupplierId,Sku,ValidFrom,ValidTo,Currency,PricePerUom,MinQty
1,SKU-1001,2025-01-01,2025-12-31,USD,25.50,10
1,SKU-1002,2025-01-01,,EUR,18.75,5
2,SKU-1001,2025-02-01,2025-11-30,USD,24.00,15
```

### Validation Rules
- ✅ **Data Types**: All fields must match expected types
- ✅ **Foreign Keys**: Supplier must exist in system
- ✅ **Date Logic**: ValidTo > ValidFrom (if provided)
- ✅ **Currency**: Must be supported (USD, EUR, EGP)
- ✅ **Positive Values**: Price and quantity must be > 0
- ✅ **No Overlaps**: Same supplier/SKU cannot have overlapping date ranges

## 🏛️ Domain Model

### Core Entities
```csharp
// Supplier
public class Supplier
{
    public int Id { get; }
    public string Name { get; }
    public string Country { get; }
    public bool Preferred { get; }
    public LeadTime LeadTime { get; }
}

// Product
public class Product
{
    public int Id { get; }
    public Sku Sku { get; }
    public string Name { get; }
    public string UnitOfMeasure { get; }
    public string? HazardClass { get; }
}

// Price List Entry
public class PriceListEntry
{
    public int Id { get; }
    public int SupplierId { get; }
    public Sku Sku { get; }
    public DateRange ValidityPeriod { get; }
    public Money Price { get; }
    public Quantity MinimumQuantity { get; }
}
```

### Value Objects
- **Sku**: Product identifier with validation
- **Money**: Amount + Currency with conversion support
- **DateRange**: From/To dates with overlap detection
- **Quantity**: Positive integer with minimum checks
- **LeadTime**: Days with business constraints

## 🔧 Configuration

### Application Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PricingDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Pricing": "Debug"
    }
  }
}
```

### Environment Variables
- `DOTNET_ENVIRONMENT`: Development/Staging/Production
- `ConnectionStrings__DefaultConnection`: Database connection override

## 🧪 Testing

The project includes comprehensive unit and integration tests using modern .NET testing practices.

### Test Framework & Tools
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for isolating dependencies
- **FluentAssertions**: Expressive and fluent test assertions
- **Coverlet**: Code coverage analysis

### Test Structure
```
Pricing.Application.Tests/
├── UseCases/
│   └── Pricing/
│       └── Queries/
│           └── GetBestPrice/
│               ├── GetBestPriceHandlerTests.cs          # Unit tests
│               ├── GetBestPriceHandlerIntegrationTests.cs # Integration tests
│               └── GetBestPriceTestDataHelper.cs        # Test data helpers
└── GlobalUsings.cs
```

### Running Tests

#### Run All Tests
```bash
dotnet test
```

#### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

#### Run Specific Test Project
```bash
dotnet test Pricing.Application.Tests
```

#### Run Tests in Watch Mode
```bash
dotnet watch test --project Pricing.Application.Tests
```

### Test Categories

#### Unit Tests
- **Business Logic**: Domain entities and value objects
- **Application Handlers**: Use case implementations with mocked dependencies
- **Service Logic**: Individual service components in isolation

#### Integration Tests
- **End-to-End Scenarios**: Complete request flows through the application
- **Database Integration**: Repository and data access layer testing
- **External Service Integration**: Currency conversion and external APIs

### Test Coverage
The test suite covers:
- ✅ **Best Price Calculation Logic**: All pricing scenarios and tie-breaking rules
- ✅ **Currency Conversion**: Multi-currency support and rate conversion
- ✅ **Validation Logic**: Input validation and business rule enforcement
- ✅ **Domain Models**: Entity behavior and value object validation

### Sample Test Cases
- Single price candidate selection
- Multiple candidates with lowest price selection
- Preferred supplier tie-breaking
- Currency conversion scenarios
- No candidates found handling
- Invalid input validation

### Test Data
Use the `/dev/seed` endpoint to populate the database with sample data:
- 10 Suppliers (mixed preferred/non-preferred)
- 10 Products (various categories)
- 50+ Price List Entries (diverse pricing scenarios)

## 🏗️ Project Structure

```
PartA/
├── Pricing.Api/
│   ├── Endpoints/
│   │   ├── Pricing/PricingEndpoints.cs
│   │   ├── Products/ProductEndpoints.cs
│   │   ├── Suppliers/SupplierEndpoints.cs
│   │   └── Development/DevEndpoints.cs
│   ├── Extensions/ApiExtensions.cs
│   └── Program.cs
├── Pricing.Application/
│   ├── UseCases/
│   │   ├── Pricing/Queries/GetBestPrice/
│   │   ├── PriceLists/Queries/GetPricesPagedQuery/
│   │   ├── PriceLists/Commands/ImportPricesFromCsv/
│   │   ├── Products/Commands/ & Queries/
│   │   └── Suppliers/Commands/ & Queries/
│   ├── Common/Interfaces/ & Models/
│   ├── Contracts/
│   └── Services/
├── Pricing.Application.Tests/
│   ├── UseCases/
│   │   └── Pricing/Queries/GetBestPrice/
│   └── GlobalUsings.cs
├── Pricing.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Common/
│   └── Constants/
└── Pricing.Infrastructure/
    ├── Persistence/
    │   ├── Configurations/
    │   ├── Repositories/
    │   └── PricingDbContext.cs
    ├── Extensions/
    └── Rates/





