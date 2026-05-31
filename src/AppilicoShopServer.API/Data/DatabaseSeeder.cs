using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Constants;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.API.Data;

/// <summary>
/// Seeds the database with initial data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>Seeds all initial data.</summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            var customers = await SeedUsersAsync(userManager, context);
            await SeedCategoriesAsync(context);
            await SeedBrandsAsync(context);
            await SeedProductsAsync(context);
            await SeedProductImagesAsync(context);
            await SeedDiscountsAsync(context);
            await SeedVouchersAsync(context);
            await SeedSpecialOffersAsync(context);
            await SeedOrdersAndPaymentsAsync(context, customers);
            await SeedReviewsAsync(context, customers);
            await SeedSettingsAsync(context);
            await SeedBlogPostsAsync(context);
            await SeedVisualsAsync(context);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        string[] roles = { AppConstants.Roles.SuperAdmin, AppConstants.Roles.Admin, AppConstants.Roles.Manager, AppConstants.Roles.Customer };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new AppRole { Name = role });
        }
    }

    private static async Task<List<Customer>> SeedUsersAsync(UserManager<AppUser> userManager, AppDbContext context)
    {
        var existingCustomers = await context.Customers.ToListAsync();
        if (await userManager.FindByEmailAsync("admin@appilico.com") != null && existingCustomers.Any())
            return existingCustomers;

        // SuperAdmin — full system access
        var superAdmin = new AppUser
        {
            UserName = "admin@appilico.com.au",
            Email = "admin@appilico.com.au",
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var r = await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        if (r.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, AppConstants.Roles.SuperAdmin);
            await userManager.AddToRoleAsync(superAdmin, AppConstants.Roles.Admin);
        }

        // Admin
        var admin = new AppUser { UserName = "admin@appilico.com", Email = "admin@appilico.com", FirstName = "System", LastName = "Admin", EmailConfirmed = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        r = await userManager.CreateAsync(admin, "Admin@123!");
        if (r.Succeeded) await userManager.AddToRoleAsync(admin, AppConstants.Roles.Admin);

        // Manager
        var manager = new AppUser { UserName = "manager@appilico.com", Email = "manager@appilico.com", FirstName = "Store", LastName = "Manager", EmailConfirmed = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        r = await userManager.CreateAsync(manager, "Manager@123!");
        if (r.Succeeded) await userManager.AddToRoleAsync(manager, AppConstants.Roles.Manager);

        // 5 Customers
        var customerData = new[]
        {
            ("customer1@appilico.com", "John", "Doe", "Customer@123!", MembershipTier.Bronze),
            ("customer2@appilico.com", "Jane", "Smith", "Customer@123!", MembershipTier.Silver),
            ("customer3@appilico.com", "Robert", "Johnson", "Customer@123!", MembershipTier.Gold),
            ("customer4@appilico.com", "Emily", "Davis", "Customer@123!", MembershipTier.Bronze),
            ("customer5@appilico.com", "Michael", "Wilson", "Customer@123!", MembershipTier.Silver),
        };

        var customers = new List<Customer>();
        int code = 1;
        foreach (var (email, first, last, pass, tier) in customerData)
        {
            var user = new AppUser { UserName = email, Email = email, FirstName = first, LastName = last, EmailConfirmed = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            r = await userManager.CreateAsync(user, pass);
            if (r.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);
                var cust = new Customer
                {
                    UserId = user.Id,
                    CustomerCode = $"CUST-{code:D8}",
                    JoinDate = DateTime.UtcNow.AddDays(-code * 30),
                    MembershipTier = tier,
                    LoyaltyPoints = code * 150,
                    TotalPurchases = code * 250m,
                    CreatedBy = "system"
                };
                context.Customers.Add(cust);
                customers.Add(cust);
                code++;
            }
        }
        await context.SaveChangesAsync();

        // Add addresses for all customers
        var cities = new[] { "Perth", "Melbourne", "Sydney", "Brisbane", "Adelaide" };
        var states = new[] { "WA", "VIC", "NSW", "QLD", "SA" };
        var zips = new[] { "6000", "3000", "2000", "4000", "5000" };
        for (int i = 0; i < customers.Count; i++)
        {
            context.CustomerAddresses.Add(new CustomerAddress
            {
                CustomerId = customers[i].Id,
                AddressType = AddressType.Shipping,
                Title = "Home",
                AddressLine1 = $"{10 + i} High Street",
                City = cities[i % cities.Length],
                State = states[i % states.Length],
                PostalCode = zips[i % zips.Length],
                Country = "AU",
                IsDefault = true,
                CreatedBy = "system"
            });
        }
        await context.SaveChangesAsync();
        return customers;
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var topLevel = new List<Category>
        {
            new() { Name = "Electronics", Description = "Laptops, phones, audio, cameras, gaming and more", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=800", SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Apparel & Fashion", Description = "Clothing, footwear, bags, watches and accessories", ImageUrl = "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=800", SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Home & Living", Description = "Furniture, kitchen, decor, lighting and outdoor", ImageUrl = "https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=800", SortOrder = 3, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(topLevel);
        await context.SaveChangesAsync();

        Category Top(string name) => topLevel.First(c => c.Name == name);

        var subCats = new List<Category>
        {
            // Electronics
            new() { Name = "Laptops & Computers", Description = "Ultrabooks, desktops and workstations", ParentCategoryId = Top("Electronics").Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Smartphones & Tablets", Description = "Phones, tablets and e-readers", ParentCategoryId = Top("Electronics").Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Audio & Headphones", Description = "Headphones, earbuds and speakers", ParentCategoryId = Top("Electronics").Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Wearables", Description = "Smartwatches and fitness trackers", ParentCategoryId = Top("Electronics").Id, SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Cameras", Description = "Mirrorless, action and instant cameras", ParentCategoryId = Top("Electronics").Id, SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Gaming", Description = "Consoles, controllers and accessories", ParentCategoryId = Top("Electronics").Id, SortOrder = 6, IsActive = true, CreatedBy = "system" },
            new() { Name = "TV & Home Theatre", Description = "Smart TVs, projectors and soundbars", ParentCategoryId = Top("Electronics").Id, SortOrder = 7, IsActive = true, CreatedBy = "system" },
            new() { Name = "Computer Accessories", Description = "Keyboards, mice, monitors and storage", ParentCategoryId = Top("Electronics").Id, SortOrder = 8, IsActive = true, CreatedBy = "system" },
            // Apparel & Fashion
            new() { Name = "Men's Clothing", Description = "Shirts, jackets, trousers and knitwear", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Women's Clothing", Description = "Dresses, tops, knitwear and outerwear", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Footwear", Description = "Sneakers, boots, sandals and formal shoes", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Bags & Luggage", Description = "Backpacks, totes, suitcases and wallets", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Watches & Jewellery", Description = "Watches, bracelets and accessories", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Activewear", Description = "Performance tops, leggings and shorts", ParentCategoryId = Top("Apparel & Fashion").Id, SortOrder = 6, IsActive = true, CreatedBy = "system" },
            // Home & Living
            new() { Name = "Furniture", Description = "Sofas, chairs, tables and storage", ParentCategoryId = Top("Home & Living").Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Kitchen & Dining", Description = "Cookware, appliances and tableware", ParentCategoryId = Top("Home & Living").Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Bedding & Bath", Description = "Linen, towels, pillows and quilts", ParentCategoryId = Top("Home & Living").Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Home Decor", Description = "Cushions, rugs, art and mirrors", ParentCategoryId = Top("Home & Living").Id, SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lighting", Description = "Lamps, pendants and smart lighting", ParentCategoryId = Top("Home & Living").Id, SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Garden & Outdoor", Description = "Outdoor furniture, planters and BBQs", ParentCategoryId = Top("Home & Living").Id, SortOrder = 6, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(subCats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(AppDbContext context)
    {
        if (await context.Brands.AnyAsync()) return;
        var brands = new List<Brand>
        {
            new() { Name = "Nimbus", Description = "Everyday electronics and smart devices", IsActive = true, CreatedBy = "system" },
            new() { Name = "Vertex", Description = "Performance computing and gaming gear", IsActive = true, CreatedBy = "system" },
            new() { Name = "Aterra", Description = "Sustainable home and living essentials", IsActive = true, CreatedBy = "system" },
            new() { Name = "Lumora", Description = "Lighting, audio and lifestyle tech", IsActive = true, CreatedBy = "system" },
            new() { Name = "Northwind", Description = "Outdoor, travel and activewear", IsActive = true, CreatedBy = "system" },
            new() { Name = "Cobalt", Description = "Premium accessories and wearables", IsActive = true, CreatedBy = "system" },
            new() { Name = "Sienna", Description = "Contemporary apparel and footwear", IsActive = true, CreatedBy = "system" },
            new() { Name = "Forma", Description = "Modern furniture and decor", IsActive = true, CreatedBy = "system" },
            new() { Name = "Pulse", Description = "Fitness, wearables and audio", IsActive = true, CreatedBy = "system" },
            new() { Name = "Evershade", Description = "Bedding, bath and soft furnishings", IsActive = true, CreatedBy = "system" },
            new() { Name = "Halcyon", Description = "Watches, jewellery and gifts", IsActive = true, CreatedBy = "system" },
            new() { Name = "Meridian", Description = "Kitchen, dining and small appliances", IsActive = true, CreatedBy = "system" },
        };
        context.Brands.AddRange(brands);
        await context.SaveChangesAsync();
    }

    /// <summary>Describes how to generate a batch of products for one subcategory.</summary>
    private sealed record CatalogSpec(
        string Category,
        string SkuPrefix,
        decimal BasePrice,
        decimal PriceStep,
        decimal Weight,
        string[] Models,
        string[] Qualifiers,
        string[] Images,
        bool SizeVariants);

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var categories = await context.Categories.ToListAsync();
        var brands = await context.Brands.ToListAsync();
        var cat = (string name) => categories.First(c => c.Name == name);

        var specs = BuildCatalogSpecs();
        var products = new List<Product>();
        var variantSeeds = new List<(string Sku, string[] Variants, bool Sizes)>();

        var brandIndex = 0;
        var featuredCounter = 0;
        foreach (var spec in specs)
        {
            var category = cat(spec.Category);
            var number = 0;
            foreach (var model in spec.Models)
            {
                foreach (var qualifier in spec.Qualifiers)
                {
                    number++;
                    var brand = brands[brandIndex % brands.Count];
                    brandIndex++;

                    var sku = $"{spec.SkuPrefix}-{number:D3}";
                    var name = $"{brand.Name} {model} {qualifier}".Trim();
                    var price = decimal.Round(spec.BasePrice + spec.PriceStep * (number - 1), 2);
                    var cost = decimal.Round(price * 0.55m, 2);
                    var stock = 25 + (number * 7 % 80);
                    var featured = (++featuredCounter % 6) == 0;
                    var description = $"{model} {qualifier} by {brand.Name}. Quality {spec.Category.ToLowerInvariant()} built for everyday use.";

                    products.Add(P(name, description, sku, category, brand, price, cost, stock, 5, true, featured, spec.Weight));

                    if (spec.SizeVariants && featured)
                        variantSeeds.Add((sku, new[] { "Small", "Medium", "Large" }, true));
                    else if (!spec.SizeVariants && featured && spec.SkuPrefix is "LAP" or "PHN")
                        variantSeeds.Add((sku, new[] { "128GB", "256GB", "512GB" }, false));
                }
            }
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        foreach (var (sku, variants, sizes) in variantSeeds)
        {
            var parent = products.First(p => p.SKU == sku);
            var i = 0;
            foreach (var variant in variants)
            {
                i++;
                var multiplier = sizes ? 1m : 1m + 0.2m * (i - 1);
                var code = sizes ? variant[..1].ToUpperInvariant() : variant.Replace("GB", string.Empty);
                context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = parent.Id,
                    VariantName = variant,
                    SKU = $"{sku}-{code}",
                    Price = decimal.Round(parent.BasePrice * multiplier, 2),
                    StockQuantity = 20,
                    Attributes = sizes ? $"{{\"size\":\"{variant}\"}}" : $"{{\"storage\":\"{variant}\"}}",
                    CreatedBy = "system"
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static List<CatalogSpec> BuildCatalogSpecs()
    {
        return new List<CatalogSpec>
        {
            new("Laptops & Computers", "LAP", 899m, 140m, 1.6m,
                new[] { "UltraBook", "ProBook", "Studio Desktop", "Mini PC", "Creator Laptop" },
                new[] { "13\"", "14\"", "15\"", "16\"" },
                ElectronicsImages, false),
            new("Smartphones & Tablets", "PHN", 449m, 110m, 0.3m,
                new[] { "Phone X", "Phone Pro", "Tablet Air", "Tablet Mini", "E-Reader" },
                new[] { "Black", "Silver", "Blue", "Graphite" },
                ElectronicsImages, false),
            new("Audio & Headphones", "AUD", 89m, 35m, 0.4m,
                new[] { "Wireless Headphones", "Noise-Cancelling Earbuds", "Bookshelf Speaker", "Portable Speaker", "Studio Monitor" },
                new[] { "Standard", "Plus", "Pro", "SE" },
                AudioImages, false),
            new("Wearables", "WER", 129m, 40m, 0.1m,
                new[] { "Smartwatch", "Fitness Band", "Sports Watch", "Hybrid Watch", "Kids Tracker" },
                new[] { "Black", "Rose", "Midnight", "Sand" },
                WearableImages, false),
            new("Cameras", "CAM", 379m, 120m, 0.6m,
                new[] { "Mirrorless Camera", "Action Camera", "Instant Camera", "Vlogging Kit", "Security Cam" },
                new[] { "Body Only", "Kit Lens", "Travel Kit", "Pro Bundle" },
                CameraImages, false),
            new("Gaming", "GAM", 59m, 45m, 0.5m,
                new[] { "Game Controller", "Gaming Headset", "Console Stand", "Racing Wheel", "Capture Card" },
                new[] { "Standard", "Elite", "Wireless", "RGB" },
                GamingImages, false),
            new("TV & Home Theatre", "TVH", 549m, 160m, 8m,
                new[] { "Smart TV 50\"", "Smart TV 65\"", "Soundbar", "Projector", "Media Streamer" },
                new[] { "Standard", "4K", "QLED", "Pro" },
                TvImages, false),
            new("Computer Accessories", "ACC", 39m, 18m, 0.4m,
                new[] { "Mechanical Keyboard", "Wireless Mouse", "USB-C Hub", "Webcam", "Portable SSD" },
                new[] { "Black", "White", "Compact", "Pro" },
                AccessoryImages, false),
            new("Men's Clothing", "MEN", 39m, 12m, 0.5m,
                new[] { "Oxford Shirt", "Chino Trousers", "Merino Sweater", "Bomber Jacket", "Polo Shirt" },
                new[] { "Navy", "Charcoal", "Olive", "Stone" },
                ApparelImages, true),
            new("Women's Clothing", "WMN", 45m, 13m, 0.45m,
                new[] { "Wrap Dress", "Linen Blouse", "Knit Cardigan", "Tailored Blazer", "Midi Skirt" },
                new[] { "Black", "Blush", "Sage", "Cream" },
                ApparelImages, true),
            new("Footwear", "FTW", 69m, 20m, 0.9m,
                new[] { "Running Sneaker", "Leather Boot", "Canvas Slip-On", "Hiking Shoe", "Dress Loafer" },
                new[] { "Black", "White", "Tan", "Grey" },
                FootwearImages, true),
            new("Bags & Luggage", "BAG", 59m, 22m, 1.1m,
                new[] { "Everyday Backpack", "Leather Tote", "Cabin Suitcase", "Crossbody Bag", "Travel Duffel" },
                new[] { "Black", "Tan", "Navy", "Olive" },
                BagImages, false),
            new("Watches & Jewellery", "WAT", 99m, 45m, 0.2m,
                new[] { "Automatic Watch", "Minimalist Watch", "Chronograph", "Bracelet Set", "Pendant Necklace" },
                new[] { "Silver", "Gold", "Rose Gold", "Black" },
                WatchImages, false),
            new("Activewear", "ACT", 35m, 11m, 0.3m,
                new[] { "Training Tee", "Compression Leggings", "Running Shorts", "Track Jacket", "Seamless Tank" },
                new[] { "Black", "Slate", "Teal", "Coral" },
                ActivewearImages, true),
            new("Furniture", "FUR", 199m, 95m, 14m,
                new[] { "Lounge Sofa", "Accent Chair", "Coffee Table", "Bookshelf", "Bed Frame" },
                new[] { "Oak", "Walnut", "Grey", "Charcoal" },
                FurnitureImages, false),
            new("Kitchen & Dining", "KIT", 39m, 24m, 1.5m,
                new[] { "Non-Stick Pan Set", "Stand Mixer", "Knife Block", "Dinnerware Set", "Espresso Machine" },
                new[] { "Standard", "Deluxe", "Compact", "Pro" },
                KitchenImages, false),
            new("Bedding & Bath", "BED", 49m, 16m, 1.2m,
                new[] { "Cotton Sheet Set", "Linen Quilt Cover", "Bath Towel Set", "Memory Pillow", "Waffle Robe" },
                new[] { "White", "Sage", "Charcoal", "Blush" },
                BeddingImages, false),
            new("Home Decor", "DEC", 29m, 18m, 0.8m,
                new[] { "Velvet Cushion", "Woven Rug", "Wall Art Print", "Round Mirror", "Ceramic Vase" },
                new[] { "Natural", "Terracotta", "Slate", "Ivory" },
                DecorImages, false),
            new("Lighting", "LGT", 35m, 22m, 1.0m,
                new[] { "Floor Lamp", "Table Lamp", "Pendant Light", "Smart Bulb Pack", "LED Strip Kit" },
                new[] { "Black", "Brass", "White", "Warm" },
                LightingImages, false),
            new("Garden & Outdoor", "GRD", 79m, 55m, 6m,
                new[] { "Outdoor Lounge Set", "Planter Box", "Charcoal BBQ", "Patio Umbrella", "Garden Tool Kit" },
                new[] { "Standard", "Large", "Premium", "Compact" },
                GardenImages, false),
        };
    }

    private static readonly string[] ElectronicsImages =
    {
        "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=800",
        "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800",
        "https://images.unsplash.com/photo-1593642702821-c8da6771f0c6?w=800",
        "https://images.unsplash.com/photo-1531297484001-80022131f5a1?w=800",
    };

    private static readonly string[] AudioImages =
    {
        "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800",
        "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=800",
        "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=800",
        "https://images.unsplash.com/photo-1583394838336-acd977736f90?w=800",
    };

    private static readonly string[] WearableImages =
    {
        "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800",
        "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=800",
        "https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?w=800",
        "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=800",
    };

    private static readonly string[] CameraImages =
    {
        "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=800",
        "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=800",
        "https://images.unsplash.com/photo-1495707902641-75cac588d2e9?w=800",
        "https://images.unsplash.com/photo-1606980625105-5e7d2eba2f0a?w=800",
    };

    private static readonly string[] GamingImages =
    {
        "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800",
        "https://images.unsplash.com/photo-1486401899868-0e435ed85128?w=800",
        "https://images.unsplash.com/photo-1592840496694-26d035b52b48?w=800",
        "https://images.unsplash.com/photo-1612801799890-4ba4760b6590?w=800",
    };

    private static readonly string[] TvImages =
    {
        "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=800",
        "https://images.unsplash.com/photo-1461151304267-38535e780c79?w=800",
        "https://images.unsplash.com/photo-1467293622093-9f15c96be70f?w=800",
        "https://images.unsplash.com/photo-1556089687-9e1f5b1c6f64?w=800",
    };

    private static readonly string[] AccessoryImages =
    {
        "https://images.unsplash.com/photo-1527814050087-3793815479db?w=800",
        "https://images.unsplash.com/photo-1545239351-1141bd82e8a6?w=800",
        "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800",
        "https://images.unsplash.com/photo-1625842268584-8f3296236761?w=800",
    };

    private static readonly string[] ApparelImages =
    {
        "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=800",
        "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=800",
        "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=800",
        "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=800",
    };

    private static readonly string[] FootwearImages =
    {
        "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800",
        "https://images.unsplash.com/photo-1460353581641-37baddab0fa2?w=800",
        "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=800",
        "https://images.unsplash.com/photo-1491553895911-0055eca6402d?w=800",
    };

    private static readonly string[] BagImages =
    {
        "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=800",
        "https://images.unsplash.com/photo-1547949003-9792a18a2601?w=800",
        "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=800",
        "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3?w=800",
    };

    private static readonly string[] WatchImages =
    {
        "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=800",
        "https://images.unsplash.com/photo-1547996160-81dfa63595aa?w=800",
        "https://images.unsplash.com/photo-1611591437281-460bfbe1220a?w=800",
        "https://images.unsplash.com/photo-1605100804763-247f67b3557e?w=800",
    };

    private static readonly string[] ActivewearImages =
    {
        "https://images.unsplash.com/photo-1556817411-31ae72fa3ea0?w=800",
        "https://images.unsplash.com/photo-1483721310020-03333e577078?w=800",
        "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=800",
        "https://images.unsplash.com/photo-1517344884509-a0c97ec11bcc?w=800",
    };

    private static readonly string[] FurnitureImages =
    {
        "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=800",
        "https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=800",
        "https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=800",
        "https://images.unsplash.com/photo-1538688525198-9b88f6f53126?w=800",
    };

    private static readonly string[] KitchenImages =
    {
        "https://images.unsplash.com/photo-1556909212-d5b604d0c90d?w=800",
        "https://images.unsplash.com/photo-1584990347449-a2d4c2c9b6e9?w=800",
        "https://images.unsplash.com/photo-1522184216316-3c25379f9760?w=800",
        "https://images.unsplash.com/photo-1495195134817-aeb325a55b65?w=800",
    };

    private static readonly string[] BeddingImages =
    {
        "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800",
        "https://images.unsplash.com/photo-1584100936595-c0654b55a2e2?w=800",
        "https://images.unsplash.com/photo-1567016432779-094069958ea5?w=800",
        "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=800",
    };

    private static readonly string[] DecorImages =
    {
        "https://images.unsplash.com/photo-1513519245088-0e12902e35aa?w=800",
        "https://images.unsplash.com/photo-1534349762230-e0cadf78f5da?w=800",
        "https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=800",
        "https://images.unsplash.com/photo-1567225557594-88d73e55f2cb?w=800",
    };

    private static readonly string[] LightingImages =
    {
        "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=800",
        "https://images.unsplash.com/photo-1524634126442-357e0eac3c14?w=800",
        "https://images.unsplash.com/photo-1540932239986-30128078f3c5?w=800",
        "https://images.unsplash.com/photo-1565636192335-cdba9786a4d9?w=800",
    };

    private static readonly string[] GardenImages =
    {
        "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=800",
        "https://images.unsplash.com/photo-1558904541-efa843a96f01?w=800",
        "https://images.unsplash.com/photo-1523575708161-ad0fc2a9b951?w=800",
        "https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?w=800",
    };

    private static Product P(string name, string desc, string sku, Category cat, Brand brand, decimal price, decimal cost, int stock, int min, bool active, bool featured, decimal weight)
        => new() { Name = name, Description = desc, SKU = sku, CategoryId = cat.Id, BrandId = brand.Id, BasePrice = price, CostPrice = cost, StockQuantity = stock, MinStockLevel = min, IsActive = active, IsFeatured = featured, Weight = weight, CreatedBy = "system" };

    private static async Task SeedProductImagesAsync(AppDbContext context)
    {
        if (await context.ProductImages.AnyAsync()) return;
        var products = await context.Products.OrderBy(p => p.SKU).ToListAsync();

        // Map each SKU prefix to a generic, category-appropriate image pool.
        var poolByPrefix = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["LAP"] = ElectronicsImages,
            ["PHN"] = ElectronicsImages,
            ["AUD"] = AudioImages,
            ["WER"] = WearableImages,
            ["CAM"] = CameraImages,
            ["GAM"] = GamingImages,
            ["TVH"] = TvImages,
            ["ACC"] = AccessoryImages,
            ["MEN"] = ApparelImages,
            ["WMN"] = ApparelImages,
            ["FTW"] = FootwearImages,
            ["BAG"] = BagImages,
            ["WAT"] = WatchImages,
            ["ACT"] = ActivewearImages,
            ["FUR"] = FurnitureImages,
            ["KIT"] = KitchenImages,
            ["BED"] = BeddingImages,
            ["DEC"] = DecorImages,
            ["LGT"] = LightingImages,
            ["GRD"] = GardenImages,
        };

        var images = new List<ProductImage>();
        var counter = 0;
        foreach (var product in products)
        {
            var prefix = product.SKU.Split('-').FirstOrDefault() ?? string.Empty;
            if (!poolByPrefix.TryGetValue(prefix, out var pool) || pool.Length == 0)
                continue;

            // Primary plus one secondary image, rotating through the pool.
            var primary = pool[counter % pool.Length];
            var secondary = pool[(counter + 1) % pool.Length];
            counter++;

            images.Add(new ProductImage { ProductId = product.Id, ImageUrl = primary, AltText = product.Name, SortOrder = 0, IsPrimary = true, CreatedBy = "system" });
            images.Add(new ProductImage { ProductId = product.Id, ImageUrl = secondary, AltText = product.Name, SortOrder = 1, IsPrimary = false, CreatedBy = "system" });
        }

        context.ProductImages.AddRange(images);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDiscountsAsync(AppDbContext context)
    {
        if (await context.Discounts.AnyAsync()) return;
        var now = DateTime.UtcNow;
        var discounts = new List<Discount>
        {
            new() { Code = "FIRSTORDER", Name = "First Order 15% Off", Description = "15% off your first online order", DiscountType = DiscountType.Percentage, Value = 15, MinOrderAmount = 30, MaxDiscountAmount = 50, StartDate = now, EndDate = now.AddDays(365), IsActive = true, UsageLimit = 10000, UsedCount = 0, CreatedBy = "system" },
            new() { Code = "TECH20", Name = "Tech Sale 20%", Description = "20% off selected electronics", DiscountType = DiscountType.Percentage, Value = 20, MinOrderAmount = 50, MaxDiscountAmount = 80, StartDate = now.AddDays(-5), EndDate = now.AddDays(45), IsActive = true, UsageLimit = 500, UsedCount = 38, CreatedBy = "system" },
            new() { Code = "HOME10", Name = "Home & Living $10 Off", Description = "$10 off home & living orders over $60", DiscountType = DiscountType.Fixed, Value = 10, MinOrderAmount = 60, StartDate = now.AddDays(-10), EndDate = now.AddDays(60), IsActive = true, UsageLimit = 300, UsedCount = 67, CreatedBy = "system" },
            new() { Code = "BUNDLE25", Name = "Bundle 25% Off", Description = "25% off orders over $150", DiscountType = DiscountType.Percentage, Value = 25, MinOrderAmount = 150, MaxDiscountAmount = 120, StartDate = now, EndDate = now.AddDays(30), IsActive = true, UsageLimit = 200, UsedCount = 12, CreatedBy = "system" },
            new() { Code = "FREESHIP", Name = "Free Shipping", Description = "Free shipping on orders over $80", DiscountType = DiscountType.Fixed, Value = 9.99m, MinOrderAmount = 80, StartDate = now.AddDays(-30), EndDate = now.AddDays(60), IsActive = true, UsageLimit = 1000, UsedCount = 189, CreatedBy = "system" },
        };
        context.Discounts.AddRange(discounts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedVouchersAsync(AppDbContext context)
    {
        if (await context.Vouchers.AnyAsync()) return;
        var now = DateTime.UtcNow;
        var vouchers = new List<Voucher>
        {
            new() { Code = "GIFT25", Description = "$25 gift voucher", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 25, StartDate = now.AddDays(-30), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
            new() { Code = "GIFT50", Description = "$50 gift voucher", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 50, StartDate = now.AddDays(-15), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
            new() { Code = "LOYAL15", Description = "15% off for returning customers", VoucherType = VoucherType.Promo, ValueType = VoucherValueType.Percentage, Value = 15, StartDate = now, ExpiryDate = now.AddDays(90), IsActive = true, MaxRedemptions = 100, CurrentRedemptions = 8, CreatedBy = "system" },
            new() { Code = "WELCOME10", Description = "$10 welcome voucher for new members", VoucherType = VoucherType.Reward, ValueType = VoucherValueType.Fixed, Value = 10, StartDate = now.AddDays(-60), ExpiryDate = now.AddDays(60), IsActive = true, MaxRedemptions = 500, CurrentRedemptions = 55, CreatedBy = "system" },
            new() { Code = "REFER20", Description = "20% off referral reward", VoucherType = VoucherType.Promo, ValueType = VoucherValueType.Percentage, Value = 20, StartDate = now, ExpiryDate = now.AddDays(365), IsActive = true, MaxRedemptions = 5000, CurrentRedemptions = 0, CreatedBy = "system" },
        };
        context.Vouchers.AddRange(vouchers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSpecialOffersAsync(AppDbContext context)
    {
        if (await context.SpecialOffers.AnyAsync()) return;
        var now = DateTime.UtcNow;
        var products = await context.Products.ToListAsync();

        var offers = new List<SpecialOffer>
        {
            new() { Name = "Tech Weekend Deals", Description = "Save on laptops, phones and audio this weekend", OfferType = OfferType.Bundle, StartDate = now, EndDate = now.AddDays(3), IsActive = true, CreatedBy = "system" },
            new() { Name = "Wardrobe Refresh", Description = "Flash sale on apparel and footwear", OfferType = OfferType.Flash, StartDate = now.AddDays(-1), EndDate = now.AddDays(7), IsActive = true, CreatedBy = "system" },
            new() { Name = "Home Makeover Sale", Description = "15% off furniture, kitchen and decor", OfferType = OfferType.Seasonal, StartDate = now, EndDate = now.AddDays(60), IsActive = true, CreatedBy = "system" },
        };
        context.SpecialOffers.AddRange(offers);
        await context.SaveChangesAsync();

        // Link products to offers by category prefix
        var techProducts = products.Where(p => p.SKU.StartsWith("LAP") || p.SKU.StartsWith("PHN") || p.SKU.StartsWith("AUD")).ToList();
        foreach (var p in techProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[0].Id, ProductId = p.Id, CreatedBy = "system" });

        var apparelProducts = products.Where(p => p.SKU.StartsWith("MEN") || p.SKU.StartsWith("WMN") || p.SKU.StartsWith("FTW")).ToList();
        foreach (var p in apparelProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[1].Id, ProductId = p.Id, CreatedBy = "system" });

        var homeProducts = products.Where(p => p.SKU.StartsWith("FUR") || p.SKU.StartsWith("KIT") || p.SKU.StartsWith("DEC")).ToList();
        foreach (var p in homeProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[2].Id, ProductId = p.Id, CreatedBy = "system" });

        await context.SaveChangesAsync();
    }

    private static async Task SeedOrdersAndPaymentsAsync(AppDbContext context, List<Customer> customers)
    {
        if (await context.Orders.AnyAsync() || customers.Count == 0) return;
        var products = await context.Products.Take(20).ToListAsync();
        if (products.Count == 0) return;

        var rng = new Random(42); // fixed seed for reproducibility
        var now = DateTime.UtcNow;

        // Create 10 sample orders across customers
        for (int i = 0; i < 10; i++)
        {
            var customer = customers[i % customers.Count];
            var orderDate = now.AddDays(-rng.Next(1, 60));
            var itemCount = rng.Next(1, 4);
            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;

            for (int j = 0; j < itemCount; j++)
            {
                var product = products[rng.Next(products.Count)];
                var qty = rng.Next(1, 3);
                var price = product.BasePrice;
                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = qty,
                    UnitPrice = price,
                    TotalPrice = price * qty,
                    CreatedBy = "system"
                });
                subtotal += price * qty;
            }

            var tax = Math.Round(subtotal * 0.10m, 2);
            var shipping = subtotal >= 100 ? 0m : 9.99m;
            var status = i < 3 ? OrderStatus.Delivered : i < 6 ? OrderStatus.Processing : OrderStatus.Pending;

            // Use the first customer address for shipping/billing
            var address = await context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerId == customer.Id);
            if (address == null) continue;

            var order = new Order
            {
                OrderNumber = $"ORD-{(i + 1):D6}",
                CustomerId = customer.Id,
                OrderDate = orderDate,
                OrderStatus = status,
                SubTotal = subtotal,
                TaxAmount = tax,
                ShippingAmount = shipping,
                TotalAmount = subtotal + tax + shipping,
                ShippingAddressId = address.Id,
                BillingAddressId = address.Id,
                Items = orderItems,
                CreatedBy = "system"
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Add status history
            context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = order.Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Pending, Notes = "Order placed", CreatedBy = "system" });
            if (status != OrderStatus.Pending)
                context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = order.Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, Notes = "Order is being processed", CreatedBy = "system" });
            if (status == OrderStatus.Delivered)
                context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = order.Id, OldStatus = OrderStatus.Processing, NewStatus = OrderStatus.Delivered, Notes = "Order delivered", CreatedBy = "system" });

            // Create payment for non-pending orders
            if (status != OrderStatus.Pending)
            {
                context.Payments.Add(new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Paid,
                    TransactionId = $"TXN-{Guid.NewGuid():N}"[..20],
                    CreatedBy = "system"
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewsAsync(AppDbContext context, List<Customer> customers)
    {
        if (await context.ProductReviews.AnyAsync() || customers.Count == 0) return;
        var products = await context.Products.Take(15).ToListAsync();
        if (products.Count == 0) return;

        var reviews = new List<ProductReview>();
        var comments = new[]
        {
            ("Excellent quality", "Exactly as described and well made. Very happy with this purchase."),
            ("Great value", "Fantastic for the price. Would buy again without hesitation."),
            ("Highly recommend", "Arrived quickly and works perfectly. Five stars."),
            ("Love it", "Looks even better in person. Exactly what I was after."),
            ("Solid purchase", "Good build quality and does the job well."),
            ("Very happy", "Comfortable, stylish and durable. No complaints at all."),
            ("Impressive", "Exceeded my expectations. The finish is premium."),
            ("Perfect fit", "True to size and great materials. Will order more."),
            ("Works great", "Easy to set up and performs reliably every day."),
            ("Beautiful design", "Stylish and practical. A great addition to my home."),
        };

        var rng = new Random(123);
        for (int i = 0; i < 20; i++)
        {
            var customer = customers[i % customers.Count];
            var product = products[i % products.Count];
            var (title, comment) = comments[i % comments.Length];
            reviews.Add(new ProductReview
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                Rating = rng.Next(3, 6),
                Title = title,
                Comment = comment,
                IsApproved = i < 15,
                CreatedBy = "system"
            });
        }

        context.ProductReviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.AppSettings.AnyAsync()) return;
        var settings = new List<AppSetting>
        {
            new() { Key = "Store.Name", Value = "Appilico Shop", Group = "General", Description = "Store display name", CreatedBy = "system" },
            new() { Key = "Store.Currency", Value = "AUD", Group = "General", Description = "Default currency", CreatedBy = "system" },
            new() { Key = "Store.TaxRate", Value = "10", Group = "General", Description = "GST rate percentage", CreatedBy = "system" },
            new() { Key = "Store.DeliveryFee", Value = "9.99", Group = "Delivery", Description = "Default delivery fee", CreatedBy = "system" },
            new() { Key = "Store.FreeDeliveryThreshold", Value = "80", Group = "Delivery", Description = "Free delivery threshold amount", CreatedBy = "system" },
            new() { Key = "Loyalty.PointsPerDollar", Value = "2", Group = "Loyalty", Description = "Loyalty points earned per dollar spent", CreatedBy = "system" },
            new() { Key = "Loyalty.PointsRedemptionRate", Value = "100", Group = "Loyalty", Description = "Points needed for $1 discount", CreatedBy = "system" },
        };
        context.AppSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBlogPostsAsync(AppDbContext context)
    {
        // Idempotent: check by canonical slug
        if (await context.BlogPosts.AnyAsync(p => p.Slug == "why-australian-mining-companies-are-moving-to-real-time-analytics"))
            return;

        var posts = new[]
        {
            new Domain.Entities.BlogPost
            {
                Title = "Why Australian Mining Companies Are Moving to Real-Time Analytics",
                Slug = "why-australian-mining-companies-are-moving-to-real-time-analytics",
                Excerpt = "Discover how leading Australian mining operations are leveraging real-time data analytics to cut costs, improve safety, and boost productivity in an increasingly competitive global market.",
                Content = @"<p>Australia's mining sector contributes over $200 billion annually to the national economy, yet many operations still rely on manual reporting processes that are days or even weeks behind reality. In 2026, that's changing fast.</p>

<h2>The Pressure to Modernise</h2>
<p>Iron ore, gold, and lithium producers across Western Australia, Queensland, and the Northern Territory are facing simultaneous pressure from investors demanding ESG transparency, regulators requiring stricter safety reporting, and a global market where operational efficiency determines margin.</p>

<p>The companies that once relied on weekly Excel reports are now operating at a fundamental disadvantage. When a competitor can identify a throughput bottleneck in real-time and correct it before the shift ends, the cumulative gap in output becomes enormous.</p>

<h2>The Shift to Real-Time</h2>
<p>Real-time analytics platforms allow site managers to monitor equipment performance, ore grades, and safety metrics as they happen. When a conveyor belt starts showing signs of failure, you know in seconds — not the next morning when the shift report lands on your desk.</p>

<p>Modern IoT sensor networks, combined with edge computing capabilities, now make it possible to stream data from even the most remote operations to cloud analytics platforms with sub-second latency. The infrastructure cost that once made this prohibitive for mid-tier miners has dropped by over 80% in five years.</p>

<h2>Key Benefits Seen Across the Industry</h2>
<ul>
<li><strong>Cost reduction:</strong> Predictive maintenance alone can save 15–25% on equipment costs.</li>
<li><strong>Safety improvements:</strong> Real-time hazard monitoring reduces incidents by up to 30%.</li>
<li><strong>Productivity gains:</strong> Optimised scheduling and throughput analysis increase output without additional capital expenditure.</li>
<li><strong>ESG reporting:</strong> Automated emissions tracking and water usage monitoring satisfy investor and regulatory requirements without additional headcount.</li>
</ul>

<h2>What's Driving Adoption Now</h2>
<p>Power BI custom visuals tailored to mining operations have made it dramatically easier to visualise complex operational data. Instead of Excel spreadsheets, site managers now work with interactive dashboards showing ore grade waterfalls, equipment heatmaps, and AI-powered natural language query interfaces.</p>

<p>The combination of affordable cloud infrastructure, purpose-built mining analytics tools, and pressure to reduce operating costs has made this moment a tipping point for the industry. The question is no longer whether to adopt real-time analytics — it's how quickly you can get there before competitors extend their lead.</p>

<h2>Starting Points for Mine Operators</h2>
<p>Most successful implementations start narrow: pick one high-value use case, such as equipment utilisation or production reconciliation, and build a proof of concept in 4–6 weeks. Demonstrating ROI on a single process makes the board-level case for broader rollout straightforward.</p>",
                Category = "Analytics",
                Author = "Appilico Engineering",
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                ReadTimeMinutes = 6,
                Tags = "analytics,mining,real-time,Australia,Power BI",
                IsPublished = true
            },
            new Domain.Entities.BlogPost
            {
                Title = "Power BI Custom Visuals: A Technical Guide for Mining Analytics",
                Slug = "power-bi-custom-visuals-technical-guide",
                Excerpt = "Standard Power BI charts weren't built for mining. Learn how custom visuals designed specifically for resource extraction give your team the insights they actually need — and how they're built.",
                Content = @"<p>Power BI ships with dozens of built-in chart types, but when you're trying to visualise the production output of a 24/7 open-cut gold mine, a standard bar chart simply doesn't cut it. This guide covers the technical architecture of mining-specific custom visuals and what to look for when evaluating them.</p>

<h2>What Are Custom Visuals?</h2>
<p>Custom visuals are independently developed visualisation components built using the Power BI Visuals SDK, which wraps a TypeScript and D3.js rendering engine. They're packaged as .pbiviz files, can be imported into Power BI Desktop and the Power BI Service, and follow the same data-binding model as native visuals.</p>

<p>Unlike native visuals, custom visuals can implement any rendering logic — from specialised Gantt charts that understand shift patterns to AI-powered natural language query panels that let operators ask plain-English questions about their data.</p>

<h2>The Technical Architecture</h2>
<p>A well-built mining custom visual has three layers:</p>
<ol>
<li><strong>Data mapping layer:</strong> Translates Power BI's tabular data model into the domain objects the visual understands (shifts, equipment IDs, ore grades, etc.).</li>
<li><strong>Calculation layer:</strong> Performs mining-specific calculations such as OEE (Overall Equipment Effectiveness), TRIFR (Total Recordable Injury Frequency Rate), or ore recovery percentage.</li>
<li><strong>Rendering layer:</strong> Uses D3.js or WebGL to render the visual, with support for Power BI's theme system, selection/cross-filtering, and tooltips.</li>
</ol>

<h2>Mining-Specific Visuals That Make a Difference</h2>
<ul>
<li><strong>Production Gantt Charts:</strong> Track planned vs actual production schedules across multiple equipment units simultaneously, with drill-down by shift, crew, and activity type.</li>
<li><strong>Equipment Heatmaps:</strong> Visualise utilisation rates, downtime patterns, and maintenance windows across your entire fleet on a single canvas.</li>
<li><strong>Ore Grade Waterfall:</strong> Understand the grade profile from blast to processing plant in a single view, with variance analysis against model.</li>
<li><strong>Safety KPI Dashboards:</strong> TRIFR, LTIFR, near-miss rates, and compliance metrics rendered in a regulatory-ready format for safety committee reporting.</li>
<li><strong>Cost Per Tonne Analysis:</strong> Drill down from site-level to activity-level costs with a single click, segmented by equipment class, shift, or ore type.</li>
<li><strong>AI Insights Panel:</strong> Natural language Q&A interface powered by Azure OpenAI, letting operators query operational data without needing SQL or DAX skills.</li>
</ul>

<h2>Performance Considerations at Scale</h2>
<p>Mining datasets are large. A single site might generate 50,000+ data points per shift across hundreds of sensors. Custom visuals that don't implement data reduction strategies — aggregation, windowing, or server-side pagination — will degrade Power BI report performance significantly.</p>

<p>Look for visuals that explicitly document their data volume limits and provide configuration options for aggregation granularity.</p>

<h2>Integration with Common Mining Systems</h2>
<p>The most valuable custom visuals are those that understand the data schemas coming out of common mining systems: Wenco, Modular Mining, Pronto Xi, SAP PM, and OSIsoft PI. Pre-built connectors dramatically reduce the time-to-value compared to custom data transformation work.</p>",
                Category = "Technology",
                Author = "Appilico Engineering",
                PublishedAt = DateTime.UtcNow.AddDays(-20),
                ReadTimeMinutes = 7,
                Tags = "Power BI,custom visuals,mining,analytics,SDK,D3.js",
                IsPublished = true
            },
            new Domain.Entities.BlogPost
            {
                Title = "Azure vs On-Premises: The Right Analytics Infrastructure for WA Mining",
                Slug = "azure-vs-on-premises-wa-mining",
                Excerpt = "For Western Australian mining operations considering analytics infrastructure, the Azure vs on-premises debate has a more nuanced answer than the cloud vendors would have you believe.",
                Content = @"<p>The cloud-first narrative has dominated enterprise IT for a decade. But Western Australian mining operations have some of the world's most challenging connectivity environments — remote sites, intermittent satellite links, and data sovereignty requirements that don't always fit neatly into the standard cloud pitch.</p>

<p>Here's an honest comparison of Azure and on-premises infrastructure for mining analytics workloads in WA, based on what we've seen work and fail at real operations.</p>

<h2>Where Azure Wins</h2>
<p><strong>Elastic compute:</strong> Month-end reporting, annual reconciliations, and intensive modelling workloads can spike compute requirements 10–20x above the daily baseline. Azure's pay-per-use model means you only pay for that capacity when you need it.</p>

<p><strong>Managed services:</strong> Azure Synapse Analytics, Azure Data Factory, and Power BI Premium handle the operational overhead of running a data warehouse. Your team focuses on analysis, not patching database servers.</p>

<p><strong>Disaster recovery:</strong> Geo-redundant storage and automated backups across Azure regions provide a level of resilience that would cost millions to replicate with on-premises hardware.</p>

<p><strong>AI and ML capabilities:</strong> Azure OpenAI Service, Azure Machine Learning, and Cognitive Services give mining analysts access to AI capabilities that would require a dedicated data science team to build in-house.</p>

<h2>Where On-Premises Still Has the Edge</h2>
<p><strong>Latency at the edge:</strong> If your SCADA system needs sub-100ms response times and you're operating from a remote site with 200ms satellite latency to the nearest Azure region, cloud isn't the right answer for that specific workload.</p>

<p><strong>Data sovereignty:</strong> Some mining operations, particularly those involving joint ventures with government entities or working on culturally sensitive land, have data residency requirements that make cloud storage legally complicated.</p>

<p><strong>Connectivity reliability:</strong> A site running on a 50Mbps VSAT link with 5–10% packet loss is not an environment where cloud-dependent systems will perform reliably. On-premises systems continue operating during outages.</p>

<h2>The Hybrid Architecture That Actually Works</h2>
<p>For most WA mining operations, the answer isn't either/or. The architecture that's proven most reliable combines:</p>
<ul>
<li>On-premises edge servers at each site for real-time SCADA data collection, local dashboards, and operational system integration</li>
<li>Azure Synapse Analytics as the central data warehouse, receiving batched data from sites on a scheduled cadence (typically hourly)</li>
<li>Power BI Premium for corporate reporting, with data cached in the cloud so corporate users aren't dependent on site connectivity</li>
<li>Azure IoT Hub for streaming sensor data from sites with adequate connectivity</li>
</ul>

<h2>Making the Decision</h2>
<p>The starting question isn't 'Azure or on-premises' — it's 'what are the latency, connectivity, sovereignty, and cost requirements for each workload?' Map those requirements to the capabilities of each platform, and the right architecture usually becomes clear.</p>

<p>What we consistently find is that the real-time operational layer benefits from on-premises or edge compute, while the analytical and reporting layer benefits from cloud scale and managed services. The two aren't mutually exclusive.</p>",
                Category = "Infrastructure",
                Author = "Appilico Engineering",
                PublishedAt = DateTime.UtcNow.AddDays(-35),
                ReadTimeMinutes = 8,
                Tags = "Azure,on-premises,infrastructure,mining,cloud,Western Australia",
                IsPublished = true
            }
        };

        context.BlogPosts.AddRange(posts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedVisualsAsync(AppDbContext context)
    {
        if (await context.Visuals.AnyAsync(v => v.Slug == "mine-production-gantt"))
            return;

        var visuals = new[]
        {
            new Domain.Entities.Visual
            {
                Slug = "mine-production-gantt",
                Name = "Mine Production Gantt",
                Description = "Shift-level drill-down with crew and equipment breakdown",
                Category = Domain.Enums.VisualCategory.Production,
                RequiredPlan = Domain.Enums.SubscriptionTier.Starter,
                Tags = "[\"production\",\"gantt\",\"shifts\",\"mining\"]",
                Type = "Gantt",
                IsActive = true,
                SortOrder = 1,
                CreatedBy = "system"
            },
            new Domain.Entities.Visual
            {
                Slug = "equipment-utilisation-heatmap",
                Name = "Equipment Utilisation Heatmap",
                Description = "Real-time OEE tracking across your fleet with hourly breakdown",
                Category = Domain.Enums.VisualCategory.Equipment,
                RequiredPlan = Domain.Enums.SubscriptionTier.Starter,
                Tags = "[\"equipment\",\"heatmap\",\"oee\",\"fleet\"]",
                Type = "Heatmap",
                IsActive = true,
                SortOrder = 2,
                CreatedBy = "system"
            },
            new Domain.Entities.Visual
            {
                Slug = "safety-kpi-dashboard",
                Name = "Safety KPI Dashboard",
                Description = "Leading and lagging indicators with automated trend alerts",
                Category = Domain.Enums.VisualCategory.Safety,
                RequiredPlan = Domain.Enums.SubscriptionTier.Starter,
                Tags = "[\"safety\",\"kpi\",\"trifr\",\"ltifr\"]",
                Type = "KPI",
                IsActive = true,
                SortOrder = 3,
                CreatedBy = "system"
            },
            new Domain.Entities.Visual
            {
                Slug = "ore-grade-waterfall",
                Name = "Ore Grade Waterfall",
                Description = "Grade tracking from bench sample to plant output with variance",
                Category = Domain.Enums.VisualCategory.Quality,
                RequiredPlan = Domain.Enums.SubscriptionTier.Professional,
                Tags = "[\"ore\",\"grade\",\"waterfall\",\"quality\"]",
                Type = "Waterfall",
                IsActive = true,
                SortOrder = 4,
                CreatedBy = "system"
            },
            new Domain.Entities.Visual
            {
                Slug = "cost-per-tonne-tracker",
                Name = "Cost Per Tonne Tracker",
                Description = "Operational cost breakdown by category with anomaly detection",
                Category = Domain.Enums.VisualCategory.Finance,
                RequiredPlan = Domain.Enums.SubscriptionTier.Professional,
                Tags = "[\"cost\",\"finance\",\"anomaly\",\"tonne\"]",
                Type = "Analytics",
                IsActive = true,
                SortOrder = 5,
                CreatedBy = "system"
            },
            new Domain.Entities.Visual
            {
                Slug = "ai-natural-language-query",
                Name = "AI Natural Language Query Panel",
                Description = "Ask questions about your operations in plain English",
                Category = Domain.Enums.VisualCategory.AI,
                RequiredPlan = Domain.Enums.SubscriptionTier.Enterprise,
                Tags = "[\"ai\",\"nlp\",\"query\",\"language\"]",
                Type = "AI",
                IsActive = true,
                SortOrder = 6,
                CreatedBy = "system"
            }
        };

        context.Visuals.AddRange(visuals);
        await context.SaveChangesAsync();
    }
}
