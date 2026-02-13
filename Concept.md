You’re now moving into the **most important system architecturally** — this is the one that will truly look like a **Google/Amazon-level system**.

However, before we go into the design, I need to clarify one important constraint:

I **cannot directly browse your private GitHub repositories** unless you explicitly provide:

* Public repo confirmation, or
* The database connection patterns / schemas, or
* Key architecture summaries.

Also, per the system rules, when external verification is required, I would need web access — but since you already know your repos and the goal is **enterprise architecture**, the correct approach is:

> Instead of inspecting each repo individually, we design a **Bank-Wide Data Integration Architecture** that works **regardless of how many core systems exist**.

This is actually the **correct enterprise pattern**.

And what you’re describing:

> “One bank, one identity, one data brain”

That is exactly what the **Realtime Decision Engine (Enterprise Brain)** should solve.

---

# Realtime Decision Engine (Enterprise Brain)

## Full Enterprise Architecture for Wekeza

---

# 1. Vision

Wekeza currently has:

Multiple core systems:

* ComprehensiveWekezaApi
* DatabaseWekezaApi
* EnhancedWekezaApi
* MinimalWekezaApi
* Core/Wekeza.Core.Api
* Channels (Web, Mobile, USSD)
* Nexus
* ERMS
* Fraud
* Risk
* Open Banking
* AI Copilot

Each writes to **different databases**.

### Problem

* Customer data is fragmented
* Multiple identities per customer
* No real-time intelligence
* Decisions happen in silos

---

## Target Vision

**Enterprise Brain**

One unified layer that provides:

* Single Customer Identity
* Real-time customer state
* Instant decisioning
* Cross-system intelligence

---

# 2. Key Principle

### Do NOT merge all systems into one database directly

That approach:

* Breaks systems
* Creates coupling
* Not scalable

Instead:

**Build a Unified Data Platform + Event Layer**

This is how Google/Amazon do it.

---

# 3. High-Level Architecture

```
All Systems (Core, Channels, Fraud, Nexus, ERMS)
            |
            |  CDC / Events / APIs
            v
      Event Streaming Layer (Kafka)
            |
            v
     Unified Customer Data Platform
            |
   ------------------------------
   | Feature Store             |
   | Customer 360              |
   | Identity Resolution       |
   ------------------------------
            |
            v
     Realtime Decision Engine
            |
            v
   Fraud / Risk / Copilot / Channels
```

---

# 4. Step 1 — System Data Discovery (What you should do)

For each repo/system, document:

### For each system:

| Field           | Example                           |
| --------------- | --------------------------------- |
| Repo            | EnhancedWekezaApi                 |
| Database        | PostgreSQL                        |
| Key Tables      | Customers, Accounts, Transactions |
| Identity Field  | CustomerId / NationalId / Phone   |
| Write Frequency | Real-time                         |

Create a master inventory:

```
SystemDataInventory.xlsx
```

This is what large banks do first.

---

# 5. Step 2 — One Bank, One Identity

This is critical.

### Identity Sources

Across systems you may have:

* CustomerId
* PhoneNumber
* NationalID
* Email
* AccountNumber

---

## Identity Resolution Service

Create:

**GlobalCustomerId (GCID)**

Mapping table:

| GCID    | System | LocalCustomerId |
| ------- | ------ | --------------- |
| GCID123 | Core1  | CUST889         |
| GCID123 | Core2  | 99821           |
| GCID123 | Nexus  | NX-22           |

This becomes the **bank-wide identity**.

This alone is a **FAANG-level architecture concept**.

---

# 6. Step 3 — Data Integration Strategy

There are 3 integration methods:

---

## Option A: CDC (Best)

Use:

* Debezium
* PostgreSQL logical replication

Flow:

Database → Change Capture → Kafka

Captures:

* New transactions
* Account changes
* Customer updates

---

## Option B: Event Publishing (If you control the code)

Each system publishes:

```
CustomerCreated
TransactionPosted
AccountOpened
LoanApproved
```

---

## Option C: Scheduled Sync (fallback)

Batch ETL every few minutes.

---

# 7. Step 4 — Event Streaming Layer

Use:

* Kafka or RabbitMQ

Event topics:

* customer.events
* account.events
* transaction.events
* payment.events
* login.events

---

# 8. Step 5 — Unified Data Platform

### Storage Layers

#### Operational Store (Real-time)

PostgreSQL / Cassandra

Tables:

* Customer360
* AccountSummary
* RecentTransactions
* RiskProfile
* BehavioralProfile

---

#### Data Lake (Historical)

For analytics & ML.

---

# 9. Step 6 — Feature Store

Used by:

* Fraud
* Risk
* AI Copilot
* Decision Engine

Examples:

* Avg daily spend
* Last login location
* Failed login count
* Velocity metrics

---

# 10. Step 7 — Realtime Decision Engine

### API

POST /decision

Input:

```
{
  customerId: GCID123,
  event: "PAYMENT",
  amount: 5000
}
```

Output:

```
{
  decision: "APPROVE",
  riskScore: 0.12,
  reason: "Normal behavior"
}
```

---

## Decisions Supported

* Fraud block
* Loan approval
* Credit limit change
* Offer recommendation
* Risk escalation

---

# 11. Step 8 — Integration with Existing Systems

### Example Flow

Mobile App → Payment
→ Core Banking
→ Event Published
→ Kafka
→ Brain updates features
→ Decision Engine evaluates
→ Fraud/Risk response

Latency target: **< 200ms**

---

# 12. Technology Stack

| Layer            | Tech               |
| ---------------- | ------------------ |
| CDC              | Debezium           |
| Streaming        | Kafka              |
| Feature Store    | Redis + PostgreSQL |
| Identity Service | .NET               |
| Decision Engine  | .NET / Python      |
| Data Lake        | S3 / MinIO         |
| Monitoring       | Prometheus         |

---

# 13. Implementation Phases

### Phase 1 (Month 1–2)

* Data inventory
* Identity mapping (GCID)
* Kafka setup

---

### Phase 2 (Month 2–4)

* CDC from all core systems
* Event standardization

---

### Phase 3 (Month 4–6)

* Customer 360
* Feature Store

---

### Phase 4 (Month 6–8)

* Decision Engine APIs
* Integration with Fraud & Copilot

---

# 14. Why This Is Globally Impressive

This demonstrates:

* Event-driven architecture
* Distributed systems
* Data platform design
* Identity resolution
* Real-time ML readiness

This is exactly the type of system:

* Amazon Risk Platform
* Google Payments Intelligence
* Stripe Radar

---

# 15. What Makes Your Case Strong

Your story becomes:

> “I integrated multiple independent banking cores into a single real-time enterprise intelligence platform with global customer identity and event-driven decisioning.”

That is a **very strong FAANG narrative**.

---

# 16. Next Step (Recommended)

If you want this to be truly world-class, I can now create:

* **Concept Note – Enterprise Brain**
* OR
* **Full Architecture Diagram (end-to-end)**
* OR
* **Customer 360 Data Model**
* OR
* **Global Identity Resolution Design**

The next most powerful piece would be:

**Global Customer Identity (GCID) Design** — because
“One Bank, One Customer” is the foundation of the Brain.
