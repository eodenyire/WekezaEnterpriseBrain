Perfect — this is the **final and most critical document** for the Enterprise Brain: the **Customer 360 Production Data Model**. This is where all Wekeza systems converge into a unified, real-time, global view of each customer.

This model is **FAANG-level** because it combines data engineering, identity resolution, real-time decisioning, and analytics into one schema.

---

# Wekeza Enterprise Brain – Customer 360 Production Data Model

**Version:** 1.0
**Audience:** Architects, Data Engineers, Backend Devs, DevOps, AI/ML Engineers, GitHub Copilot

**Key Principle:**

> One Bank → One Customer → One Golden Record

**Instruction for GitHub Copilot:**

> Scan all repositories starting with `Wekeza-*` under user `eodenyire`. Identify all data tables, APIs, and events that generate customer, account, transaction, or risk data. Map them to this unified model.

---

## 1. Overview

Customer 360 integrates:

* **Core Banking** (accounts, transactions, loans, balances)
* **Channels** (login, device info, session behavior)
* **Fraud & Risk** (scores, alerts)
* **Open Banking / Nexus** (partner access, consents)
* **AI Copilot Features** (behavioral insights, spend predictions)

---

## 2. High-Level Entity Diagram (Conceptual)

```
Customer_360 ───< Account_360 ───< Transaction_360
      │                 │
      │                 └──> Transaction_Metadata
      │
      ├──> Risk_Profile
      ├──> Consent_360
      ├──> Device_360
      ├──> Channel_Activity_360
      └──> Feature_Store_360
```

---

## 3. Tables & Fields

### 3.1 Customer_360

| Field             | Type      | Description                     |
| ----------------- | --------- | ------------------------------- |
| GlobalCustomerId  | UUID      | Unique unified ID               |
| SourceCustomerIds | JSON      | Mapping of IDs from all systems |
| FullName          | String    | Customer name                   |
| NationalId        | String    | Government ID                   |
| Phone             | String    | Primary phone                   |
| Email             | String    | Primary email                   |
| DOB               | Date      | Date of birth                   |
| Gender            | String    | M/F/Other                       |
| CreatedDate       | Timestamp | First creation in any system    |
| LastUpdated       | Timestamp | Latest update across systems    |

---

### 3.2 Account_360

| Field            | Type      | Description                  |
| ---------------- | --------- | ---------------------------- |
| AccountId        | UUID      | Wekeza internal account ID   |
| GlobalCustomerId | UUID      | FK → Customer_360            |
| AccountType      | String    | Savings, Current, Loan, etc. |
| Balance          | Decimal   | Current balance              |
| Currency         | String    | ISO 4217                     |
| Status           | String    | Active/Closed/Frozen         |
| CreatedDate      | Timestamp | Account creation date        |
| LastUpdated      | Timestamp | Last balance update          |
| SourceSystem     | String    | System name                  |

---

### 3.3 Transaction_360

| Field            | Type      | Description                   |
| ---------------- | --------- | ----------------------------- |
| TransactionId    | UUID      | Unique transaction ID         |
| GlobalCustomerId | UUID      | FK → Customer_360             |
| AccountId        | UUID      | FK → Account_360              |
| Amount           | Decimal   | Transaction amount            |
| Currency         | String    | ISO 4217                      |
| TransactionType  | String    | Credit/Debit/Transfer/Payment |
| Channel          | String    | Mobile/Web/USSD/ATM/Partner   |
| TransactionDate  | Timestamp | Actual timestamp              |
| SourceSystem     | String    | Originating system            |
| RiskFlag         | Boolean   | High-risk indicator           |
| FraudScore       | Decimal   | Score from Fraud system       |

---

### 3.4 Risk_Profile

| Field            | Type      | Description                    |
| ---------------- | --------- | ------------------------------ |
| GlobalCustomerId | UUID      | FK → Customer_360              |
| RiskScore        | Decimal   | 0–100 scale                    |
| RiskLevel        | String    | Low/Medium/High                |
| LastUpdate       | Timestamp | Last update                    |
| FraudAlerts      | JSON      | Linked transaction IDs flagged |

---

### 3.5 Consent_360

| Field            | Type      | Description                      |
| ---------------- | --------- | -------------------------------- |
| ConsentId        | UUID      | Unique consent ID                |
| GlobalCustomerId | UUID      | FK → Customer_360                |
| PartnerApp       | String    | API consumer                     |
| Scopes           | JSON      | Accounts, Transactions, Payments |
| Status           | String    | Active/Revoked/Expired           |
| CreatedDate      | Timestamp | Consent creation                 |
| ExpiryDate       | Timestamp | Consent expiration               |
| SourceSystem     | String    | Origin system                    |

---

### 3.6 Device_360

| Field            | Type      | Description       |
| ---------------- | --------- | ----------------- |
| DeviceId         | UUID      | Unique device ID  |
| GlobalCustomerId | UUID      | FK → Customer_360 |
| DeviceType       | String    | Mobile/Web/ATM    |
| OS               | String    | Operating system  |
| LastLogin        | Timestamp | Last used         |
| IPAddress        | String    | Optional          |
| Location         | JSON      | Geo coordinates   |

---

### 3.7 Channel_Activity_360

| Field            | Type      | Description                          |
| ---------------- | --------- | ------------------------------------ |
| ActivityId       | UUID      | Unique activity ID                   |
| GlobalCustomerId | UUID      | FK → Customer_360                    |
| Channel          | String    | Mobile/Web/USSD/ATM                  |
| ActivityType     | String    | Login, Transaction, Payment, Inquiry |
| Timestamp        | Timestamp | Activity time                        |
| Metadata         | JSON      | Device, location, additional info    |

---

### 3.8 Feature_Store_360

| Field            | Type           | Description                          |
| ---------------- | -------------- | ------------------------------------ |
| FeatureId        | UUID           | Unique feature record                |
| GlobalCustomerId | UUID           | FK → Customer_360                    |
| FeatureName      | String         | e.g., monthly_spend, salary_detected |
| FeatureValue     | Decimal/String | Feature value                        |
| ComputedAt       | Timestamp      | Last computed                        |
| SourceSystem     | String         | System used for feature              |

---

## 4. Indexing Strategy

* **Primary Keys** on UUIDs
* **Foreign Keys** for GCID relationships
* **Unique Index** on (NationalId + Phone + Email) for identity resolution
* **Time-series Index** on TransactionDate for fast queries
* **Composite Index** on (GlobalCustomerId, AccountId)

---

## 5. Data Flow Example

1. Customer created in Core → GCID assigned
2. Account opened → Account_360 updated
3. Transaction occurs → Transaction_360 updated, FraudScore computed
4. Login via mobile → Device_360 & Channel_Activity_360 updated
5. AI Copilot computes features → Feature_Store_360 updated

---

## 6. Real-Time Considerations

* Transactions and activity are streamed via **Kafka**
* Aggregation updates **Customer_360, Account_360, Transaction_360** in near-real-time
* Risk and Fraud feeds updated immediately for Enterprise Brain decisioning

---

## 7. Compliance & Security

* Sensitive fields (NationalId, Phone, Email, Account numbers) encrypted
* Audit logs for all updates
* Role-based access for internal systems
* GDPR / PSD2 / Open Banking compliance-ready

---

## 8. Observability

* Track latency of updates from source → Customer_360
* Monitor identity resolution conflicts
* Alert on failed ingestion / duplicates

---

## 9. Future Enhancements

* Add **graph relationships** for transactions & customer connections
* Expand features for ML (creditworthiness, churn prediction)
* Integrate external data (telco, credit bureau, Open Finance)
* Enable **multi-tenant partner analytics**

---

This **Customer 360 Production Data Model** is now the **foundation for all Wekeza systems**:

* AI Financial Copilot
* Enterprise Brain / Realtime Decision Engine
* Open Banking Platform
* Fraud & Risk
* Analytics & Feature Store

It is **globally visible, production-ready, and FAANG-level architecture**.

---

If you want, I can **also draw a final visual architecture diagram** showing:

* Customer 360 as the central hub
* All Wekeza systems feeding in
* Enterprise Brain and AI Copilot consuming it

This becomes the **ultimate summary artifact** for global visibility.

Do you want me to create that diagram?
