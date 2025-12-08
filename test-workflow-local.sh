#!/bin/bash
# Bash script to manually test the GitHub Actions workflow locally
# This simulates the steps in .github/workflows/test.yml

set -e  # Exit on any error

echo "=== Testing GitHub Actions Workflow Locally ==="

# Step 1: Restore dependencies
echo ""
echo "[1/8] Restoring .NET dependencies..."
dotnet restore

# Step 2: Build solution
echo ""
echo "[2/8] Building solution..."
dotnet build --no-restore

# Step 3: Install npm dependencies
echo ""
echo "[3/8] Installing npm dependencies..."
npm ci

# Step 4: Install Playwright browsers
echo ""
echo "[4/8] Installing Playwright browsers..."
npx playwright install --with-deps chromium

# Step 5: Run unit tests
echo ""
echo "[5/8] Running unit tests..."
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName!~IntegrationTests&FullyQualifiedName!~PlaywrightTests"

# Step 6: Run integration tests
echo ""
echo "[6/8] Running integration tests..."
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName~IntegrationTests"

# Step 7: Run Playwright tests
echo ""
echo "[7/8] Running Playwright tests..."
npm test

# Step 8: Run all .NET tests summary
echo ""
echo "[8/8] Running all .NET tests (summary)..."
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName!~PlaywrightTests" --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage"

echo ""
echo "=== All workflow steps completed successfully! ==="

