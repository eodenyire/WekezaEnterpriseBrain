Below is a **detailed Business Requirements Document (BRD)** for the **Wekeza Realtime Decision Engine (Enterprise Brain)**.

It explicitly includes instructions for **GitHub Copilot** to search for **any repository starting with “Wekeza”** so it understands the structure of all data-generating systems, as you requested.

You can paste this into your project docs or Copilot context.

---

# Business Requirements Document (BRD)

## Wekeza Realtime Decision Engine (Enterprise Brain)

---

# 1. Document Information

| Item         | Description                                                                              |
| ------------ | ---------------------------------------------------------------------------------------- |
| Project Name | Wekeza Enterprise Brain                                                                  |
| System       | Realtime Decision Engine                                                                 |
| Organization | Wekeza Bank                                                                              |
| Version      | 1.0                                                                                      |
| Purpose      | Define business requirements for unified real-time decisioning across all Wekeza systems |

---

# 2. Executive Summary

Wekeza Bank operates multiple independent systems that generate customer, transaction, and behavioral data. These systems exist across several repositories and databases, resulting in fragmented customer views and siloed decision-making.

The Enterprise Brain will:

* Integrate data from all Wekeza systems
* Establish **One Bank, One Customer**
* Enable real-time decisioning
* Support Fraud, Risk, AI Copilot, Open Banking, and Channels
* Provide a centralized intelligence layer for the bank

---

# 3. Business Objectives

1. Create a unified customer view across all systems
2. Enable real-time enterprise decisioning (< 200ms)
3. Reduce fraud and operational risk
4. Improve customer experience through instant approvals
5. Support AI-driven personalization and analytics
6. Enable cross-system intelligence for all banking operations

---

# 4. Scope

## 4.1 In Scope

* All systems under the Wekeza ecosystem
* All data-generating services
* Customer, account, and transaction data
* Real-time event processing
* Global Customer Identity (GCID)
* Decision APIs

---

## 4.2 Out of Scope

* Replacement of existing core systems
* Modification of core business logic inside source systems
* Decommissioning existing databases

The Enterprise Brain will **integrate**, not replace.

---

# 5. Repository Discovery Requirement (Critical for GitHub Copilot)

### 5.1 Repository Identification

GitHub Copilot must:

Search under:

**[https://github.com/eodenyire](https://github.com/eodenyire)**

And identify:

> **Any repository whose name starts with the word “Wekeza”**

Examples:

* Wekeza.Core.Api
* WekezaERMS
* Wekeza.Fraud
* Wekeza.Nexus
* Any future repositories beginning with “Wekeza”

---

### 5.2 Additional Known Systems

Copilot should also analyze:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Channels within Core
* Any service that writes operational banking data

---

# 6. Data Source Discovery Requirements

For each repository identified, Copilot must determine:

### 6.1 Database Configuration

Search for:

* appsettings.json
* Program.cs
* Startup.cs
* Environment variables

Capture:

* Database type
* Database name
* Connection configuration
* Schemas (if available)

---

### 6.2 Data Domains Generated

Identify tables/entities related to:

* Customers
* Accounts
* Transactions
* Payments
* Loans
* Authentication / Login
* Channel activity
* Devices
* Fraud events
* Risk data

---

### 6.3 Data Write Patterns

Determine:

* Real-time transactional writes
* Batch processes
* Background workers
* Event publishing (Kafka, RabbitMQ, etc.)

---

# 7. Business Requirement: One Bank, One Customer

The system must create a **Global Customer Identity (GCID)**.

### Identity Matching Fields

* National ID
* Phone Number
* Email
* Account Number
* Existing Customer IDs

### Output

All systems must map to a single GCID.

---

# 8. Functional Requirements

## 8.1 Data Integration

The Enterprise Brain shall:

* Capture data changes from all Wekeza systems
* Support integration methods:

  1. CDC (preferred)
  2. Event streaming
  3. API extraction
  4. Scheduled ETL (fallback)

---

## 8.2 Customer 360

The system shall maintain:

* Customer profile
* Account summaries
* Transaction behavior
* Channel activity
* Risk indicators
* Fraud history

---

## 8.3 Realtime Decision API

The system shall expose:

POST /decision

Supported decisions:

* Transaction approval/block
* Fraud risk scoring
* Credit eligibility
* Limit adjustments
* Offer recommendations

Response time target:

**< 200 milliseconds**

---

## 8.4 Event Processing

The system shall process events including:

* Transaction posted
* Payment initiated
* Login activity
* Account changes
* Customer updates

---

# 9. Business Use Cases

### Use Case 1: Payment Fraud Check

Channel → Decision Engine → Approve/Decline

---

### Use Case 2: Loan Pre-Qualification

Loan system → Customer 360 → Instant eligibility

---

### Use Case 3: AI Copilot Insight

AI Copilot → Behavioral features → Financial prediction

---

### Use Case 4: Open Banking Risk Validation

External API payment → Decision Engine → Risk check

---

# 10. Non-Functional Requirements

| Requirement      | Target                         |
| ---------------- | ------------------------------ |
| Decision latency | < 200ms                        |
| Availability     | 99.9%                          |
| Scalability      | Millions of events/day         |
| Security         | Encryption + role-based access |
| Auditability     | Full decision logging          |

---

# 11. Stakeholders

* Digital Banking Team
* Risk Management
* Fraud Operations
* Credit Team
* AI & Analytics Team
* Open Banking Team
* Enterprise Architecture
* DevOps

---

# 12. Success Metrics

* 100% of Wekeza systems integrated
* All repositories starting with “Wekeza” analyzed
* Single GCID for all customers
* Real-time decisions operational
* Reduction in fraud losses
* Increased automation rate

---

# 13. Key Assumptions

* All operational systems are hosted under the GitHub user:

  **github.com/eodenyire**

* All core banking and related systems generating data will be discovered via repository analysis.

---

# 14. Risks and Mitigation

| Risk                   | Mitigation                        |
| ---------------------- | --------------------------------- |
| Unknown data sources   | Repository scanning for “Wekeza*” |
| Identity duplication   | GCID matching rules               |
| Integration complexity | Phased rollout                    |
| Performance issues     | Event-driven architecture         |

---

# 15. Approval Criteria

The project will be considered successful when:

* All repositories starting with “Wekeza” have been analyzed
* All data-generating systems are integrated
* GCID is operational
* Decision API is live and used by at least one channel

---

If you continue this path, the next strongest document (enterprise sequence) would be:

**PRD – Enterprise Brain**

After that:

* System Architecture
* SDS
* POC
* Full Implementation Plan

This system, together with Open Banking and AI Copilot, forms a **FAANG-level architecture story**.
