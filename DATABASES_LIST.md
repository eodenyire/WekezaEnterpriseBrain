# Quick Database List

## All 15 Discovered Databases

### Core Banking (5 databases)
1. **ComprehensiveWekezaApi** → CoreBanking (PostgreSQL) ✅ Connected
2. **DatabaseWekezaApi** → DatabaseWekezaApi_DB (PostgreSQL) 🟡 Ready
3. **EnhancedWekezaApi** → EnhancedBanking (PostgreSQL) 🟡 Ready
4. **MinimalWekezaApi** → MinimalBanking (PostgreSQL) 🟡 Ready
5. **Wekeza.Core.Api** → WekeazCore (PostgreSQL) 🟡 Ready

### Channels (3 databases)
6. **Mobile Banking** → MobileBanking (PostgreSQL) ✅ Connected
7. **Web Banking** → WebBanking (PostgreSQL) 🟡 Ready
8. **USSD Banking** → USSD_Banking (PostgreSQL) 🟡 Ready

### Security & Risk (2 databases)
9. **Fraud Detection** → FraudDetection (PostgreSQL) ✅ Connected
10. **ERMS** → RiskManagement (PostgreSQL) 🟡 Ready

### Integration (2 databases)
11. **Nexus (Open Banking)** → OpenBanking (PostgreSQL) 🟡 Ready
12. **AI Copilot** → AICopilot (PostgreSQL) 🟡 Ready

### Analytics & Support (3 databases)
13. **Analytics/BI** → BI_DataWarehouse (PostgreSQL) 🟡 Ready
14. **Audit Logs** → AuditLogs (PostgreSQL) 🟡 Ready
15. **Reporting** → Reporting (PostgreSQL) 🟡 Ready

---

## Summary

- **Total**: 15 databases
- **Connected**: 3 (20%)
- **Ready**: 12 (80%)
- **Technology**: PostgreSQL (100%)

## API Access

```bash
# Get full list with details
curl http://localhost:5273/api/databaseinventory

# Get summary
curl http://localhost:5273/api/databaseinventory/summary

# Get categories
curl http://localhost:5273/api/databaseinventory/categories
```

## CLI Command

```bash
# List all databases
./demo-database-discovery.sh

# Or use this quick command
curl -s http://localhost:5273/api/databaseinventory | \
  python3 -c "import sys,json; [print(f'{db[\"id\"]}. {db[\"systemName\"]}: {db[\"databaseName\"]}') for db in json.load(sys.stdin)['databases']]"
```

---

**Legend:**
- ✅ = Currently connected to Enterprise Brain
- 🟡 = Ready to connect (connector architecture in place)

For complete details, see [DATABASE_INVENTORY.md](DATABASE_INVENTORY.md)
