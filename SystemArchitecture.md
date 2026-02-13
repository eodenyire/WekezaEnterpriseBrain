Below is the **System Architecture** for the **Wekeza Realtime Decision Engine (Enterprise Brain)**.
This is written at a **solution architect / FAANG-level design** and explicitly includes the **GitHub Copilot repository discovery requirement** for all repos starting with **“Wekeza”**.

---

# System Architecture

## Wekeza Realtime Decision Engine (Enterprise Brain)

---

# 1. Architecture Overview

The Enterprise Brain is designed as a **real-time, event-driven intelligence platform** that integrates data from all Wekeza systems and provides instant enterprise decisions.

### Core Principle

> One Bank
> One Customer
> One Intelligence Layer
> Zero Tight Coupling

Existing systems remain independent. The Brain integrates via **events, CDC, and APIs**.

---

# 2. Repository Discovery Requirement (Architecture Context)

To understand all data sources, GitHub Copilot must:

### 2.1 Repository Scan Rule

Search under:

**[https://github.com/eodenyire](https://github.com/eodenyire)**

Identify:

> Any repository whose name starts with **“Wekeza”**

Examples:

* Wekeza.Core.Api
* WekezaERMS
* Wekeza.Nexus
* Wekeza.Fraud
* Any future repo starting with “Wekeza”

---

### 2.2 Additional Known Systems

Also analyze:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Channel components inside Core

---

### 2.3 For Each Repository

Identify:

* Database connection (appsettings.json)
* Data models/entities
* Customer identity fields
* Tables generating operational data
* Event publishing logic (if present)

This discovery defines **Enterprise Brain data sources**.

---

# 3. High-Level Architecture

```
Wekeza Systems (All Repos: Wekeza*)
   |
   |  CDC / Events / APIs
   v
Data Ingestion Layer
   |
   v
Event Streaming Platform (Kafka)
   |
   v
Processing & Identity Layer
   |
   v
Customer 360 + Feature Store
   |
   v
Realtime Decision Engine
   |
   v
Consumers (Channels, Fraud, Risk, AI, Open Banking)
```

---

# 4. Architecture Layers

---

# 4.1 Source Systems Layer

Includes all systems discovered from:

* Repositories starting with **Wekeza**
* Known core variants
* Channels
* Nexus
* ERMS
* Fraud
* Risk

### Data Generated

* Customers
* Accounts
* Transactions
* Payments
* Loans
* Login activity
* Device information
* Risk events

---

# 4.2 Data Ingestion Layer

Purpose: Capture changes from operational systems.

### Integration Methods (Priority Order)

1. **Change Data Capture (CDC)**

   * PostgreSQL logical replication
   * Debezium

2. **Event Publishing (Preferred if code control exists)**
   Example events:

   * CustomerCreated
   * TransactionPosted
   * PaymentInitiated

3. **API Polling**

4. **Scheduled ETL (fallback)**

---

# 4.3 Event Streaming Layer

Technology: **Kafka (recommended)**

### Topics

* customer.events
* account.events
* transaction.events
* payment.events
* login.events
* fraud.events
* risk.events

### Requirements

* High throughput
* Fault tolerance
* Horizontal scaling
* Event retention

---

# 4.4 Identity Resolution Layer

## Global Customer Identity (GCID)

Purpose: One customer across all systems.

### Matching Fields

* National ID
* Phone number
* Email
* Account number
* Local Customer IDs

### Mapping Table

| GCID | SourceSystem | LocalCustomerId |

### Components

* Identity Matching Service
* Deduplication logic
* Merge rules
* Conflict resolution

---

# 4.5 Stream Processing Layer

Processes incoming events to:

* Enrich data
* Update customer state
* Calculate behavioral metrics
* Detect anomalies

Technology options:

* Kafka Streams
* .NET background workers
* Spark Streaming (future)

---

# 4.6 Customer 360 Data Store

Operational real-time store containing:

* Customer profile
* Linked accounts
* Recent transactions
* Channel usage
* Risk indicators
* Behavioral summary

Recommended database:

* PostgreSQL (initial)
* Cassandra (future scale)

---

# 4.7 Feature Store

Provides real-time features for decisioning.

### Examples

* Transaction velocity (last 5 min)
* Avg daily spend
* Balance trend
* Failed login count
* Location change frequency
* Device risk score

Consumers:

* Fraud
* Risk
* AI Copilot
* Decision Engine

---

# 4.8 Realtime Decision Engine

Central decision service.

### API

POST /decision

### Supported Decisions

* Payment approval/block
* Fraud risk scoring
* Credit eligibility
* Limit adjustment
* Offer recommendation

### Performance Target

**< 200ms response time**

---

# 4.9 Decision Rules & Scoring

Decision Engine uses:

1. Rule Engine
2. Feature evaluation
3. Risk scoring models
4. Policy configuration

Future:

* ML model integration
* Dynamic decision policies

---

# 4.10 Decision Logging & Audit

Store:

* Request payload
* Features used
* Decision result
* Timestamp
* Source system

Used for:

* Compliance
* Model training
* Investigation

---

# 5. Consumer Systems

The Enterprise Brain will serve:

| System          | Usage                           |
| --------------- | ------------------------------- |
| Mobile/Web/USSD | Payment decisions               |
| Core Banking    | Transaction validation          |
| Fraud System    | Risk signals                    |
| Risk Management | Customer risk level             |
| AI Copilot      | Behavioral insights             |
| Open Banking    | External transaction validation |

---

# 6. Technology Stack (Recommended)

| Layer                 | Technology           |
| --------------------- | -------------------- |
| CDC                   | Debezium             |
| Streaming             | Kafka                |
| Processing            | .NET Workers         |
| Identity Service      | .NET                 |
| Operational Store     | PostgreSQL           |
| Cache / Feature Speed | Redis                |
| Decision API          | .NET                 |
| Monitoring            | Prometheus + Grafana |
| Containerization      | Docker + Kubernetes  |

---

# 7. Data Flow Example

### Payment Scenario

1. Customer initiates payment via Mobile
2. Core Banking writes transaction
3. CDC publishes event to Kafka
4. Stream processor updates features
5. Channel calls Decision API
6. Decision Engine returns APPROVE/DECLINE
7. Decision logged

Total latency target: **< 200ms**

---

# 8. Scalability Strategy

* Stateless services
* Horizontal scaling
* Kafka partitioning
* Redis caching for hot features
* Kubernetes autoscaling

---

# 9. Security Architecture

* TLS encryption
* OAuth2 / service authentication
* Role-based access
* Data masking for sensitive fields
* Full audit trail

---

# 10. Availability & Resilience

* Kafka replication
* Database replication
* Circuit breakers
* Retry mechanisms
* Graceful degradation (default approve/review policies)

---

# 11. Deployment Architecture

Environments:

* Dev
* QA
* UAT
* Production

Cloud-ready, containerized deployment.

---

# 12. Architecture Principles

* Event-driven
* Loosely coupled
* System independence
* Real-time first
* API-first
* Cloud-native ready

---

# 13. Success Criteria

* All repositories starting with **Wekeza** analyzed
* All data-generating systems integrated
* GCID operational
* Customer 360 available
* Decision latency < 200ms
* Used by at least one production channel

---

# 14. Strategic Impact

This architecture demonstrates:

* Enterprise data platform design
* Event-driven microservices
* Real-time decision systems
* Identity resolution at scale

Equivalent to architectures used by:

* Amazon Risk Systems
* Stripe Radar
* Large digital banks

---

If you continue the sequence, the next document should be:

**SDS – Software Design Specification (Enterprise Brain)**

That’s where we define:

* Microservices
* Tables
* APIs
* Event schemas
* GCID algorithms

That will make this a **complete world-class system design**.
