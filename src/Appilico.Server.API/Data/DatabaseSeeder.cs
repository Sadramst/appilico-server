using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Constants;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.API.Data;

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
            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            var customers = await SeedUsersAsync(userManager, context);
            await SeedCategoriesAsync(context);
            await SeedBrandsAsync(context);
            await SeedProductsAsync(context);
            await SeedDiscountsAsync(context);
            await SeedVouchersAsync(context);
            await SeedSpecialOffersAsync(context);
            await SeedOrdersAndPaymentsAsync(context, customers);
            await SeedReviewsAsync(context, customers);
            await SeedSettingsAsync(context);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        string[] roles = { AppConstants.Roles.Admin, AppConstants.Roles.Manager, AppConstants.Roles.Customer };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new AppRole { Name = role });
        }
    }

    private static async Task<List<Customer>> SeedUsersAsync(UserManager<AppUser> userManager, AppDbContext context)
    {
        if (await userManager.FindByEmailAsync("admin@appilico.com") != null)
            return await context.Customers.ToListAsync();

        // Admin
        var admin = new AppUser { UserName = "admin@appilico.com", Email = "admin@appilico.com", FirstName = "System", LastName = "Admin", EmailConfirmed = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var r = await userManager.CreateAsync(admin, "Admin@123!");
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
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix" };
        var states = new[] { "NY", "CA", "IL", "TX", "AZ" };
        var zips = new[] { "10001", "90001", "60601", "77001", "85001" };
        for (int i = 0; i < customers.Count; i++)
        {
            context.CustomerAddresses.Add(new CustomerAddress
            {
                CustomerId = customers[i].Id,
                AddressType = AddressType.Shipping,
                Title = "Home",
                AddressLine1 = $"{100 + i} Main Street",
                City = cities[i % cities.Length],
                State = states[i % states.Length],
                PostalCode = zips[i % zips.Length],
                Country = "US",
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
            new() { Name = "Electronics", Description = "Electronic devices and gadgets", SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Clothing", Description = "Apparel and fashion", SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Home & Garden", Description = "Home improvement and garden", SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Sports & Outdoors", Description = "Sporting goods and outdoor equipment", SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Books & Media", Description = "Books, music, and media", SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Health & Beauty", Description = "Health care and beauty products", SortOrder = 6, IsActive = true, CreatedBy = "system" },
            new() { Name = "Toys & Games", Description = "Toys, games, and hobbies", SortOrder = 7, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(topLevel);
        await context.SaveChangesAsync();

        var subCats = new List<Category>
        {
            new() { Name = "Smartphones", Description = "Mobile phones", ParentCategoryId = topLevel[0].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Laptops", Description = "Laptop computers", ParentCategoryId = topLevel[0].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Accessories", Description = "Electronic accessories", ParentCategoryId = topLevel[0].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Tablets", Description = "Tablet devices", ParentCategoryId = topLevel[0].Id, SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Men's Clothing", Description = "Men's apparel", ParentCategoryId = topLevel[1].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Women's Clothing", Description = "Women's apparel", ParentCategoryId = topLevel[1].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Furniture", Description = "Home furniture", ParentCategoryId = topLevel[2].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Kitchen", Description = "Kitchen appliances and tools", ParentCategoryId = topLevel[2].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(subCats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(AppDbContext context)
    {
        if (await context.Brands.AnyAsync()) return;
        var brands = new List<Brand>
        {
            new() { Name = "TechPro", Description = "Premium technology products", IsActive = true, CreatedBy = "system" },
            new() { Name = "StyleCo", Description = "Fashion and lifestyle brand", IsActive = true, CreatedBy = "system" },
            new() { Name = "HomeEssentials", Description = "Home and living essentials", IsActive = true, CreatedBy = "system" },
            new() { Name = "SportMax", Description = "Sports and fitness equipment", IsActive = true, CreatedBy = "system" },
            new() { Name = "ReadMore", Description = "Books and publications", IsActive = true, CreatedBy = "system" },
            new() { Name = "GlowUp", Description = "Beauty and skincare", IsActive = true, CreatedBy = "system" },
            new() { Name = "FunZone", Description = "Toys and entertainment", IsActive = true, CreatedBy = "system" },
            new() { Name = "NatureFit", Description = "Natural health products", IsActive = true, CreatedBy = "system" },
        };
        context.Brands.AddRange(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var categories = await context.Categories.ToListAsync();
        var brands = await context.Brands.ToListAsync();

        var cat = (string name) => categories.First(c => c.Name == name);
        var brand = (string name) => brands.First(b => b.Name == name);

        var products = new List<Product>
        {
            // Electronics - Smartphones (5)
            P("TechPro Smartphone X1", "Flagship smartphone with 6.7\" AMOLED display, 128GB", "TP-SM-001", cat("Smartphones"), brand("TechPro"), 999.99m, 650m, 50, 10, true, true),
            P("TechPro Smartphone X1 Pro", "Pro variant with 256GB and triple camera", "TP-SM-002", cat("Smartphones"), brand("TechPro"), 1199.99m, 780m, 30, 5, true, true),
            P("TechPro Smartphone Lite", "Budget-friendly smartphone", "TP-SM-003", cat("Smartphones"), brand("TechPro"), 399.99m, 200m, 100, 20, true, false),
            P("TechPro Smartphone SE", "Compact smartphone with powerful specs", "TP-SM-004", cat("Smartphones"), brand("TechPro"), 599.99m, 350m, 60, 10, true, false),
            P("TechPro Smartphone Fold", "Foldable smartphone", "TP-SM-005", cat("Smartphones"), brand("TechPro"), 1799.99m, 1100m, 15, 3, true, true),

            // Electronics - Laptops (5)
            P("TechPro Laptop Pro 15", "15\" professional laptop, i7, 16GB RAM", "TP-LP-001", cat("Laptops"), brand("TechPro"), 1499.99m, 900m, 30, 5, true, true),
            P("TechPro Laptop Air 13", "13\" ultralight laptop", "TP-LP-002", cat("Laptops"), brand("TechPro"), 999.99m, 600m, 40, 8, true, false),
            P("TechPro Laptop Studio", "Creative professional workstation", "TP-LP-003", cat("Laptops"), brand("TechPro"), 2499.99m, 1500m, 15, 3, true, true),
            P("TechPro Laptop Budget", "Entry-level laptop for students", "TP-LP-004", cat("Laptops"), brand("TechPro"), 499.99m, 280m, 80, 15, true, false),
            P("TechPro Gaming Laptop", "Gaming laptop with RTX graphics", "TP-LP-005", cat("Laptops"), brand("TechPro"), 1899.99m, 1200m, 20, 5, true, true),

            // Electronics - Accessories (5)
            P("TechPro Wireless Earbuds", "True wireless earbuds with ANC", "TP-AE-001", cat("Accessories"), brand("TechPro"), 149.99m, 60m, 100, 20, true, false),
            P("TechPro Smartwatch S1", "Smartwatch with health tracking", "TP-AE-002", cat("Accessories"), brand("TechPro"), 299.99m, 150m, 50, 10, true, true),
            P("TechPro USB-C Hub", "7-in-1 USB-C hub", "TP-AE-003", cat("Accessories"), brand("TechPro"), 49.99m, 18m, 200, 40, true, false),
            P("TechPro Wireless Charger", "15W fast wireless charger", "TP-AE-004", cat("Accessories"), brand("TechPro"), 29.99m, 10m, 150, 30, true, false),
            P("TechPro Laptop Backpack", "Water-resistant tech backpack", "TP-AE-005", cat("Accessories"), brand("TechPro"), 79.99m, 30m, 80, 15, true, false),

            // Electronics - Tablets (3)
            P("TechPro Tablet S10", "10.5\" tablet for media and productivity", "TP-TB-001", cat("Tablets"), brand("TechPro"), 599.99m, 350m, 40, 8, true, false),
            P("TechPro Tablet S10 Pro", "12.9\" pro tablet with stylus", "TP-TB-002", cat("Tablets"), brand("TechPro"), 899.99m, 550m, 25, 5, true, true),
            P("TechPro Tablet Mini", "8\" compact tablet", "TP-TB-003", cat("Tablets"), brand("TechPro"), 349.99m, 180m, 60, 12, true, false),

            // Clothing - Men's (5)
            P("StyleCo Premium T-Shirt", "100% organic cotton premium t-shirt", "SC-TS-001", cat("Men's Clothing"), brand("StyleCo"), 39.99m, 12m, 200, 30, true, false),
            P("StyleCo Denim Jacket", "Classic denim jacket with modern fit", "SC-DJ-001", cat("Men's Clothing"), brand("StyleCo"), 89.99m, 30m, 60, 10, true, false),
            P("StyleCo Slim Fit Chinos", "Stretch chino pants", "SC-CH-001", cat("Men's Clothing"), brand("StyleCo"), 59.99m, 20m, 100, 20, true, false),
            P("StyleCo Polo Shirt", "Classic polo shirt", "SC-PL-001", cat("Men's Clothing"), brand("StyleCo"), 44.99m, 15m, 150, 25, true, false),
            P("StyleCo Hoodie", "Premium cotton blend hoodie", "SC-HD-001", cat("Men's Clothing"), brand("StyleCo"), 69.99m, 25m, 80, 15, true, true),

            // Clothing - Women's (5)
            P("StyleCo Summer Dress", "Floral print summer dress", "SC-DR-001", cat("Women's Clothing"), brand("StyleCo"), 79.99m, 28m, 70, 12, true, true),
            P("StyleCo Silk Blouse", "Elegant silk blouse", "SC-BL-001", cat("Women's Clothing"), brand("StyleCo"), 64.99m, 22m, 90, 15, true, false),
            P("StyleCo Yoga Leggings", "High-waist performance leggings", "SC-YL-001", cat("Women's Clothing"), brand("StyleCo"), 49.99m, 16m, 120, 20, true, false),
            P("StyleCo Knit Sweater", "Cashmere blend knit sweater", "SC-KS-001", cat("Women's Clothing"), brand("StyleCo"), 99.99m, 40m, 50, 8, true, false),
            P("StyleCo Trench Coat", "Classic trench coat", "SC-TC-001", cat("Women's Clothing"), brand("StyleCo"), 149.99m, 55m, 35, 5, true, true),

            // Sports & Outdoors (5)
            P("SportMax Running Shoes", "Lightweight running shoes with cushioning", "SM-RS-001", cat("Sports & Outdoors"), brand("SportMax"), 129.99m, 45m, 75, 15, true, true),
            P("SportMax Yoga Mat", "Non-slip premium yoga mat", "SM-YM-001", cat("Sports & Outdoors"), brand("SportMax"), 49.99m, 15m, 120, 20, true, false),
            P("SportMax Dumbbell Set", "Adjustable dumbbell set 5-50lbs", "SM-DB-001", cat("Sports & Outdoors"), brand("SportMax"), 249.99m, 120m, 30, 5, true, false),
            P("SportMax Hiking Backpack", "50L hiking backpack", "SM-HB-001", cat("Sports & Outdoors"), brand("SportMax"), 89.99m, 35m, 60, 10, true, false),
            P("SportMax Resistance Bands", "Set of 5 resistance bands", "SM-RB-001", cat("Sports & Outdoors"), brand("SportMax"), 24.99m, 8m, 200, 30, true, false),

            // Home & Garden - Furniture (3)
            P("HomeEssentials Standing Desk", "Electric standing desk 60\"", "HE-SD-001", cat("Furniture"), brand("HomeEssentials"), 499.99m, 250m, 20, 3, true, true),
            P("HomeEssentials Office Chair", "Ergonomic mesh office chair", "HE-OC-001", cat("Furniture"), brand("HomeEssentials"), 349.99m, 180m, 25, 5, true, false),
            P("HomeEssentials Bookshelf", "5-tier wooden bookshelf", "HE-BS-001", cat("Furniture"), brand("HomeEssentials"), 129.99m, 60m, 40, 8, true, false),

            // Home & Garden - Kitchen (3)
            P("HomeEssentials Air Fryer", "Digital air fryer 5.8QT", "HE-AF-001", cat("Kitchen"), brand("HomeEssentials"), 79.99m, 35m, 60, 10, true, true),
            P("HomeEssentials Blender Pro", "High-speed professional blender", "HE-BP-001", cat("Kitchen"), brand("HomeEssentials"), 149.99m, 70m, 40, 8, true, false),
            P("HomeEssentials Knife Set", "15-piece stainless steel knife set", "HE-KS-001", cat("Kitchen"), brand("HomeEssentials"), 99.99m, 40m, 50, 10, true, false),

            // Books & Media (3)
            P("The Art of Programming", "Comprehensive programming guide", "RM-BK-001", cat("Books & Media"), brand("ReadMore"), 49.99m, 15m, 100, 20, true, false),
            P("Business Strategy Masterclass", "MBA-level business strategy book", "RM-BK-002", cat("Books & Media"), brand("ReadMore"), 34.99m, 10m, 80, 15, true, false),
            P("Healthy Living Cookbook", "200+ healthy recipes", "RM-BK-003", cat("Books & Media"), brand("ReadMore"), 29.99m, 8m, 120, 20, true, false),

            // Health & Beauty (4)
            P("GlowUp Face Serum", "Vitamin C brightening serum", "GU-FS-001", cat("Health & Beauty"), brand("GlowUp"), 39.99m, 12m, 100, 20, true, true),
            P("GlowUp Moisturizer", "Daily hydrating moisturizer SPF30", "GU-MO-001", cat("Health & Beauty"), brand("GlowUp"), 29.99m, 9m, 120, 25, true, false),
            P("NatureFit Protein Powder", "Plant-based protein 2lb", "NF-PP-001", cat("Health & Beauty"), brand("NatureFit"), 44.99m, 18m, 80, 15, true, false),
            P("NatureFit Multivitamin", "Daily multivitamin 90 capsules", "NF-MV-001", cat("Health & Beauty"), brand("NatureFit"), 19.99m, 6m, 200, 30, true, false),

            // Toys & Games (4)
            P("FunZone Building Blocks 500pc", "Creative building block set", "FZ-BB-001", cat("Toys & Games"), brand("FunZone"), 34.99m, 12m, 80, 15, true, false),
            P("FunZone RC Racing Car", "Remote control racing car 1:16", "FZ-RC-001", cat("Toys & Games"), brand("FunZone"), 49.99m, 20m, 50, 10, true, true),
            P("FunZone Board Game Collection", "Classic board game bundle", "FZ-BG-001", cat("Toys & Games"), brand("FunZone"), 39.99m, 15m, 60, 10, true, false),
            P("FunZone Science Kit", "STEM science experiment kit", "FZ-SK-001", cat("Toys & Games"), brand("FunZone"), 29.99m, 10m, 70, 12, true, false),
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Add variants for the t-shirt
        var tshirt = products.First(p => p.SKU == "SC-TS-001");
        string[] sizes = { "S", "M", "L", "XL" };
        string[] colors = { "Black", "White", "Navy" };
        var variants = new List<ProductVariant>();
        foreach (var sz in sizes)
            foreach (var cl in colors)
                variants.Add(new ProductVariant { ProductId = tshirt.Id, VariantName = $"{sz} - {cl}", SKU = $"SC-TS-001-{sz}-{cl[..3].ToUpper()}", Price = 39.99m, StockQuantity = 25, Attributes = $"{{\"size\":\"{sz}\",\"color\":\"{cl}\"}}", CreatedBy = "system" });
        context.ProductVariants.AddRange(variants);

        // Variants for Running Shoes
        var shoes = products.First(p => p.SKU == "SM-RS-001");
        for (int sz = 8; sz <= 12; sz++)
            context.ProductVariants.Add(new ProductVariant { ProductId = shoes.Id, VariantName = $"Size {sz}", SKU = $"SM-RS-001-{sz}", Price = 129.99m, StockQuantity = 15, Attributes = $"{{\"size\":\"{sz}\"}}", CreatedBy = "system" });

        await context.SaveChangesAsync();
    }

    private static Product P(string name, string desc, string sku, Category cat, Brand brand, decimal price, decimal cost, int stock, int min, bool active, bool featured)
        => new() { Name = name, Description = desc, SKU = sku, CategoryId = cat.Id, BrandId = brand.Id, BasePrice = price, CostPrice = cost, StockQuantity = stock, MinStockLevel = min, IsActive = active, IsFeatured = featured, CreatedBy = "system" };

    private static async Task SeedDiscountsAsync(AppDbContext context)
    {
        if (await context.Discounts.AnyAsync()) return;
        var now = DateTime.UtcNow;
        var discounts = new List<Discount>
        {
            new() { Code = "SUMMER25", Name = "Summer Sale 25%", Description = "25% off everything this summer", DiscountType = DiscountType.Percentage, Value = 25, MinOrderAmount = 50, MaxDiscountAmount = 100, StartDate = now.AddDays(-10), EndDate = now.AddDays(60), IsActive = true, UsageLimit = 500, UsedCount = 12, CreatedBy = "system" },
            new() { Code = "FLAT10", Name = "Flat $10 Off", Description = "$10 off orders over $30", DiscountType = DiscountType.Fixed, Value = 10, MinOrderAmount = 30, StartDate = now.AddDays(-5), EndDate = now.AddDays(90), IsActive = true, UsageLimit = 1000, UsedCount = 45, CreatedBy = "system" },
            new() { Code = "WELCOME15", Name = "Welcome 15%", Description = "15% off for new customers", DiscountType = DiscountType.Percentage, Value = 15, StartDate = now, EndDate = now.AddDays(365), IsActive = true, UsageLimit = 10000, UsedCount = 0, CreatedBy = "system" },
            new() { Code = "VIP50", Name = "VIP 50% Off", Description = "Exclusive 50% off for VIP members", DiscountType = DiscountType.Percentage, Value = 50, MinOrderAmount = 100, MaxDiscountAmount = 200, StartDate = now, EndDate = now.AddDays(30), IsActive = true, UsageLimit = 100, UsedCount = 3, CreatedBy = "system" },
            new() { Code = "FREESHIP", Name = "Free Shipping", Description = "$9.99 off shipping", DiscountType = DiscountType.Fixed, Value = 9.99m, StartDate = now.AddDays(-30), EndDate = now.AddDays(30), IsActive = true, UsageLimit = 2000, UsedCount = 234, CreatedBy = "system" },
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
            new() { Code = "GIFT25", Description = "$25 gift card", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 25, StartDate = now.AddDays(-30), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
            new() { Code = "GIFT50", Description = "$50 gift card", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 50, StartDate = now.AddDays(-15), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
            new() { Code = "LOYALTY20", Description = "20% off for loyal customers", VoucherType = VoucherType.Promo, ValueType = VoucherValueType.Percentage, Value = 20, StartDate = now, ExpiryDate = now.AddDays(90), IsActive = true, MaxRedemptions = 100, CurrentRedemptions = 5, CreatedBy = "system" },
            new() { Code = "BIRTHDAY10", Description = "$10 birthday voucher", VoucherType = VoucherType.Reward, ValueType = VoucherValueType.Fixed, Value = 10, StartDate = now.AddDays(-60), ExpiryDate = now.AddDays(30), IsActive = true, MaxRedemptions = 500, CurrentRedemptions = 42, CreatedBy = "system" },
            new() { Code = "REFER15", Description = "15% off referral reward", VoucherType = VoucherType.Promo, ValueType = VoucherValueType.Percentage, Value = 15, StartDate = now, ExpiryDate = now.AddDays(365), IsActive = true, MaxRedemptions = 5000, CurrentRedemptions = 0, CreatedBy = "system" },
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
            new() { Name = "Tech Flash Sale", Description = "Up to 30% off selected electronics", OfferType = OfferType.Flash, StartDate = now.AddDays(-2), EndDate = now.AddDays(5), IsActive = true, CreatedBy = "system" },
            new() { Name = "Buy More Save More", Description = "20% off when you buy 3+ items", OfferType = OfferType.Bundle, StartDate = now, EndDate = now.AddDays(30), IsActive = true, CreatedBy = "system" },
            new() { Name = "Weekend Special", Description = "15% off all clothing this weekend", OfferType = OfferType.Seasonal, StartDate = now, EndDate = now.AddDays(3), IsActive = true, CreatedBy = "system" },
        };
        context.SpecialOffers.AddRange(offers);
        await context.SaveChangesAsync();

        // Link products to offers
        var techProducts = products.Where(p => p.SKU.StartsWith("TP-")).Take(5).ToList();
        foreach (var p in techProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[0].Id, ProductId = p.Id, CreatedBy = "system" });

        var clothingProducts = products.Where(p => p.SKU.StartsWith("SC-")).Take(5).ToList();
        foreach (var p in clothingProducts)
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
            ("Excellent quality!", "Exceeded my expectations. Great value for the price."),
            ("Good product", "Solid build quality. Would recommend."),
            ("Amazing!", "Best purchase I've made this year. Absolutely love it."),
            ("Pretty decent", "Does what it's supposed to. Nothing fancy but works well."),
            ("Worth every penny", "Premium feel and great performance."),
            ("Not bad", "Average product. Could be better at this price point."),
            ("Love it!", "My whole family enjoys this product. Will buy again."),
            ("Great for the price", "Budget-friendly and good quality."),
            ("Impressive", "Sleek design and fantastic features."),
            ("Solid choice", "Reliable and well-made. Happy with my purchase."),
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
            new() { Key = "Store.Name", Value = "Appilico Store", Group = "General", Description = "Store display name", CreatedBy = "system" },
            new() { Key = "Store.Currency", Value = "USD", Group = "General", Description = "Default currency", CreatedBy = "system" },
            new() { Key = "Store.TaxRate", Value = "10", Group = "General", Description = "Tax rate percentage", CreatedBy = "system" },
            new() { Key = "Store.ShippingFee", Value = "9.99", Group = "Shipping", Description = "Default shipping fee", CreatedBy = "system" },
            new() { Key = "Store.FreeShippingThreshold", Value = "100", Group = "Shipping", Description = "Free shipping threshold amount", CreatedBy = "system" },
            new() { Key = "Loyalty.PointsPerDollar", Value = "1", Group = "Loyalty", Description = "Loyalty points earned per dollar spent", CreatedBy = "system" },
            new() { Key = "Loyalty.PointsRedemptionRate", Value = "100", Group = "Loyalty", Description = "Points needed for $1 discount", CreatedBy = "system" },
        };
        context.AppSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }
}
