# Phase 1 Foundation Architecture Notes

## Azure Service Choices
- **Azure Container Apps**: Host for AnimStudio.API (modular monolith .NET 8).
  - Internal ingress only with Private Link via Azure Front Door.
  - Autoscale configured using KEDA HTTP-based scaler.
  - System-assigned managed identity for secure access to Azure Key Vault and ACR.

- **Azure SQL**: Serverless General Purpose (dev environment); General Purpose zone-redundant (production).
  - Segmented schemas: identity.*, content.*, delivery.*, analytics.*.
  - Defender for SQL enabled in production.
  - Separate scalable database for Hangfire storage.

- **Azure Cache for Redis**: Application-level distributed cache.
  - Basic SKU for development environments.
  - Geo-replicated Standard SKU private endpoint for production.
  - Managed resilience policies implemented with Polly.

- **Azure Front Door Premium**: Global SSL termination, WAF with rate limiting, DDoS protection.
  - Allows routing traffic to /api/ (Container App), / (SSR frontend), /assets/, and WebSockets routes.
  - WAF custom rules include SQL injection and XSS protection.

- **Azure Key Vault**: Secrets, keys, and configuration storage.
  - RBAC tightly scoped containers (e.g. SQL connection strings, Stripe API keys, Redis access tokens).
  - Soft delete and purge protection enabled.

- **Azure SignalR Service**: SignalR backend for real-time APIs like progress hubs.

- **Azure Service Bus**:
  - Queue-based for background jobs and event-based pipelines with maxDelivery retries and DLQ monitoring.

- **Azure Application Insights**:
  - Metrics exporter for OpenTelemetry.
  - JSON-based Serilog instrumentation.

## Authentication Flow
- Microsoft Entra External ID as OAuth authority using JWT-based tokens.
  - Corresponding claims injected via TokenValidated middleware:
    - `sub -> ExternalId`
    - `email`
    - Identified subscription-tier custom claim (`tier -> Basic/Pro/Studio`).

- Package integration:
  - Next-auth for Next.js frontend asynchronous handling.

## Stripe Integration Patterns
- **Stripe Service** (Daily Plan, Portal): Core monetization billing through required user checkout.
- **Checkout session creation**: Guarantees subscription activation → Domain triggers.
  - Ongoing handling retries (Event Listener).

## Key Design Notes

- MediatR orchestration role splits Events, Notification handlers (broadcast).
- Version-aware API surfaces interact via strict rate limits.
...
