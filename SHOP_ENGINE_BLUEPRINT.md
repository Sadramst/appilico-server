# AppilicoShopServer Shop Engine Blueprint

AppilicoShopServer is the server-side engine for reusable buy-and-sell storefronts. The API already covers the core commerce backbone: authentication, products, categories, brands, carts, orders, payments, subscriptions, vouchers, discounts, offers, inventory, reviews, wishlists, settings, visuals, blog, newsletter, contact, waitlist, and admin dashboard data.

This document tracks the generic-engine direction: every storefront should be configuration plus catalog, content, payments, shipping, policies, and theme data rather than a one-off hard-coded site.

## Engine Principles

- Keep the API generic: no client-specific UI assumptions in domain entities, DTOs, or services.
- Treat each storefront as configuration plus catalog, content, payments, shipping, policies, and theme tokens.
- Keep public shopper APIs stable under `/api` and admin APIs role-protected with `Admin` or `Manager` roles.
- Prefer additive API changes over breaking response shapes because clients should upgrade safely.
- Keep all protected APIs behind JWT bearer auth and maintain the standard `ApiResponse<T>` envelope.
- Design for single-store mode first, but expose store-context hints so future multi-store hosting can be added without rewriting clients.

## Completed Generic Foundation

- Project, solution, namespaces, docs, and tests have been renamed to `AppilicoShopServer`.
- `GET /api/storefront/config` is public and returns the reusable client bootstrap contract.
- `GET /api/storefront/theme` is public and returns standalone theme tokens.
- The bootstrap contract includes engine identity, storefront key/mode, brand profile, locale, theme, capabilities, endpoint registry, navigation links, checkout policy references, auth roles, store context hints, SEO metadata, and generation time.
- Storefront options are configurable from the `Storefront` configuration section, which means future stores can change branding, theme, links, locale, feature flags, and policy paths without client code changes.
- Unit and integration tests cover the storefront service contract and anonymous public endpoints.
- Production deployment is verified at `https://api.appilico.com`, and the deployed storefront config now uses production values.

## Existing Engine Surface

- Catalog: products, variants, SKUs, brands, hierarchical categories, images, featured products, filtering, and sorting.
- Commerce: carts, order creation, order status history, payment records, Stripe PaymentIntents, refunds, discounts, vouchers, and special offers.
- Customer: registration, login, profile, addresses, wishlist, reviews, loyalty points, membership tiers, and customer order history.
- Operations: inventory adjustments, low-stock alerts, dashboard summaries, audit logs, health checks, Swagger, Docker, CI, rate limiting, CORS, and correlation IDs.
- Content and engagement: settings, blog posts, visuals/downloads, newsletter subscriptions, waitlist, and contact messages.

## Public Client Contract

A generic storefront should bootstrap from these endpoints first:

- `GET /health/live`, `GET /health/ready`, `GET /health`
- `GET /api/storefront/config`
- `GET /api/storefront/theme`

The config endpoint is the client runtime source of truth for:

- Store identity: name, tagline, logo, favicon, support contacts, timezone, social links, legal links.
- Locale: default locale, supported locales, currency, country.
- Theme: preset, layout, product card style, color tokens, typography tokens, spacing tokens, homepage sections, product card fields.
- Capabilities: catalog, cart, orders, payments, discounts, vouchers, wishlist, reviews, subscriptions, content, newsletter, visuals, store context headers.
- API registry: stable endpoint IDs, methods, paths, auth requirements, use cases.
- Navigation: slots and route links.
- Checkout: guest checkout, endpoint IDs, shipping/tax strategy keys, returns/terms/privacy URLs.
- Auth: token scheme, customer role, privileged roles.
- Future multi-store context: default storefront key, header name, resolution strategy, multi-store support flag.
- SEO: default title, description, keywords.

Core shopper dependencies remain:

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh-token`
- `GET /api/auth/profile`, profile update endpoints where available
- `GET /api/products`, `GET /api/products/{id}`, `GET /api/products/featured`, `GET /api/products/sku/{sku}`
- `GET /api/categories`, `GET /api/categories/tree`, `GET /api/brands`
- `GET /api/cart`, `POST /api/cart/items`, cart update/remove/clear endpoints where available
- `POST /api/orders`, `GET /api/orders/my`, order detail/cancel endpoints where available
- `POST /api/payments`, Stripe confirmation/webhook aware payment flows, refunds where role-allowed
- `GET /api/reviews/product/{productId}`, customer review create/update flows
- `GET /api/wishlist`, add/remove wishlist item flows
- `POST /api/newsletter/subscribe`, `POST /api/contact`

All responses use the existing `ApiResponse<T>` envelope with `success`, `message`, `data`, `pagination`, `errors`, and `timestamp`.

## Generic Engine Work To Build Next

1. Persistent store profile and theme admin
   - Move editable storefront fields from static configuration into typed settings or a `StoreProfile` aggregate.
   - Add admin APIs for store name, logo, favicon, support contacts, social links, legal links, SEO, theme tokens, homepage sections, and navigation.

2. Multi-store persistence
   - Add `StoreId` or tenant scope to catalog, orders, customers, carts, settings, payments, content, assets, and audit records when true multi-store hosting is required.
   - Resolve store context from host/domain, `X-Storefront-Key`, or path while keeping single-store mode as the default.

3. Checkout policy APIs
   - Add typed shipping methods, tax rules, return policy, payment options, fulfillment rules, and stock reservation behavior.
   - Expose public policy endpoints so clients can render checkout without hard-coded business rules.

4. Page and content builder APIs
   - Model reusable page sections, page slots, media references, SEO metadata, publish states, and scheduled content.
   - Keep clients thin: they should render configured sections over stable data contracts.

5. App/plugin extension points
   - Model feature flags, integrations, webhooks, external events, provider-specific settings, and app lifecycle hooks.
   - Keep provider-specific details behind business services, not in controllers or client DTOs.

6. Admin operations hardening
   - Expand admin endpoints for theme editor, navigation, pages, media, SEO, shipping, tax, payment provider settings, and storefront publishing.
   - Keep every admin mutation audited.

## Client Implementation Rule

The client should become a generic renderer over `GET /api/storefront/config`, `GET /api/storefront/theme`, and the endpoint registry returned by the server. Brand text, navigation, theme, feature flags, legal links, homepage section order, locale, currency, and SEO should be data-driven. Hard-coded Appilico content belongs only in development fallbacks or docs.

## Roadmap

1. Keep the current config-driven single-store contract stable.
2. Update Appilico client into `AppilicoShopClient` using the generated client prompt.
3. Add persistent admin-managed storefront settings.
4. Add checkout policy APIs for shipping, tax, returns, and fulfillment rules.
5. Add multi-store database scoping and request context resolution.
6. Add page/theme builder APIs and plugin extension points.
