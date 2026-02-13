Below is a **detailed Product Requirements Document (PRD)** for the **Wekeza Realtime Decision Engine (Enterprise Brain)**.

This version is written from a **product perspective** (what will be built, how users interact, features, releases) and includes explicit instructions for **GitHub Copilot** to analyze repositories starting with **“Wekeza”** to understand all data-generating systems.

---

# Product Requirements Document (PRD)

## Wekeza Realtime Decision Engine (Enterprise Brain)

---

# 1. Product Overview

## Product Name

Wekeza Enterprise Brain

## Product Type

Realtime Decision Intelligence Platform

## Purpose

The Enterprise Brain is a centralized platform that:

* Integrates data from all Wekeza systems
* Creates a unified Customer 360 view
* Establishes a Global Customer Identity (GCID)
* Processes events in real time
* Provides instant enterprise decisions for channels, fraud, risk, lending, Open Banking, and AI systems

---

# 2. Product Vision

> Enable Wekeza Bank to operate as a real-time, AI-driven financial institution with one unified customer intelligence layer powering every decision.

---

# 3. Product Goals

| Goal                    | Description                            |
| ----------------------- | -------------------------------------- |
| One Bank, One Customer  | Unified identity across all systems    |
| Real-time intelligence  | Decisions in < 200ms                   |
| Cross-system visibility | Customer behavior across channels      |
| Risk reduction          | Enterprise fraud and risk awareness    |
| AI readiness            | Real-time features for AI Copilot      |
| Platform enablement     | Decision service usable by all systems |

---

# 4. Target Users

## Internal Users

* Digital Channels (Web, Mobile, USSD)
* Core Banking Systems
* Fraud Management
* Risk Management
* Credit/Lending Systems
* AI Financial Copilot
* Open Banking Platform

## Indirect Users

* Bank customers (through faster decisions)
* External partners (via Open Banking risk validation)

---

# 5. Repository Discovery Requirement (For Copilot Context)

To understand the full data landscape, GitHub Copilot must:

### 5.1 Repository Search Rule

Scan under:

**[https://github.com/eodenyire](https://github.com/eodenyire)**

Identify:

> Any repository whose name starts with **“Wekeza”**

Examples:

* Wekeza.Core.Api
* WekezaERMS
* Wekeza.Nexus
* Wekeza.Fraud
* Any future repository beginning with “Wekeza”

---

### 5.2 Additional Known Systems

Copilot should also analyze:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Core/Wekeza.Core.Api
* Channel components within Core

---

### 5.3 For Each Repository

Extract:

* Database configuration (appsettings.json)
* Entities/models
* DbContext definitions
* Tables generating operational data
* Customer identity fields
* Event publishing patterns (if any)

---

# 6. Product Scope

## 6.1 In Scope

* Data integration from all Wekeza systems
* Global Customer Identity (GCID)
* Customer 360 service
* Event ingestion and processing
* Feature Store
* Realtime Decision API
* Decision logging and monitoring

---

## 6.2 Out of Scope

* Rewriting existing core systems
* Replacing operational databases
* Customer-facing UI (initial phase)

---

# 7. Core Product Features

---

## Feature 1: Global Customer Identity (GCID)

### Description

Create a single enterprise customer identifier across all systems.

### Matching Fields

* National ID
* Phone number
* Email
* Account number
* Local Customer IDs

### Output

| GCID | Source System | LocalCustomerId |

---

## Feature 2: Data Integration Layer

### Description

Capture operational data from all repositories.

### Supported Methods

1. Change Data Capture (CDC) – preferred
2. Event streaming
3. API integration
4. Scheduled ETL (fallback)

### Data Domains

* Customers
* Accounts
* Transactions
* Payments
* Loans
* Login activity
* Device information
* Fraud signals
* Risk scores

---

## Feature 3: Event Streaming

### Event Types

* CustomerCreated
* TransactionPosted
* PaymentInitiated
* LoginEvent
* AccountUpdated

System must process **millions of events per day**.

---

## Feature 4: Customer 360 Service

Maintains:

* Profile
* Account summary
* Recent transactions
* Behavioral metrics
* Channel usage
* Risk level

---

## Feature 5: Feature Store

Provides real-time features such as:

* Transaction velocity
* Average daily spend
* Last login location
* Failed login count
* Balance trends

Used by:

* Fraud
* Risk
* AI Copilot
* Decision Engine

---

## Feature 6: Realtime Decision API

### Endpoint

POST /decision

### Example Input

```
{
  "gcid": "GCID1001",
  "eventType": "PAYMENT",
  "amount": 5000,
  "channel": "MOBILE"
}
```

### Response

```
{
  "decision": "APPROVE",
  "riskScore": 0.18,
  "reason": "Normal behavior"
}
```

### Performance Requirement

Response time: **< 200ms**

---

## Feature 7: Decision Logging

Store:

* Input data
* Features used
* Decision outcome
* Timestamp
* Source system

Used for:

* Audit
* Model training
* Compliance

---

# 8. Product Use Cases

### UC1 – Payment Authorization

Mobile App → Decision Engine → Approve/Decline

---

### UC2 – Fraud Check

Core Banking → Transaction event → Risk evaluation

---

### UC3 – Loan Pre-Approval

Lending System → Customer 360 → Instant eligibility

---

### UC4 – AI Copilot Analysis

AI Copilot → Feature Store → Financial predictions

---

### UC5 – Open Banking Risk Control

External API payment → Decision Engine → Risk validation

---

# 9. Non-Functional Requirements

| Requirement      | Target                |
| ---------------- | --------------------- |
| Decision latency | < 200ms               |
| Availability     | 99.9%                 |
| Event throughput | Millions/day          |
| Scalability      | Horizontal scaling    |
| Security         | Encryption + RBAC     |
| Auditability     | Full decision history |

---

# 10. Release Plan

## Phase 1 – Foundation

* Repository discovery (Wekeza*)
* GCID service
* Basic Customer 360

---

## Phase 2 – Data Integration

* CDC setup
* Event streaming
* Core systems integration

---

## Phase 3 – Decision Engine

* Decision API
* Feature Store
* Logging

---

## Phase 4 – Enterprise Integration

* Channels
* Fraud
* AI Copilot
* Open Banking

---

# 11. Success Metrics

* All repositories starting with “Wekeza” analyzed
* 100% customer identity mapped to GCID
* Decision latency < 200ms
* All major systems integrated
* Reduction in manual risk reviews
* Increased automated approvals

---

# 12. Risks and Mitigation

| Risk                   | Mitigation                      |
| ---------------------- | ------------------------------- |
| Unknown data sources   | Repository scanning for Wekeza* |
| Identity conflicts     | GCID matching rules             |
| Integration complexity | Phased rollout                  |
| Performance issues     | Event-driven architecture       |

---

# 13. Product Success Definition

The Enterprise Brain will be considered successful when:

* Every system under `github.com/eodenyire` starting with **Wekeza** is integrated
* A single GCID exists for each customer
* Real-time decisions are used by at least one production channel
* Customer 360 is accessible across the enterprise

---

If you continue the enterprise sequence, the next document should be:

**System Architecture – Enterprise Brain**

That’s where this becomes a full **Google/Amazon-level system design** (event flow, CDC, Kafka, data layers, decision pipeline).
