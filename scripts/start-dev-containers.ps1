# Start development containers if they're not running
# Run from repository root: pwsh ./scripts/start-dev-containers.ps1

Set-StrictMode -Version Latest

function Write-Err { Write-Host "[ERROR]" $args -ForegroundColor Red }
function Write-Ok { Write-Host "[OK]" $args -ForegroundColor Green }
function Write-Info { Write-Host "[INFO]" $args -ForegroundColor Cyan }

try {
    docker --version > $null 2>&1
} catch {
    Write-Err "Docker CLI not found. Install Docker Desktop and ensure 'docker' is on PATH."
    exit 1
}

try {
    docker info > $null 2>&1
} catch {
    Write-Err "Docker daemon not available. Is Docker Desktop running?"
    exit 1
}

$composeFile = "docker-compose.yml"
if (-not (Test-Path $composeFile)) {
    Write-Err "docker-compose.yml not found in current directory. Run from repository root."
    exit 1
}

Write-Info "Starting containers using docker compose (idempotent)..."
$start = docker compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Err "Failed to start compose services. See Docker output above."
    exit $LASTEXITCODE
}

Write-Ok "Compose services started or already running."

# Optional: show containers of interest
$services = @('rabbitmq','redis','redisinsight','mailhog')
foreach ($s in $services) {
    $row = docker ps --filter "name=$s" --format "{{.Names}}\t{{.Status}}"
    if ([string]::IsNullOrWhiteSpace($row)) {
        Write-Err "Service '$s' is not running"
    } else {
        Write-Ok "$row"
    }
}

Write-Info "Access UIs:"
Write-Host " - RabbitMQ UI: http://localhost:15672"
Write-Host " - RedisInsight UI: http://localhost:5540"
Write-Host " - MailHog UI: http://localhost:8025"

Write-Info "You can run this script from a pre-launch task in VS Code or invoke it manually before starting the Blazor app."
