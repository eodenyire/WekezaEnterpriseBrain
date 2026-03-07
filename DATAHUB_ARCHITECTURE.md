# Wekeza Main Datahub - Architecture & Implementation Guide

**Version:** 1.0.0  
**Status:** Implemented  
**Database:** PostgreSQL (wekeza_datahub)

---

## Overview

The **Wekeza Main Datahub** is a unified PostgreSQL data warehouse that consolidates data from **all Wekeza banking systems** into a single intelligence hub. It resolves customer identity across systems, stores all transactional, risk, CRM and interaction data, and powers AI/ML-ready features for real-time decision-making.

---

## Source Systems Discovered (github.com/eodenyire)

| Repository | System Type | Database | Technology | Write Frequency |
|---|---|---|---|---|
| [WekezaBank](https://github.com/eodenyire/WekezaBank) | Risk Management / KYC-AML | PostgreSQL `risk_management` | Python / SQLAlchemy | Every 30s (polling) |
| [WekezaCRM](https://github.com/eodenyire/WekezaCRM) | Customer Relationship Mgmt | SQL Server `WekezaCRM` | .NET 8 / EF Core | Real-time (high volume) |
| [WekezaOpenBanking](https://github.com/eodenyire/WekezaOpenBanking) | Open Banking API | PostgreSQL `wekeza_banking` | Node.js / pg | Real-time + Webhooks |
| [Wekeza](https://github.com/eodenyire/Wekeza) | Core Banking | PostgreSQL `WekezaCoreDB` | .NET 8 / EF Core + Dapper | Real-time (very high) |
| [WekezaNextGenPersonalBanking](https://github.com/eodenyire/WekezaNextGenPersonalBanking) | Personal Banking AI Channel | API Aggregation Layer | .NET Core | On-demand |
| [WekezaGlobal](https://github.com/eodenyire/WekezaGlobal) | Cross-Border Banking | *pending* | *pending* | *pending* |
| [WekezaDFS](https://github.com/eodenyire/WekezaDFS) | Digital Financial Services | *pending* | *pending* | *pending* |
| [WekezaHela](https://github.com/eodenyire/WekezaHela) | *pending* | *pending* | *pending* | *pending* |
| [WekezaPublicSectorBanking](https://github.com/eodenyire/WekezaPublicSectorBanking) | Public Sector Banking | *pending* | *pending* | *pending* |

---

## Database Schema Architecture

```
                    ┌─────────────────────────────────────────────────────────┐
                    │              wekeza_datahub (PostgreSQL)                │
                    │                                                         │
  ┌────────────┐    │  ┌──────────────────┐   ┌──────────────────────────┐  │
  │  WekezaBank│───▶│  │  STAGING SCHEMA  │   │   WAREHOUSE SCHEMA        │  │
  │ (risk_mgmt)│    │  │                  │   │                          │  │
  └────────────┘    │  │ stg_customers    │──▶│  dim_customers (GCID)     │  │
                    │  │ stg_accounts     │──▶│  dim_accounts             │  │
  ┌────────────┐    │  │ stg_transactions │──▶│  dim_date                 │  │
  │WekezaCRM   │───▶│  │ stg_risk_assess. │   │                          │  │
  │(SQL Server)│    │  │ stg_crm_interact │──▶│  fact_transactions        │  │
  └────────────┘    │  │ stg_crm_cases    │──▶│  fact_payments            │  │
                    │  │ stg_ob_payments  │──▶│  fact_risk_assessments    │  │
  ┌────────────┐    │  │ stg_webhook_evts │──▶│  fact_interactions        │  │
  │WekezaOpenBk│───▶│  └──────────────────┘   │  fact_cases               │  │
  │(wkz_bankng)│    │                          └──────────────────────────┘  │
  └────────────┘    │  ┌──────────────────────────────────────────────────┐  │
                    │  │              ANALYTICS SCHEMA                    │  │
  ┌────────────┐    │  │                                                  │  │
  │WekeazCore  │───▶│  │  mv: customer_360              (materialized)    │  │
  │(WekezaCoreD│    │  │  mv: daily_transaction_summary (materialized)    │  │
  └────────────┘    │  │  mv: risk_dashboard            (materialized)    │  │
                    │  │  tbl: customer_features        (feature store)   │  │
                    │  └──────────────────────────────────────────────────┘  │
                    │                                                         │
                    │  ┌──────────────────────────────────────────────────┐  │
                    │  │              AUDIT SCHEMA                        │  │
                    │  │  etl_sync_log (all ETL run history)              │  │
                    │  └──────────────────────────────────────────────────┘  │
                    └─────────────────────────────────────────────────────────┘
```

---

## Data Flow

```
Source System  →  ETL Service  →  Staging  →  Identity Resolution  →  Warehouse  →  Analytics
     │                │                              │
     │         [Dapper raw SQL]           [resolve_customer_gcid()]
     │         [incremental load]         [match by email/phone/natId]
     │
     ▼
WekezaBank:
  transaction_history → fact_transactions (with risk_score, channel mapping)
  analyst_cases       → fact_risk_assessments (with Tazama/CISO/Ballerine data)

WekezaCRM:
  Customers           → dim_customers (email, phone, CreditScore, KYCStatus)
  Accounts            → dim_accounts
  Transactions        → fact_transactions
  Interactions        → fact_interactions (with SentimentAnalysis JOIN)
  Cases               → fact_cases (with SLA breach calculation)

WekezaOpenBanking:
  customers           → dim_customers (customer_number, kyc_status)
  accounts            → dim_accounts (balance, available_balance)
  transactions        → fact_transactions (API channel)
  payments            → fact_payments (risk_score, oauth_client)
```

---

## Global Customer Identity (GCID)

The GCID (Global Customer ID) is the heart of the datahub. Every customer gets exactly **one GCID** regardless of how many Wekeza systems they appear in.

### Identity Resolution Algorithm

```
1. Match by national_id (strongest signal)
2. Match by primary_email (normalized to lowercase)
3. Match by primary_phone
4. No match → create new dim_customer record
```

### Cross-System ID Mapping

```sql
-- Each dim_customer row stores all local system IDs
SELECT gcid,
       core_banking_id,   -- Wekeza Core Banking
       crm_id,            -- WekezaCRM  
       open_banking_id,   -- WekezaOpenBanking (customer_number)
       risk_system_id,    -- WekezaBank (customer_id)
       personal_banking_id -- WekezaNextGenPersonalBanking
FROM warehouse.dim_customers
WHERE primary_email = 'customer@example.com';
```

---

## WekezaBank - Deep Schema Analysis

**Database:** `risk_management` (PostgreSQL / SQLite for dev)  
**Tables:**

```sql
-- Transaction monitoring
transaction_history (
    id, transaction_id (unique), customer_id, account_number,
    amount (DECIMAL 15,2), currency (KES), transaction_type (TRANSFER/PAYMENT/WITHDRAWAL/DEPOSIT),
    merchant_name, merchant_category, location,
    channel (MOBILE/ONLINE/ATM/BRANCH),
    timestamp, status (PENDING/APPROVED/REJECTED/BLOCKED)
)

-- Risk case management  
analyst_cases (
    case_id, transaction_id (unique FK), customer_id,
    amount, currency, merchant_name, transaction_type,
    risk_score (FLOAT 0-1), risk_level (LOW/MEDIUM/HIGH),
    status (ASSIGNED/REVIEWED/CLOSED),
    analyst_id, analyst_comment, flagged_reason,
    created_at, updated_at, closed_at
)

-- Portfolio risk metrics
risk_metrics (
    metric_id, metric_type (CREDIT/LIQUIDITY/MARKET/OPERATIONAL),
    metric_name, metric_value (DECIMAL 15,4), threshold_value,
    status (OK/WARNING/CRITICAL), calculated_at
)
```

**Risk Thresholds:**
- HIGH: score ≥ 0.8 (auto-block, create analyst case, log to CISO)
- MEDIUM: 0.5 ≤ score < 0.8 (create analyst case, flag for review)
- LOW: score < 0.5 (auto-approve)

**External Integrations:**
- **Ballerine** - KYC/AML workflow management
- **CISO Assistant** - Security risk logging
- **Tazama** - ISO20022 transaction monitoring, fraud typology detection

---

## WekezaCRM - Deep Schema Analysis

**Database:** `WekezaCRM` (SQL Server)  
**DbContext:** `CRMDbContext`  
**22 Entity Classes:**

### Phase 1 (Core CRM)
| Entity | Key Fields | Write Frequency |
|--------|-----------|-----------------|
| Customer | Email (unique), PhoneNumber, NationalId, CustomerReference (unique), CreditScore, LifetimeValue, Segment (enum), KYCStatus (enum) | Low |
| Account | AccountNumber (unique), Balance (18,2), Currency, IsActive | Medium |
| Transaction | TransactionReference (unique), Amount (18,2), BalanceAfter, TransactionDate | High |
| Case | CaseNumber (unique), Status (Open/InProgress/Resolved/Closed), Priority, SLADurationHours | Medium |
| CaseNote | Note, CaseId (FK) | Medium |
| Interaction | Channel (Phone/Email/SMS/Chat), Subject, DurationMinutes, InteractionDate | High |
| Campaign | TargetSegment, StartDate, EndDate, TargetCustomers, ReachedCustomers | Low |

### Phase 2 (AI/Analytics)
| Entity | Key Fields | Write Frequency |
|--------|-----------|-----------------|
| NextBestAction | ActionType (enum), ConfidenceScore (5,2), RecommendedProduct, AIModelVersion | High |
| SentimentAnalysis | SentimentType (Positive/Negative/Neutral), SentimentScore (5,2), KeyPhrases, TextAnalyzed | High |
| WorkflowDefinition | TriggerType, TriggerConditions (JSON), Actions (JSON), ExecutionOrder | Low |
| WorkflowInstance | Status (enum), StartedAt, CompletedAt, Result (JSON) | High |
| Notification | NotificationType (enum), IsRead, ActionUrl | High |
| AnalyticsReport | ReportType, ReportData (JSON), GeneratedDate | Low |

### Phase 3 (Omnichannel)
| Entity | Key Fields | Write Frequency |
|--------|-----------|-----------------|
| WhatsAppMessage | PhoneNumber, MessageType (enum), Status (enum), Content, IsInbound, SentAt, DeliveredAt, ReadAt | Very High |
| USSDSession | SessionId (unique), PhoneNumber, CurrentMenu, MenuHistory (JSON), TransactionData (JSON) | Very High |
| ReportTemplate | CronExpression, QueryDefinition (SQL), ParametersSchema (JSON), LayoutTemplate (HTML) | Low |
| ReportSchedule | CronExpression, Frequency, Recipients (JSON), NextExecutionAt | Low |
| GeneratedReport | ReportContent, Format (PDF/Excel/HTML), GeneratedAt, RecipientCount | Medium |

---

## WekezaOpenBanking - Deep Schema Analysis

**Database:** `wekeza_banking` (PostgreSQL)  
**Technology:** Node.js / Express / pg connection pooling (max: 20)

```sql
-- OAuth2 infrastructure
oauth_clients (id UUID, client_id, client_secret, name, redirect_uris TEXT[], scopes TEXT[])
oauth_tokens (id UUID, access_token (unique), refresh_token (unique), client_id FK, user_id UUID, scopes TEXT[], expires_at)

-- Banking entities
customers (id UUID, customer_number (unique), first_name, last_name, email (unique), phone, date_of_birth, kyc_status)
accounts (id UUID, account_number (unique), customer_id FK, account_type, currency (KES), balance DECIMAL(15,2), available_balance, status)
transactions (id UUID, transaction_ref (unique), account_id FK, transaction_type, amount DECIMAL(15,2), currency, balance_after, description, status, transaction_date)
payments (id UUID, payment_ref (unique), source_account_id FK, destination_account_number, amount, currency, risk_score DECIMAL(3,2), idempotency_key, status, completed_at)

-- Event-driven webhook infrastructure
webhooks (id UUID, client_id FK, url, events TEXT[], secret HMAC-SHA256, is_active)
webhook_deliveries (id UUID, webhook_id FK, event_type, payload JSONB, status, attempts, next_retry_at)
```

**Payment Flow:**
1. Check idempotency key (prevent duplicate charges)
2. Verify source account exists & active
3. Check available balance
4. Calculate risk_score (amount-based + random factor)
5. Create payment in 'processing' → immediately debit available_balance
6. Create debit transaction record
7. Mark payment 'completed'
8. **Trigger webhooks asynchronously** (HMAC-SHA256 signature, 7 retries with exponential backoff: 1s, 2s, 4s, 8s, 16s, 32s, 64s)

---

## API Endpoints - DataHub

Base URL: `http://localhost:5273/api/datahub`

### Overview & Status
```bash
# Get datahub overview with counts and last sync times
GET /api/datahub

# Test connectivity to all source systems
GET /api/datahub/connections

# Get statistics (channel usage, risk distribution, categories)
GET /api/datahub/statistics
```

### ETL Operations
```bash
# Full sync from all source systems
POST /api/datahub/sync

# Incremental sync (only records since timestamp)
POST /api/datahub/sync?since=2026-01-01T00:00:00Z

# Sync specific source system
POST /api/datahub/sync/WekezaBank
POST /api/datahub/sync/WekezaCRM
POST /api/datahub/sync/WekezaOpenBanking

# Refresh analytics materialized views
POST /api/datahub/analytics/refresh
```

### Customer Intelligence
```bash
# Get full Customer 360 view from datahub
GET /api/datahub/customers/{gcid}

# Search customers across all systems
GET /api/datahub/customers/search?email=john@example.com
GET /api/datahub/customers/search?phone=+254711000001
GET /api/datahub/customers/search?nationalId=12345678
```

---

## PostgreSQL DataHub Setup

```bash
# 1. Create the database
psql -U postgres -c "CREATE DATABASE wekeza_datahub;"
psql -U postgres -c "CREATE USER wekeza_hub_user WITH PASSWORD 'your_secure_password';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE wekeza_datahub TO wekeza_hub_user;"

# 2. Run the migration script
psql -U wekeza_hub_user -d wekeza_datahub -f migrations/001_CreateWekezaDataHub.sql

# 3. Configure connection strings in appsettings.json
# Update ConnectionStrings:WekezaDataHub
# Update SourceSystems:WekezaBank:ConnectionString
# Update SourceSystems:WekezaCRM:ConnectionString
# Update SourceSystems:WekezaOpenBanking:ConnectionString

# 4. Run the API
cd src/WekezaEnterpriseBrain.Api && dotnet run

# 5. Trigger initial full sync
curl -X POST http://localhost:5273/api/datahub/sync

# 6. View overview
curl http://localhost:5273/api/datahub
```

---

## Feature Store (ML-Ready)

The `analytics.customer_features` table provides pre-computed features for every customer, refreshed hourly. These are used by the decision engine for sub-200ms decisions.

| Feature Group | Features |
|---|---|
| **Transaction Velocity** | txn_count_7d, txn_count_30d, txn_count_90d, total_spend_30d, total_income_30d, net_cashflow_30d |
| **Channel Behaviour** | mobile_txn_pct, web_txn_pct, ussd_txn_pct, atm_txn_pct, branch_txn_pct, api_txn_pct |
| **Risk Signals** | avg_risk_score_30d, high_risk_txn_count_30d, fraud_flags_90d, risk_level_current |
| **Account Health** | account_count, total_balance, max_account_balance |
| **CRM Intelligence** | open_cases_count, avg_sentiment_score_90d, interaction_count_90d, case_resolution_rate |
| **Open Banking** | ob_payment_count_30d, ob_payment_amount_30d, ob_failed_payment_pct_30d |
| **AI Insights** | financial_health_score, stress_level, financial_personality |
| **Temporal Patterns** | days_since_last_txn, avg_txn_hour_of_day, weekend_txn_pct |

---

## Production Considerations

### Incremental Loading
All ETL services support watermark-based incremental loading:
```bash
POST /api/datahub/sync?since=2026-03-07T00:00:00Z
```

### Recommended Sync Schedule
- **WekezaBank**: Every 1 minute (30s polling in source)
- **WekezaOpenBanking**: Every 1 minute (event-driven webhooks)
- **WekezaCRM**: Every 5 minutes
- **WekeazCore**: Every 1 minute
- **Analytics refresh**: Every hour (CONCURRENTLY to avoid locks)
- **Feature store refresh**: Every hour

### Scaling to Production
For production scale (millions of events per day), replace polling ETL with:
1. **WekezaBank** → Debezium CDC on `transaction_history` table
2. **WekezaOpenBanking** → Consume webhook events directly via Kafka
3. **WekezaCRM** → Debezium CDC on SQL Server with transactional outbox pattern
4. **WekeazCore** → MediatR domain events → Kafka → Datahub

### Security
- All connection strings in `appsettings.json` use placeholder passwords
- In production, use **Azure Key Vault** or **AWS Secrets Manager**
- Datahub user should have **READ ONLY** access to source databases
- TLS required for all cross-system connections

---

## Files Created

```
migrations/
└── 001_CreateWekezaDataHub.sql              PostgreSQL DDL (schemas, tables, views, functions)

src/WekezaEnterpriseBrain.Infrastructure/
└── DataHub/
    ├── WekezaDataHubDbContext.cs             EF Core DbContext for the datahub
    ├── Models/
    │   ├── DimCustomer.cs                   Global Customer Dimension (GCID)
    │   ├── DimAccount.cs                    Account Dimension
    │   └── FactModels.cs                    All fact tables + CustomerFeatures
    └── Etl/
        ├── IEtlService.cs                   ETL interfaces
        ├── DataHubOrchestrator.cs           Orchestrates all ETL services
        ├── WekezaBankEtlService.cs          Python risk_management DB → datahub
        ├── WekezaOpenBankingEtlService.cs   Node.js wekeza_banking DB → datahub
        └── WekezaCrmEtlService.cs           .NET SQL Server WekezaCRM → datahub

src/WekezaEnterpriseBrain.Api/
├── Controllers/DataHubController.cs         REST API for datahub management
└── appsettings.json                         Updated with connection strings
```
