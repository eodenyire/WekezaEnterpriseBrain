#!/bin/bash

# Wekeza Enterprise Brain API Demo Script
# This script demonstrates the key features of the Enterprise Brain API

set -e

API_URL="http://localhost:5273"
echo "=================================================="
echo "Wekeza Enterprise Brain - API Demo"
echo "=================================================="
echo ""

# 1. Health Check
echo "1. Testing Health Check..."
curl -s "${API_URL}/health" | python3 -m json.tool
echo ""
echo ""

# 2. Identity Resolution
echo "2. Testing Identity Resolution (Create new GCID)..."
GCID_RESPONSE=$(curl -s "${API_URL}/api/customer/resolve?nationalId=987654321&phone=%2B254712345678&email=john.doe@example.com")
echo "$GCID_RESPONSE" | python3 -m json.tool
GCID=$(echo "$GCID_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")
echo ""
echo "Created GCID: $GCID"
echo ""
echo ""

# 3. Try to resolve the same identity again (should return same GCID)
echo "3. Testing Identity Resolution (Should return existing GCID)..."
curl -s "${API_URL}/api/customer/resolve?nationalId=987654321" | python3 -m json.tool
echo ""
echo ""

# 4. Test Decision Engine - Customer not found scenario
echo "4. Testing Decision Engine - Customer Not Found..."
curl -s -X POST "${API_URL}/api/decision" \
  -H "Content-Type: application/json" \
  -d "{
    \"globalCustomerId\": \"${GCID}\",
    \"eventType\": \"PAYMENT\",
    \"amount\": 5000,
    \"channel\": \"Mobile\"
  }" | python3 -m json.tool
echo ""
echo ""

# 5. Test Risk Score Calculation
echo "5. Testing Risk Score Calculation..."
curl -s "${API_URL}/api/decision/risk-score/${GCID}?eventType=PAYMENT" | python3 -m json.tool
echo ""
echo ""

# 6. Test Customer Not Found
echo "6. Testing Customer Retrieval - Not Found..."
curl -s "${API_URL}/api/customer/${GCID}" | python3 -m json.tool
echo ""
echo ""

echo "=================================================="
echo "Demo Complete!"
echo "=================================================="
echo ""
echo "Key Endpoints Demonstrated:"
echo "  ✓ Health Check"
echo "  ✓ Identity Resolution (GCID creation)"
echo "  ✓ Identity Resolution (GCID retrieval)"
echo "  ✓ Decision Engine"
echo "  ✓ Risk Score Calculation"
echo "  ✓ Customer Retrieval"
echo ""
echo "Note: This is a POC with in-memory storage."
echo "Customer 360 profiles need to be created separately for full functionality."
