# PowerShell script to manually test the GitHub Actions workflow locally
# This simulates the steps in .github/workflows/test.yml

Write-Host "=== Testing GitHub Actions Workflow Locally ===" -ForegroundColor Green

# Step 1: Restore dependencies
Write-Host "`n[1/8] Restoring .NET dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 2: Build solution
Write-Host "`n[2/8] Building solution..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 3: Install npm dependencies
Write-Host "`n[3/8] Installing npm dependencies..." -ForegroundColor Yellow
npm ci
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 4: Install Playwright browsers
Write-Host "`n[4/8] Installing Playwright browsers..." -ForegroundColor Yellow
npx playwright install --with-deps chromium
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 5: Run unit tests
Write-Host "`n[5/8] Running unit tests..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName!~IntegrationTests&FullyQualifiedName!~PlaywrightTests"
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 6: Run integration tests
Write-Host "`n[6/8] Running integration tests..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName~IntegrationTests"
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 7: Run Playwright tests
Write-Host "`n[7/8] Running Playwright tests..." -ForegroundColor Yellow
npm test
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

# Step 8: Run all .NET tests summary
Write-Host "`n[8/8] Running all .NET tests (summary)..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal --filter "FullyQualifiedName!~PlaywrightTests" --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0) { Write-Host "FAILED" -ForegroundColor Red; exit 1 }

Write-Host "`n=== All workflow steps completed successfully! ===" -ForegroundColor Green

