Great — this POC is **critical**.
If done well, this alone can demonstrate **FAANG-level data engineering + real-time architecture**.

This POC will prove:

> “Wekeza can aggregate data from multiple independent banking systems and produce a unified real-time Customer 360 for the Enterprise Brain.”

---

# Wekeza Data Aggregation – Detailed POC Plan

**System:** Enterprise Brain – Data Aggregation Layer
**Version:** 1.0

**Instruction for GitHub Copilot**

> Search all repositories under GitHub user **eodenyire**.
> Identify any repository whose name starts with:
>
> * `Wekeza`
> * `Core`
> * `Nexus`
> * `Fraud`
> * `Risk`
>
> For each repository:
>
> * Locate database configuration files (appsettings.json, environment variables, connection strings)
> * Identify database type and schema
> * Identify tables for:
>
>   * Customers
>   * Accounts
>   * Transactions
>   * Loans
>   * Events / Logs
>
> These sources must be integrated into the POC ingestion pipeline.

---

# 1. POC Objective

Demonstrate end-to-end capability to:

1. Extract data from **multiple Wekeza systems**
2. Consolidate into **one unified database**
3. Create **Global Customer Identity**
4. Produce **Customer 360**
5. Enable **real-time updates**
6. Expose API for Enterprise Brain

---

# 2. POC Scope (Minimal but Powerful)

### Systems Included (minimum)

1. One Core Banking system
   (e.g., ComprehensiveWekezaApi)

2. One Channel
   (login/session data)

3. One Fraud or Risk system

This proves **cross-system aggregation**.

---

# 3. POC Architecture

```
Source DBs
   ↓
Data Extractor Services
   ↓
Event Stream (Kafka or lightweight queue)
   ↓
Aggregation Service
   ↓
Enterprise Brain DB (Postgres)
   ↓
Customer 360 API
```

If Kafka is heavy for POC:

* Use simple Event Bus (RabbitMQ or even polling)

---

# 4. POC Data Model (Target Database)

Database: **WekezaEnterpriseBrain**

## 4.1 Global Customer

Customer_360

Fields:

* GlobalCustomerId (UUID)
* SourceCustomerId
* SourceSystem
* NationalId
* Phone
* Email
* FullName
* CreatedDate

---

## 4.2 Account Summary

Account_360

* AccountId
* GlobalCustomerId
* AccountType
* Balance
* Currency
* SourceSystem

---

## 4.3 Transaction Summary

Transaction_360

* TransactionId
* GlobalCustomerId
* AccountId
* Amount
* Type
* Channel
* Timestamp
* SourceSystem

---

## 4.4 Risk Profile

Risk_Profile

* GlobalCustomerId
* FraudScore
* RiskLevel
* LastUpdated

---

# 5. Identity Resolution Logic (POC)

Match customers using:

Priority order:

1. National ID
2. Phone number
3. Email

If match found → use existing GlobalCustomerId
If not → create new UUID

---

# 6. Data Ingestion Approach

## Option A (Simplest POC)

Polling Service:

Every 30 seconds:

* Query new records
* Push to aggregation

Good enough for demo.

---

## Option B (Better)

Add event publishing inside:

Core system:

```
CustomerCreated
TransactionPosted
```

Publish to queue.

---

# 7. POC Components

### 7.1 Source Connectors

Microservices:

CoreConnector
ChannelConnector
FraudConnector

Responsibilities:

* Read new data
* Transform to common format
* Send to Aggregation Service

---

### 7.2 Aggregation Service

Responsibilities:

* Identity matching
* Create/update Customer_360
* Update Account_360
* Insert Transaction_360
* Update Risk_Profile

---

### 7.3 Enterprise Brain API

Endpoints:

GET /customer/{globalId}
GET /customer/{globalId}/accounts
GET /customer/{globalId}/transactions
GET /customer/{globalId}/risk

---

# 8. Sample End-to-End Scenario

1. Customer created in Core
2. Connector reads record
3. Aggregation creates GlobalCustomerId
4. Transaction occurs
5. Connector sends transaction
6. Transaction_360 updated
7. Fraud system sends alert
8. Risk_Profile updated
9. Enterprise Brain API shows unified profile

---

# 9. POC Tech Stack

Backend:

* .NET (align with Wekeza stack)

Database:

* PostgreSQL

Messaging:

* RabbitMQ or Kafka (optional)

API:

* ASP.NET Web API

Container:

* Docker (optional but impressive)

---

# 10. Success Criteria

POC is successful if:

* Data from **2+ systems** appears in one DB
* Same customer from multiple systems merges into one GlobalCustomerId
* Real-time or near-real-time updates (<1 minute)
* Customer 360 API returns unified data

---

# 11. Demo Script (Very Important)

Scenario:

1. Create customer in Core
2. Login via Channel
3. Perform transaction
4. Trigger fraud alert

Then call:

GET /customer/{id}

Show:

* Account
* Transactions
* Last login
* Risk score

This demo is **very powerful**.

---

# 12. Observability (POC Level)

Log:

* Records processed
* Identity matches
* Errors

Metrics:

* Processing latency
* Records per minute

---

# 13. Timeline (Realistic)

Week 1:

* Repo discovery (Copilot)
* Data model
* Enterprise DB setup

Week 2:

* Core connector
* Aggregation service

Week 3:

* Channel + Fraud connectors
* API

Week 4:

* Demo + documentation

---

# 14. Future After POC

Next upgrades:

* Kafka event streaming
* CDC (Debezium)
* Feature Store
* Graph relationships
* Real-Time Decision Engine integration

---

# 15. Why This POC Is World-Class

Because it demonstrates:

* Data engineering
* Microservices
* Identity resolution
* Real-time processing
* Enterprise architecture thinking

This is exactly the kind of system:

Google / Amazon / Microsoft engineers design.

---

If you want the **most powerful next step**, we should build:

**Customer 360 – Full Production Data Model (table-level design + indexes)**

This becomes the **heart of the Enterprise Brain**.
