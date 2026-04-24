# Appilico E-Commerce — Complete Server API Manual

> **Base URL (local):** `http://localhost:5034`
> **Base URL (deployed):** `https://appilico-server.onrender.com`
> **All responses** use the standard envelope. **Enums** are serialized as **integers** (not strings).

---

## Table of Contents

1. [Response Envelope](#1-response-envelope)
2. [Authentication & JWT Flow](#2-authentication--jwt-flow)
3. [Auth Endpoints](#3-auth-endpoints)
4. [Products](#4-products)
5. [Categories](#5-categories)
6. [Brands](#6-brands)
7. [Orders](#7-orders)
8. [Cart](#8-cart)
9. [Payments](#9-payments)
10. [Reviews](#10-reviews)
11. [Wishlist](#11-wishlist)
12. [Customers](#12-customers)
13. [Discounts](#13-discounts)
14. [Vouchers](#14-vouchers)
15. [Special Offers](#15-special-offers)
16. [Inventory](#16-inventory)
17. [Dashboard](#17-dashboard)
18. [Settings](#18-settings)
19. [File Upload (Images)](#19-file-upload-images)
20. [Addresses](#20-addresses)
21. [Enums Reference](#21-enums-reference)
22. [Seeded Test Data](#22-seeded-test-data)
23. [Rate Limiting](#23-rate-limiting)
24. [CORS](#24-cors)

---

## 1. Response Envelope

Every endpoint returns this wrapper:

```jsonc
{
  "success": true,                    // bool
  "message": "Success",              // string
  "data": { ... } | [ ... ],        // T — object or array
  "pagination": {                    // null when not paginated
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 74,
    "totalPages": 8,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": [],                      // string[] — populated on failure
  "timestamp": "2026-04-24T08:26:33.792901"  // UTC ISO 8601
}
```

**Error response** (e.g. 400, 401, 404, 500):

```jsonc
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "pagination": null,
  "errors": ["Email is required", "Password must be at least 8 characters"],
  "timestamp": "2026-04-24T..."
}
```

---

## 2. Authentication & JWT Flow

### How it works

1. **Register** → Creates AppUser + Customer record → returns `AuthResponse`
2. **Login** → Validates credentials → returns `AuthResponse` with `accessToken` + `refreshToken`
3. **Use token** → Send `Authorization: Bearer <accessToken>` on every authenticated request
4. **Refresh** → When access token expires, POST the `refreshToken` to get new tokens
5. **Revoke** → Invalidate a refresh token (logout)

### Token details

| Property | Value |
|----------|-------|
| Access token lifetime | 60 minutes (from `appsettings.json`) |
| Refresh token lifetime | 7 days |
| Token type | JWT Bearer |
| Header | `Authorization: Bearer <token>` |

### Roles (3 total)

| Role | Description |
|------|-------------|
| `Admin` | Full access — CRUD everything, delete, manage users |
| `Manager` | Create/update products, categories, brands, orders, inventory, reviews |
| `Customer` | Browse, cart, orders, reviews, wishlist — own data only |

### Password requirements

- Minimum 8 characters
- At least 1 uppercase, 1 lowercase, 1 digit, 1 special character

---

## 3. Auth Endpoints

**Base route:** `/api/auth`

### POST `/api/auth/register`

**Auth:** None

**Request body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "SecureP@ss1",
  "confirmPassword": "SecureP@ss1",
  "phoneNumber": "0412345678"        // optional
}
```

**Response** `200` → `AuthResponse`:

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "dGhpcyBpcyBh...",
    "expiresAt": "2026-04-24T09:26:33Z",
    "user": {
      "id": "user-guid-string",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "avatar": null,
      "roles": ["Customer"]
    }
  }
}
```

### POST `/api/auth/login`

**Auth:** None

**Request body:**

```json
{
  "email": "admin@appilico.com",
  "password": "Admin@123!"
}
```

**Response** `200` → same `AuthResponse` shape as register.

### POST `/api/auth/refresh`

**Auth:** None

**Request body:**

```json
{
  "refreshToken": "dGhpcyBpcyBh..."
}
```

**Response** `200` → `AuthResponse` with new tokens.

### POST `/api/auth/revoke`

**Auth:** `Bearer` (any authenticated user)

**Request body:**

```json
{
  "token": "refresh-token-to-revoke"
}
```

**Response** `200`:

```json
{ "success": true, "message": "Token revoked" }
```

### GET `/api/auth/profile`

**Auth:** `Bearer`

**Response** `200` → `UserDto`:

```json
{
  "success": true,
  "data": {
    "id": "user-guid-string",
    "firstName": "Admin",
    "lastName": "User",
    "email": "admin@appilico.com",
    "avatar": null,
    "roles": ["Admin"]
  }
}
```

### PUT `/api/auth/profile`

**Auth:** `Bearer`

**Request body:**

```json
{
  "firstName": "Updated",
  "lastName": "Name",
  "phoneNumber": "0400000000",      // optional
  "dateOfBirth": "1990-01-15"       // optional, ISO date
}
```

**Response** `200` → updated `UserDto`.

### POST `/api/auth/forgot-password`

**Auth:** None

```json
{ "email": "user@example.com" }
```

### POST `/api/auth/reset-password`

**Auth:** None

```json
{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewP@ssw0rd"
}
```

---

## 4. Products

**Base route:** `/api/products`

### GET `/api/products` — Search/List products

**Auth:** None

**Query parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `searchTerm` | string | null | Search in name/description |
| `categoryId` | Guid | null | Filter by category |
| `brandId` | Guid | null | Filter by brand |
| `minPrice` | decimal | null | Minimum price |
| `maxPrice` | decimal | null | Maximum price |
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 50) |
| `sortBy` | string | null | Sort field (e.g. `name`, `basePrice`, `createdAt`) |
| `sortDescending` | bool | false | Sort direction |

**Response** `200`:

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Scotch Fillet Steak",
      "description": "300g premium grain-fed scotch fillet...",
      "sku": "PM-BS-001",
      "barcode": null,
      "categoryId": "guid",
      "categoryName": "Beef Steaks",
      "brandId": "guid",
      "brandName": "Primo Cuts",
      "basePrice": 32.99,
      "stockQuantity": 40,
      "isActive": true,
      "isFeatured": true,
      "averageRating": 4.5,
      "totalReviews": 3,
      "primaryImageUrl": "https://images.unsplash.com/photo-...",
      "createdAt": "2026-04-24T08:26:33Z",
      "images": [
        {
          "id": "guid",
          "imageUrl": "https://images.unsplash.com/photo-...",
          "altText": "Scotch Fillet Steak",
          "sortOrder": 0,
          "isPrimary": true
        }
      ],
      "variants": [
        {
          "id": "guid",
          "variantName": "Original",
          "sku": "PM-BS-001-ORI",
          "price": 32.99,
          "stockQuantity": 25,
          "attributes": "{\"flavour\":\"Original\"}"
        }
      ]
    }
  ],
  "pagination": { "currentPage": 1, "pageSize": 10, "totalCount": 74, "totalPages": 8, "hasPrevious": false, "hasNext": true }
}
```

### GET `/api/products/{id}` — Get product by ID

**Auth:** None

**Response** `200` → single `ProductDto` (same shape as above).

**Response** `404` → `{ "success": false, "message": "Product not found" }`

### GET `/api/products/sku/{sku}` — Get product by SKU

**Auth:** None

**Response** `200` → single `ProductDto`.

### GET `/api/products/featured` — Get featured products

**Auth:** None

**Query:** `count` (int, default 10)

**Response** `200` → array of `ProductDto`.

### POST `/api/products` — Create product

**Auth:** `Admin` or `Manager`

**Request body:**

```json
{
  "name": "New Product",
  "description": "Description here",    // optional
  "sku": "PM-XX-001",
  "barcode": null,                       // optional
  "categoryId": "guid",
  "brandId": "guid",
  "basePrice": 29.99,
  "costPrice": 15.00,
  "stockQuantity": 100,
  "minStockLevel": 10,
  "weight": 0.5,                         // optional, decimal kg
  "dimensions": null,                    // optional, string
  "isFeatured": false
}
```

**Response** `201` → created `ProductDto`.

### PUT `/api/products/{id}` — Update product

**Auth:** `Admin` or `Manager`

**Request body:**

```json
{
  "name": "Updated Product",
  "description": "Updated description",
  "barcode": null,
  "categoryId": "guid",
  "brandId": "guid",
  "basePrice": 34.99,
  "costPrice": 16.00,
  "stockQuantity": 80,
  "minStockLevel": 10,
  "weight": 0.5,
  "dimensions": null,
  "isActive": true,
  "isFeatured": true
}
```

**Response** `200` → updated `ProductDto`.

### DELETE `/api/products/{id}` — Delete product

**Auth:** `Admin` only

**Response** `200` → `{ "success": true, "message": "Product deleted" }`

### POST `/api/products/{productId}/variants` — Add variant

**Auth:** `Admin` or `Manager`

**Request body:**

```json
{
  "variantName": "Large",
  "sku": "PM-XX-001-LG",
  "price": 39.99,
  "stockQuantity": 20,
  "attributes": "{\"size\":\"large\"}"   // optional, JSON string
}
```

**Response** `201` → `ProductVariantDto`.

---

## 5. Categories

**Base route:** `/api/categories`

### GET `/api/categories` — Get all categories (flat list)

**Auth:** None

**Response** `200` → array of `CategoryDto` (all 25 — top-level + subcategories):

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Beef",
      "description": "Premium cuts of beef",
      "imageUrl": null,
      "parentCategoryId": null,
      "sortOrder": 0,
      "isActive": true,
      "subCategories": [
        {
          "id": "guid",
          "name": "Beef Steaks",
          "description": "Steaks for grilling and pan-frying",
          "imageUrl": null,
          "parentCategoryId": "parent-guid",
          "sortOrder": 0,
          "isActive": true,
          "subCategories": []
        }
      ]
    }
  ]
}
```

### GET `/api/categories/tree` — Get category tree (hierarchical)

**Auth:** None

Returns **only top-level** categories (8) with nested `subCategories`. Same shape.

### GET `/api/categories/{id}` — Get category by ID

**Auth:** None

**Response** `200` → single `CategoryDto`.

### POST `/api/categories` — Create category

**Auth:** `Admin` or `Manager`

```json
{
  "name": "New Category",
  "description": "Description",           // optional
  "parentCategoryId": "guid-or-null",      // optional — null = top-level
  "sortOrder": 0
}
```

**Response** `201` → `CategoryDto`.

### PUT `/api/categories/{id}` — Update category

**Auth:** `Admin` or `Manager`

```json
{
  "name": "Updated Name",
  "description": "Updated desc",
  "parentCategoryId": null,
  "sortOrder": 1,
  "isActive": true
}
```

### DELETE `/api/categories/{id}` — Delete category

**Auth:** `Admin` only

---

## 6. Brands

**Base route:** `/api/brands`

### GET `/api/brands` — Get all brands

**Auth:** None

**Response** `200`:

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Primo Cuts",
      "description": "Premium meat cuts",
      "logoUrl": null,
      "isActive": true
    }
  ]
}
```

### GET `/api/brands/{id}` — Get brand by ID

### POST `/api/brands` — Create brand

**Auth:** `Admin` or `Manager`

```json
{
  "name": "New Brand",
  "description": "Description",    // optional
  "logoUrl": "https://..."         // optional
}
```

### PUT `/api/brands/{id}` — Update brand

**Auth:** `Admin` or `Manager`

```json
{
  "name": "Updated Brand",
  "description": "Updated desc",
  "logoUrl": null,
  "isActive": true
}
```

### DELETE `/api/brands/{id}` — Delete brand

**Auth:** `Admin` only

---

## 7. Orders

**Base route:** `/api/orders`

**All endpoints require authentication.**

### GET `/api/orders` — Get all orders (admin/manager)

**Auth:** `Admin` or `Manager`

**Query:** `page` (int, default 1), `pageSize` (int, default 10)

**Response** `200`:

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "orderNumber": "ORD-000001",
      "customerId": "guid",
      "customerName": "John Doe",
      "orderStatus": 0,             // enum integer — see OrderStatus
      "subTotal": 65.98,
      "discountAmount": 0,
      "taxAmount": 6.60,
      "shippingAmount": 9.99,
      "totalAmount": 82.57,
      "paymentStatus": 1,           // enum integer — see PaymentStatus
      "paymentMethod": 0,           // enum integer — see PaymentMethod
      "orderDate": "2026-04-20T...",
      "voucherCode": null,
      "notes": null,
      "items": [
        {
          "id": "guid",
          "productId": "guid",
          "productName": "Scotch Fillet Steak",
          "unitPrice": 32.99,
          "quantity": 2,
          "totalPrice": 65.98,
          "discount": 0
        }
      ]
    }
  ],
  "pagination": { ... }
}
```

### GET `/api/orders/{id}` — Get order by ID

**Auth:** `Bearer` (own order or Admin/Manager)

### GET `/api/orders/my` — Get my orders

**Auth:** `Bearer` (Customer)

**Query:** `page`, `pageSize`

### POST `/api/orders` — Create order from cart

**Auth:** `Bearer`

Creates an order from the currently active cart items.

```json
{
  "shippingAddressId": "guid",
  "billingAddressId": "guid",
  "paymentMethod": 0,                // PaymentMethod enum integer
  "voucherCode": "FIRSTORDER",       // optional
  "notes": "Please leave at door"    // optional
}
```

**Response** `201` → `OrderDto`.

### PUT `/api/orders/{id}/status` — Update order status

**Auth:** `Admin` or `Manager`

```json
{
  "newStatus": 2,                    // OrderStatus enum integer
  "notes": "Being processed"        // optional
}
```

### GET `/api/orders/{id}/history` — Get order status history

**Auth:** `Bearer`

**Response** `200`:

```json
{
  "data": [
    {
      "oldStatus": 0,
      "newStatus": 1,
      "notes": "Order confirmed",
      "changedAt": "2026-04-20T..."
    }
  ]
}
```

### POST `/api/orders/{id}/cancel` — Cancel order

**Auth:** `Bearer` (own order)

---

## 8. Cart

**Base route:** `/api/cart`

**All endpoints require authentication.**

### GET `/api/cart` — Get my cart

**Auth:** `Bearer`

**Response** `200`:

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "customerId": "guid",
    "items": [
      {
        "id": "cart-item-guid",
        "productId": "guid",
        "productName": "Scotch Fillet Steak",
        "imageUrl": "https://images.unsplash.com/...",
        "variantId": null,
        "variantName": null,
        "quantity": 2,
        "unitPrice": 32.99,
        "lineTotal": 65.98
      }
    ],
    "total": 65.98
  }
}
```

### POST `/api/cart/items` — Add item to cart

**Auth:** `Bearer`

```json
{
  "productId": "guid",
  "variantId": null,                 // optional — Guid of variant
  "quantity": 1                      // default 1
}
```

**Response** `200` → updated `CartDto`.

### PUT `/api/cart/items/{cartItemId}` — Update cart item quantity

**Auth:** `Bearer`

```json
{
  "quantity": 3
}
```

**Response** `200` → updated `CartDto`.

### DELETE `/api/cart/items/{cartItemId}` — Remove cart item

**Auth:** `Bearer`

### DELETE `/api/cart` — Clear entire cart

**Auth:** `Bearer`

---

## 9. Payments

**Base route:** `/api/payments`

**All endpoints require authentication.**

### GET `/api/payments/order/{orderId}` — Get payments for order

**Auth:** `Bearer`

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "orderId": "guid",
      "amount": 82.57,
      "paymentMethod": 0,           // PaymentMethod enum
      "transactionId": "TXN-abc123...",
      "status": 1,                   // PaymentStatus enum
      "paidAt": "2026-04-20T..."
    }
  ]
}
```

### GET `/api/payments/{id}` — Get payment by ID

### POST `/api/payments` — Process payment

**Auth:** `Bearer`

```json
{
  "orderId": "guid",
  "amount": 82.57,
  "paymentMethod": 0,               // PaymentMethod enum integer
  "transactionId": "TXN-unique-id"  // optional
}
```

**Response** `201` → `PaymentDto`.

### POST `/api/payments/{paymentId}/refunds` — Create refund

**Auth:** `Admin` or `Manager`

```json
{
  "amount": 32.99,
  "reason": "Customer returned item"  // optional
}
```

**Response** `201`:

```json
{
  "data": {
    "id": "guid",
    "orderId": "guid",
    "paymentId": "guid",
    "amount": 32.99,
    "reason": "Customer returned item",
    "status": 0,                     // RefundStatus enum
    "refundedAt": null
  }
}
```

### GET `/api/payments/order/{orderId}/refunds` — Get refunds for order

**Auth:** `Admin` or `Manager`

---

## 10. Reviews

**Base route:** `/api/reviews`

### GET `/api/reviews/product/{productId}` — Get reviews for product

**Auth:** None

**Query:** `page` (default 1), `pageSize` (default 10)

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Scotch Fillet Steak",
      "customerId": "guid",
      "customerName": "John Doe",
      "rating": 5,
      "title": "Absolutely tender!",
      "comment": "The scotch fillet was perfectly marbled...",
      "isVerifiedPurchase": true,
      "isApproved": true,
      "createdAt": "2026-04-20T..."
    }
  ],
  "pagination": { ... }
}
```

### GET `/api/reviews/{id}` — Get review by ID

### POST `/api/reviews` — Create review

**Auth:** `Bearer`

```json
{
  "productId": "guid",
  "rating": 5,                      // 1-5
  "title": "Great steak!",          // optional
  "comment": "Best I've had..."     // optional
}
```

### PUT `/api/reviews/{id}` — Update review

**Auth:** `Bearer` (own review)

```json
{
  "rating": 4,
  "title": "Updated title",
  "comment": "Updated comment"
}
```

### DELETE `/api/reviews/{id}` — Delete review

**Auth:** `Bearer` (own review or Admin)

### POST `/api/reviews/{id}/approve` — Approve review

**Auth:** `Admin` or `Manager`

---

## 11. Wishlist

**Base route:** `/api/wishlist`

**All endpoints require authentication.**

### GET `/api/wishlist` — Get my wishlist

**Auth:** `Bearer`

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Scotch Fillet Steak",
      "price": 32.99,
      "imageUrl": "https://images.unsplash.com/...",
      "addedAt": "2026-04-20T..."
    }
  ]
}
```

### POST `/api/wishlist/{productId}` — Add to wishlist

**Auth:** `Bearer`

**Response** `201` → `WishlistDto`.

### DELETE `/api/wishlist/{productId}` — Remove from wishlist

**Auth:** `Bearer`

### GET `/api/wishlist/check/{productId}` — Check if product is in wishlist

**Auth:** `Bearer`

**Response** `200`:

```json
{ "success": true, "data": true }
```

---

## 12. Customers

**Base route:** `/api/customers`

**All endpoints require authentication.**

### GET `/api/customers` — Get all customers

**Auth:** `Admin` or `Manager`

**Query:** `page` (default 1), `pageSize` (default 10), `search` (optional string)

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "userId": "user-id-string",
      "customerCode": "CUST-0001",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phoneNumber": "0412345678",
      "loyaltyPoints": 150,
      "membershipTier": 0,          // MembershipTier enum
      "totalPurchases": 342.50,
      "joinDate": "2026-01-01T...",
      "addresses": [
        {
          "id": "guid",
          "title": "Home",
          "addressLine1": "123 Main St",
          "addressLine2": null,
          "city": "Melbourne",
          "state": "VIC",
          "postalCode": "3000",
          "country": "Australia",
          "isDefault": true,
          "addressType": 2           // AddressType enum
        }
      ]
    }
  ],
  "pagination": { ... }
}
```

### GET `/api/customers/{id}` — Get customer by ID

**Auth:** `Admin`, `Manager`, or own profile

### GET `/api/customers/me` — Get my customer profile

**Auth:** `Bearer`

**Response** `200` → single `CustomerDto` (same shape, includes addresses).

### PUT `/api/customers/{id}` — Update customer

**Auth:** `Bearer`

```json
{
  "firstName": "Updated",
  "lastName": "Name",
  "phoneNumber": "0400000000",
  "membershipTier": 1               // optional — MembershipTier enum (Admin only)
}
```

### GET `/api/customers/{id}/loyalty` — Get loyalty info

**Auth:** `Bearer`

```json
{
  "data": {
    "customerId": "guid",
    "loyaltyPoints": 150,
    "membershipTier": 0,
    "totalPurchases": 342.50
  }
}
```

### POST `/api/customers/{id}/loyalty/points?points=50` — Add loyalty points

**Auth:** `Admin` or `Manager`

**Query:** `points` (int)

---

## 13. Discounts

**Base route:** `/api/discounts`

### GET `/api/discounts` — Get all discounts

**Auth:** `Admin` or `Manager`

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "code": "FIRSTORDER",
      "name": "First Order 15% Off",
      "description": "15% off your first online order",
      "discountType": 0,            // DiscountType enum
      "value": 15,
      "minOrderAmount": 30,
      "maxDiscountAmount": 50,
      "startDate": "2026-04-24T...",
      "endDate": "2027-04-24T...",
      "usageLimit": 10000,
      "usedCount": 0,
      "isActive": true
    }
  ]
}
```

### GET `/api/discounts/active` — Get active discounts

**Auth:** None

### GET `/api/discounts/{id}` — Get discount by ID

**Auth:** `Admin` or `Manager`

### POST `/api/discounts` — Create discount

**Auth:** `Admin` or `Manager`

```json
{
  "code": "SUMMER20",
  "name": "Summer Sale",
  "description": "20% off summer meats",
  "discountType": 0,                // 0=Percentage, 1=Fixed, 2=BuyXGetY
  "value": 20,
  "minOrderAmount": 25,             // optional
  "maxDiscountAmount": 50,          // optional
  "startDate": "2026-06-01T...",
  "endDate": "2026-08-31T...",
  "usageLimit": 500                  // optional
}
```

### PUT `/api/discounts/{id}` — Update discount

**Auth:** `Admin` or `Manager`

```json
{
  "name": "Updated Name",
  "description": "Updated desc",
  "value": 25,
  "minOrderAmount": 30,
  "maxDiscountAmount": 60,
  "startDate": "2026-06-01T...",
  "endDate": "2026-09-30T...",
  "usageLimit": 1000,
  "isActive": true
}
```

### DELETE `/api/discounts/{id}` — Delete discount

**Auth:** `Admin` only

### POST `/api/discounts/validate` — Validate discount code

**Auth:** None

```json
{
  "code": "FIRSTORDER",
  "orderAmount": 80.00
}
```

**Response** `200`:

```json
{
  "data": {
    "isValid": true,
    "discountAmount": 12.00,
    "message": "Discount applied successfully"
  }
}
```

---

## 14. Vouchers

**Base route:** `/api/vouchers`

### GET `/api/vouchers` — Get all vouchers

**Auth:** `Admin` or `Manager`

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "code": "MEAT25",
      "description": "$25 gift voucher",
      "voucherType": 0,             // VoucherType enum
      "value": 25,
      "valueType": 1,               // VoucherValueType enum
      "minOrderAmount": null,
      "maxRedemptions": 1,
      "currentRedemptions": 0,
      "startDate": "2026-03-25T...",
      "expiryDate": "2026-10-21T...",
      "isActive": true,
      "isSingleUse": false
    }
  ]
}
```

### GET `/api/vouchers/{id}` — Get voucher by ID

**Auth:** `Admin` or `Manager`

### POST `/api/vouchers` — Create voucher

**Auth:** `Admin` or `Manager`

```json
{
  "code": "GIFT100",
  "description": "$100 gift voucher",
  "voucherType": 0,                 // 0=Gift, 1=Promo, 2=Reward
  "value": 100,
  "valueType": 1,                   // 0=Percentage, 1=Fixed
  "minOrderAmount": null,
  "maxRedemptions": 1,
  "startDate": "2026-04-24T...",
  "expiryDate": "2027-04-24T...",
  "isSingleUse": true
}
```

### PUT `/api/vouchers/{id}` — Update voucher

**Auth:** `Admin` or `Manager`

```json
{
  "description": "Updated desc",
  "value": 50,
  "minOrderAmount": null,
  "maxRedemptions": 5,
  "startDate": "...",
  "expiryDate": "...",
  "isActive": true
}
```

### DELETE `/api/vouchers/{id}` — Delete voucher

**Auth:** `Admin` only

### POST `/api/vouchers/validate` — Validate voucher code

**Auth:** `Bearer`

```json
{
  "code": "MEAT25",
  "orderAmount": 80.00
}
```

**Response** `200`:

```json
{
  "data": {
    "isValid": true,
    "discountAmount": 25.00,
    "message": "Voucher is valid"
  }
}
```

### POST `/api/vouchers/redeem` — Redeem voucher

**Auth:** `Bearer`

```json
{
  "code": "MEAT25",
  "orderId": "order-guid"
}
```

---

## 15. Special Offers

**Base route:** `/api/offers`

### GET `/api/offers` — Get all offers

**Auth:** None

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "name": "Weekend BBQ Pack",
      "description": "Save on sausages, burgers and marinades",
      "bannerImageUrl": null,
      "offerType": 3,               // OfferType enum
      "startDate": "2026-04-24T...",
      "endDate": "2026-04-27T...",
      "isActive": true,
      "products": [
        {
          "productId": "guid",
          "productName": "Beef BBQ Sausages",
          "offerPrice": 0,
          "originalPrice": 14.00,
          "maxQuantityPerCustomer": null
        }
      ]
    }
  ]
}
```

### GET `/api/offers/active` — Get active offers only

### GET `/api/offers/{id}` — Get offer by ID

### POST `/api/offers` — Create offer

**Auth:** `Admin` or `Manager`

```json
{
  "name": "Flash Sale",
  "description": "24hr steak sale",
  "offerType": 0,                   // 0=Flash, 1=Seasonal, 2=Clearance, 3=Bundle
  "startDate": "2026-05-01T...",
  "endDate": "2026-05-02T..."
}
```

### PUT `/api/offers/{id}` — Update offer

**Auth:** `Admin` or `Manager`

```json
{
  "name": "Updated Name",
  "description": "Updated desc",
  "startDate": "...",
  "endDate": "...",
  "isActive": true
}
```

### DELETE `/api/offers/{id}` — Delete offer

**Auth:** `Admin` only

### POST `/api/offers/{offerId}/products` — Add products to offer

**Auth:** `Admin` or `Manager`

```json
{
  "products": [
    {
      "productId": "guid",
      "offerPrice": 24.99,
      "maxQuantityPerCustomer": 5    // optional
    }
  ]
}
```

---

## 16. Inventory

**Base route:** `/api/inventory`

**All endpoints require `Admin` or `Manager` role.**

### GET `/api/inventory/product/{productId}` — Get inventory transactions

**Query:** `page` (default 1), `pageSize` (default 20)

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Scotch Fillet Steak",
      "variantId": null,
      "transactionType": 0,          // InventoryTransactionType enum
      "quantity": 50,
      "reference": "PO-001",
      "notes": "Initial stock",
      "createdAt": "2026-04-24T..."
    }
  ],
  "pagination": { ... }
}
```

### POST `/api/inventory/adjust` — Adjust inventory

**Auth:** `Admin` or `Manager`

```json
{
  "productId": "guid",
  "variantId": null,                 // optional
  "transactionType": 0,             // 0=StockIn, 1=StockOut, 2=Adjustment, 3=Return
  "quantity": 20,
  "reference": "PO-002",            // optional
  "notes": "Restocking"             // optional
}
```

### GET `/api/inventory/low-stock` — Get low-stock products

**Query:** `threshold` (int, default 10)

**Response** `200`:

```json
{
  "data": [
    {
      "id": "guid",
      "name": "Dry-Aged Ribeye 45 Day",
      "sku": "PM-BS-006",
      "stockQuantity": 12,
      "minStockLevel": 3
    }
  ]
}
```

---

## 17. Dashboard

**Base route:** `/api/dashboard`

**All endpoints require `Admin` or `Manager` role.**

### GET `/api/dashboard/sales-summary` — Get sales summary

**Query:** `from` (DateTime, optional), `to` (DateTime, optional)

```json
{
  "data": {
    "totalRevenue": 12500.00,
    "totalOrders": 45,
    "averageOrderValue": 277.78,
    "totalCustomers": 12
  }
}
```

### GET `/api/dashboard/top-products` — Get top selling products

**Query:** `count` (int, default 10), `from`, `to` (both optional)

```json
{
  "data": [
    {
      "productId": "guid",
      "productName": "Scotch Fillet Steak",
      "totalSold": 120,
      "totalRevenue": 3958.80
    }
  ]
}
```

### GET `/api/dashboard/revenue-chart` — Get revenue chart data

**Query:** `from` (DateTime, **required**), `to` (DateTime, **required**)

```json
{
  "data": [
    {
      "date": "2026-04-01T...",
      "revenue": 450.00,
      "orderCount": 3
    }
  ]
}
```

### GET `/api/dashboard/customer-stats` — Get customer statistics

```json
{
  "data": {
    "totalCustomers": 12,
    "newCustomersThisMonth": 3,
    "activeCustomers": 8
  }
}
```

---

## 18. Settings

**Base route:** `/api/settings`

**All endpoints require `Admin` role.**

### GET `/api/settings` — Get all settings

```json
{
  "data": [
    {
      "id": "guid",
      "key": "Store.Name",
      "value": "Primo Meats",
      "group": "General",
      "description": "Store display name"
    },
    {
      "key": "Store.Currency",
      "value": "AUD",
      "group": "General",
      "description": "Default currency"
    },
    {
      "key": "Store.TaxRate",
      "value": "10",
      "group": "General",
      "description": "GST rate percentage"
    },
    {
      "key": "Store.DeliveryFee",
      "value": "12.99",
      "group": "Delivery",
      "description": "Default delivery fee"
    },
    {
      "key": "Store.FreeDeliveryThreshold",
      "value": "80",
      "group": "Delivery",
      "description": "Free delivery threshold amount"
    },
    {
      "key": "Loyalty.PointsPerDollar",
      "value": "2",
      "group": "Loyalty",
      "description": "Loyalty points earned per dollar spent"
    },
    {
      "key": "Loyalty.PointsRedemptionRate",
      "value": "100",
      "group": "Loyalty",
      "description": "Points needed for $1 discount"
    }
  ]
}
```

### GET `/api/settings/group/{group}` — Get settings by group

**Query path:** group name (e.g. `General`, `Delivery`, `Loyalty`)

### GET `/api/settings/{key}` — Get single setting

**Query path:** key name (e.g. `Store.Name`)

### PUT `/api/settings` — Update settings (batch)

```json
{
  "settings": [
    { "key": "Store.Name", "value": "Primo Meats & Deli" },
    { "key": "Store.DeliveryFee", "value": "14.99" }
  ]
}
```

---

## 19. File Upload (Images)

**Base route:** `/api/images`

**Uses Cloudinary for storage.**

### POST `/api/images/upload` — Upload image

**Auth:** `Admin` only

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `file` | File | Yes | Image file (max 5MB) |
| `folder` | string | No | Cloudinary folder (default: `general`) |

**Allowed types:** `image/jpeg`, `image/png`, `image/gif`, `image/webp`
**Allowed extensions:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`

**Response** `200`:

```json
{
  "data": {
    "url": "https://res.cloudinary.com/...",
    "publicId": "general/abc123",
    "thumbnailUrl": "https://res.cloudinary.com/.../t_thumb/...",
    "width": 1200,
    "height": 800,
    "format": "jpg",
    "size": 245678
  }
}
```

### DELETE `/api/images?publicId=general/abc123` — Delete image

**Auth:** `Admin` only

---

## 20. Addresses

**Note:** There is no standalone `/api/addresses` endpoint. Customer addresses are **embedded** in the `CustomerDto` and returned with customer endpoints:

- `GET /api/customers/me` → includes `addresses[]`
- `GET /api/customers/{id}` → includes `addresses[]`

**Address shape:**

```json
{
  "id": "guid",
  "title": "Home",
  "addressLine1": "42 Collins Street",
  "addressLine2": "Suite 1",
  "city": "Melbourne",
  "state": "VIC",
  "postalCode": "3000",
  "country": "Australia",
  "isDefault": true,
  "addressType": 2                   // 0=Shipping, 1=Billing, 2=Both
}
```

The `shippingAddressId` and `billingAddressId` used in `CreateOrderRequest` reference these address IDs.

---

## 21. Enums Reference

All enums are serialized as **integers** in JSON responses.

### OrderStatus

| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Confirmed |
| 2 | Processing |
| 3 | Shipped |
| 4 | Delivered |
| 5 | Cancelled |
| 6 | Returned |
| 7 | Refunded |

### PaymentStatus

| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Paid |
| 2 | Failed |
| 3 | Refunded |
| 4 | PartiallyRefunded |

### PaymentMethod

| Value | Name |
|-------|------|
| 0 | CreditCard |
| 1 | DebitCard |
| 2 | PayPal |
| 3 | BankTransfer |
| 4 | CashOnDelivery |

### DiscountType

| Value | Name |
|-------|------|
| 0 | Percentage |
| 1 | Fixed |
| 2 | BuyXGetY |

### VoucherType

| Value | Name |
|-------|------|
| 0 | Gift |
| 1 | Promo |
| 2 | Reward |

### VoucherValueType

| Value | Name |
|-------|------|
| 0 | Percentage |
| 1 | Fixed |

### OfferType

| Value | Name |
|-------|------|
| 0 | Flash |
| 1 | Seasonal |
| 2 | Clearance |
| 3 | Bundle |

### MembershipTier

| Value | Name |
|-------|------|
| 0 | Bronze |
| 1 | Silver |
| 2 | Gold |
| 3 | Platinum |

### InventoryTransactionType

| Value | Name |
|-------|------|
| 0 | StockIn |
| 1 | StockOut |
| 2 | Adjustment |
| 3 | Return |

### RefundStatus

| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Approved |
| 2 | Processed |
| 3 | Rejected |

### AddressType

| Value | Name |
|-------|------|
| 0 | Shipping |
| 1 | Billing |
| 2 | Both |

---

## 22. Seeded Test Data

### Users & Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@appilico.com | Admin@123! |
| Manager | manager@appilico.com | Manager@123! |
| Customer 1 | customer1@appilico.com | Customer@123! |
| Customer 2 | customer2@appilico.com | Customer@123! |
| Customer 3 | customer3@appilico.com | Customer@123! |

### Categories (25 total)

**8 top-level:**
Beef, Veal, Lamb, Pork, Poultry, Deli & Cold Cuts, Ready Meals, Pantry

**17 subcategories:**

| Parent | Subcategory |
|--------|-------------|
| Beef | Beef Steaks, Beef Roasting Joints, Beef Other Cuts |
| Veal | Veal Steaks & Cutlets, Veal Specialty |
| Lamb | Lamb Chops & Cutlets, Lamb Roasting Joints |
| Pork | Pork Steaks & Chops, Pork Roasting Joints, Pork Other Cuts |
| Poultry | Chicken Portions, Whole Birds |
| Ready Meals | Italian Classics, Sausages & Burgers, Curries & Stews |
| Pantry | Pasta & Sauces, Oils & Seasonings |

### Brands (6)

Primo Cuts, Heritage Reserve, Valley Fresh, Artisan Kitchen, Rustic Pantry, Grill Master

### Products (74 total)

| Category | Count | Price Range (AUD) |
|----------|-------|-------------------|
| Beef Steaks | 7 | $19.99 – $54.99 |
| Beef Roasting Joints | 5 | $28.99 – $80.00 |
| Beef Other Cuts | 4 | $14.00 – $22.00 |
| Veal Steaks & Cutlets | 3 | $14.00 – $22.00 |
| Veal Specialty | 3 | $8.00 – $38.99 |
| Lamb Chops & Cutlets | 5 | $17.50 – $40.00 |
| Lamb Roasting Joints | 4 | $18.00 – $85.00 |
| Pork Steaks & Chops | 3 | $14.99 – $18.99 |
| Pork Roasting Joints | 3 | $32.99 – $50.00 |
| Pork Other Cuts | 3 | $15.00 – $45.00 |
| Chicken Portions | 5 | $8.99 – $16.99 |
| Whole Birds | 4 | $22.00 – $60.00 |
| Deli & Cold Cuts | 6 | $12.50 – $220.00 |
| Italian Classics | 3 | $18.00 – $24.99 |
| Sausages & Burgers | 6 | $4.00 – $16.00 |
| Curries & Stews | 2 | $22.99 – $24.99 |
| Pasta & Sauces | 4 | $6.99 – $18.50 |
| Oils & Seasonings | 4 | $7.99 – $18.99 |

### Discounts (5 seeded)

| Code | Type | Value | Min Order |
|------|------|-------|-----------|
| FIRSTORDER | Percentage | 15% | $30 |
| BBQ20 | Percentage | 20% | $25 |
| ROAST10 | Fixed | $10 | $40 |
| FAMILY25 | Percentage | 25% | $100 |
| FREEDELIVERY | Fixed | $12.99 | $50 |

### Vouchers (5 seeded)

| Code | Type | Value | Max Uses |
|------|------|-------|----------|
| MEAT25 | Gift | $25 | 1 |
| MEAT50 | Gift | $50 | 1 |
| LOYAL15 | Promo | 15% | 100 |
| WELCOME10 | Reward | $10 | 500 |
| REFER20 | Promo | 20% | 5000 |

### Special Offers (3 seeded)

| Name | Type | Products |
|------|------|----------|
| Weekend BBQ Pack | Bundle | All PM-SB-* + PM-OS-* products |
| Steak Night Special | Flash | All PM-BS-* products |
| Winter Roast Season | Seasonal | All PM-BR-* + PM-LR-* + PM-PR-* products |

---

## 23. Rate Limiting

| Endpoint | Period | Limit |
|----------|--------|-------|
| All endpoints | 1 minute | 100 requests |
| POST /api/auth/login | 1 minute | 5 requests |
| POST /api/auth/register | 1 minute | 3 requests |

When rate limited, the API returns HTTP `429` with the standard error envelope.

---

## 24. CORS

Allowed origins:
- `http://localhost:3000`
- `https://appilico-client-aif9.vercel.app`
- `https://appilico-client.vercel.app`
- `https://appilico-web.vercel.app`
- Any `*.vercel.app` subdomain (for preview deployments)

Methods: Any | Headers: Any | Credentials: Allowed
