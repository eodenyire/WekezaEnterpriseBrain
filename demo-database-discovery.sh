#!/bin/bash

# Wekeza Enterprise Brain - Database Discovery Demo
# Shows all 15 discovered databases across Wekeza systems

set -e

API_URL="http://localhost:5273"

echo "================================================================"
echo "  Wekeza Enterprise Brain - Database Discovery Demo"
echo "================================================================"
echo ""
echo "Answering the question: How many databases have you discovered?"
echo ""
echo "================================================================"
echo ""

# 1. Database Summary
echo "1. Database Discovery Summary"
echo "----------------------------------------"
curl -s "${API_URL}/api/databaseinventory/summary" | python3 -m json.tool
echo ""
echo ""

# 2. Database Categories
echo "2. Database Categories"
echo "----------------------------------------"
curl -s "${API_URL}/api/databaseinventory/categories" | python3 -m json.tool
echo ""
echo ""

# 3. Complete Database Inventory
echo "3. Complete Database Inventory (15 Databases)"
echo "----------------------------------------"
echo ""

INVENTORY=$(curl -s "${API_URL}/api/databaseinventory")

echo "Core Banking Databases (5):"
echo "$INVENTORY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
core_dbs = [db for db in data['databases'] if db['category'] == 'Core Banking']
for db in core_dbs:
    status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
    print(f\"  {status} {db['systemName']}: {db['databaseName']} ({db['technology']})\")
"
echo ""

echo "Channel Databases (3):"
echo "$INVENTORY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
channel_dbs = [db for db in data['databases'] if db['category'] == 'Channels']
for db in channel_dbs:
    status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
    print(f\"  {status} {db['systemName']}: {db['databaseName']} ({db['technology']})\")
"
echo ""

echo "Security & Risk Databases (2):"
echo "$INVENTORY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
risk_dbs = [db for db in data['databases'] if db['category'] == 'Security & Risk']
for db in risk_dbs:
    status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
    print(f\"  {status} {db['systemName']}: {db['databaseName']} ({db['technology']})\")
"
echo ""

echo "Integration Databases (2):"
echo "$INVENTORY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
int_dbs = [db for db in data['databases'] if db['category'] == 'Integration']
for db in int_dbs:
    status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
    print(f\"  {status} {db['systemName']}: {db['databaseName']} ({db['technology']})\")
"
echo ""

echo "Analytics & Support Databases (3):"
echo "$INVENTORY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
analytics_dbs = [db for db in data['databases'] if db['category'] in ['Analytics', 'Support']]
for db in analytics_dbs:
    status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
    print(f\"  {status} {db['systemName']}: {db['databaseName']} ({db['technology']})\")
"
echo ""
echo ""

# 4. Integration Statistics
echo "4. Integration Statistics"
echo "----------------------------------------"
SUMMARY=$(curl -s "${API_URL}/api/databaseinventory/summary")

TOTAL=$(echo "$SUMMARY" | python3 -c "import sys, json; print(json.load(sys.stdin)['totalDatabases'])")
CONNECTED=$(echo "$SUMMARY" | python3 -c "import sys, json; print(json.load(sys.stdin)['connectedDatabases'])")
READY=$(echo "$SUMMARY" | python3 -c "import sys, json; print(json.load(sys.stdin)['readyToConnect'])")

echo "Total Databases Discovered: $TOTAL"
echo "Currently Connected: $CONNECTED"
echo "Ready to Connect: $READY"
echo ""
echo ""

# 5. Technology Stack
echo "5. Database Technology Stack"
echo "----------------------------------------"
echo "$SUMMARY" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print('Technologies in Use:')
for tech, count in data['databasesByTechnology'].items():
    if count > 0:
        print(f\"  • {tech}: {count} database(s)\")
"
echo ""
echo ""

echo "================================================================"
echo "  Database Discovery Complete!"
echo "================================================================"
echo ""
echo "Answer: We have discovered 15 databases across all Wekeza systems"
echo ""
echo "Breakdown by Category:"
echo "  • Core Banking Systems: 5 databases"
echo "  • Channel Systems: 3 databases"
echo "  • Security & Risk: 2 databases"
echo "  • Integration Systems: 2 databases"
echo "  • Analytics & Support: 3 databases"
echo ""
echo "Current Integration Status:"
echo "  ✅ Connected: 3 databases (20%)"
echo "  🟡 Ready to Connect: 12 databases (80%)"
echo ""
echo "Primary Technology: PostgreSQL (15 databases)"
echo ""
echo "For detailed information, see:"
echo "  • DATABASE_INVENTORY.md"
echo "  • API: GET /api/databaseinventory"
echo ""
echo "================================================================"
