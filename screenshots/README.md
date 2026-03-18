# Wekeza Enterprise Brain – Screenshots

This folder contains screenshots of the Wekeza Enterprise Brain integration dashboard,
capturing all modules, integrations, data flows, and system views.

---

## Screenshots

| File | Section |
|------|---------|
| `01_dashboard_header_kpis.png` | Header, KPI cards, system status indicators |
| `02_full_dashboard.png` | **Full dashboard** (all sections, full-page) |
| `03_integration_architecture.png` | Integration architecture flow diagram |
| `04_connected_systems.png` | Connected systems grid (WekezaCore, WekezaBank, CRM, OpenBanking, DFS…) |
| `05_core_banking_modules.png` | Core banking module cards – CIF, Accounts, Loans, Teller, Branch, Cards, GL, Payments, Trade Finance, Treasury |
| `06_etl_pipeline_status.png` | ETL pipeline status table |
| `07_risk_fraud_management.png` | Risk management & fraud detection panel |
| `08_customer_360.png` | Customer 360 view (GCID-based unified identity) |
| `09_analytics_charts.png` | Analytics dashboards – transaction volumes, risk distribution, AI decisions |
| `10_datahub_schema_api_endpoints.png` | Datahub schema documentation & Enterprise Brain API endpoints |

---

## Systems Shown

### Live ETL Integrations (4 Active Pipelines)

| System | Source | Database | Status |
|--------|--------|----------|--------|
| **WekezaCore** | ComprehensiveWekezaApi (.NET 8/EF Core) | `wekeza_banking_comprehensive` (PostgreSQL) | ✅ Live |
| **WekezaBank** | Python / SQLAlchemy | `risk_management` (PostgreSQL) | ✅ Live |
| **WekezaCRM** | .NET 8 / EF Core | `WekezaCRM` (SQL Server) | ✅ Live |
| **WekezaOpenBanking** | Node.js / pg | `wekeza_banking` (PostgreSQL) | ✅ Live |

### Other Registered Systems (22 total)
- WekezaGlobal – Cross-border financial rail
- WekezaDFS – Digital Financial Services (Mobile Money)
- Mobile Banking – iOS / Android
- Web Banking Portal
- USSD Banking
- Fraud Detection System
- ERMS (Enterprise Risk Management System)
- WekezaNextGenPersonalBanking

---

## Core Banking Modules (WekezaCore – 18 Modules)

1. CIF – Customer Information File
2. Account Management
3. Transaction Processing
4. Loan Management
5. Fixed Deposits & Investments
6. Teller Operations
7. Branch Operations
8. Cards, ATM & POS
9. General Ledger (GL)
10. Payments & Transfers (RTGS, EFT, PesaLink, Mobile)
11. Products
12. Trade Finance (Letters of Credit)
13. Treasury & FX
14. Staff Management

---

## Wekeza Datahub Schema

Database: `wekeza_datahub` (PostgreSQL)

| Schema | Purpose |
|--------|---------|
| `staging` | Ephemeral landing zone for raw ETL data |
| `warehouse` | dim_customers, dim_accounts, dim_dates, fact_transactions, fact_payments, fact_risk_assessments, fact_interactions, fact_cases, customer_features |
| `analytics` | Materialized views: customer_360, daily_transaction_summary, risk_dashboard |
| `audit` | ETL sync logs, data quality issues – 7-year retention |

---

## Fraud & Risk Management Integrations

- **Tazama** – Real-time transaction monitoring & typology detection (FATF-aligned)
- **Ballerine** – KYC/AML platform, customer due diligence
- **CISO Assistant** – Security GRC, ISO 27001 / SOC 2 compliance
- **Isolation Forest ML** – Unsupervised anomaly detection (polled every 30s)
