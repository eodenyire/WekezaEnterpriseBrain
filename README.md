# Wekeza Enterprise Brain

A real-time decision engine for unified customer intelligence across banking systems.

## Overview

Wekeza Enterprise Brain is an enterprise-level data platform that provides:

- **Global Customer Identity (GCID)** - One unified customer ID across all systems
- **Customer 360** - Real-time unified view of customer data
- **Real-time Decision Engine** - Sub-200ms decision-making for fraud, risk, and AI systems
- **Event-Driven Architecture** - Scalable data aggregation from multiple sources

## Architecture

```
┌─────────────────────────────────────────────────────┐
│  Multiple Banking Systems (Core, Channels, Fraud)  │
└──────────────────┬──────────────────────────────────┘
                   │
                   │ Events/CDC/APIs
                   ▼
┌─────────────────────────────────────────────────────┐
│         Wekeza Enterprise Brain                      │
│  ┌──────────────────────────────────────────────┐   │
│  │  Identity Resolution Service (GCID)          │   │
│  └──────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────┐   │
│  │  Customer 360 Aggregation                    │   │
│  └──────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────┐   │
│  │  Real-time Decision Engine                   │   │
│  └──────────────────────────────────────────────┘   │
└──────────────────┬──────────────────────────────────┘
                   │
                   │ REST APIs
                   ▼
┌─────────────────────────────────────────────────────┐
│  Consuming Systems (AI Copilot, Fraud, Risk, etc)  │
└─────────────────────────────────────────────────────┘
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

### 1. Global Customer Identity Resolution
- Unifies customer identities across multiple systems
- Supports National ID, Phone, Email matching
- Automatic identity linking and deduplication

### 2. Customer 360 View
- Real-time customer profile aggregation
- Account and transaction history
- Behavioral metrics and risk scoring
- Financial summary across all products

### 3. Real-time Decision Engine
- Sub-200ms decision latency
- Context-aware fraud detection
- Automated risk assessment
- Support for multiple event types:
  - Payment authorization
  - Loan approval
  - Login verification
  - Account opening

## API Endpoints

### Customer APIs
- `GET /api/customer/{gcid}` - Get customer 360 profile
- `GET /api/customer/{gcid}/accounts` - Get customer accounts
- `GET /api/customer/{gcid}/transactions` - Get recent transactions
- `GET /api/customer/resolve` - Resolve identity to GCID

### Decision Engine APIs
- `POST /api/decision` - Make real-time decision
- `GET /api/decision/risk-score/{gcid}` - Calculate risk score

### Health Check
- `GET /health` - Service health status

## Getting Started

### Prerequisites
- .NET 9.0 SDK or higher
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

### Resolve Customer Identity

```bash
curl -X GET "http://localhost:5000/api/customer/resolve?nationalId=12345678&phone=+254700000000"
```

### Get Customer 360

```bash
curl -X GET "http://localhost:5000/api/customer/{gcid}"
```

### Make a Decision

```bash
curl -X POST "http://localhost:5000/api/decision" \
  -H "Content-Type: application/json" \
  -d '{
    "globalCustomerId": "guid-here",
    "eventType": "PAYMENT",
    "amount": 5000,
    "channel": "Mobile"
  }'
```

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Language**: C# 13
- **Architecture**: Clean Architecture / Onion Architecture
- **Storage**: In-memory (POC), PostgreSQL (Production)
- **Testing**: xUnit

## Current Status

This is a Proof of Concept (POC) implementation with in-memory storage. For production deployment, the following enhancements are planned:

- [ ] PostgreSQL database implementation
- [ ] Event streaming with Kafka
- [ ] CDC (Change Data Capture) connectors
- [ ] Redis caching layer
- [ ] Authentication & Authorization
- [ ] Comprehensive monitoring & observability
- [ ] Kubernetes deployment

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
