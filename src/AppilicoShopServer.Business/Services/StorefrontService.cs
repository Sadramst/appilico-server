using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Storefront;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Domain.Constants;
using Microsoft.Extensions.Options;

namespace AppilicoShopServer.Business.Services;

/// <summary>Builds public storefront metadata for generic shop clients.</summary>
public class StorefrontService : IStorefrontService
{
    private readonly StorefrontOptions _options;

    /// <summary>Initializes the storefront service.</summary>
    public StorefrontService(IOptions<StorefrontOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Task<ApiResponse<StorefrontConfigDto>> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var apiBasePath = NormalizeApiBasePath(_options.ApiBasePath);
        var config = new StorefrontConfigDto
        {
            EngineName = _options.EngineName,
            ApiVersion = "v1",
            Brand = new StorefrontBrandDto
            {
                StoreName = _options.StoreName,
                PublicBaseUrl = _options.PublicBaseUrl
            },
            Locale = new StorefrontLocaleDto
            {
                DefaultLocale = _options.DefaultLocale,
                SupportedLocales = _options.SupportedLocales,
                Currency = _options.Currency,
                Country = _options.Country
            },
            Theme = new StorefrontThemeDto
            {
                Preset = _options.ThemePreset,
                ProductCardFields = _options.ProductCardFields
            },
            Capabilities = new StorefrontCapabilitiesDto
            {
                ProductCatalog = true,
                CategoryTree = true,
                Brands = true,
                Cart = true,
                CustomerAccounts = true,
                Orders = true,
                Payments = true,
                Discounts = _options.EnableDiscounts,
                Vouchers = _options.EnableVouchers,
                Wishlist = _options.EnableWishlist,
                Reviews = _options.EnableReviews,
                Subscriptions = _options.EnableSubscriptions,
                Blog = _options.EnableBlog,
                Newsletter = _options.EnableNewsletter,
                Visuals = _options.EnableVisuals,
                StoreContextHeaders = _options.EnableStoreContextHeaders
            },
            Endpoints = BuildEndpoints(apiBasePath),
            Navigation = new StorefrontNavigationDto
            {
                Slots = _options.NavigationSlots,
                CategoryTreeEndpointId = "categories.tree"
            },
            Checkout = new StorefrontCheckoutPolicyDto
            {
                AllowGuestCheckout = _options.AllowGuestCheckout,
                CartEndpointId = "cart.current",
                CreateOrderEndpointId = "orders.create",
                CreatePaymentEndpointId = "payments.create"
            },
            Auth = new StorefrontAuthContractDto
            {
                TokenScheme = "Bearer",
                CustomerRole = AppConstants.Roles.Customer,
                PrivilegedRoles = new List<string>
                {
                    AppConstants.Roles.SuperAdmin,
                    AppConstants.Roles.Admin,
                    AppConstants.Roles.Manager
                }
            },
            GeneratedAtUtc = DateTime.UtcNow
        };

        return Task.FromResult(ApiResponse<StorefrontConfigDto>.SuccessResponse(config));
    }

    private static List<StorefrontEndpointDto> BuildEndpoints(string apiBasePath)
    {
        return new List<StorefrontEndpointDto>
        {
            Endpoint("auth.register", "POST", $"{apiBasePath}/auth/register", false, "Create a customer account"),
            Endpoint("auth.login", "POST", $"{apiBasePath}/auth/login", false, "Login and receive tokens"),
            Endpoint("auth.refresh", "POST", $"{apiBasePath}/auth/refresh-token", false, "Refresh access token"),
            Endpoint("auth.profile", "GET", $"{apiBasePath}/auth/profile", true, "Load authenticated user profile"),
            Endpoint("products.search", "GET", $"{apiBasePath}/products", false, "Browse and filter products"),
            Endpoint("products.featured", "GET", $"{apiBasePath}/products/featured", false, "Load featured products"),
            Endpoint("products.byId", "GET", $"{apiBasePath}/products/{{id}}", false, "Load product details"),
            Endpoint("products.bySku", "GET", $"{apiBasePath}/products/sku/{{sku}}", false, "Load product by SKU"),
            Endpoint("categories.list", "GET", $"{apiBasePath}/categories", false, "Load categories"),
            Endpoint("categories.tree", "GET", $"{apiBasePath}/categories/tree", false, "Load nested category navigation"),
            Endpoint("brands.list", "GET", $"{apiBasePath}/brands", false, "Load brands"),
            Endpoint("offers.active", "GET", $"{apiBasePath}/offers/active", false, "Load active merchandising offers"),
            Endpoint("discounts.active", "GET", $"{apiBasePath}/discounts/active", false, "Load active discounts"),
            Endpoint("vouchers.validate", "POST", $"{apiBasePath}/vouchers/validate", false, "Validate voucher before checkout"),
            Endpoint("cart.current", "GET", $"{apiBasePath}/cart", true, "Load current cart"),
            Endpoint("cart.add", "POST", $"{apiBasePath}/cart/items", true, "Add item to cart"),
            Endpoint("orders.mine", "GET", $"{apiBasePath}/orders/my", true, "Load customer orders"),
            Endpoint("orders.create", "POST", $"{apiBasePath}/orders", true, "Create order from cart"),
            Endpoint("payments.create", "POST", $"{apiBasePath}/payments", true, "Create payment"),
            Endpoint("wishlist.current", "GET", $"{apiBasePath}/wishlist", true, "Load wishlist"),
            Endpoint("reviews.product", "GET", $"{apiBasePath}/reviews/product/{{productId}}", false, "Load product reviews"),
            Endpoint("reviews.create", "POST", $"{apiBasePath}/reviews", true, "Create product review"),
            Endpoint("newsletter.subscribe", "POST", $"{apiBasePath}/newsletter/subscribe", false, "Subscribe to newsletter"),
            Endpoint("blog.list", "GET", $"{apiBasePath}/blog", false, "Load blog posts"),
            Endpoint("contact.create", "POST", $"{apiBasePath}/contact", false, "Send contact message"),
            Endpoint("waitlist.subscribe", "POST", $"{apiBasePath}/waitlist/subscribe", false, "Join waitlist"),
            Endpoint("subscriptions.current", "GET", $"{apiBasePath}/subscription/current", true, "Load current subscription"),
            Endpoint("visuals.list", "GET", $"{apiBasePath}/visuals", false, "Load digital visual products")
        };
    }

    private static StorefrontEndpointDto Endpoint(string id, string method, string path, bool requiresAuth, string useCase)
    {
        return new StorefrontEndpointDto
        {
            Id = id,
            Method = method,
            Path = path,
            RequiresAuth = requiresAuth,
            UseCase = useCase
        };
    }

    private static string NormalizeApiBasePath(string? apiBasePath)
    {
        if (string.IsNullOrWhiteSpace(apiBasePath))
            return "/api";

        var trimmed = apiBasePath.Trim().TrimEnd('/');
        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }
}
