# Quick Database List

## All 15 Databases - 100% Connected! ✅

### Core Banking (5 databases)
1. **ComprehensiveWekezaApi** → CoreBanking (PostgreSQL) ✅ Connected
2. **DatabaseWekezaApi** → DatabaseWekezaApi_DB (PostgreSQL) ✅ Connected
3. **EnhancedWekezaApi** → EnhancedBanking (PostgreSQL) ✅ Connected
4. **MinimalWekezaApi** → MinimalBanking (PostgreSQL) ✅ Connected
5. **Wekeza.Core.Api** → WekeazCore (PostgreSQL) ✅ Connected

### Channels (3 databases)
6. **Mobile Banking** → MobileBanking (PostgreSQL) ✅ Connected
7. **Web Banking** → WebBanking (PostgreSQL) ✅ Connected
8. **USSD Banking** → USSD_Banking (PostgreSQL) ✅ Connected

### Security & Risk (2 databases)
9. **Fraud Detection** → FraudDetection (PostgreSQL) ✅ Connected
10. **ERMS** → RiskManagement (PostgreSQL) ✅ Connected

### Integration (2 databases)
11. **Nexus (Open Banking)** → OpenBanking (PostgreSQL) ✅ Connected
12. **AI Copilot** → AICopilot (PostgreSQL) ✅ Connected

### Analytics & Support (3 databases)
13. **Analytics/BI** → BI_DataWarehouse (PostgreSQL) ✅ Connected
14. **Audit Logs** → AuditLogs (PostgreSQL) ✅ Connected
15. **Reporting** → Reporting (PostgreSQL) ✅ Connected

---

## Summary

- **Total**: 15 databases
- **Connected**: 15 (100%) ✅
- **Ready**: 0 (0%)
- **Technology**: PostgreSQL (100%)

**🎉 Integration Complete - All databases connected!**

---

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
./list-databases.sh

# Or use this quick command
curl -s http://localhost:5273/api/databaseinventory | \
  python3 -c "import sys,json; [print(f'{db[\"id\"]}. {db[\"systemName\"]}: {db[\"databaseName\"]}') for db in json.load(sys.stdin)['databases']]"
```

---

**Legend:**
- ✅ = Connected to Enterprise Brain

For complete details, see [DATABASE_INVENTORY.md](DATABASE_INVENTORY.md)
