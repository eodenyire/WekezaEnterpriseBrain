# Wekeza Enterprise Brain

A real-time decision engine with comprehensive multi-system integration for unified customer intelligence across banking systems.

**Databases Discovered: 15 across all Wekeza systems**

## Overview

Wekeza Enterprise Brain is an enterprise-level data platform that provides:

- **Multi-Database Integration** - Connects to 15 databases across 9+ Wekeza systems
- **Global Customer Identity (GCID)** - One unified customer ID across all systems
- **Multi-System Integration** - Connect to all Wekeza banking systems
- **Customer 360** - Real-time unified view of customer data
- **Event-Driven Architecture** - Real-time data streaming and processing
- **Feature Store** - ML-ready features for AI/analytics
- **Real-time Decision Engine** - Sub-200ms decision-making for fraud, risk, and AI systems

## System Integration

### Database Discovery

Through comprehensive system analysis, we have **discovered and connected all 15 databases** across the Wekeza ecosystem:

**Database Inventory:**
- **Core Banking**: 5 databases (ComprehensiveWekezaApi, DatabaseWekezaApi, EnhancedWekezaApi, MinimalWekezaApi, Wekeza.Core.Api)
- **Channels**: 3 databases (Mobile Banking, Web Banking, USSD)
- **Security & Risk**: 2 databases (Fraud Detection, ERMS)
- **Integration**: 2 databases (Open Banking/Nexus, AI Copilot)
- **Analytics & Support**: 3 databases (BI/Analytics, Audit Logs, Reporting)

**Integration Status:**
- ✅ **15 databases connected** (100%) 🎉
- 🟡 **0 databases waiting** (0%)

**🎉 Integration Complete - All databases connected!**

See [DATABASE_INVENTORY.md](DATABASE_INVENTORY.md) for complete details.

### How We Connect

The Enterprise Brain uses a **sophisticated connector pattern architecture** to integrate all databases:

**Connection Methods:**
- **Direct Database**: PostgreSQL connections with pooling (8 systems)
- **REST API**: OAuth2-secured HTTP APIs (3 systems)
- **Event-Driven**: Real-time pub/sub streams (2 systems)
- **Hybrid**: Combined direct + event patterns (2 systems)

**Key Features:**
- 11 specialized connector implementations
- Unified `IDataSourceConnector` interface
- Async/await for scalability
- TLS/OAuth2 security
- Health monitoring & auto-recovery
- Connection pooling & retry logic

**📖 See [CONNECTION_ARCHITECTURE.md](CONNECTION_ARCHITECTURE.md) for complete technical details**

**🔌 Test Connections:**
```bash
# Via API
curl http://localhost:5273/api/connectioninfo/architecture
curl http://localhost:5273/api/connectioninfo/status

# Via demo
./demo-integration.sh
```

### Integrated Wekeza Systems

The Enterprise Brain aggregates data from:

1. **Core Banking Systems**
   - ComprehensiveWekezaApi
   - DatabaseWekezaApi
   - EnhancedWekezaApi
   - MinimalWekezaApi

2. **Banking Channels**
   - Mobile Banking (iOS/Android)
   - Web Banking
   - USSD

3. **Risk & Fraud**
   - Fraud Detection System
   - Risk Management (ERMS)

4. **Other Systems**
   - Open Banking (Nexus)
   - AI Financial Copilot
   - Analytics & Reporting

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│  Multiple Wekeza Systems                                 │
│  (Core Banking, Mobile, Web, USSD, Fraud, Risk, etc.)   │
└───────────────────────┬──────────────────────────────────┘
                        │
                        │ Data Connectors + Events
                        ▼
┌──────────────────────────────────────────────────────────┐
│         Wekeza Enterprise Brain                          │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Data Source Registry & Connectors                 │  │
│  └────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Event Bus (CustomerEvents, TransactionEvents)     │  │
│  └────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Identity Resolution Service (GCID)                │  │
│  └────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Customer 360 Aggregation                          │  │
│  └────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Feature Store (ML-Ready)                          │  │
│  └────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Real-time Decision Engine                         │  │
│  └────────────────────────────────────────────────────┘  │
└───────────────────────┬──────────────────────────────────┘
                        │
                        │ REST APIs
                        ▼
┌──────────────────────────────────────────────────────────┐
│  Consuming Systems                                       │
│  (AI Copilot, Fraud Detection, Risk, Open Banking, BI)  │
└──────────────────────────────────────────────────────────┘
```

## Project Structure

```
WekezaEnterpriseBrain/
├── src/
│   ├── WekezaEnterpriseBrain.Api/          # REST API Layer
│   ├── WekezaEnterpriseBrain.Core/         # Domain Models & Interfaces
│   └── WekezaEnterpriseBrain.Infrastructure/ # Data Access & External Services
├── tests/
│   └── WekezaEnterpriseBrain.Tests/        # Unit & Integration Tests
└── docs/                                    # Architecture Documentation
```

## Key Features

### 1. Multi-System Data Integration
- **Data Source Connectors**: Plug-and-play connectors for all Wekeza systems
- **Connection Management**: Test, enable/disable data sources dynamically
- **Real-time Sync**: Batch and real-time data synchronization
- **Aggregation Stats**: Monitor data flow across all systems

### 2. Global Customer Identity Resolution
- Unifies customer identities across multiple systems
- Supports National ID, Phone, Email matching
- Automatic identity linking and deduplication
- Single GCID for "One Bank, One Customer"

### 3. Customer 360 View
- Real-time customer profile aggregation
- Account and transaction history across all systems
- Behavioral metrics and risk scoring
- Financial summary across all products

### 4. Event-Driven Architecture
- **Domain Events**: CustomerEvent, TransactionEvent, FraudAlertEvent, etc.
- **Event Bus**: In-memory (POC) with Kafka-ready design
- **Real-time Processing**: Event handlers for instant reactions
- **Async Operations**: Non-blocking event publishing

### 5. Feature Store for AI/ML
- **30+ Pre-computed Features**:
  - Transaction patterns (monthly spend, velocity)
  - Channel behavior (login counts, preferred channel)
  - Risk indicators (fraud alerts, failed logins)
  - Financial health (balances, loans)
  - Behavioral patterns (hourly/daily patterns)
- **ML-Ready**: Features ready for model consumption
- **Feature Importance**: Track which features matter most

### 6. Real-time Decision Engine
- Sub-200ms decision latency
- Context-aware fraud detection
- Automated risk assessment
- Support for multiple event types:
  - Payment authorization
  - Loan approval
  - Login verification
  - Account opening

## API Endpoints

**Total: 22 endpoints across 5 controllers**

### Database Inventory APIs (3 endpoints) 🆕
- `GET /api/databaseinventory` - Get complete database inventory
- `GET /api/databaseinventory/summary` - Get database discovery summary
- `GET /api/databaseinventory/categories` - Get database categories

### Customer APIs (4 endpoints)
- `GET /api/customer/{gcid}` - Get customer 360 profile
- `GET /api/customer/{gcid}/accounts` - Get customer accounts
- `GET /api/customer/{gcid}/transactions` - Get recent transactions
- `GET /api/customer/resolve` - Resolve identity to GCID

### Decision Engine APIs (2 endpoints)
- `POST /api/decision` - Make real-time decision
- `GET /api/decision/risk-score/{gcid}` - Calculate risk score

### Data Source Management APIs (9 endpoints)
- `GET /api/datasources` - List all data sources
- `GET /api/datasources/{id}` - Get specific data source
- `POST /api/datasources` - Register new data source
- `POST /api/datasources/{id}/enable` - Enable data source
- `POST /api/datasources/{id}/disable` - Disable data source
- `GET /api/datasources/test-connections` - Test all connections
- `POST /api/datasources/{id}/sync` - Sync single data source
- `POST /api/datasources/sync-all` - Sync all data sources
- `GET /api/datasources/statistics` - Get aggregation statistics

### Feature Store APIs (4 endpoints)
- `GET /api/features/{gcid}` - Get customer features
- `POST /api/features/{gcid}/calculate` - Calculate features
- `GET /api/features/importance` - Get feature importance scores
- `POST /api/features/refresh-all` - Refresh all features

### Health Check
- `GET /health` - Service health status

## Getting Started

### Prerequisites
- .NET 10.0 SDK or higher
- (Optional) PostgreSQL for production deployment

### Build and Run

```bash
# Build the solution
dotnet build

# Run the API
cd src/WekezaEnterpriseBrain.Api
dotnet run

# The API will be available at http://localhost:5000
```

### Run Tests

```bash
dotnet test
```

## Example Usage

### List All Databases

```bash
# Quick database list with nice formatting
./list-databases.sh

# Or via API
curl http://localhost:5273/api/databaseinventory

# Or view the quick reference
cat DATABASES_LIST.md
```

### 1. List Integrated Systems

```bash
curl -X GET "http://localhost:5273/api/datasources"
```

### 2. Sync Data from All Systems

```bash
curl -X POST "http://localhost:5273/api/datasources/sync-all"
```

### 3. Resolve Customer Identity

```bash
curl -X GET "http://localhost:5273/api/customer/resolve?nationalId=12345678&phone=+254700000000"
```

### 4. Get Customer 360

```bash
curl -X GET "http://localhost:5273/api/customer/{gcid}"
```

### 5. Calculate AI Features

```bash
curl -X POST "http://localhost:5273/api/features/{gcid}/calculate"
```

### 6. Make a Decision

```bash
curl -X POST "http://localhost:5273/api/decision" \
  -H "Content-Type: application/json" \
  -d '{
    "globalCustomerId": "guid-here",
    "eventType": "PAYMENT",
    "amount": 5000,
    "channel": "Mobile"
  }'
```

### 7. Run Complete Integration Demo

```bash
./demo-integration.sh
```

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# 13
- **Architecture**: Clean Architecture / Onion Architecture
- **Storage**: In-memory (POC), PostgreSQL (Production)
- **Testing**: xUnit

## Production Enhancement Roadmap

For full production deployment, enhance with:

## Documentation

See the `/docs` directory for detailed architecture documentation:

- `Concept.md` - System concept and vision
- `ImplementationPlan.md` - Full implementation roadmap
- `Customer360DataModel.md` - Data model specifications
- `SystemArchitecture.md` - Technical architecture
- `PRD.md` - Product requirements
- `BRD.md` - Business requirements

## Contributing

This is an enterprise project. For contribution guidelines, please contact the development team.

## License

Proprietary - Wekeza Bank

## Support

For support and questions, contact: [support@wekeza.bank]
