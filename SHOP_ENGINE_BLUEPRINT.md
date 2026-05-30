# AppilicoShopServer Shop Engine Blueprint

AppilicoShopServer is the server-side engine for reusable buy-and-sell storefronts. The current API already covers the core commerce backbone: authentication, products, categories, brands, carts, orders, payments, subscriptions, vouchers, discounts, offers, inventory, reviews, wishlists, settings, visuals, blog, newsletter, contact, waitlist, and admin dashboard data.

This document defines the direction for turning the existing e-commerce backend into a generic shop engine that can power different storefront clients from the same server contract.

## Engine Principles

- Keep the API generic: no client-specific UI assumptions in domain entities, DTOs, or services.
- Treat each storefront as configuration plus catalog, content, payments, shipping, and policies.
- Keep public shopper APIs stable under `/api` and admin APIs role-protected with `Admin` or `Manager` roles.
- Use `Settings` as the first extension point for store identity, theme tokens, currency, locale, tax, shipping, SEO, and checkout copy.
- Prefer additive API changes over breaking response shapes because the client should be able to upgrade safely.
- Keep all protected APIs behind JWT bearer auth and maintain the standard response envelope.

## Existing Engine Surface

- Catalog: products, variants, SKUs, brands, hierarchical categories, images, featured products, filtering, and sorting.
- Commerce: carts, order creation, order status history, payment records, Stripe PaymentIntents, refunds, discounts, vouchers, and special offers.
- Customer: registration, login, profile, addresses, wishlist, reviews, loyalty points, membership tiers, and customer order history.
- Operations: inventory adjustments, low-stock alerts, dashboard summaries, audit logs, health checks, Swagger, Docker, CI, rate limiting, CORS, and correlation IDs.
- Content and engagement: settings, blog posts, visuals/downloads, newsletter subscriptions, waitlist, and contact messages.

## Generic Engine Gaps To Build Next

1. Store profile and branding
   - Add a `StoreProfile` or typed settings facade for name, logo, favicon, contact details, social links, support email, legal links, timezone, currency, locale, and default country.
   - Expose a public `GET /api/storefront/config` endpoint that returns all client bootstrap data in one call.

2. Theme and layout configuration
   - Store theme tokens such as colors, typography scale, button radius, product-card style, navigation layout, footer columns, and homepage sections.
   - Expose `GET /api/storefront/theme` and keep values generic enough for any client framework.

3. Multi-store readiness
   - Add `StoreId`/tenant scope to catalog, orders, customers, carts, settings, payments, content, and assets when true multi-store hosting is required.
   - Resolve store context from host/domain, header, or path while keeping single-store mode as the default.

4. Checkout policies
   - Add typed shipping methods, tax rules, return policy, payment options, fulfillment rules, and stock reservation behavior.
   - Expose public policy endpoints so the client can render checkout without hard-coded business rules.

5. Admin engine APIs
   - Expand admin endpoints for store setup, theme editor, navigation, pages, media, SEO, shipping, tax, payment provider settings, and storefront publishing.
   - Keep every admin mutation audited.

6. App/plugin style extension points
   - Model feature flags, integrations, webhooks, external events, and provider-specific settings.
   - Keep provider-specific details behind business services, not in controllers or client DTOs.

7. Client bootstrap contract
   - Create one public bootstrap endpoint that returns store profile, theme, navigation, homepage sections, cart summary, auth state hint, currency, feature flags, and enabled integrations.
   - This lets any future storefront become a thin generic renderer over the engine contract.

## Recommended Public Client Contract

The client should treat these as core server dependencies:

- `GET /health/live`, `GET /health/ready`, `GET /health`
- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/revoke`
- `GET /api/auth/profile`, `PUT /api/auth/profile`
- `GET /api/products`, `GET /api/products/{id}`, `GET /api/products/featured`, `GET /api/products/sku/{sku}`
- `GET /api/categories`, `GET /api/categories/tree`, `GET /api/brands`
- `GET /api/cart`, `POST /api/cart/items`, `PUT /api/cart/items/{id}`, `DELETE /api/cart/items/{id}`, `DELETE /api/cart`
- `POST /api/orders`, `GET /api/orders/my`, `GET /api/orders/{id}`, `POST /api/orders/{id}/cancel`
- `POST /api/payments`, Stripe confirmation/webhook aware payment flows, refunds where role-allowed
- `GET /api/reviews/product/{productId}`, customer review create/update flows
- `GET /api/wishlist`, add/remove wishlist item flows
- `GET /api/settings` or typed future storefront settings endpoints
- `POST /api/newsletter/subscribe`, `POST /api/contact`

All responses use the existing `ApiResponse<T>` envelope with `success`, `message`, `data`, `pagination`, `errors`, and `timestamp`.

## Implementation Roadmap

1. Finish the AppilicoShopServer rename and verification.
2. Add typed storefront bootstrap DTOs and a public `StorefrontController`.
3. Refactor settings into typed groups: store, theme, navigation, checkout, shipping, tax, SEO, integrations, and feature flags.
4. Add tests for the bootstrap endpoint, settings validation, and client-safe response shapes.
5. Update the client to consume only API/bootstrap configuration instead of hard-coded store content.
6. Add multi-store scoping after the single-store generic contract is stable.
