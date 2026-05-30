# Super Prompt For The Appilico Client

Use this prompt in the client repository: `https://github.com/Sadramst/appilico-client`.

```text
You are GitHub Copilot working in the Appilico client repository. Transform this client into a generic, reusable storefront for AppilicoShopServer, a .NET 8 buy-and-sell shop engine. The goal is not a one-off Appilico-branded website; the goal is a configurable storefront that can become a shop app for any future business by changing server data, settings, theme tokens, assets, and environment variables.

Server source of truth
- Server name: AppilicoShopServer.
- API base URL must come from environment, for example NEXT_PUBLIC_API_BASE_URL or VITE_API_BASE_URL depending on the framework.
- Production API may be https://api.appilico.com/api, but never hard-code it outside environment defaults/docs.
- Swagger is available at /swagger/index.html when enabled.
- Health endpoints: GET /health/live, GET /health/ready, GET /health.
- All API responses use an envelope:
  {
    success: boolean,
    message: string,
    data: T | null,
    pagination: { currentPage, pageSize, totalCount, totalPages, hasPrevious, hasNext } | null,
    errors: string[],
    timestamp: string
  }

Core API areas to support
- Auth: register, login, refresh, revoke, get/update profile, forgot/reset password.
- Catalog: products, featured products, product by id, product by sku, categories, category tree, brands.
- Product detail: images, variants, SKU, price, stock, category, brand, reviews, related/featured products where available.
- Cart: get cart, add item, update item quantity, remove item, clear cart, anonymous session cart where supported.
- Checkout/orders: create order from cart, my orders, order detail, order status history, cancel order.
- Payments: create payment or payment intent, support Stripe client confirmation when a clientSecret is returned, show pending/paid/failed states.
- Promotions: discounts, vouchers, special offers, validation/redemption flows where exposed.
- Customer: profile, addresses, loyalty/membership display if returned, wishlist, reviews.
- Content and engagement: settings, blog, visuals if still enabled, newsletter subscribe, contact form, waitlist if enabled.
- Admin/manager: products, categories, brands, inventory, orders, discounts, vouchers, offers, reviews approval, dashboard, settings. Protect these behind role checks.

Architecture requirements
1. Build a typed API client layer.
   - Create one fetch wrapper that handles base URL, JSON, auth headers, refresh-token retry, response envelope unwrapping, and normalized errors.
   - Never scatter raw fetch calls through UI components.
   - Define TypeScript types for ApiResponse<T>, Pagination, Product, ProductVariant, ProductImage, Category, Brand, Cart, CartItem, Order, Payment, Review, WishlistItem, AppSetting, User, AuthResponse, Dashboard summaries, and request DTOs.
   - Keep endpoint paths in a single API module.

2. Make the storefront generic.
   - Replace hard-coded Appilico brand text with data from server settings or a local fallback config.
   - Create a StorefrontConfig model with store name, logo, favicon, tagline, support email, phone, social links, currency, locale, nav links, footer links, theme tokens, feature flags, and legal links.
   - Until the server has a dedicated /api/storefront/config endpoint, load settings from the existing settings endpoint when authorized/available and fall back to a local config file.
   - Product/category/brand content must come from APIs, not local arrays.

3. Build expected shop pages and states.
   - Home: configurable sections, featured products, categories, offers, newsletter.
   - Catalog/search: product grid, filters, sort, pagination, empty states, loading skeletons, retry state.
   - Product detail: gallery, variants, price, stock status, quantity, add to cart, reviews, wishlist.
   - Cart: line items, quantity edits, remove, voucher/discount entry if API supports it, totals, checkout CTA.
   - Checkout: address selection/input, shipping/payment placeholders from config, order summary, Stripe payment confirmation if clientSecret is returned, success/failure states.
   - Account: profile, addresses, orders, order detail, wishlist, reviews.
   - Auth: login, register, forgot/reset password, token persistence, logout.
   - Admin dashboard: role-gated sections for catalog, inventory, orders, promotions, customers, reviews, settings, and dashboard metrics.

4. Handle auth and roles properly.
   - Store access token safely for the app architecture and keep refresh-token handling centralized.
   - Attach Authorization: Bearer <token> for protected endpoints.
   - Decode or use profile roles to show Admin/Manager/Customer experiences.
   - Never expose admin actions to non-privileged users in navigation.
   - On 401, attempt refresh once, then logout and redirect to login.

5. Make data fetching production-grade.
   - Use the project’s existing data-fetching library if present. If none exists, choose a simple consistent pattern.
   - Cache public catalog data reasonably, invalidate cart/orders/profile after mutations.
   - Every mutation must show loading, success, and error feedback.
   - Use URL query params for catalog search/filter/sort/page so pages are shareable.
   - Keep API errors user-friendly while preserving details in console/dev tools only.

6. Design quality requirements.
   - Build the actual shop experience as the first screen, not a marketing landing page.
   - Keep the UI clean, responsive, and suitable for repeated shopping/admin workflows.
   - Use existing design system/components if the repo has them.
   - Use icons from the existing icon library or lucide-react if available.
   - Do not hard-code a single color theme; consume theme tokens from config with professional fallbacks.
   - Ensure mobile layout has no text overlap, no overflowing buttons, and usable cart/checkout flows.

7. Environment and configuration.
   - Add or update .env.example with API base URL and any payment public key variables.
   - Document local dev startup and how to point the client to local AppilicoShopServer.
   - Keep production URLs configurable for Vercel or the chosen host.

8. Testing and verification.
   - Add focused tests for the API client envelope handling, auth refresh behavior, and key page rendering if the test stack exists.
   - Run lint/typecheck/build.
   - If a dev server is needed, start it and verify the primary shop flows in browser: home, catalog, product detail, cart, login/register, checkout skeleton, account, admin route guard.

Implementation order
1. Inspect the current client framework, routing, state management, API utilities, env setup, and UI system.
2. Create the typed AppilicoShopServer API client and DTO types.
3. Replace hard-coded content sources with API-backed loaders and generic storefront config.
4. Implement/repair shopper flows: home, catalog, product detail, cart, auth, account, checkout.
5. Implement/repair role-gated admin flows using existing endpoints.
6. Add robust loading/error/empty states and responsive polish.
7. Update docs/env examples.
8. Run verification and report exactly what works, what is mocked/fallback, and what server endpoints are still needed.

Important constraints
- Do not invent server endpoints silently. If an endpoint is missing, create a typed placeholder/fallback and document the exact AppilicoShopServer endpoint needed.
- Do not break existing routes unless replacing them with a better generic route structure and redirects.
- Do not hard-code Appilico-specific copy, categories, products, prices, images, emails, or social links in reusable components.
- Keep the client ready for future multi-store support by passing store config/context through layout and API layers.
- Preserve any existing user work in the repo; do not reset or delete unrelated changes.

Final deliverable
- A working generic storefront client connected to AppilicoShopServer.
- A concise summary of changed files.
- The exact API base URL configuration used.
- Verification commands and results.
- A list of server endpoints/config gaps that should be added next, especially /api/storefront/config, /api/storefront/theme, shipping/tax policies, and multi-store tenant resolution.
```
