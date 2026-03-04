# Pay2Bill — Azure App Service Demo Application

A reference implementation of a simplified bill-payment web application built on **ASP.NET MVC (.NET Framework 4.8)**, designed to demonstrate best practices for hosting on **Azure App Service**.  
This is a **DEMO / EDE session reference** — no real payments are processed and no real Azure services are required to run it.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [Functional Features](#functional-features)
4. [Health Check Endpoint](#health-check-endpoint)
5. [Monitoring & Observability](#monitoring--observability)
6. [Configuration & Secrets](#configuration--secrets)
7. [Azure App Service Mapping](#azure-app-service-mapping)
8. [Running Locally](#running-locally)
9. [Deployment to Azure](#deployment-to-azure)
10. [EDE / Health Check Discussion Points](#ede--health-check-discussion-points)

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                        Azure App Service                         │
│                      (Windows, .NET 4.8)                         │
│                                                                  │
│  ┌─────────────┐   ┌──────────────┐   ┌──────────────────────┐  │
│  │ HomeController│  │BillController│   │  HealthController    │  │
│  │   /          │  │  /Bill       │   │  /health  (JSON)     │  │
│  └─────────────┘  └──────────────┘   └──────────────────────┘  │
│                                                │                  │
│          ┌─────────────────────────────────────┘                  │
│          ▼                                                        │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │                    HealthCheckService                      │   │
│  │   IRedisHealthCheck  │  IServiceBusHealthCheck            │   │
│  │  MockRedisHealthCheck│ MockServiceBusHealthCheck           │   │
│  │  (toggle via config) │ (toggle via config)                 │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │                    Application Insights SDK                │   │
│  │   Exceptions │ Dependencies │ Custom Events │ Metrics      │   │
│  └───────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
         │                    │                     │
         ▼                    ▼                     ▼
  Azure App Service    Azure Monitor /      Azure Key Vault
  Health Check probe   App Insights         (secrets)
```

---

## Project Structure

```
Pay2Bill.sln
└── Pay2Bill/
    ├── App_Start/
    │   ├── RouteConfig.cs          # MVC routes — /health mapped here
    │   └── FilterConfig.cs         # Global exception filter
    ├── Controllers/
    │   ├── HomeController.cs       # Dashboard — unpaid bills summary
    │   ├── BillController.cs       # Pay Bill flow (GET + POST)
    │   └── HealthController.cs     # /health — returns JSON health status
    ├── Models/
    │   ├── Bill.cs                 # Bill entity (BillType enum)
    │   ├── PaymentRequest.cs       # Payment form model (validated)
    │   ├── PaymentResult.cs        # Result returned by payment service
    │   └── HealthCheckResult.cs    # Health check JSON contract
    ├── Services/
    │   ├── IBillService.cs / BillService.cs         # Mock in-memory bills
    │   ├── IPaymentService.cs / PaymentService.cs   # Mock payment processor
    │   ├── IRedisHealthCheck.cs / MockRedisHealthCheck.cs
    │   ├── IServiceBusHealthCheck.cs / MockServiceBusHealthCheck.cs
    │   └── IHealthCheckService.cs / HealthCheckService.cs
    ├── Views/
    │   ├── Home/Index.cshtml        # Dashboard
    │   ├── Bill/Index.cshtml        # Bill list
    │   ├── Bill/Pay.cshtml          # Payment form
    │   ├── Bill/Confirmation.cshtml # Success page
    │   └── Shared/_Layout.cshtml   # Master layout
    ├── Content/site.css
    ├── Web.config                   # App settings + connection strings (heavily commented)
    ├── Global.asax / Global.asax.cs
    └── Pay2Bill.csproj
```

---

## Functional Features

| Feature | Description |
|---|---|
| **Home page** | Shows all bills with summary cards (total, unpaid, amount due) |
| **Pay Bill page** | Card payment form with client/server validation |
| **Confirmation page** | Shows transaction ID and payment summary |
| **Mock data** | 4 in-memory bills (2 Internet, 2 Phone; 1 pre-paid) |
| **Mock payment** | Always succeeds; tracks events to Application Insights |
| **Health endpoint** | `/health` — JSON with per-dependency status |

---

## Health Check Endpoint

### Endpoint

```
GET /health
```

### Sample Response — All Healthy

```json
{
  "overallStatus": "healthy",
  "applicationVersion": "1.0.0.0",
  "timestamp": "2024-11-01T12:00:00Z",
  "environment": "Production",
  "dependencies": [
    {
      "name": "Application",
      "status": "healthy",
      "message": "Application is running.",
      "latencyMs": 0
    },
    {
      "name": "Redis Cache",
      "status": "healthy",
      "message": "Connected to demo-redis.redis.cache.windows.net:6380",
      "latencyMs": 5
    },
    {
      "name": "Service Bus",
      "status": "healthy",
      "message": "Connected to demo-servicebus.servicebus.windows.net",
      "latencyMs": 8
    }
  ]
}
```

### Sample Response — Dependency Failure (HTTP 503)

```json
{
  "overallStatus": "unhealthy",
  "dependencies": [
    { "name": "Application", "status": "healthy", ... },
    { "name": "Redis Cache", "status": "unhealthy", "message": "Unable to connect (simulated failure)", ... },
    { "name": "Service Bus", "status": "healthy", ... }
  ]
}
```

### HTTP Status Codes

| Status | HTTP Code | Meaning |
|---|---|---|
| `healthy` | 200 | All dependencies OK |
| `degraded` | 200 | Non-critical issues |
| `unhealthy` | 503 | One or more dependencies failed |

### Simulating Failures (for demos)

Toggle dependency health via **Azure App Service Application Settings** (no redeploy required):

| Setting | Value | Effect |
|---|---|---|
| `HealthCheck:Redis:IsHealthy` | `false` | Redis reports unhealthy |
| `HealthCheck:ServiceBus:IsHealthy` | `false` | Service Bus reports unhealthy |

---

## Monitoring & Observability

### Application Insights Integration

The app uses the **Application Insights SDK for .NET Framework** (`Microsoft.ApplicationInsights.Web`).

#### Tracked Telemetry

| Telemetry Type | Event/Source | Properties |
|---|---|---|
| **Custom Event** | `PaymentAttempt` | BillId, Amount, CorrelationId |
| **Custom Event** | `PaymentSuccess` | BillId, TransactionId, Amount, CorrelationId |
| **Custom Event** | `PaymentFailure` | BillId, Reason, CorrelationId |
| **Custom Event** | `HealthCheck` | OverallStatus, Redis, ServiceBus |
| **Metric** | `PaymentAmount` | Dollar value of each payment |
| **Exception** | `Application_Error` | Unhandled exceptions via Global.asax |
| **Dependency** | Auto-collected | HTTP dependencies via SDK |
| **Request** | Auto-collected | All HTTP requests |

#### Correlation IDs

Every payment request generates (or propagates) a `X-Correlation-Id` header:

```
X-Correlation-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

This ID is attached to all Application Insights telemetry for that request, enabling end-to-end distributed tracing across services.

---

## Configuration & Secrets

### `Web.config` Structure

```xml
<!-- App Settings → Azure App Service: Configuration > Application settings -->
<appSettings>
  <add key="AppInsights:InstrumentationKey" value="00000000-..." />
  <add key="HealthCheck:Redis:IsHealthy"    value="true" />
  <add key="HealthCheck:ServiceBus:IsHealthy" value="true" />
</appSettings>

<!-- Connection Strings → Azure App Service: Configuration > Connection strings -->
<connectionStrings>
  <add name="DefaultConnection" connectionString="..." />
</connectionStrings>
```

### Azure Key Vault References

In production, replace hardcoded values with Key Vault references:

```xml
<add key="AppInsights:InstrumentationKey"
     value="@Microsoft.KeyVault(VaultName=myVault;SecretName=AppInsightsKey)" />
```

Requirements:
- App Service must have a **System-assigned Managed Identity** enabled
- Key Vault must grant the identity `Secret Get` permission

---

## Azure App Service Mapping

### Feature Alignment

| App Feature | Azure App Service Feature |
|---|---|
| `/health` endpoint | **Monitoring > Health check** — path `/health` |
| App Settings override | **Configuration > Application settings** |
| Connection strings | **Configuration > Connection strings** |
| Secrets | **Key Vault References** + Managed Identity |
| App Insights telemetry | **Application Insights** (auto-instrument or SDK) |
| HTTPS enforcement | **TLS/SSL settings > HTTPS Only = On** |

### Health Check Configuration

1. In the Azure Portal: **App Service > Monitoring > Health check**
2. Set **Path** to `/health`
3. Set **Load balancing threshold** (e.g., 2 unhealthy instances before removing from LB)

**What happens when `/health` returns 503:**
- Azure App Service waits for the configured threshold (default: 2 minutes)
- The unhealthy instance is removed from the load balancer
- A restart is attempted automatically
- Alerts can be configured in Azure Monitor

### Deployment Slots

```
Production  ──►  slot: production   (/health pinged before swap)
                 slot: staging      (pre-warm, validate, then swap)
```

Slot swap health check:
- Azure pings `/health` on the staging slot before completing the swap
- If `/health` returns non-200, the swap is aborted
- This ensures zero-downtime deployments with health validation

### Scaling Considerations

| Scenario | Recommendation |
|---|---|
| Scale out | Ensure `/health` responds within 5s |
| Dependency failure | Return 503 to prevent traffic routing to degraded instance |
| Session state | Use Azure Cache for Redis (stateless app) |
| Minimum instances | Set ≥ 2 to allow health check removal without full outage |

---

## Running Locally

### Prerequisites

- Visual Studio 2022 with ASP.NET and web development workload
- .NET Framework 4.8 SDK (Windows only)
- IIS Express (included with Visual Studio)

### Steps

```powershell
# 1. Clone the repository
git clone https://github.com/dvazqueb11/demo-pay2bill-app.git
cd demo-pay2bill-app

# 2. Restore NuGet packages
nuget restore Pay2Bill.sln

# 3. Build
msbuild Pay2Bill.sln /p:Configuration=Debug

# 4. Run via IIS Express (or open Pay2Bill.sln in Visual Studio and press F5)
```

Navigate to:
- **App:** `http://localhost:<port>/`
- **Health:** `http://localhost:<port>/health`

---

## Deployment to Azure

### GitHub Actions CI/CD

A GitHub Actions workflow can build and deploy this application:

```yaml
name: Deploy to Azure App Service
on:
  push:
    branches: [main]
jobs:
  build-and-deploy:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v2
      - name: Restore NuGet
        run: nuget restore Pay2Bill.sln
      - name: Build
        run: msbuild Pay2Bill.sln /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=AzureAppService
      - name: Deploy
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: .
```

### Required Secrets

| Secret | Description |
|---|---|
| `AZURE_WEBAPP_NAME` | Name of the Azure App Service |
| `AZURE_PUBLISH_PROFILE` | Publish profile XML from Azure Portal |

---

## EDE / Health Check Discussion Points

This application is designed for **Enterprise Design Engagements** and health check conversations with customers. Key talking points:

### 1. What is the `/health` endpoint?
> A single, unified endpoint that aggregates the health of all critical dependencies. Azure App Service polls this endpoint to determine if an instance is healthy enough to serve traffic.

### 2. Why return 503 for unhealthy?
> Azure App Service (and load balancers) interpret any non-2xx response as unhealthy, removing the instance from rotation. Returning 503 explicitly ensures correct behavior with all proxy layers.

### 3. How do you simulate failure without deploying?
> Toggle `HealthCheck:Redis:IsHealthy = false` in Azure App Service **Application Settings** — no code change or redeployment needed. This demonstrates the power of externalized configuration.

### 4. How does this map to real health check best practices?
> - Check all **external** dependencies (Redis, Service Bus, SQL)
> - Keep the check **fast** (< 2 seconds total)
> - Do **not** check internal app logic (only infrastructure)
> - Return **structured JSON** for monitoring systems to parse
> - Alert on sustained unhealthy status (Azure Monitor alerts)

### 5. What about Application Insights?
> Every payment generates a `PaymentAttempt` + `PaymentSuccess` or `PaymentFailure` custom event, enabling:
> - **Failure rate dashboards** — Kusto query on custom events
> - **SLA reporting** — payments processed per hour
> - **Distributed tracing** — follow a payment via Correlation ID across all services

---

*Built as a demo reference for Azure App Service health check and monitoring workshops.*
