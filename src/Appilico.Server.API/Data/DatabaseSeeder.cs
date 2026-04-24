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
            await SeedUsersAsync(userManager, context);
            await SeedCategoriesAsync(context);
            await SeedBrandsAsync(context);
            await SeedProductsAsync(context);
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
            {
                await roleManager.CreateAsync(new AppRole { Name = role });
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager, AppDbContext context)
    {
        if (await userManager.FindByEmailAsync("admin@appilico.com") != null) return;

        var admin = new AppUser
        {
            UserName = "admin@appilico.com",
            Email = "admin@appilico.com",
            FirstName = "System",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, AppConstants.Roles.Admin);
        }

        var manager = new AppUser
        {
            UserName = "manager@appilico.com",
            Email = "manager@appilico.com",
            FirstName = "Store",
            LastName = "Manager",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        result = await userManager.CreateAsync(manager, "Manager@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(manager, AppConstants.Roles.Manager);
        }

        var customer = new AppUser
        {
            UserName = "customer@appilico.com",
            Email = "customer@appilico.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        result = await userManager.CreateAsync(customer, "Customer@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(customer, AppConstants.Roles.Customer);

            var customerProfile = new Customer
            {
                UserId = customer.Id,
                CustomerCode = "CUST-00000001",
                JoinDate = DateTime.UtcNow,
                MembershipTier = MembershipTier.Bronze,
                CreatedBy = "system"
            };

            context.Customers.Add(customerProfile);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Electronic devices and gadgets", SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Clothing", Description = "Apparel and fashion", SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Home & Garden", Description = "Home improvement and garden", SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Sports & Outdoors", Description = "Sporting goods and outdoor equipment", SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Books & Media", Description = "Books, music, and media", SortOrder = 5, IsActive = true, CreatedBy = "system" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Sub-categories for Electronics
        var electronics = categories[0];
        var subCategories = new List<Category>
        {
            new() { Name = "Smartphones", Description = "Mobile phones", ParentCategoryId = electronics.Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Laptops", Description = "Laptop computers", ParentCategoryId = electronics.Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Accessories", Description = "Electronic accessories", ParentCategoryId = electronics.Id, SortOrder = 3, IsActive = true, CreatedBy = "system" }
        };

        context.Categories.AddRange(subCategories);
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
            new() { Name = "ReadMore", Description = "Books and publications", IsActive = true, CreatedBy = "system" }
        };

        context.Brands.AddRange(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var categories = await context.Categories.ToListAsync();
        var brands = await context.Brands.ToListAsync();

        var electronics = categories.First(c => c.Name == "Electronics");
        var smartphones = categories.First(c => c.Name == "Smartphones");
        var laptops = categories.First(c => c.Name == "Laptops");
        var clothing = categories.First(c => c.Name == "Clothing");
        var sports = categories.First(c => c.Name == "Sports & Outdoors");

        var techPro = brands.First(b => b.Name == "TechPro");
        var styleCo = brands.First(b => b.Name == "StyleCo");
        var sportMax = brands.First(b => b.Name == "SportMax");

        var products = new List<Product>
        {
            new()
            {
                Name = "TechPro Smartphone X1", Description = "Latest flagship smartphone with 6.7\" AMOLED display",
                SKU = "TP-SM-001", CategoryId = smartphones.Id, BrandId = techPro.Id,
                BasePrice = 999.99m, CostPrice = 650m, StockQuantity = 50, MinStockLevel = 10,
                IsActive = true, IsFeatured = true, CreatedBy = "system"
            },
            new()
            {
                Name = "TechPro Laptop Pro 15", Description = "High-performance laptop for professionals",
                SKU = "TP-LP-001", CategoryId = laptops.Id, BrandId = techPro.Id,
                BasePrice = 1499.99m, CostPrice = 900m, StockQuantity = 30, MinStockLevel = 5,
                IsActive = true, IsFeatured = true, CreatedBy = "system"
            },
            new()
            {
                Name = "TechPro Wireless Earbuds", Description = "True wireless earbuds with ANC",
                SKU = "TP-AE-001", CategoryId = electronics.Id, BrandId = techPro.Id,
                BasePrice = 149.99m, CostPrice = 60m, StockQuantity = 100, MinStockLevel = 20,
                IsActive = true, IsFeatured = false, CreatedBy = "system"
            },
            new()
            {
                Name = "StyleCo Premium T-Shirt", Description = "100% organic cotton premium t-shirt",
                SKU = "SC-TS-001", CategoryId = clothing.Id, BrandId = styleCo.Id,
                BasePrice = 39.99m, CostPrice = 12m, StockQuantity = 200, MinStockLevel = 30,
                IsActive = true, IsFeatured = false, CreatedBy = "system"
            },
            new()
            {
                Name = "SportMax Running Shoes", Description = "Lightweight running shoes with cushioning",
                SKU = "SM-RS-001", CategoryId = sports.Id, BrandId = sportMax.Id,
                BasePrice = 129.99m, CostPrice = 45m, StockQuantity = 75, MinStockLevel = 15,
                IsActive = true, IsFeatured = true, CreatedBy = "system"
            },
            new()
            {
                Name = "TechPro Tablet S10", Description = "10.5\" tablet for media and productivity",
                SKU = "TP-TB-001", CategoryId = electronics.Id, BrandId = techPro.Id,
                BasePrice = 599.99m, CostPrice = 350m, StockQuantity = 40, MinStockLevel = 8,
                IsActive = true, IsFeatured = false, CreatedBy = "system"
            },
            new()
            {
                Name = "StyleCo Denim Jacket", Description = "Classic denim jacket with modern fit",
                SKU = "SC-DJ-001", CategoryId = clothing.Id, BrandId = styleCo.Id,
                BasePrice = 89.99m, CostPrice = 30m, StockQuantity = 60, MinStockLevel = 10,
                IsActive = true, IsFeatured = false, CreatedBy = "system"
            },
            new()
            {
                Name = "SportMax Yoga Mat", Description = "Non-slip premium yoga mat",
                SKU = "SM-YM-001", CategoryId = sports.Id, BrandId = sportMax.Id,
                BasePrice = 49.99m, CostPrice = 15m, StockQuantity = 120, MinStockLevel = 20,
                IsActive = true, IsFeatured = false, CreatedBy = "system"
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Add variants for the t-shirt
        var tshirt = products.First(p => p.SKU == "SC-TS-001");
        var variants = new List<ProductVariant>
        {
            new() { ProductId = tshirt.Id, VariantName = "Small - Black", SKU = "SC-TS-001-S-BLK", Price = 39.99m, StockQuantity = 50, Attributes = "{\"size\":\"S\",\"color\":\"Black\"}", CreatedBy = "system" },
            new() { ProductId = tshirt.Id, VariantName = "Medium - Black", SKU = "SC-TS-001-M-BLK", Price = 39.99m, StockQuantity = 50, Attributes = "{\"size\":\"M\",\"color\":\"Black\"}", CreatedBy = "system" },
            new() { ProductId = tshirt.Id, VariantName = "Large - Black", SKU = "SC-TS-001-L-BLK", Price = 39.99m, StockQuantity = 50, Attributes = "{\"size\":\"L\",\"color\":\"Black\"}", CreatedBy = "system" },
            new() { ProductId = tshirt.Id, VariantName = "Medium - White", SKU = "SC-TS-001-M-WHT", Price = 39.99m, StockQuantity = 50, Attributes = "{\"size\":\"M\",\"color\":\"White\"}", CreatedBy = "system" }
        };

        context.ProductVariants.AddRange(variants);
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
            new() { Key = "Loyalty.PointsRedemptionRate", Value = "100", Group = "Loyalty", Description = "Points needed for $1 discount", CreatedBy = "system" }
        };

        context.AppSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }
}
