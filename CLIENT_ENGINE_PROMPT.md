# Super Prompt For The Appilico Client

Use this prompt in the client repository: `https://github.com/Sadramst/appilico-client`.

```text
You are GitHub Copilot working in the Appilico client repository. Your mission is to turn the client into AppilicoShopClient: a generic, reusable, production-ready storefront and admin client for AppilicoShopServer. This must not become a one-off Appilico-branded website. Build a configurable shop client that can power any future store by reading server config, API data, theme tokens, navigation, policies, assets, and environment variables.

Server source of truth
- Server name: AppilicoShopServer.
- Production API root: https://api.appilico.com.
- API base path: /api.
- API base URL must come from environment, for example NEXT_PUBLIC_API_BASE_URL or VITE_API_BASE_URL depending on the framework.
- Never hard-code production URLs outside .env.example, README, or deployment docs.
- Health endpoints: GET /health/live, GET /health/ready, GET /health.
- Bootstrap endpoint: GET /api/storefront/config.
- Theme endpoint: GET /api/storefront/theme.
- Store context header exposed by the server: X-Storefront-Key. Keep it optional today, but design the API client so it can send it later for multi-store hosting.

Standard API response envelope
All API calls return this envelope. Centralize its handling in one API client wrapper.

{
  success: boolean,
  message: string,
  data: T | null,
  pagination: {
    currentPage: number,
    pageSize: number,
    totalCount: number,
    totalPages: number,
    hasPrevious: boolean,
    hasNext: boolean
  } | null,
  errors: string[],
  timestamp: string
}

Storefront bootstrap contract
On app startup, load /api/storefront/config before rendering the main layout. Treat it as the client runtime contract.

Expected config groups:
- engineName, apiVersion, storefrontKey, storefrontMode, generatedAtUtc
- brand: storeName, tagline, logoUrl, faviconUrl, publicBaseUrl, supportEmail, supportPhone, timeZone, socialLinks, legalLinks
- locale: defaultLocale, supportedLocales, currency, country
- theme: preset, layoutPreset, productCardStyle, colorTokens, typographyTokens, spacingTokens, productCardFields, homepageSections
- capabilities: productCatalog, categoryTree, brands, cart, customerAccounts, orders, payments, discounts, vouchers, wishlist, reviews, subscriptions, blog, newsletter, visuals, storeContextHeaders
- endpoints: stable endpoint IDs, method, path, requiresAuth, useCase
- navigation: slots, categoryTreeEndpointId, links with id, label, path, slot, sortOrder, requiresAuth, requiredRole
- checkout: allowGuestCheckout, cartEndpointId, createOrderEndpointId, createPaymentEndpointId, shippingStrategy, taxStrategy, returnsPolicyUrl, termsUrl, privacyUrl
- auth: tokenScheme, customerRole, privilegedRoles
- context: defaultStorefrontKey, headerName, resolutionStrategy, supportsMultiStore
- seo: defaultTitle, defaultDescription, keywords

Core rule: UI text, brand, navigation, footer links, theme colors, product card fields, homepage section order, legal links, locale, currency, and feature visibility must come from config or API data. Do not hard-code Appilico-specific shop content in reusable components.

Project rename and identity
- Rename package/app metadata, app title defaults, documentation headings, and internal project labels to AppilicoShopClient where appropriate.
- Do not rename the visible shop brand to AppilicoShopClient. Visible brand must come from config.brand.storeName.
- Keep environment variable names framework-appropriate and documented.

Architecture requirements
1. Build a typed API platform layer.
   - Create one fetch wrapper that handles base URL, store context header, JSON, auth headers, response envelope unwrapping, refresh-token retry, and normalized errors.
   - Keep endpoint paths and endpoint IDs in a single API registry. Prefer resolving paths from /api/storefront/config endpoints by ID.
   - Never scatter raw fetch calls through UI components.
   - Define TypeScript types for ApiResponse<T>, Pagination, StorefrontConfig, StorefrontTheme, StorefrontEndpoint, Product, ProductVariant, ProductImage, Category, Brand, Cart, CartItem, Order, Payment, Review, WishlistItem, AppSetting, User, AuthResponse, Dashboard summaries, and request DTOs.
   - API client functions should be domain-oriented: authApi.login, storefrontApi.getConfig, productsApi.search, cartApi.addItem, ordersApi.create, paymentsApi.create, reviewsApi.create, etc.

2. Build a generic storefront runtime.
   - Add a StorefrontProvider or equivalent app-level runtime that loads config, exposes theme tokens, locale, feature flags, navigation, endpoint registry, and store context.
   - Render loading, retry, and safe fallback states if the config endpoint is unavailable.
   - Apply theme tokens via CSS variables or the project design system token layer.
   - Set document title, meta description, favicon, and brand assets from config.
   - Gate features from config.capabilities instead of assuming every module is enabled.

3. Implement shopper flows from API data.
   - Home: render homepageSections from config; support hero, featured products, category rail, offers, newsletter, blog/content sections with graceful fallbacks.
   - Catalog/search: product grid, filters, sort, pagination, query params, empty states, loading skeletons, retry state.
   - Product detail: image gallery, variants, SKU, price, stock status, quantity, add to cart, wishlist, reviews, related/featured products when available.
   - Cart: line items, quantity edits, remove, clear, voucher/discount entry when enabled, totals, checkout CTA.
   - Checkout: address input/selection, shipping/tax policy display from config, order summary, create order, payment creation, Stripe client confirmation if a clientSecret is returned, success/failure states.
   - Account: profile, addresses, my orders, order detail, wishlist, reviews.
   - Auth: login, register, forgot/reset password, token persistence, refresh, logout.

4. Implement role-gated admin/manager flows.
   - Use auth.customerRole and auth.privilegedRoles from config.
   - Protect admin navigation and routes behind role checks.
   - Support dashboard, products, categories, brands, inventory, orders, discounts, vouchers, offers, customers, reviews, settings, and content where endpoints exist.
   - Never expose admin actions to non-privileged users in navigation or direct route rendering.

5. Handle auth correctly.
   - Attach Authorization: Bearer <token> for protected endpoints.
   - On 401, attempt refresh once, replay the original request once, then logout and redirect to login.
   - Keep refresh-token behavior centralized.
   - Store tokens using the safest pattern already used by the project; if none exists, choose one consistent approach and document the tradeoff.

6. Data fetching and mutations
   - Use the project data-fetching library if present. If none exists, add a simple consistent pattern.
   - Cache public storefront config/theme/catalog data reasonably.
   - Invalidate cart/orders/profile after mutations.
   - Every mutation must show loading, success, and error states.
   - User-facing errors should be friendly; detailed API errors should remain available for debugging.

7. Design and UX quality
   - Build the actual shop as the first screen, not a marketing landing page.
   - Use the existing component system if present.
   - Use icons from the existing icon library or lucide-react if available.
   - Keep layouts responsive and dense enough for real shopping/admin workflows.
   - Do not hard-code one color palette. Consume config.theme tokens with professional fallbacks.
   - Make sure mobile layouts have no text overlap, no clipped buttons, and usable cart/checkout flows.

8. Environment and documentation
   - Add or update .env.example with API base URL and payment public key variables.
   - Document local development against AppilicoShopServer and production deployment configuration.
   - Keep production URLs configurable for Vercel or the chosen host.

9. Testing and verification
   - Add focused tests for API envelope handling, config/theme bootstrap, endpoint registry lookup, auth refresh retry, route guards, and key page rendering if the test stack exists.
   - Run lint, typecheck, tests, and build.
   - If a dev server is needed, start it and verify in browser: home, catalog, product detail, cart, login/register, checkout skeleton, account, admin guard.

Implementation order
1. Inspect the current client framework, routing, state management, API utilities, env setup, package metadata, and UI system.
2. Rename client project metadata to AppilicoShopClient while keeping visible store branding config-driven.
3. Create TypeScript DTOs for the AppilicoShopServer response envelope, storefront config, theme, endpoint registry, auth, catalog, cart, checkout, account, admin, and content models.
4. Build the centralized API client with base URL, endpoint registry, store context, auth, refresh retry, and normalized errors.
5. Build StorefrontProvider and bootstrap /api/storefront/config plus /api/storefront/theme.
6. Replace hard-coded brand/navigation/theme/content with config-driven runtime values.
7. Implement or repair shopper pages: home, catalog, product detail, cart, auth, account, checkout.
8. Implement or repair role-gated admin pages using existing endpoints.
9. Add loading, empty, error, retry, offline-ish, and unauthorized states.
10. Update docs/env examples.
11. Run verification and report what is live, what uses fallback config, and what server endpoints should be added next.

Important constraints
- Do not invent server endpoints silently. If an endpoint is missing, create a typed placeholder/fallback and document the exact AppilicoShopServer endpoint needed.
- Prefer resolving endpoint paths from config.endpoints by ID, with typed fallback paths only for resilience.
- Do not break existing routes unless replacing them with a better generic route structure and redirects.
- Do not hard-code Appilico-specific copy, categories, products, prices, images, emails, social links, legal links, or colors in reusable components.
- Keep the client ready for future multi-store support by passing store config/context through layout and API layers.
- Preserve any existing user work in the repo; do not reset or delete unrelated changes.

Final deliverable
- A working AppilicoShopClient generic storefront connected to AppilicoShopServer.
- A concise summary of changed files.
- The exact API base URL configuration used.
- Verification commands and results.
- A list of remaining server gaps, especially persistent storefront admin editing, shipping/tax provider APIs, multi-store tenant persistence, and advanced page/theme builder APIs.
```
