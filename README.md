# Pricing API System

A comprehensive .NET 9 Web API for managing product pricing across multiple suppliers with advanced features including best price calculation, CSV bulk import, and currency conversion support.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns:

```
├── Pricing.Api/          # API Layer (Controllers/Endpoints)
├── Pricing.Application/   # Application Layer (Use Cases/Handlers)
├── Pricing.Domain/       # Domain Layer (Entities/Value Objects)
└── Pricing.Infrastructure/ # Infrastructure Layer (Data Access/External Services)
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

## 🛠️ Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9**: ORM for data access
- **SQL Server**: Primary database
- **OpenAPI/Swagger**: API documentation
- **CORS**: Cross-origin resource sharing support

## 📋 Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB/Express/Full)
- Visual Studio 2022 or VS Code
- Git

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd PartA
```

### 2. Database Setup
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PricingDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Database Migration
```bash
dotnet ef database update --project Pricing.Infrastructure --startup-project Pricing.Api
```

### 4. Seed Sample Data (Optional)
```bash
# Start the application first, then:
curl -X POST "https://localhost:7001/dev/seed"
```

### 5. Run the Application
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
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PricingDb;Trusted_Connection=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables
- `DOTNET_ENVIRONMENT`: Development/Staging/Production
- `ConnectionStrings__DefaultConnection`: Database connection override

## 🧪 Testing


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
```





