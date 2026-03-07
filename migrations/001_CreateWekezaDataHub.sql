-- =============================================================================
-- Wekeza Main Datahub - PostgreSQL Schema
-- Version: 1.0.0
-- Description: Unified data warehouse consolidating all Wekeza banking systems
--
-- Source Systems:
--   1. Wekeza Core Banking (PostgreSQL: WekezaCoreDB) - Accounts, Transactions, Loans
--   2. WekezaCRM (SQL Server: WekezaCRM) - CRM, Interactions, Sentiment
--   3. WekezaOpenBanking (PostgreSQL: wekeza_banking) - OAuth, Payments, Webhooks
--   4. WekezaBank Risk Engine (PostgreSQL: risk_management) - Risk, Fraud
--   5. WekezaNextGenPersonalBanking (API aggregator) - AI Financial Insights
-- =============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";     -- For fuzzy text matching (identity resolution)

-- =============================================================================
-- SCHEMA: staging (raw data landed from each source system)
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS staging;
CREATE SCHEMA IF NOT EXISTS warehouse;
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE SCHEMA IF NOT EXISTS audit;

-- =============================================================================
-- SOURCE SYSTEM REGISTRY
-- =============================================================================

CREATE TABLE IF NOT EXISTS warehouse.source_systems (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    system_name     VARCHAR(100) NOT NULL UNIQUE,
    system_type     VARCHAR(50)  NOT NULL,   -- CoreBanking, CRM, OpenBanking, RiskManagement, PersonalBanking
    database_type   VARCHAR(50),             -- PostgreSQL, SQLServer, SQLite
    database_name   VARCHAR(100),
    host            VARCHAR(255),
    port            INTEGER,
    github_repo     VARCHAR(255),
    tech_stack      VARCHAR(100),            -- .NET, Node.js, Python
    is_active       BOOLEAN DEFAULT TRUE,
    last_sync_at    TIMESTAMPTZ,
    sync_status     VARCHAR(20) DEFAULT 'pending',  -- pending, running, succeeded, failed
    sync_error      TEXT,
    records_synced  BIGINT DEFAULT 0,
    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);

-- Seed known source systems
INSERT INTO warehouse.source_systems (system_name, system_type, database_type, database_name, host, port, github_repo, tech_stack) VALUES
  ('WekezaCore',             'CoreBanking',      'PostgreSQL', 'WekezaCoreDB',    'localhost', 5432, 'https://github.com/eodenyire/Wekeza',                        '.NET 8 / EF Core'),
  ('WekezaCRM',             'CRM',              'SQLServer',  'WekezaCRM',       'localhost', 1433, 'https://github.com/eodenyire/WekezaCRM',                     '.NET 8 / EF Core'),
  ('WekezaOpenBanking',     'OpenBanking',      'PostgreSQL', 'wekeza_banking',  'localhost', 5432, 'https://github.com/eodenyire/WekezaOpenBanking',             'Node.js / pg'),
  ('WekezaBank',            'RiskManagement',   'PostgreSQL', 'risk_management', 'localhost', 5432, 'https://github.com/eodenyire/WekezaBank',                    'Python / SQLAlchemy'),
  ('WekezaNextGenPersonal', 'PersonalBanking',  NULL,         NULL,              'localhost', 5000, 'https://github.com/eodenyire/WekezaNextGenPersonalBanking',  '.NET Core / API')
ON CONFLICT (system_name) DO NOTHING;

-- =============================================================================
-- ETL SYNC LOG
-- =============================================================================

CREATE TABLE IF NOT EXISTS audit.etl_sync_log (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system   VARCHAR(100) NOT NULL,
    entity_type     VARCHAR(100) NOT NULL,   -- customers, accounts, transactions, etc.
    sync_started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    sync_ended_at   TIMESTAMPTZ,
    status          VARCHAR(20) NOT NULL DEFAULT 'running',  -- running, succeeded, failed
    records_read    INTEGER DEFAULT 0,
    records_inserted INTEGER DEFAULT 0,
    records_updated INTEGER DEFAULT 0,
    records_skipped INTEGER DEFAULT 0,
    error_message   TEXT,
    watermark       TIMESTAMPTZ  -- Last record timestamp processed (for incremental loads)
);

CREATE INDEX IF NOT EXISTS idx_etl_log_source_entity  ON audit.etl_sync_log (source_system, entity_type);
CREATE INDEX IF NOT EXISTS idx_etl_log_started_at     ON audit.etl_sync_log (sync_started_at DESC);

-- =============================================================================
-- STAGING TABLES (raw data from each source)
-- =============================================================================

-- Staging: Customers (all sources)
CREATE TABLE IF NOT EXISTS staging.customers (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system       VARCHAR(50)  NOT NULL,
    source_customer_id  VARCHAR(100) NOT NULL,
    first_name          VARCHAR(100),
    last_name           VARCHAR(100),
    email               VARCHAR(255),
    phone_number        VARCHAR(30),
    national_id         VARCHAR(50),
    date_of_birth       DATE,
    address             TEXT,
    city                VARCHAR(100),
    country             VARCHAR(10) DEFAULT 'KE',
    kyc_status          VARCHAR(30),
    credit_score        DECIMAL(5,2),
    risk_score          INTEGER,
    customer_segment    VARCHAR(50),
    lifetime_value      DECIMAL(18,2),
    is_active           BOOLEAN DEFAULT TRUE,
    source_created_at   TIMESTAMPTZ,
    source_updated_at   TIMESTAMPTZ,
    ingested_at         TIMESTAMPTZ DEFAULT NOW(),
    gcid                UUID,            -- Assigned after identity resolution
    is_resolved         BOOLEAN DEFAULT FALSE,
    UNIQUE (source_system, source_customer_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_customers_email   ON staging.customers (email);
CREATE INDEX IF NOT EXISTS idx_stg_customers_phone   ON staging.customers (phone_number);
CREATE INDEX IF NOT EXISTS idx_stg_customers_natid   ON staging.customers (national_id);
CREATE INDEX IF NOT EXISTS idx_stg_customers_gcid    ON staging.customers (gcid);

-- Staging: Accounts (all sources)
CREATE TABLE IF NOT EXISTS staging.accounts (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system       VARCHAR(50)  NOT NULL,
    source_account_id   VARCHAR(100) NOT NULL,
    source_customer_id  VARCHAR(100) NOT NULL,
    account_number      VARCHAR(60),
    account_type        VARCHAR(50),
    currency            VARCHAR(5) DEFAULT 'KES',
    balance             DECIMAL(18,2),
    available_balance   DECIMAL(18,2),
    overdraft_limit     DECIMAL(18,2) DEFAULT 0,
    status              VARCHAR(30),
    product_name        VARCHAR(100),
    interest_rate       DECIMAL(6,4),
    minimum_balance     DECIMAL(18,2),
    opened_date         DATE,
    closed_date         DATE,
    source_created_at   TIMESTAMPTZ,
    source_updated_at   TIMESTAMPTZ,
    ingested_at         TIMESTAMPTZ DEFAULT NOW(),
    gcid                UUID,
    UNIQUE (source_system, source_account_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_accounts_gcid    ON staging.accounts (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_accounts_number  ON staging.accounts (account_number);

-- Staging: Transactions (all sources)
CREATE TABLE IF NOT EXISTS staging.transactions (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system           VARCHAR(50)  NOT NULL,
    source_transaction_id   VARCHAR(100) NOT NULL,
    source_account_id       VARCHAR(100) NOT NULL,
    source_customer_id      VARCHAR(100),
    transaction_date        TIMESTAMPTZ  NOT NULL,
    transaction_type        VARCHAR(50),   -- Credit, Debit, Transfer, Payment, Withdrawal, Deposit
    amount                  DECIMAL(18,2) NOT NULL,
    currency                VARCHAR(5) DEFAULT 'KES',
    channel                 VARCHAR(50),   -- Mobile, Web, USSD, ATM, Branch, API
    description             TEXT,
    reference               VARCHAR(150),
    balance_after           DECIMAL(18,2),
    status                  VARCHAR(30) DEFAULT 'completed',
    risk_score              DECIMAL(5,4),
    ai_category             VARCHAR(100),
    merchant_name           VARCHAR(255),
    merchant_category       VARCHAR(100),
    location                VARCHAR(150),
    related_account_number  VARCHAR(60),
    source_created_at       TIMESTAMPTZ,
    ingested_at             TIMESTAMPTZ DEFAULT NOW(),
    gcid                    UUID,
    UNIQUE (source_system, source_transaction_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_txn_date     ON staging.transactions (transaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_stg_txn_gcid     ON staging.transactions (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_txn_account  ON staging.transactions (source_account_id);

-- Staging: Risk Assessments (WekezaBank)
CREATE TABLE IF NOT EXISTS staging.risk_assessments (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system           VARCHAR(50) NOT NULL DEFAULT 'WekezaBank',
    source_case_id          INTEGER,
    source_transaction_id   VARCHAR(100) NOT NULL,
    source_customer_id      VARCHAR(100),
    amount                  DECIMAL(18,2),
    currency                VARCHAR(5) DEFAULT 'KES',
    risk_score              DECIMAL(5,4),
    risk_level              VARCHAR(20),   -- LOW, MEDIUM, HIGH
    metric_type             VARCHAR(50),   -- CREDIT, LIQUIDITY, MARKET, OPERATIONAL
    flagged_reason          TEXT,
    analyst_id              VARCHAR(100),
    analyst_comment         TEXT,
    outcome                 VARCHAR(50),   -- APPROVED, REJECTED, BLOCKED, FLAGGED, UNDER_REVIEW
    ballerine_case_id       VARCHAR(100),
    tazama_fraud_score      DECIMAL(5,4),
    tazama_typologies       TEXT[],
    tazama_recommendation   VARCHAR(50),
    ciso_risk_id            VARCHAR(100),
    assessed_at             TIMESTAMPTZ,
    closed_at               TIMESTAMPTZ,
    source_created_at       TIMESTAMPTZ,
    ingested_at             TIMESTAMPTZ DEFAULT NOW(),
    gcid                    UUID,
    UNIQUE (source_system, source_transaction_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_risk_gcid   ON staging.risk_assessments (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_risk_level  ON staging.risk_assessments (risk_level);
CREATE INDEX IF NOT EXISTS idx_stg_risk_date   ON staging.risk_assessments (assessed_at DESC);

-- Staging: CRM Interactions (WekezaCRM)
CREATE TABLE IF NOT EXISTS staging.crm_interactions (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system       VARCHAR(50) NOT NULL DEFAULT 'WekezaCRM',
    source_interaction_id VARCHAR(100) NOT NULL,
    source_customer_id  VARCHAR(100) NOT NULL,
    interaction_type    VARCHAR(50),   -- Phone, Email, SMS, Chat, WhatsApp, USSD
    channel             VARCHAR(50),
    subject             VARCHAR(255),
    description         TEXT,
    sentiment_type      VARCHAR(30),   -- Positive, Negative, Neutral
    sentiment_score     DECIMAL(5,4),
    key_phrases         TEXT,
    duration_minutes    INTEGER,
    case_number         VARCHAR(50),
    resolved            BOOLEAN,
    interaction_date    TIMESTAMPTZ,
    source_created_at   TIMESTAMPTZ,
    ingested_at         TIMESTAMPTZ DEFAULT NOW(),
    gcid                UUID,
    UNIQUE (source_system, source_interaction_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_crm_gcid  ON staging.crm_interactions (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_crm_date  ON staging.crm_interactions (interaction_date DESC);

-- Staging: CRM Cases (WekezaCRM)
CREATE TABLE IF NOT EXISTS staging.crm_cases (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system       VARCHAR(50) NOT NULL DEFAULT 'WekezaCRM',
    source_case_id      VARCHAR(100) NOT NULL,
    source_customer_id  VARCHAR(100) NOT NULL,
    case_number         VARCHAR(50),
    title               VARCHAR(255),
    category            VARCHAR(100),
    sub_category        VARCHAR(100),
    status              VARCHAR(30),   -- Open, InProgress, Resolved, Closed
    priority            VARCHAR(20),   -- Low, Medium, High, Critical
    sla_hours           INTEGER,
    opened_at           TIMESTAMPTZ,
    resolved_at         TIMESTAMPTZ,
    closed_at           TIMESTAMPTZ,
    resolution          TEXT,
    source_created_at   TIMESTAMPTZ,
    ingested_at         TIMESTAMPTZ DEFAULT NOW(),
    gcid                UUID,
    UNIQUE (source_system, source_case_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_cases_gcid    ON staging.crm_cases (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_cases_status  ON staging.crm_cases (status);

-- Staging: Payments (WekezaOpenBanking)
CREATE TABLE IF NOT EXISTS staging.open_banking_payments (
    id                          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system               VARCHAR(50) NOT NULL DEFAULT 'WekezaOpenBanking',
    source_payment_id           UUID NOT NULL,
    payment_ref                 VARCHAR(100) NOT NULL,
    source_account_id           UUID,
    destination_account_number  VARCHAR(60),
    amount                      DECIMAL(18,2),
    currency                    VARCHAR(5) DEFAULT 'KES',
    status                      VARCHAR(20),   -- pending, processing, completed, failed
    risk_score                  DECIMAL(5,4),
    oauth_client_id             VARCHAR(255),
    idempotency_key             VARCHAR(255),
    description                 TEXT,
    completed_at                TIMESTAMPTZ,
    source_created_at           TIMESTAMPTZ,
    ingested_at                 TIMESTAMPTZ DEFAULT NOW(),
    gcid                        UUID,
    UNIQUE (source_system, source_payment_id)
);
CREATE INDEX IF NOT EXISTS idx_stg_payments_gcid   ON staging.open_banking_payments (gcid);
CREATE INDEX IF NOT EXISTS idx_stg_payments_status ON staging.open_banking_payments (status);

-- Staging: Webhook Events (WekezaOpenBanking)
CREATE TABLE IF NOT EXISTS staging.webhook_events (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_system   VARCHAR(50) NOT NULL DEFAULT 'WekezaOpenBanking',
    event_type      VARCHAR(100) NOT NULL,
    payload         JSONB NOT NULL,
    delivery_status VARCHAR(20),
    attempts        INTEGER DEFAULT 0,
    delivered_at    TIMESTAMPTZ,
    source_created_at TIMESTAMPTZ,
    ingested_at     TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_stg_webhooks_type    ON staging.webhook_events (event_type);
CREATE INDEX IF NOT EXISTS idx_stg_webhooks_date    ON staging.webhook_events (ingested_at DESC);

-- =============================================================================
-- WAREHOUSE DIMENSION TABLES
-- =============================================================================

-- Global Customer Identity (the heart of the datahub)
CREATE TABLE IF NOT EXISTS warehouse.dim_customers (
    gcid                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    -- Primary identity fields (best-available from all sources)
    primary_email       VARCHAR(255) UNIQUE,
    primary_phone       VARCHAR(30),
    national_id         VARCHAR(50),
    first_name          VARCHAR(100),
    last_name           VARCHAR(100),
    full_name           VARCHAR(255) GENERATED ALWAYS AS (first_name || ' ' || last_name) STORED,
    date_of_birth       DATE,
    gender              VARCHAR(20),
    address             TEXT,
    city                VARCHAR(100),
    country             VARCHAR(10) DEFAULT 'KE',

    -- Cross-system local IDs (maps GCID back to each source)
    core_banking_id     VARCHAR(100),   -- Wekeza Core Banking
    crm_id              VARCHAR(100),   -- WekezaCRM
    open_banking_id     VARCHAR(100),   -- WekezaOpenBanking (customer_number)
    risk_system_id      VARCHAR(100),   -- WekezaBank
    personal_banking_id VARCHAR(100),   -- WekezaNextGenPersonalBanking

    -- Aggregated intelligence (updated by ETL)
    overall_kyc_status  VARCHAR(30) DEFAULT 'unknown',  -- unknown, pending, verified, rejected
    overall_risk_level  VARCHAR(20) DEFAULT 'unknown',  -- unknown, low, medium, high
    credit_score        DECIMAL(5,2),
    lifetime_value      DECIMAL(18,2),
    customer_segment    VARCHAR(50),

    -- Behavioural flags (from AI analysis)
    financial_personality VARCHAR(50),  -- Saver, Spender, Balanced, Impulsive
    stress_level          INTEGER,      -- 0-100 (from WekezaNextGen)
    financial_health_score INTEGER,     -- 0-100 (from WekezaNextGen)

    -- Data lineage
    identity_confidence DECIMAL(5,4) DEFAULT 1.0,  -- How confident in the match
    sources_count       INTEGER DEFAULT 1,          -- How many systems have this customer
    first_seen_at       TIMESTAMPTZ DEFAULT NOW(),
    last_activity_at    TIMESTAMPTZ,
    is_active           BOOLEAN DEFAULT TRUE,

    created_at          TIMESTAMPTZ DEFAULT NOW(),
    updated_at          TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_dim_cust_email     ON warehouse.dim_customers (primary_email);
CREATE INDEX IF NOT EXISTS idx_dim_cust_phone     ON warehouse.dim_customers (primary_phone);
CREATE INDEX IF NOT EXISTS idx_dim_cust_natid     ON warehouse.dim_customers (national_id);
CREATE INDEX IF NOT EXISTS idx_dim_cust_risk      ON warehouse.dim_customers (overall_risk_level);
CREATE INDEX IF NOT EXISTS idx_dim_cust_segment   ON warehouse.dim_customers (customer_segment);
CREATE INDEX IF NOT EXISTS idx_dim_cust_fullname  ON warehouse.dim_customers USING gin(full_name gin_trgm_ops);

-- Account Dimension
CREATE TABLE IF NOT EXISTS warehouse.dim_accounts (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    source_system       VARCHAR(50) NOT NULL,
    source_account_id   VARCHAR(100) NOT NULL,
    account_number      VARCHAR(60) NOT NULL,
    account_type        VARCHAR(50),   -- Savings, Current, Loan, Credit, Fixed Deposit
    currency            VARCHAR(5) DEFAULT 'KES',
    current_balance     DECIMAL(18,2),
    available_balance   DECIMAL(18,2),
    overdraft_limit     DECIMAL(18,2) DEFAULT 0,
    status              VARCHAR(30),   -- Active, Inactive, Frozen, Closed
    product_name        VARCHAR(100),
    interest_rate       DECIMAL(6,4),
    minimum_balance     DECIMAL(18,2),
    opened_date         DATE,
    closed_date         DATE,
    last_transaction_at TIMESTAMPTZ,
    transaction_count   INTEGER DEFAULT 0,
    created_at          TIMESTAMPTZ DEFAULT NOW(),
    updated_at          TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_account_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_acct_gcid    ON warehouse.dim_accounts (gcid);
CREATE INDEX IF NOT EXISTS idx_dim_acct_number  ON warehouse.dim_accounts (account_number);
CREATE INDEX IF NOT EXISTS idx_dim_acct_type    ON warehouse.dim_accounts (account_type);

-- Date Dimension (for BI time-series analysis)
CREATE TABLE IF NOT EXISTS warehouse.dim_date (
    date_key        INTEGER PRIMARY KEY,   -- YYYYMMDD format
    full_date       DATE NOT NULL UNIQUE,
    day_of_week     INTEGER,
    day_name        VARCHAR(10),
    day_of_month    INTEGER,
    day_of_year     INTEGER,
    week_of_year    INTEGER,
    month_number    INTEGER,
    month_name      VARCHAR(15),
    quarter         INTEGER,
    year            INTEGER,
    is_weekend      BOOLEAN,
    is_public_holiday BOOLEAN DEFAULT FALSE,
    fiscal_year     INTEGER,
    fiscal_quarter  INTEGER
);

-- Populate date dimension for 5 years (2020-2030)
INSERT INTO warehouse.dim_date (date_key, full_date, day_of_week, day_name, day_of_month, day_of_year, week_of_year, month_number, month_name, quarter, year, is_weekend, fiscal_year, fiscal_quarter)
SELECT
    TO_CHAR(d, 'YYYYMMDD')::INTEGER                    AS date_key,
    d::DATE                                             AS full_date,
    EXTRACT(DOW FROM d)::INTEGER                        AS day_of_week,
    TO_CHAR(d, 'Day')                                   AS day_name,
    EXTRACT(DAY FROM d)::INTEGER                        AS day_of_month,
    EXTRACT(DOY FROM d)::INTEGER                        AS day_of_year,
    EXTRACT(WEEK FROM d)::INTEGER                       AS week_of_year,
    EXTRACT(MONTH FROM d)::INTEGER                      AS month_number,
    TO_CHAR(d, 'Month')                                 AS month_name,
    EXTRACT(QUARTER FROM d)::INTEGER                    AS quarter,
    EXTRACT(YEAR FROM d)::INTEGER                       AS year,
    EXTRACT(DOW FROM d) IN (0, 6)                       AS is_weekend,
    EXTRACT(YEAR FROM d)::INTEGER                       AS fiscal_year,
    EXTRACT(QUARTER FROM d)::INTEGER                    AS fiscal_quarter
FROM GENERATE_SERIES('2020-01-01'::DATE, '2030-12-31'::DATE, '1 day'::INTERVAL) d
ON CONFLICT (date_key) DO NOTHING;

-- =============================================================================
-- WAREHOUSE FACT TABLES
-- =============================================================================

-- Fact: Transactions (core banking + open banking + CRM)
CREATE TABLE IF NOT EXISTS warehouse.fact_transactions (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                    UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    account_id              UUID REFERENCES warehouse.dim_accounts(id),
    date_key                INTEGER REFERENCES warehouse.dim_date(date_key),
    source_system           VARCHAR(50) NOT NULL,
    source_transaction_id   VARCHAR(100) NOT NULL,
    transaction_date        TIMESTAMPTZ NOT NULL,
    transaction_type        VARCHAR(50),
    amount                  DECIMAL(18,2) NOT NULL,
    currency                VARCHAR(5) DEFAULT 'KES',
    channel                 VARCHAR(50),
    description             TEXT,
    reference               VARCHAR(150),
    balance_after           DECIMAL(18,2),
    status                  VARCHAR(30),
    risk_score              DECIMAL(5,4),
    ai_category             VARCHAR(100),   -- Food, Transport, Utilities, Salary, etc.
    merchant_name           VARCHAR(255),
    merchant_category       VARCHAR(100),
    location                VARCHAR(150),
    related_account_number  VARCHAR(60),
    ingested_at             TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_transaction_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_txn_gcid    ON warehouse.fact_transactions (gcid);
CREATE INDEX IF NOT EXISTS idx_fact_txn_date    ON warehouse.fact_transactions (transaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_fact_txn_account ON warehouse.fact_transactions (account_id);
CREATE INDEX IF NOT EXISTS idx_fact_txn_channel ON warehouse.fact_transactions (channel);
CREATE INDEX IF NOT EXISTS idx_fact_txn_type    ON warehouse.fact_transactions (transaction_type);

-- Fact: Payments (open banking)
CREATE TABLE IF NOT EXISTS warehouse.fact_payments (
    id                          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                        UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    account_id                  UUID REFERENCES warehouse.dim_accounts(id),
    date_key                    INTEGER REFERENCES warehouse.dim_date(date_key),
    source_system               VARCHAR(50) NOT NULL DEFAULT 'WekezaOpenBanking',
    source_payment_id           UUID NOT NULL,
    payment_ref                 VARCHAR(100) NOT NULL,
    destination_account_number  VARCHAR(60),
    amount                      DECIMAL(18,2) NOT NULL,
    currency                    VARCHAR(5) DEFAULT 'KES',
    status                      VARCHAR(20),
    risk_score                  DECIMAL(5,4),
    oauth_client_name           VARCHAR(255),
    completed_at                TIMESTAMPTZ,
    ingested_at                 TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_payment_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_pay_gcid    ON warehouse.fact_payments (gcid);
CREATE INDEX IF NOT EXISTS idx_fact_pay_date    ON warehouse.fact_payments (completed_at DESC);
CREATE INDEX IF NOT EXISTS idx_fact_pay_status  ON warehouse.fact_payments (status);

-- Fact: Risk Assessments (WekezaBank)
CREATE TABLE IF NOT EXISTS warehouse.fact_risk_assessments (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                    UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    date_key                INTEGER REFERENCES warehouse.dim_date(date_key),
    source_system           VARCHAR(50) NOT NULL DEFAULT 'WekezaBank',
    source_case_id          INTEGER,
    source_transaction_id   VARCHAR(100) NOT NULL,
    amount                  DECIMAL(18,2),
    currency                VARCHAR(5) DEFAULT 'KES',
    risk_score              DECIMAL(5,4) NOT NULL,
    risk_level              VARCHAR(20) NOT NULL,   -- LOW, MEDIUM, HIGH
    metric_type             VARCHAR(50),            -- CREDIT, LIQUIDITY, MARKET, OPERATIONAL
    flagged_reasons         TEXT[],
    outcome                 VARCHAR(50),            -- APPROVED, REJECTED, BLOCKED, FLAGGED
    analyst_reviewed        BOOLEAN DEFAULT FALSE,
    tazama_fraud_score      DECIMAL(5,4),
    tazama_typologies       TEXT[],
    tazama_recommendation   VARCHAR(50),
    assessed_at             TIMESTAMPTZ NOT NULL,
    closed_at               TIMESTAMPTZ,
    ingested_at             TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_transaction_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_risk_gcid    ON warehouse.fact_risk_assessments (gcid);
CREATE INDEX IF NOT EXISTS idx_fact_risk_level   ON warehouse.fact_risk_assessments (risk_level);
CREATE INDEX IF NOT EXISTS idx_fact_risk_date    ON warehouse.fact_risk_assessments (assessed_at DESC);

-- Fact: CRM Interactions (WekezaCRM)
CREATE TABLE IF NOT EXISTS warehouse.fact_interactions (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                    UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    date_key                INTEGER REFERENCES warehouse.dim_date(date_key),
    source_system           VARCHAR(50) NOT NULL DEFAULT 'WekezaCRM',
    source_interaction_id   VARCHAR(100) NOT NULL,
    interaction_type        VARCHAR(50),   -- Phone, Email, SMS, Chat, WhatsApp, USSD
    channel                 VARCHAR(50),
    subject                 VARCHAR(255),
    sentiment_type          VARCHAR(30),   -- Positive, Negative, Neutral
    sentiment_score         DECIMAL(5,4),
    key_phrases             TEXT,
    duration_minutes        INTEGER,
    case_number             VARCHAR(50),
    resolved                BOOLEAN,
    interaction_date        TIMESTAMPTZ NOT NULL,
    ingested_at             TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_interaction_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_int_gcid  ON warehouse.fact_interactions (gcid);
CREATE INDEX IF NOT EXISTS idx_fact_int_date  ON warehouse.fact_interactions (interaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_fact_int_type  ON warehouse.fact_interactions (interaction_type);

-- Fact: CRM Cases (WekezaCRM)
CREATE TABLE IF NOT EXISTS warehouse.fact_cases (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    gcid                UUID NOT NULL REFERENCES warehouse.dim_customers(gcid),
    date_key            INTEGER REFERENCES warehouse.dim_date(date_key),
    source_system       VARCHAR(50) NOT NULL DEFAULT 'WekezaCRM',
    source_case_id      VARCHAR(100) NOT NULL,
    case_number         VARCHAR(50),
    title               VARCHAR(255),
    category            VARCHAR(100),
    sub_category        VARCHAR(100),
    status              VARCHAR(30),
    priority            VARCHAR(20),
    sla_hours           INTEGER,
    resolution_hours    DECIMAL(8,2),  -- Calculated: (resolved_at - opened_at) in hours
    is_sla_breached     BOOLEAN,
    opened_at           TIMESTAMPTZ NOT NULL,
    resolved_at         TIMESTAMPTZ,
    closed_at           TIMESTAMPTZ,
    ingested_at         TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (source_system, source_case_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_cases_gcid    ON warehouse.fact_cases (gcid);
CREATE INDEX IF NOT EXISTS idx_fact_cases_status  ON warehouse.fact_cases (status);
CREATE INDEX IF NOT EXISTS idx_fact_cases_prio    ON warehouse.fact_cases (priority);

-- =============================================================================
-- ANALYTICS VIEWS (Customer 360 + Intelligence)
-- =============================================================================

-- Customer 360 View
CREATE MATERIALIZED VIEW IF NOT EXISTS analytics.customer_360 AS
SELECT
    dc.gcid,
    dc.first_name,
    dc.last_name,
    dc.full_name,
    dc.primary_email,
    dc.primary_phone,
    dc.national_id,
    dc.date_of_birth,
    EXTRACT(YEAR FROM AGE(dc.date_of_birth))::INTEGER     AS age,
    dc.city,
    dc.country,
    dc.overall_kyc_status,
    dc.overall_risk_level,
    dc.credit_score,
    dc.customer_segment,
    dc.lifetime_value,
    dc.financial_personality,
    dc.stress_level,
    dc.financial_health_score,
    dc.sources_count,
    dc.first_seen_at,
    dc.last_activity_at,

    -- Account Summary
    COUNT(DISTINCT da.id)                                                                      AS total_accounts,
    SUM(da.current_balance)                                                                    AS total_balance_kes,
    COUNT(DISTINCT da.id) FILTER (WHERE da.status = 'Active')                                 AS active_accounts,

    -- Transaction Summary (last 30 days)
    COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days')             AS txn_count_30d,
    COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Credit'
             AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0)                       AS income_30d,
    COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Debit'
             AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0)                       AS spending_30d,
    COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Credit'
             AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0) -
    COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Debit'
             AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0)                       AS net_cashflow_30d,

    -- Risk Summary
    AVG(fra.risk_score) FILTER (WHERE fra.assessed_at >= NOW() - INTERVAL '90 days')          AS avg_risk_score_90d,
    COUNT(fra.id) FILTER (WHERE fra.risk_level = 'HIGH')                                       AS high_risk_events,

    -- CRM Summary
    COUNT(DISTINCT fi.id) FILTER (WHERE fi.interaction_date >= NOW() - INTERVAL '90 days')    AS interactions_90d,
    AVG(fi.sentiment_score) FILTER (WHERE fi.interaction_date >= NOW() - INTERVAL '90 days')  AS avg_sentiment_90d,
    COUNT(fc.id) FILTER (WHERE fc.status NOT IN ('Resolved', 'Closed'))                        AS open_cases,

    -- Channel Preferences
    MODE() WITHIN GROUP (ORDER BY ft.channel) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '90 days') AS preferred_channel,

    MAX(ft.transaction_date)                                                                   AS last_transaction_at,
    dc.created_at,
    NOW() AS refreshed_at
FROM warehouse.dim_customers dc
LEFT JOIN warehouse.dim_accounts     da  ON dc.gcid = da.gcid
LEFT JOIN warehouse.fact_transactions ft  ON da.id   = ft.account_id
LEFT JOIN warehouse.fact_risk_assessments fra ON dc.gcid = fra.gcid
LEFT JOIN warehouse.fact_interactions fi  ON dc.gcid = fi.gcid
LEFT JOIN warehouse.fact_cases        fc  ON dc.gcid = fc.gcid
GROUP BY
    dc.gcid, dc.first_name, dc.last_name, dc.full_name, dc.primary_email, dc.primary_phone,
    dc.national_id, dc.date_of_birth, dc.city, dc.country, dc.overall_kyc_status,
    dc.overall_risk_level, dc.credit_score, dc.customer_segment, dc.lifetime_value,
    dc.financial_personality, dc.stress_level, dc.financial_health_score,
    dc.sources_count, dc.first_seen_at, dc.last_activity_at, dc.created_at;

CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_c360_gcid ON analytics.customer_360 (gcid);
CREATE INDEX IF NOT EXISTS idx_mv_c360_risk        ON analytics.customer_360 (overall_risk_level);
CREATE INDEX IF NOT EXISTS idx_mv_c360_segment     ON analytics.customer_360 (customer_segment);

-- Daily Transaction Summary (for dashboards)
CREATE MATERIALIZED VIEW IF NOT EXISTS analytics.daily_transaction_summary AS
SELECT
    TO_CHAR(ft.transaction_date, 'YYYY-MM-DD')::DATE   AS txn_date,
    ft.source_system,
    ft.transaction_type,
    ft.channel,
    ft.ai_category,
    ft.currency,
    COUNT(*)                                            AS transaction_count,
    SUM(ft.amount)                                      AS total_amount,
    AVG(ft.amount)                                      AS avg_amount,
    MAX(ft.amount)                                      AS max_amount,
    MIN(ft.amount)                                      AS min_amount,
    COUNT(DISTINCT ft.gcid)                             AS unique_customers,
    AVG(ft.risk_score)                                  AS avg_risk_score,
    COUNT(*) FILTER (WHERE ft.risk_score > 0.8)        AS high_risk_count
FROM warehouse.fact_transactions ft
GROUP BY 1, 2, 3, 4, 5, 6;

CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_daily_txn
    ON analytics.daily_transaction_summary (txn_date, source_system, transaction_type, channel, COALESCE(ai_category,''), currency);

-- Risk Metrics Summary
CREATE MATERIALIZED VIEW IF NOT EXISTS analytics.risk_dashboard AS
SELECT
    DATE_TRUNC('day', fra.assessed_at)                  AS assessment_date,
    fra.source_system,
    fra.risk_level,
    fra.metric_type,
    fra.outcome,
    COUNT(*)                                            AS assessment_count,
    SUM(fra.amount)                                     AS total_amount_at_risk,
    AVG(fra.risk_score)                                 AS avg_risk_score,
    AVG(fra.tazama_fraud_score)                         AS avg_fraud_score,
    COUNT(DISTINCT fra.gcid)                            AS unique_customers,
    COUNT(*) FILTER (WHERE fra.analyst_reviewed)        AS analyst_reviewed_count
FROM warehouse.fact_risk_assessments fra
GROUP BY 1, 2, 3, 4, 5;

-- =============================================================================
-- FEATURE STORE TABLES (ML-ready pre-computed features)
-- =============================================================================

CREATE TABLE IF NOT EXISTS analytics.customer_features (
    gcid                        UUID PRIMARY KEY REFERENCES warehouse.dim_customers(gcid),
    computed_at                 TIMESTAMPTZ DEFAULT NOW(),

    -- Transaction velocity features
    txn_count_7d                INTEGER DEFAULT 0,
    txn_count_30d               INTEGER DEFAULT 0,
    txn_count_90d               INTEGER DEFAULT 0,
    total_spend_30d             DECIMAL(18,2) DEFAULT 0,
    total_income_30d            DECIMAL(18,2) DEFAULT 0,
    avg_txn_amount_30d          DECIMAL(18,2) DEFAULT 0,
    max_single_txn_30d          DECIMAL(18,2) DEFAULT 0,
    net_cashflow_30d            DECIMAL(18,2) DEFAULT 0,

    -- Channel behaviour features
    mobile_txn_pct              DECIMAL(5,4) DEFAULT 0,  -- % of transactions via mobile
    web_txn_pct                 DECIMAL(5,4) DEFAULT 0,
    ussd_txn_pct                DECIMAL(5,4) DEFAULT 0,
    atm_txn_pct                 DECIMAL(5,4) DEFAULT 0,
    branch_txn_pct              DECIMAL(5,4) DEFAULT 0,
    api_txn_pct                 DECIMAL(5,4) DEFAULT 0,

    -- Risk features
    avg_risk_score_30d          DECIMAL(5,4) DEFAULT 0,
    high_risk_txn_count_30d     INTEGER DEFAULT 0,
    fraud_flags_90d             INTEGER DEFAULT 0,
    risk_level_current          VARCHAR(20) DEFAULT 'unknown',

    -- Account features
    account_count               INTEGER DEFAULT 0,
    total_balance               DECIMAL(18,2) DEFAULT 0,
    max_account_balance         DECIMAL(18,2) DEFAULT 0,
    months_since_account_opened INTEGER DEFAULT 0,

    -- CRM features
    open_cases_count            INTEGER DEFAULT 0,
    avg_sentiment_score_90d     DECIMAL(5,4),
    interaction_count_90d       INTEGER DEFAULT 0,
    case_resolution_rate        DECIMAL(5,4),

    -- Payment features (Open Banking)
    ob_payment_count_30d        INTEGER DEFAULT 0,
    ob_payment_amount_30d       DECIMAL(18,2) DEFAULT 0,
    ob_failed_payment_pct_30d   DECIMAL(5,4) DEFAULT 0,

    -- AI features (WekezaNextGen)
    financial_health_score      INTEGER,
    stress_level                INTEGER,
    financial_personality       VARCHAR(50),

    -- Time-based features
    days_since_last_txn         INTEGER,
    days_since_last_login       INTEGER,
    avg_txn_hour_of_day         DECIMAL(4,2),  -- average hour when transactions happen
    weekend_txn_pct             DECIMAL(5,4),   -- % transactions on weekends

    -- Feature vector version
    feature_version             VARCHAR(20) DEFAULT 'v1.0'
);

-- =============================================================================
-- HELPER FUNCTIONS
-- =============================================================================

-- Function: Resolve customer identity (match by email, phone, or national_id)
CREATE OR REPLACE FUNCTION warehouse.resolve_customer_gcid(
    p_email         VARCHAR,
    p_phone         VARCHAR,
    p_national_id   VARCHAR,
    p_source_system VARCHAR,
    p_source_id     VARCHAR
) RETURNS UUID AS $$
DECLARE
    v_gcid UUID;
BEGIN
    -- 1. Try to match by national_id (strongest match)
    IF p_national_id IS NOT NULL AND p_national_id != '' THEN
        SELECT gcid INTO v_gcid FROM warehouse.dim_customers
        WHERE national_id = p_national_id LIMIT 1;
        IF FOUND THEN RETURN v_gcid; END IF;
    END IF;

    -- 2. Try to match by email
    IF p_email IS NOT NULL AND p_email != '' THEN
        SELECT gcid INTO v_gcid FROM warehouse.dim_customers
        WHERE primary_email = LOWER(TRIM(p_email)) LIMIT 1;
        IF FOUND THEN RETURN v_gcid; END IF;
    END IF;

    -- 3. Try to match by phone
    IF p_phone IS NOT NULL AND p_phone != '' THEN
        SELECT gcid INTO v_gcid FROM warehouse.dim_customers
        WHERE primary_phone = p_phone LIMIT 1;
        IF FOUND THEN RETURN v_gcid; END IF;
    END IF;

    -- 4. No match found - return NULL (caller creates a new dim_customer)
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Function: Update customer feature store
CREATE OR REPLACE FUNCTION analytics.refresh_customer_features(p_gcid UUID)
RETURNS VOID AS $$
BEGIN
    INSERT INTO analytics.customer_features (
        gcid, computed_at,
        txn_count_7d, txn_count_30d, txn_count_90d,
        total_spend_30d, total_income_30d, avg_txn_amount_30d, max_single_txn_30d, net_cashflow_30d,
        mobile_txn_pct, web_txn_pct, ussd_txn_pct, atm_txn_pct, branch_txn_pct, api_txn_pct,
        avg_risk_score_30d, high_risk_txn_count_30d, fraud_flags_90d,
        account_count, total_balance, max_account_balance,
        open_cases_count, avg_sentiment_score_90d, interaction_count_90d,
        ob_payment_count_30d, ob_payment_amount_30d,
        days_since_last_txn, weekend_txn_pct
    )
    SELECT
        p_gcid, NOW(),
        COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '7 days'),
        COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'),
        COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '90 days'),
        COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Debit' AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0),
        COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Credit' AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0),
        COALESCE(AVG(ft.amount) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0),
        COALESCE(MAX(ft.amount) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0),
        COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Credit' AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0) -
        COALESCE(SUM(ft.amount) FILTER (WHERE ft.transaction_type = 'Debit' AND ft.transaction_date >= NOW() - INTERVAL '30 days'), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'Mobile' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'Web' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'USSD' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'ATM' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'Branch' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(COUNT(ft.id) FILTER (WHERE ft.channel = 'API' AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0),
        COALESCE(AVG(fra.risk_score) FILTER (WHERE fra.assessed_at >= NOW() - INTERVAL '30 days'), 0),
        COUNT(fra.id) FILTER (WHERE fra.risk_level = 'HIGH' AND fra.assessed_at >= NOW() - INTERVAL '30 days'),
        COUNT(fra.id) FILTER (WHERE fra.risk_level IN ('MEDIUM', 'HIGH') AND fra.assessed_at >= NOW() - INTERVAL '90 days'),
        COUNT(DISTINCT da.id),
        COALESCE(SUM(da.current_balance), 0),
        COALESCE(MAX(da.current_balance), 0),
        COUNT(fc.id) FILTER (WHERE fc.status NOT IN ('Resolved', 'Closed')),
        AVG(fi.sentiment_score) FILTER (WHERE fi.interaction_date >= NOW() - INTERVAL '90 days'),
        COUNT(fi.id) FILTER (WHERE fi.interaction_date >= NOW() - INTERVAL '90 days'),
        COUNT(fp.id) FILTER (WHERE fp.completed_at >= NOW() - INTERVAL '30 days'),
        COALESCE(SUM(fp.amount) FILTER (WHERE fp.completed_at >= NOW() - INTERVAL '30 days'), 0),
        EXTRACT(DAY FROM NOW() - MAX(ft.transaction_date))::INTEGER,
        COALESCE(COUNT(ft.id) FILTER (WHERE EXTRACT(DOW FROM ft.transaction_date) IN (0,6) AND ft.transaction_date >= NOW() - INTERVAL '30 days')::DECIMAL / NULLIF(COUNT(ft.id) FILTER (WHERE ft.transaction_date >= NOW() - INTERVAL '30 days'), 0), 0)
    FROM warehouse.dim_customers dc
    LEFT JOIN warehouse.dim_accounts da          ON dc.gcid = da.gcid
    LEFT JOIN warehouse.fact_transactions ft     ON da.id   = ft.account_id
    LEFT JOIN warehouse.fact_risk_assessments fra ON dc.gcid = fra.gcid
    LEFT JOIN warehouse.fact_interactions fi     ON dc.gcid = fi.gcid
    LEFT JOIN warehouse.fact_cases fc            ON dc.gcid = fc.gcid
    LEFT JOIN warehouse.fact_payments fp         ON dc.gcid = fp.gcid
    WHERE dc.gcid = p_gcid
    ON CONFLICT (gcid) DO UPDATE SET
        computed_at = EXCLUDED.computed_at,
        txn_count_7d = EXCLUDED.txn_count_7d,
        txn_count_30d = EXCLUDED.txn_count_30d,
        txn_count_90d = EXCLUDED.txn_count_90d,
        total_spend_30d = EXCLUDED.total_spend_30d,
        total_income_30d = EXCLUDED.total_income_30d,
        avg_txn_amount_30d = EXCLUDED.avg_txn_amount_30d,
        max_single_txn_30d = EXCLUDED.max_single_txn_30d,
        net_cashflow_30d = EXCLUDED.net_cashflow_30d,
        mobile_txn_pct = EXCLUDED.mobile_txn_pct,
        web_txn_pct = EXCLUDED.web_txn_pct,
        ussd_txn_pct = EXCLUDED.ussd_txn_pct,
        atm_txn_pct = EXCLUDED.atm_txn_pct,
        branch_txn_pct = EXCLUDED.branch_txn_pct,
        api_txn_pct = EXCLUDED.api_txn_pct,
        avg_risk_score_30d = EXCLUDED.avg_risk_score_30d,
        high_risk_txn_count_30d = EXCLUDED.high_risk_txn_count_30d,
        fraud_flags_90d = EXCLUDED.fraud_flags_90d,
        account_count = EXCLUDED.account_count,
        total_balance = EXCLUDED.total_balance,
        max_account_balance = EXCLUDED.max_account_balance,
        open_cases_count = EXCLUDED.open_cases_count,
        avg_sentiment_score_90d = EXCLUDED.avg_sentiment_score_90d,
        interaction_count_90d = EXCLUDED.interaction_count_90d,
        ob_payment_count_30d = EXCLUDED.ob_payment_count_30d,
        ob_payment_amount_30d = EXCLUDED.ob_payment_amount_30d,
        days_since_last_txn = EXCLUDED.days_since_last_txn,
        weekend_txn_pct = EXCLUDED.weekend_txn_pct;
END;
$$ LANGUAGE plpgsql;

-- Trigger: Update dim_customers.updated_at automatically
CREATE OR REPLACE FUNCTION warehouse.set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_dim_customers_updated_at
    BEFORE UPDATE ON warehouse.dim_customers
    FOR EACH ROW EXECUTE FUNCTION warehouse.set_updated_at();

CREATE OR REPLACE TRIGGER trg_dim_accounts_updated_at
    BEFORE UPDATE ON warehouse.dim_accounts
    FOR EACH ROW EXECUTE FUNCTION warehouse.set_updated_at();

-- =============================================================================
-- COMMENTS (Documentation)
-- =============================================================================

COMMENT ON SCHEMA staging  IS 'Raw data landed from each source system before transformation';
COMMENT ON SCHEMA warehouse IS 'Core data warehouse: dimension and fact tables';
COMMENT ON SCHEMA analytics IS 'Pre-aggregated views, materialized views, and feature store for BI/ML';
COMMENT ON SCHEMA audit     IS 'ETL sync logs and data lineage tracking';

COMMENT ON TABLE warehouse.dim_customers IS 'Global customer identity - one row per unique customer across all Wekeza systems';
COMMENT ON COLUMN warehouse.dim_customers.gcid IS 'Global Customer ID - the single unified identifier across all Wekeza systems';
COMMENT ON TABLE analytics.customer_360  IS 'Materialized view combining all customer data for instant 360-degree view';
COMMENT ON TABLE analytics.customer_features IS 'Pre-computed ML feature store - refreshed hourly for real-time decisions';
