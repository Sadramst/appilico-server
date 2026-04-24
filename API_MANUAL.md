# Primo Meats API — Client Integration Manual

> **Base URL (Production):** `https://appilico-server.onrender.com`
> **Base URL (Local Dev):** `http://localhost:5034`
> **Swagger UI:** `{BASE_URL}/swagger`

All endpoints return a standard JSON envelope:

```json
{
  "success": true,
  "message": "Success",
  "data": { ... },
  "pagination": { "currentPage": 1, "pageSize": 10, "totalCount": 56, "totalPages": 6, "hasPrevious": false, "hasNext": true },
  "errors": [],
  "timestamp": "2026-04-24T07:00:00Z"
}
```

> `pagination` is included only on paginated endpoints. `errors` is populated when `success` is `false`.

---

## Table of Contents

1. [Authentication](#1-authentication)
2. [Products](#2-products)
3. [Categories](#3-categories)
4. [Brands](#4-brands)
5. [Cart](#5-cart)
6. [Orders](#6-orders)
7. [Payments](#7-payments)
8. [Reviews](#8-reviews)
9. [Wishlist](#9-wishlist)
10. [Discounts](#10-discounts)
11. [Vouchers](#11-vouchers)
12. [Special Offers](#12-special-offers)
13. [Customers](#13-customers)
14. [Inventory](#14-inventory)
15. [Dashboard](#15-dashboard)
16. [Settings](#16-settings)
17. [Images](#17-images)
18. [Enum Reference](#18-enum-reference)
19. [Seed Accounts](#19-seed-accounts)
20. [Rate Limiting](#20-rate-limiting)

---

## 1. Authentication

### Register

```
POST /api/auth/register
```

**Body:**

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "phoneNumber": "+61400000000"
}
```

**Response (`data`):** `AuthResponse` (see Login)

---

### Login

```
POST /api/auth/login
```

**Body:**

```json
{
  "email": "admin@appilico.com",
  "password": "Admin123!@#"
}
```

**Response (`data`):**

```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "c3f1a2b...",
  "expiresAt": "2026-04-24T08:00:00Z",
  "user": {
    "id": "guid",
    "firstName": "Admin",
    "lastName": "User",
    "email": "admin@appilico.com",
    "avatar": null,
    "roles": ["Admin"]
  }
}
```

> Use the `accessToken` as a Bearer token in the `Authorization` header for all authenticated requests:
> `Authorization: Bearer eyJhbGci...`

---

### Refresh Token

```
POST /api/auth/refresh
```

**Body:**

```json
{
  "refreshToken": "c3f1a2b..."
}
```

---

### Revoke Token 🔒

```
POST /api/auth/revoke
```

**Body:**

```json
{
  "token": "c3f1a2b..."
}
```

---

### Get Profile 🔒

```
GET /api/auth/profile
```

---

### Update Profile 🔒

```
PUT /api/auth/profile
```

**Body:**

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+61400000000",
  "dateOfBirth": "1990-01-15"
}
```

---

### Forgot Password

```
POST /api/auth/forgot-password
```

**Body:**

```json
{
  "email": "john@example.com"
}
```

---

### Reset Password

```
POST /api/auth/reset-password
```

**Body:**

```json
{
  "email": "john@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123!"
}
```

---

## 2. Products

### Search / List Products

```
GET /api/products
```

**Query parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `searchTerm` | string | — | Search in name/description |
| `categoryId` | guid | — | Filter by category |
| `brandId` | guid | — | Filter by brand |
| `minPrice` | decimal | — | Minimum price filter |
| `maxPrice` | decimal | — | Maximum price filter |
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page |
| `sortBy` | string | — | Field to sort by (e.g. `name`, `basePrice`, `createdAt`) |
| `sortDescending` | bool | false | Sort direction |

**Response (`data`):** Array of `ProductDto`

```json
{
  "id": "guid",
  "name": "Scotch Fillet Steak",
  "description": "300g premium grain-fed scotch fillet, beautifully marbled",
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
  "averageRating": 4.50,
  "totalReviews": 3,
  "primaryImageUrl": "https://images.unsplash.com/photo-...",
  "createdAt": "2026-04-24T07:00:00Z",
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
      "variantName": "4-pack",
      "sku": "PM-SB-004-4PK",
      "price": 16.99,
      "stockQuantity": 40,
      "attributes": "{\"size\":\"4-pack\"}"
    }
  ]
}
```

---

### Get Product by ID

```
GET /api/products/{id}
```

---

### Get Product by SKU

```
GET /api/products/sku/{sku}
```

**Example:** `GET /api/products/sku/PM-BS-001`

---

### Get Featured Products

```
GET /api/products/featured?count=10
```

---

### Create Product 🔒 Admin/Manager

```
POST /api/products
```

**Body:**

```json
{
  "name": "Wagyu Steak",
  "description": "Premium A5 Wagyu",
  "sku": "PM-BS-006",
  "barcode": null,
  "categoryId": "guid",
  "brandId": "guid",
  "basePrice": 120.00,
  "costPrice": 80.00,
  "stockQuantity": 10,
  "minStockLevel": 2,
  "weight": 0.3,
  "dimensions": null,
  "isFeatured": true
}
```

---

### Update Product 🔒 Admin/Manager

```
PUT /api/products/{id}
```

**Body:**

```json
{
  "name": "Wagyu Steak",
  "description": "Updated description",
  "barcode": null,
  "categoryId": "guid",
  "brandId": "guid",
  "basePrice": 125.00,
  "costPrice": 80.00,
  "stockQuantity": 10,
  "minStockLevel": 2,
  "weight": 0.3,
  "dimensions": null,
  "isActive": true,
  "isFeatured": true
}
```

---

### Delete Product 🔒 Admin

```
DELETE /api/products/{id}
```

---

### Add Product Variant 🔒 Admin/Manager

```
POST /api/products/{productId}/variants
```

**Body:**

```json
{
  "variantName": "Large",
  "sku": "PM-BS-006-LG",
  "price": 140.00,
  "stockQuantity": 5,
  "attributes": "{\"size\":\"Large\"}"
}
```

---

## 3. Categories

### Get All Categories

```
GET /api/categories
```

**Response (`data`):** Array of `CategoryDto`

```json
{
  "id": "guid",
  "name": "Beef",
  "description": "Premium grain-fed and grass-fed beef cuts",
  "imageUrl": "https://images.unsplash.com/photo-...",
  "parentCategoryId": null,
  "sortOrder": 1,
  "isActive": true,
  "productCount": 13,
  "subCategories": []
}
```

---

### Get Category Tree (nested)

```
GET /api/categories/tree
```

Returns parent categories with nested `subCategories`.

---

### Get Category by ID

```
GET /api/categories/{id}
```

---

### Create Category 🔒 Admin/Manager

```
POST /api/categories
```

**Body:**

```json
{
  "name": "Seafood",
  "description": "Fresh fish and shellfish",
  "imageUrl": null,
  "parentCategoryId": null,
  "sortOrder": 11,
  "isActive": true
}
```

---

### Update Category 🔒 Admin/Manager

```
PUT /api/categories/{id}
```

---

### Delete Category 🔒 Admin

```
DELETE /api/categories/{id}
```

---

## 4. Brands

### Get All Brands

```
GET /api/brands
```

**Response (`data`):** Array of `BrandDto`

```json
{
  "id": "guid",
  "name": "Primo Cuts",
  "description": "Our signature premium meat range",
  "logoUrl": null,
  "isActive": true
}
```

---

### Get Brand by ID

```
GET /api/brands/{id}
```

---

### Create Brand 🔒 Admin/Manager

```
POST /api/brands
```

**Body:**

```json
{
  "name": "New Brand",
  "description": "Brand description",
  "logoUrl": null
}
```

---

### Update Brand 🔒 Admin/Manager

```
PUT /api/brands/{id}
```

---

### Delete Brand 🔒 Admin

```
DELETE /api/brands/{id}
```

---

## 5. Cart

> All cart endpoints require authentication 🔒

### Get My Cart

```
GET /api/cart
```

**Response (`data`):**

```json
{
  "id": "guid",
  "customerId": "guid",
  "items": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Scotch Fillet Steak",
      "imageUrl": "https://images.unsplash.com/photo-...",
      "variantId": null,
      "variantName": null,
      "quantity": 2,
      "unitPrice": 32.99,
      "lineTotal": 65.98
    }
  ],
  "total": 65.98
}
```

---

### Add Item to Cart

```
POST /api/cart/items
```

**Body:**

```json
{
  "productId": "guid",
  "variantId": null,
  "quantity": 2
}
```

---

### Update Cart Item

```
PUT /api/cart/items/{cartItemId}
```

**Body:**

```json
{
  "quantity": 3
}
```

---

### Remove Cart Item

```
DELETE /api/cart/items/{cartItemId}
```

---

### Clear Cart

```
DELETE /api/cart
```

---

## 6. Orders

> All order endpoints require authentication 🔒

### Get All Orders 🔒 Admin/Manager

```
GET /api/orders?page=1&pageSize=10
```

---

### Get Order by ID

```
GET /api/orders/{id}
```

**Response (`data`):**

```json
{
  "id": "guid",
  "orderNumber": "ORD-20260424-001",
  "customerId": "guid",
  "customerName": "John Smith",
  "orderStatus": 2,
  "subTotal": 65.98,
  "discountAmount": 0,
  "taxAmount": 6.60,
  "shippingAmount": 12.99,
  "totalAmount": 85.57,
  "paymentStatus": 1,
  "paymentMethod": 0,
  "orderDate": "2026-04-24T07:00:00Z",
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
```

---

### Get My Orders

```
GET /api/orders/my?page=1&pageSize=10
```

---

### Create Order (from cart)

```
POST /api/orders
```

**Body:**

```json
{
  "shippingAddressId": "guid",
  "billingAddressId": "guid",
  "paymentMethod": 0,
  "voucherCode": null,
  "notes": "Please ring doorbell"
}
```

> Creates an order from the items currently in your cart.

---

### Update Order Status 🔒 Admin/Manager

```
PUT /api/orders/{id}/status
```

**Body:**

```json
{
  "newStatus": 2,
  "notes": "Being packed"
}
```

---

### Get Order Status History

```
GET /api/orders/{id}/history
```

**Response (`data`):** Array of status changes

```json
{
  "oldStatus": 0,
  "newStatus": 1,
  "notes": "Order confirmed",
  "changedAt": "2026-04-24T07:05:00Z"
}
```

---

### Cancel Order

```
POST /api/orders/{id}/cancel
```

---

## 7. Payments

> All payment endpoints require authentication 🔒

### Get Payments for Order

```
GET /api/payments/order/{orderId}
```

**Response (`data`):**

```json
{
  "id": "guid",
  "orderId": "guid",
  "amount": 85.57,
  "paymentMethod": 0,
  "transactionId": "TXN-12345",
  "status": 1,
  "paidAt": "2026-04-24T07:10:00Z"
}
```

---

### Get Payment by ID

```
GET /api/payments/{id}
```

---

### Process Payment

```
POST /api/payments
```

**Body:**

```json
{
  "orderId": "guid",
  "amount": 85.57,
  "paymentMethod": 0,
  "transactionId": "TXN-12345"
}
```

---

### Create Refund 🔒 Admin/Manager

```
POST /api/payments/{paymentId}/refunds
```

**Body:**

```json
{
  "amount": 32.99,
  "reason": "Product damaged"
}
```

---

### Get Refunds for Order 🔒 Admin/Manager

```
GET /api/payments/order/{orderId}/refunds
```

---

## 8. Reviews

### Get Reviews for Product

```
GET /api/reviews/product/{productId}?page=1&pageSize=10
```

**Response (`data`):** Array of `ReviewDto`

```json
{
  "id": "guid",
  "productId": "guid",
  "productName": "Scotch Fillet Steak",
  "customerId": "guid",
  "customerName": "John Smith",
  "rating": 5,
  "title": "Absolutely tender!",
  "comment": "The scotch fillet was perfectly marbled. Best steak I've had at home.",
  "isVerifiedPurchase": false,
  "isApproved": true,
  "createdAt": "2026-04-24T07:00:00Z"
}
```

---

### Get Review by ID

```
GET /api/reviews/{id}
```

---

### Create Review 🔒

```
POST /api/reviews
```

**Body:**

```json
{
  "productId": "guid",
  "rating": 5,
  "title": "Amazing quality",
  "comment": "Best meat I've ever purchased"
}
```

---

### Update Review 🔒

```
PUT /api/reviews/{id}
```

**Body:**

```json
{
  "rating": 4,
  "title": "Updated title",
  "comment": "Updated comment"
}
```

---

### Delete Review 🔒

```
DELETE /api/reviews/{id}
```

---

### Approve Review 🔒 Admin/Manager

```
POST /api/reviews/{id}/approve
```

---

## 9. Wishlist

> All wishlist endpoints require authentication 🔒

### Get My Wishlist

```
GET /api/wishlist
```

**Response (`data`):** Array of `WishlistDto`

```json
{
  "id": "guid",
  "productId": "guid",
  "productName": "Dry-Aged Ribeye 45 Day",
  "price": 54.99,
  "imageUrl": "https://images.unsplash.com/photo-...",
  "addedAt": "2026-04-24T07:00:00Z"
}
```

---

### Add to Wishlist

```
POST /api/wishlist/{productId}
```

---

### Remove from Wishlist

```
DELETE /api/wishlist/{productId}
```

---

### Check if Product in Wishlist

```
GET /api/wishlist/check/{productId}
```

---

## 10. Discounts

### Get Active Discounts

```
GET /api/discounts/active
```

**Response (`data`):** Array of `DiscountDto`

```json
{
  "id": "guid",
  "code": "BBQ20",
  "name": "BBQ Season 20%",
  "description": "20% off all sausages and burgers",
  "discountType": 0,
  "value": 20,
  "minOrderAmount": 25,
  "maxDiscountAmount": 30,
  "startDate": "2026-04-19T00:00:00Z",
  "endDate": "2026-06-08T00:00:00Z",
  "usageLimit": 500,
  "usedCount": 38,
  "isActive": true
}
```

---

### Validate Discount Code

```
POST /api/discounts/validate
```

**Body:**

```json
{
  "code": "BBQ20",
  "orderAmount": 50.00
}
```

**Response (`data`):**

```json
{
  "isValid": true,
  "discountAmount": 10.00,
  "message": "Discount applied successfully"
}
```

---

### Get All Discounts 🔒 Admin/Manager

```
GET /api/discounts
```

---

### Get Discount by ID 🔒 Admin/Manager

```
GET /api/discounts/{id}
```

---

### Create Discount 🔒 Admin/Manager

```
POST /api/discounts
```

**Body:**

```json
{
  "code": "SUMMER30",
  "name": "Summer Special",
  "description": "30% off orders over $60",
  "discountType": 0,
  "value": 30,
  "minOrderAmount": 60,
  "maxDiscountAmount": 50,
  "startDate": "2026-06-01",
  "endDate": "2026-08-31",
  "usageLimit": 200
}
```

---

### Update Discount 🔒 Admin/Manager

```
PUT /api/discounts/{id}
```

---

### Delete Discount 🔒 Admin

```
DELETE /api/discounts/{id}
```

---

## 11. Vouchers

### Validate Voucher 🔒

```
POST /api/vouchers/validate
```

**Body:**

```json
{
  "code": "MEAT25",
  "orderAmount": 60.00
}
```

**Response (`data`):**

```json
{
  "isValid": true,
  "discountAmount": 25.00,
  "message": "Voucher is valid"
}
```

---

### Redeem Voucher 🔒

```
POST /api/vouchers/redeem
```

**Body:**

```json
{
  "code": "MEAT25",
  "orderId": "guid"
}
```

---

### Get All Vouchers 🔒 Admin/Manager

```
GET /api/vouchers
```

**Response (`data`):** Array of `VoucherDto`

```json
{
  "id": "guid",
  "code": "MEAT25",
  "description": "$25 gift voucher",
  "voucherType": 0,
  "value": 25,
  "valueType": 1,
  "minOrderAmount": null,
  "maxRedemptions": 1,
  "currentRedemptions": 0,
  "startDate": "2026-03-25T00:00:00Z",
  "expiryDate": "2026-10-21T00:00:00Z",
  "isActive": true,
  "isSingleUse": false
}
```

---

### Get Voucher by ID 🔒 Admin/Manager

```
GET /api/vouchers/{id}
```

---

### Create Voucher 🔒 Admin/Manager

```
POST /api/vouchers
```

**Body:**

```json
{
  "code": "XMAS50",
  "description": "$50 Christmas gift voucher",
  "voucherType": 0,
  "value": 50,
  "valueType": 1,
  "minOrderAmount": null,
  "maxRedemptions": 1,
  "startDate": "2026-12-01",
  "expiryDate": "2027-03-01",
  "isSingleUse": true
}
```

---

### Update Voucher 🔒 Admin/Manager

```
PUT /api/vouchers/{id}
```

---

### Delete Voucher 🔒 Admin

```
DELETE /api/vouchers/{id}
```

---

## 12. Special Offers

### Get All Offers

```
GET /api/offers
```

**Response (`data`):** Array of `SpecialOfferDto`

```json
{
  "id": "guid",
  "name": "Weekend BBQ Pack",
  "description": "Save on sausages, burgers and marinades this weekend",
  "bannerImageUrl": null,
  "offerType": 3,
  "startDate": "2026-04-24T00:00:00Z",
  "endDate": "2026-04-27T00:00:00Z",
  "isActive": true,
  "products": [
    {
      "productId": "guid",
      "productName": "Beef BBQ Sausages",
      "offerPrice": 0,
      "originalPrice": 12.99,
      "maxQuantityPerCustomer": null
    }
  ]
}
```

---

### Get Active Offers

```
GET /api/offers/active
```

---

### Get Offer by ID

```
GET /api/offers/{id}
```

---

### Create Offer 🔒 Admin/Manager

```
POST /api/offers
```

**Body:**

```json
{
  "name": "New Year Sale",
  "description": "Kick off the new year with savings",
  "offerType": 1,
  "startDate": "2027-01-01",
  "endDate": "2027-01-15"
}
```

---

### Update Offer 🔒 Admin/Manager

```
PUT /api/offers/{id}
```

---

### Delete Offer 🔒 Admin

```
DELETE /api/offers/{id}
```

---

### Add Products to Offer 🔒 Admin/Manager

```
POST /api/offers/{offerId}/products
```

**Body:**

```json
{
  "products": [
    {
      "productId": "guid",
      "offerPrice": 25.99,
      "maxQuantityPerCustomer": 2
    }
  ]
}
```

---

## 13. Customers

> All customer endpoints require authentication 🔒

### Get All Customers 🔒 Admin/Manager

```
GET /api/customers?page=1&pageSize=10&search=John
```

---

### Get Customer by ID

```
GET /api/customers/{id}
```

**Response (`data`):**

```json
{
  "id": "guid",
  "userId": "guid",
  "customerCode": "CUST001",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john@example.com",
  "phoneNumber": "+61400000000",
  "loyaltyPoints": 150,
  "membershipTier": 0,
  "totalPurchases": 245.50,
  "joinDate": "2026-04-24T07:00:00Z",
  "addresses": [
    {
      "id": "guid",
      "title": "Home",
      "addressLine1": "10 High Street",
      "addressLine2": null,
      "city": "Perth",
      "state": "WA",
      "postalCode": "6000",
      "country": "AU",
      "isDefault": true,
      "addressType": 0
    }
  ]
}
```

---

### Get My Profile

```
GET /api/customers/me
```

---

### Update Customer

```
PUT /api/customers/{id}
```

**Body:**

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+61400000000",
  "membershipTier": 1
}
```

---

### Get Customer Loyalty

```
GET /api/customers/{id}/loyalty
```

**Response (`data`):**

```json
{
  "customerId": "guid",
  "loyaltyPoints": 150,
  "membershipTier": 0,
  "totalPurchases": 245.50
}
```

---

### Add Loyalty Points 🔒 Admin/Manager

```
POST /api/customers/{id}/loyalty/points?points=50
```

---

## 14. Inventory

> All inventory endpoints require Admin/Manager role 🔒

### Get Inventory Transactions

```
GET /api/inventory/product/{productId}?page=1&pageSize=20
```

**Response (`data`):** Array of `InventoryTransactionDto`

```json
{
  "id": "guid",
  "productId": "guid",
  "productName": "Scotch Fillet Steak",
  "variantId": null,
  "transactionType": 0,
  "quantity": 20,
  "reference": "RESTOCK-001",
  "notes": "Weekly delivery from supplier",
  "createdAt": "2026-04-24T07:00:00Z"
}
```

---

### Adjust Inventory

```
POST /api/inventory/adjust
```

**Body:**

```json
{
  "productId": "guid",
  "variantId": null,
  "transactionType": 0,
  "quantity": 20,
  "reference": "RESTOCK-001",
  "notes": "Weekly delivery from supplier"
}
```

---

### Get Low Stock Products

```
GET /api/inventory/low-stock?threshold=10
```

**Response (`data`):** Array of `LowStockProductDto`

```json
{
  "id": "guid",
  "name": "Duck (Whole)",
  "sku": "PM-WB-003",
  "stockQuantity": 10,
  "minStockLevel": 2
}
```

---

## 15. Dashboard

> All dashboard endpoints require Admin/Manager role 🔒

### Sales Summary

```
GET /api/dashboard/sales-summary?from=2026-01-01&to=2026-12-31
```

**Response (`data`):**

```json
{
  "totalRevenue": 12450.00,
  "totalOrders": 85,
  "averageOrderValue": 146.47,
  "totalCustomers": 42
}
```

---

### Top Products

```
GET /api/dashboard/top-products?count=10&from=2026-01-01&to=2026-12-31
```

**Response (`data`):** Array of `TopProductDto`

```json
{
  "productId": "guid",
  "productName": "Scotch Fillet Steak",
  "totalSold": 120,
  "totalRevenue": 3958.80
}
```

---

### Revenue Chart

```
GET /api/dashboard/revenue-chart?from=2026-04-01&to=2026-04-24
```

**Response (`data`):** Array of `RevenueChartDto`

```json
{
  "date": "2026-04-01",
  "revenue": 520.00,
  "orderCount": 4
}
```

---

### Customer Stats

```
GET /api/dashboard/customer-stats
```

**Response (`data`):**

```json
{
  "totalCustomers": 42,
  "newCustomersThisMonth": 8,
  "activeCustomers": 35
}
```

---

## 16. Settings

> All settings endpoints require Admin role 🔒

### Get All Settings

```
GET /api/settings
```

**Response (`data`):** Array of `AppSettingDto`

```json
{
  "id": "guid",
  "key": "Store.Name",
  "value": "Primo Meats",
  "group": "General",
  "description": "Store display name"
}
```

---

### Get Settings by Group

```
GET /api/settings/group/General
```

---

### Get Setting by Key

```
GET /api/settings/Store.Name
```

---

### Update Settings

```
PUT /api/settings
```

**Body:**

```json
{
  "settings": [
    { "key": "Store.Name", "value": "Primo Meats" },
    { "key": "Store.Currency", "value": "AUD" }
  ]
}
```

---

## 17. Images

> All image endpoints require Admin role 🔒

### Upload Image

```
POST /api/images/upload?folder=products
Content-Type: multipart/form-data
```

**Form data:**

| Field | Type | Description |
|-------|------|-------------|
| `file` | File | Image file (max 5 MB) |
| `folder` | Query | Cloudinary folder (e.g. `products`, `categories`) |

**Response (`data`):**

```json
{
  "url": "https://res.cloudinary.com/dijoqk8f7/image/upload/v1.../products/image.jpg",
  "publicId": "products/image"
}
```

---

### Delete Image

```
DELETE /api/images?publicId=products/image
```

---

## 18. Enum Reference

### DiscountType

| Value | Name |
|-------|------|
| 0 | Percentage |
| 1 | Fixed |
| 2 | BuyXGetY |

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

### PaymentMethod

| Value | Name |
|-------|------|
| 0 | CreditCard |
| 1 | DebitCard |
| 2 | PayPal |
| 3 | BankTransfer |
| 4 | CashOnDelivery |

### PaymentStatus

| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Paid |
| 2 | Failed |
| 3 | Refunded |
| 4 | PartiallyRefunded |

### RefundStatus

| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Approved |
| 2 | Processed |
| 3 | Rejected |

### MembershipTier

| Value | Name |
|-------|------|
| 0 | Bronze |
| 1 | Silver |
| 2 | Gold |
| 3 | Platinum |

### AddressType

| Value | Name |
|-------|------|
| 0 | Shipping |
| 1 | Billing |
| 2 | Both |

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

### InventoryTransactionType

| Value | Name |
|-------|------|
| 0 | StockIn |
| 1 | StockOut |
| 2 | Adjustment |
| 3 | Return |

---

## 19. Seed Accounts

The database comes pre-seeded with these accounts:

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@appilico.com` | `Admin123!@#` |
| Manager | `manager@appilico.com` | `Manager123!@#` |
| Customer | `customer1@appilico.com` | `Customer123!@#` |
| Customer | `customer2@appilico.com` | `Customer123!@#` |
| Customer | `customer3@appilico.com` | `Customer123!@#` |
| Customer | `customer4@appilico.com` | `Customer123!@#` |
| Customer | `customer5@appilico.com` | `Customer123!@#` |

### Pre-seeded Discount Codes

| Code | Description |
|------|-------------|
| `FIRSTORDER` | 15% off first order (min $30) |
| `BBQ20` | 20% off sausages & burgers (min $25) |
| `ROAST10` | $10 off roasting joints (min $40) |
| `FAMILY25` | 25% off orders over $100 |
| `FREEDELIVERY` | Free delivery on orders over $50 |

### Pre-seeded Voucher Codes

| Code | Description |
|------|-------------|
| `MEAT25` | $25 gift voucher |
| `MEAT50` | $50 gift voucher |
| `LOYAL15` | 15% off for returning customers |
| `WELCOME10` | $10 welcome voucher |
| `REFER20` | 20% off referral reward |

---

## 20. Rate Limiting

The API applies rate limiting to prevent abuse:

| Endpoint | Limit |
|----------|-------|
| All endpoints | 100 requests per minute |
| `POST /api/auth/login` | 5 requests per minute |
| `POST /api/auth/register` | 3 requests per minute |

When rate limited, the API returns HTTP `429 Too Many Requests`.

---

## Quick Start Example

```bash
# 1. Login as admin
curl -X POST {BASE_URL}/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@appilico.com","password":"Admin123!@#"}'

# 2. Use the returned accessToken
TOKEN="eyJhbGci..."

# 3. Browse products
curl {BASE_URL}/api/products?page=1&pageSize=10

# 4. Get featured products
curl {BASE_URL}/api/products/featured?count=5

# 5. Add to cart (authenticated)
curl -X POST {BASE_URL}/api/cart/items \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"productId":"PRODUCT_GUID","quantity":2}'

# 6. View cart
curl {BASE_URL}/api/cart \
  -H "Authorization: Bearer $TOKEN"

# 7. Create order from cart
curl -X POST {BASE_URL}/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"shippingAddressId":"ADDRESS_GUID","billingAddressId":"ADDRESS_GUID","paymentMethod":0}'
```

---

*Legend: 🔒 = Requires authentication (Bearer token). Role noted where restricted.*
