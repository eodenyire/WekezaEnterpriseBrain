Excellent choice.
A **Data Aggregation Strategy** is actually one of the most powerful pieces for your **Enterprise Brain / Real-Time Decision Engine**, because globally, banks and big tech are moving toward:

> **One Bank → One Customer → One Data View → Real-Time Intelligence**

This document will guide **GitHub Copilot** to discover all Wekeza data sources and design a unified aggregation layer.

---

# Wekeza Data Aggregation Strategy

**Version:** 1.0
**Applies to:**

* Real-Time Decision Engine (Enterprise Brain)
* AI Financial Copilot
* Open Banking Platform
* Fraud & Risk Systems
* Analytics & Personalization

**Instruction for GitHub Copilot**

> Search all repositories under the GitHub user **`eodenyire`** and identify any repository whose name starts with:
>
> * `Wekeza`
> * `Core`
> * `Nexus`
> * `Channels`
> * `Fraud`
> * `Risk`
>
> For each repository:
>
> * Identify database connection strings
> * Identify database type (Postgres, SQL Server, etc.)
> * Identify tables where transactions, customers, accounts, or events are written
> * Identify APIs that produce or consume data
>
> These sources must be integrated into the **Enterprise Data Aggregation Layer**.

---

# 1. Objective

Create a **Unified Data Layer** that provides:

* One Customer Identity
* One Financial Profile
* Real-time event streaming
* Historical data consolidation
* Feature-ready datasets for AI

Goal:

> Transform multiple Wekeza systems into a **single intelligent data ecosystem**.

---

# 2. Current Data Landscape (Sources)

GitHub Copilot must locate data from:

## 2.1 Core Banking Systems

Repositories:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Core/Wekeza.Core.Api

Data:

* Customers
* Accounts
* Transactions
* Loans
* Balances

---

## 2.2 Channels

Inside `Core`:

* Mobile Banking
* Web Banking
* USSD
* ATM / Agency (if any)

Data:

* Login activity
* Device info
* Session behavior
* Transaction initiation

---

## 2.3 Risk & Fraud

Repositories:

* Wekeza-Fraud
* Wekeza-ERMS
* Risk modules

Data:

* Fraud alerts
* Risk scores
* Suspicious activity
* Decision outcomes

---

## 2.4 Open Banking / Nexus

Repository:

* Nexus

Data:

* API usage
* Third-party access
* Consent records
* External transactions

---

## 2.5 Other Systems (Search Automatically)

Copilot should scan for:

* BI databases
* Reporting DBs
* Audit logs
* Event tables

---

# 3. Target Architecture

## 3.1 Data Aggregation Layers

### Layer 1 – Source Systems

All Wekeza databases

↓

### Layer 2 – Ingestion Layer

Methods:

1. CDC (Change Data Capture)

   * Debezium
   * Logical replication

2. API Pull (if CDC not possible)

3. Event Publishing (recommended)

   * Core systems publish events

---

### Layer 3 – Event Streaming

Kafka Topics:

* customer.created
* account.updated
* transaction.posted
* login.performed
* fraud.alert
* loan.approved

This enables:

* Real-time decisioning
* AI Copilot
* Fraud detection

---

### Layer 4 – Unified Operational Store (ODS)

Database: PostgreSQL / Distributed SQL

Tables:

Customer_360
Account_360
Transaction_360
Risk_Profile
Behavior_Profile

Purpose:

* Real-time querying by Enterprise Brain

---

### Layer 5 – Data Lakehouse

Storage:

* S3 / Data Lake equivalent

Zones:

Raw
Clean
Curated
Feature Store

Used for:

* ML training
* BI
* Advanced analytics

---

# 4. One Bank, One Identity Strategy

## 4.1 Master Customer Index (MCI)

Problem:
Same customer may exist in multiple systems.

Solution:

Identity Matching Keys:

* National ID
* Phone
* Email
* Core Customer ID
* Device fingerprint

Create:

GlobalCustomerId (UUID)

Mapping Table:

CustomerIdentityMap

---

## 4.2 Golden Record

Customer_360 contains:

* Demographics
* Accounts
* Total balance
* Risk score
* Channel behavior
* API usage

---

# 5. Real-Time vs Batch Strategy

| Data Type       | Method            |
| --------------- | ----------------- |
| Transactions    | Real-time (Kafka) |
| Fraud alerts    | Real-time         |
| Logins          | Real-time         |
| Balances        | Near real-time    |
| Historical data | Batch (nightly)   |

---

# 6. Data Quality Framework

Checks:

* Duplicate customers
* Missing IDs
* Negative balances
* Invalid currency
* Timestamp consistency

Data Quality Service:

* Validation rules
* Reconciliation reports

---

# 7. Metadata & Data Catalog

Create:

Wekeza Data Catalog

Includes:

* Source system
* Table description
* Data owner
* Sensitivity level
* PII classification

---

# 8. Security & Privacy

Sensitive data:

* National ID
* Phone
* Email
* Account numbers

Controls:

* Encryption at rest
* Masking in analytics
* Role-based access
* Audit logs

Compliance ready for:

* GDPR-style privacy
* Data minimization

---

# 9. Feature Store (for AI)

Features:

* Monthly spend
* Salary detection
* Transaction frequency
* Fraud risk score
* Channel usage score

Used by:

* AI Copilot
* Credit models
* Decision Engine

---

# 10. Data Access Patterns

### Real-Time (Enterprise Brain)

API:
GET /customer/{GlobalCustomerId}/profile

Latency target: < 50ms

---

### Analytics

SQL / BI tools

---

### Open Banking

Partner access via APIs

---

# 11. Implementation Phases

## Phase 1 – Discovery (Copilot Task)

For each repo starting with **Wekeza**:

* Identify DB
* Identify connection config
* Identify key tables:

  * Customer
  * Account
  * Transaction
  * Loan
  * Audit

---

## Phase 2 – Historical Consolidation

* Extract all historical data
* Load into Data Lake
* Create GlobalCustomerId mapping

---

## Phase 3 – Real-Time Integration

Add event publishing to:

Core Banking
Channels
Fraud
Risk

---

## Phase 4 – Build ODS

Create:

Customer_360
Transaction_360
Behavior_360

---

## Phase 5 – Enable Enterprise Brain

Enterprise Brain consumes:

* Kafka events
* Customer_360

---

# 12. Observability

Monitor:

* Event lag
* Failed ingestion
* Data mismatches
* CDC failures

---

# 13. Global-Level Impact

This architecture positions Wekeza as:

* AI-native bank
* Real-time data platform
* Comparable to:

  * Capital One
  * Nubank
  * Stripe data platform
  * Google real-time analytics

This is a **FAANG-level data engineering story**.

---

# 14. Future Enhancements

* External data aggregation (telco, credit bureau)
* Open Finance data ingestion
* Real-time customer digital twin
* Graph database for relationships
* Customer behavior timeline

---

If you want the **next strongest step** for the Enterprise Brain, we should build:

**Customer 360 Data Model (full table design)**

That becomes the **heart of AI, Fraud, Risk, Copilot, and Decision Engine**.
