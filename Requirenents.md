Understood. You want this captured as a **clear requirement statement** so that when you paste the document into **GitHub Copilot**, it will:

* Recognize that multiple Wekeza repos exist
* Search them under your username
* Identify where each system writes data
* Support the **Enterprise Brain integration goal**

Below is a **precise requirement section** you can paste into your architecture/implementation documents.

---

## Repository Discovery & Data Source Integration Requirement

**(For Enterprise Brain – Realtime Decision Engine)**

### Context

Wekeza Bank currently operates multiple independent backend systems and services, each developed as separate repositories under the GitHub user:

**GitHub Organization/User:**
`https://github.com/eodenyire`

These systems represent different versions and components of the Wekeza Core and related banking platforms. Each system writes operational data to one or more databases.

The **Realtime Decision Engine (Enterprise Brain)** requires unified access to all operational data across these systems to enable:

* Single Customer Identity (One Bank, One Customer)
* Real-time decisioning
* Customer 360 analytics
* Cross-channel intelligence
* AI and risk scoring

---

# Requirement 1: Repository Discovery

GitHub Copilot (or any automated analysis tool) must:

1. Scan all repositories under:

   `https://github.com/eodenyire`

2. Identify repositories with names containing:

* Wekeza
* Core
* Nexus
* ERMS
* Fraud
* Risk
* Channel
* Banking

3. At minimum, the following known repositories must be analyzed:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Core/Wekeza.Core.Api
* Nexus
* WekezaERMS
* Other repositories containing Wekeza-related services

---

# Requirement 2: Data Source Identification

For each identified repository, Copilot should locate and extract:

### 2.1 Database Configuration

Search for:

* Connection strings
* Database providers
* Environment configuration

Common locations:

* `appsettings.json`
* `appsettings.Development.json`
* `Program.cs`
* `Startup.cs`
* Environment variables

Capture:

| Field         | Description                  |
| ------------- | ---------------------------- |
| Database type | PostgreSQL, SQL Server, etc. |
| Database name | Target database              |
| Schema        | If specified                 |
| Host/source   | If available                 |

---

### 2.2 Data Write Locations

Identify:

* Entities / Models
* DbContext definitions
* Repository patterns
* Tables receiving transactional data

Priority data domains:

* Customers
* Accounts
* Transactions
* Payments
* Loans
* Channels activity
* Authentication / login events

---

# Requirement 3: Event Source Identification

Copilot should check whether systems:

* Publish domain events
* Use message brokers
* Call external APIs
* Expose webhooks

Search for:

* Kafka
* RabbitMQ
* Azure Service Bus
* Event publishers
* Background workers

This helps determine integration method for the Enterprise Brain.

---

# Requirement 4: Identity Fields Discovery

For each system, identify customer identity fields such as:

* CustomerId
* NationalId
* PhoneNumber
* Email
* AccountNumber
* CIF

These fields will be used for **Global Customer Identity (GCID)** mapping.

---

# Requirement 5: Integration Output

For each repository, produce a summary:

```
Repository: EnhancedWekezaApi
Database: PostgreSQL
Database Name: wekeza_enhanced
Key Tables:
- Customers
- Accounts
- Transactions

Identity Fields:
- CustomerId
- PhoneNumber
Integration Method:
- CDC via PostgreSQL logical replication
```

---

# Requirement 6: Enterprise Brain Objective

All discovered data sources must be integrated into a unified architecture that supports:

### One Bank, One Identity

Create:

**GlobalCustomerId (GCID)**

Mapping structure:

| GCID | SourceSystem | LocalCustomerId |

---

### Real-Time Integration Strategy

Preferred order:

1. Change Data Capture (CDC)
2. Domain Event Streaming
3. API-based data extraction
4. Scheduled ETL (fallback)

Target streaming platform:

* Kafka (preferred) or equivalent

---

# Requirement 7: Data Domains for Enterprise Brain

The unified platform must support:

* Customer Master Data
* Account Balances
* Transactions (real-time)
* Payments
* Channel Activity
* Fraud Signals
* Risk Scores
* Login / Device activity

---

# Requirement 8: Final Goal

Enable the Realtime Decision Engine to:

* Receive real-time events from all Wekeza systems
* Maintain Customer 360 profiles
* Provide instant decisions for:

  * Payments
  * Fraud
  * Lending
  * Risk
  * Personalization
  * AI Financial Copilot

---

# Requirement 9: Architectural Principle

**Do NOT merge operational databases.**

Instead:

* Discover all data sources
* Integrate via events or CDC
* Build a centralized intelligence layer

This ensures:

* Loose coupling
* Scalability
* System independence
* Enterprise-grade architecture

---

# Requirement 10: Scope Statement

The Enterprise Brain must integrate **all Wekeza operational systems** found under:

`github.com/eodenyire`

to achieve:

> One Bank
> One Customer
> One Intelligence Layer

---

If you want to make Copilot even more powerful, I can next give you:

* A **Copilot Prompt Template** (how to instruct it to analyze each repo automatically)
  or
* The next document: **Concept Note – Realtime Decision Engine (Enterprise Brain)**

That would complete System #3 at the same level as your Open Banking work.
