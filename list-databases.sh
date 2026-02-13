#!/bin/bash

# Quick Database List Script
# Usage: ./list-databases.sh

API_URL="http://localhost:5273"

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║         WEKEZA ENTERPRISE BRAIN - DATABASE LIST           ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Check if API is running
if ! curl -s "${API_URL}/health" > /dev/null 2>&1; then
    echo "⚠️  API is not running. Please start it first:"
    echo "   cd src/WekezaEnterpriseBrain.Api && dotnet run"
    echo ""
    exit 1
fi

# Get database list
RESPONSE=$(curl -s "${API_URL}/api/databaseinventory")

if [ $? -ne 0 ]; then
    echo "❌ Failed to retrieve database list"
    exit 1
fi

# Parse and display
python3 << EOF
import json

data = json.loads('''$RESPONSE''')
total = data['totalDatabasesDiscovered']
connected = data['connectedDatabases']
ready = data['readyToConnect']

print(f"📊 Total Databases: {total}")
print(f"✅ Connected: {connected}")
print(f"🟡 Ready: {ready}")
print("")

# Group by category
from collections import defaultdict
by_category = defaultdict(list)
for db in data['databases']:
    by_category[db['category']].append(db)

# Categories in order
categories = [
    'Core Banking',
    'Channels', 
    'Security & Risk',
    'Integration',
    'Analytics',
    'Support'
]

for category in categories:
    dbs = by_category.get(category, [])
    if dbs:
        print(f"\n📁 {category.upper()}")
        print("─" * 60)
        for db in dbs:
            status = '✅' if db['integrationStatus'] == 'Connected' else '🟡'
            print(f"{status} {db['id']:2}. {db['systemName']:<28} → {db['databaseName']}")
EOF

echo ""
echo "─────────────────────────────────────────────────────────────"
echo ""
echo "💡 For detailed information:"
echo "   • Full documentation: cat DATABASE_INVENTORY.md"
echo "   • Quick reference: cat DATABASES_LIST.md"
echo "   • Interactive demo: ./demo-database-discovery.sh"
echo ""
