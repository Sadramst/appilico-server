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
            new() { Name = "Beef", Description = "Premium grain-fed and grass-fed beef – steaks, roasts and more", ImageUrl = "https://images.unsplash.com/photo-1603048297172-c92544798d5a?w=800", SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Veal", Description = "Ethically raised premium veal cuts", ImageUrl = "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800", SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lamb", Description = "Tender spring lamb sourced from local Australian farms", ImageUrl = "https://images.unsplash.com/photo-1514516345957-556ca7d90a29?w=800", SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pork", Description = "Free-range pork from trusted producers", ImageUrl = "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800", SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Poultry", Description = "Free-range chicken, duck and turkey", ImageUrl = "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800", SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Deli & Cold Cuts", Description = "Cured meats, salumi, antipasto and cold cut platters", ImageUrl = "https://images.unsplash.com/photo-1626200419199-391ae4be7a41?w=800", SortOrder = 6, IsActive = true, CreatedBy = "system" },
            new() { Name = "Ready Meals", Description = "Chef-prepared meals ready to heat and serve", ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=800", SortOrder = 7, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pantry", Description = "Artisan pasta, sauces, oils, condiments and seasonings", ImageUrl = "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800", SortOrder = 8, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(topLevel);
        await context.SaveChangesAsync();

        var subCats = new List<Category>
        {
            // Beef subs
            new() { Name = "Beef Steaks", Description = "Premium steak cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Beef Roasting Joints", Description = "Slow roast and Sunday roast cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Beef Other Cuts", Description = "Mince, diced, strips and specialty cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            // Veal subs
            new() { Name = "Veal Steaks & Cutlets", Description = "Scallopini, cutlets and rib eye", ParentCategoryId = topLevel[1].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Veal Specialty", Description = "Osso buco, involtini and rolled roasts", ParentCategoryId = topLevel[1].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            // Lamb subs
            new() { Name = "Lamb Chops & Cutlets", Description = "Chops, cutlets and racks", ParentCategoryId = topLevel[2].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lamb Roasting Joints", Description = "Leg, shoulder and rolled roasts", ParentCategoryId = topLevel[2].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lamb Other Cuts", Description = "Mince, diced, kofta and stir-fry strips", ParentCategoryId = topLevel[2].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            // Pork subs
            new() { Name = "Pork Steaks & Chops", Description = "Loin chops, steaks and cutlets", ParentCategoryId = topLevel[3].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pork Roasting Joints", Description = "Rolled pork, rack and belly roasts", ParentCategoryId = topLevel[3].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pork Other Cuts", Description = "Ribs, belly strips and shoulder", ParentCategoryId = topLevel[3].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            // Poultry subs
            new() { Name = "Chicken Portions", Description = "Breast, thigh, drumstick and wings", ParentCategoryId = topLevel[4].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Whole Birds", Description = "Whole chicken, spatchcock and duck", ParentCategoryId = topLevel[4].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Turkey & Game", Description = "Whole turkeys, turkey portions, duck and game birds", ParentCategoryId = topLevel[4].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            // Deli subs
            new() { Name = "Cured Meats", Description = "Prosciutto, salami, pancetta and other cured meats", ParentCategoryId = topLevel[5].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Prepared Deli", Description = "Hams, antipasto, olives and cheese selections", ParentCategoryId = topLevel[5].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            // Ready Meals subs
            new() { Name = "Italian Classics", Description = "Lasagne, arancini, involtini and more", ParentCategoryId = topLevel[6].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Sausages & Burgers", Description = "Handmade sausages and gourmet burger patties", ParentCategoryId = topLevel[6].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Curries & Stews", Description = "Slow-cooked curries, stews and braised sauces", ParentCategoryId = topLevel[6].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "BBQ Ready", Description = "Marinated meats, kebabs and BBQ platters", ParentCategoryId = topLevel[6].Id, SortOrder = 4, IsActive = true, CreatedBy = "system" },
            // Pantry subs
            new() { Name = "Pasta & Sauces", Description = "Dried pasta, fresh sauces and condiments", ParentCategoryId = topLevel[7].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Oils & Seasonings", Description = "Olive oils, marinades, rubs and spice blends", ParentCategoryId = topLevel[7].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Condiments & Preserves", Description = "Mustards, chutneys, honey and pickles", ParentCategoryId = topLevel[7].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(subCats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(AppDbContext context)
    {
        if (await context.Brands.AnyAsync()) return;
        var brands = new List<Brand>
        {
            new() { Name = "Primo Cuts", Description = "Our signature premium meat range", IsActive = true, CreatedBy = "system" },
            new() { Name = "Heritage Reserve", Description = "Dry-aged and specialty beef selection", IsActive = true, CreatedBy = "system" },
            new() { Name = "Valley Fresh", Description = "Free-range lamb and poultry from local farms", IsActive = true, CreatedBy = "system" },
            new() { Name = "Artisan Kitchen", Description = "Chef-prepared ready meals and deli items", IsActive = true, CreatedBy = "system" },
            new() { Name = "Rustic Pantry", Description = "Imported and artisan pantry goods", IsActive = true, CreatedBy = "system" },
            new() { Name = "Grill Master", Description = "BBQ-ready sausages, burgers and marinades", IsActive = true, CreatedBy = "system" },
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
            // ── Beef Steaks (12) ─────────────────────────────────
            P("Scotch Fillet Steak", "300g premium grain-fed scotch fillet, beautifully marbled for maximum flavour", "PM-BS-001", cat("Beef Steaks"), brand("Primo Cuts"), 32.99m, 18m, 40, 5, true, true, 0.3m),
            P("Eye Fillet Steak", "250g centre-cut eye fillet, the tenderest steak cut available", "PM-BS-002", cat("Beef Steaks"), brand("Primo Cuts"), 38.99m, 22m, 30, 5, true, true, 0.25m),
            P("T-Bone Steak", "450g bone-in T-bone combining fillet and sirloin", "PM-BS-003", cat("Beef Steaks"), brand("Primo Cuts"), 29.99m, 16m, 35, 5, true, false, 0.45m),
            P("Porterhouse Steak", "350g thick-cut porterhouse, ideal for the BBQ", "PM-BS-004", cat("Beef Steaks"), brand("Primo Cuts"), 27.99m, 14m, 45, 8, true, false, 0.35m),
            P("Beef Tomahawk", "800g dry-aged tomahawk with full rib bone, showstopper cut", "PM-BS-005", cat("Beef Steaks"), brand("Heritage Reserve"), 49.50m, 28m, 15, 3, true, true, 0.8m),
            P("Dry-Aged Ribeye 45 Day", "400g 45-day dry-aged ribeye, intense beefy flavour", "PM-BS-006", cat("Beef Steaks"), brand("Heritage Reserve"), 54.99m, 30m, 12, 3, true, true, 0.4m),
            P("Beef Rump Steak", "300g lean rump steak, great value everyday cut", "PM-BS-007", cat("Beef Steaks"), brand("Primo Cuts"), 19.99m, 10m, 60, 10, true, false, 0.3m),
            P("Wagyu Ribeye Steak", "300g marble score 6+ wagyu ribeye, melt-in-your-mouth tenderness", "PM-BS-008", cat("Beef Steaks"), brand("Heritage Reserve"), 79.99m, 48m, 8, 2, true, true, 0.3m),
            P("Flat Iron Steak", "350g flavourful flat iron from the shoulder, great marbling", "PM-BS-009", cat("Beef Steaks"), brand("Primo Cuts"), 24.99m, 13m, 35, 5, true, false, 0.35m),
            P("Flank Steak", "500g lean flank ideal for marinating and grilling", "PM-BS-010", cat("Beef Steaks"), brand("Primo Cuts"), 22.99m, 11m, 40, 8, true, false, 0.5m),
            P("Hanger Steak", "350g butcher's secret cut, rich and full-flavoured", "PM-BS-011", cat("Beef Steaks"), brand("Heritage Reserve"), 28.99m, 15m, 20, 4, true, false, 0.35m),
            P("Beef Minute Steak", "4-pack thinly sliced steaks for quick pan-frying", "PM-BS-012", cat("Beef Steaks"), brand("Primo Cuts"), 18.99m, 9m, 50, 10, true, false, 0.4m),

            // ── Beef Roasting Joints (9) ─────────────────────────
            P("Beef Sirloin Roast", "1.2kg boneless sirloin roast, perfect for Sunday lunch", "PM-BR-001", cat("Beef Roasting Joints"), brand("Primo Cuts"), 80.00m, 45m, 20, 4, true, true, 1.2m),
            P("Beef Rib Eye Roast", "1.5kg bone-in rib eye roast for special occasions", "PM-BR-002", cat("Beef Roasting Joints"), brand("Heritage Reserve"), 69.50m, 38m, 10, 2, true, true, 1.5m),
            P("Beef Brisket", "1.5kg whole brisket for low and slow smoking or braising", "PM-BR-003", cat("Beef Roasting Joints"), brand("Primo Cuts"), 28.99m, 14m, 25, 5, true, false, 1.5m),
            P("Beef Fillet Roast", "1kg whole eye fillet, trimmed and tied", "PM-BR-004", cat("Beef Roasting Joints"), brand("Heritage Reserve"), 69.99m, 40m, 12, 3, true, true, 1.0m),
            P("Beef Asado Ribs", "1.2kg short ribs cut Argentinian style for BBQ", "PM-BR-005", cat("Beef Roasting Joints"), brand("Grill Master"), 35.00m, 18m, 20, 4, true, false, 1.2m),
            P("Beef Cheeks", "1kg slow-cook beef cheeks, fall-apart tender after braising", "PM-BR-006", cat("Beef Roasting Joints"), brand("Primo Cuts"), 26.00m, 13m, 20, 4, true, false, 1.0m),
            P("Standing Rib Roast", "2.5kg impressive bone-in rib roast for celebrations", "PM-BR-007", cat("Beef Roasting Joints"), brand("Heritage Reserve"), 120.00m, 65m, 6, 2, true, true, 2.5m),
            P("Oxtail", "1kg oxtail pieces for rich, gelatinous braises and soups", "PM-BR-008", cat("Beef Roasting Joints"), brand("Primo Cuts"), 18.99m, 9m, 25, 5, true, false, 1.0m),
            P("Beef Short Ribs", "1.2kg bone-in short ribs, perfect for braising or Korean BBQ", "PM-BR-009", cat("Beef Roasting Joints"), brand("Primo Cuts"), 32.00m, 16m, 18, 4, true, false, 1.2m),

            // ── Beef Other Cuts (8) ──────────────────────────────
            P("Premium Beef Mince", "500g lean premium mince, less than 10% fat", "PM-BO-001", cat("Beef Other Cuts"), brand("Primo Cuts"), 14.00m, 6m, 80, 15, true, false, 0.5m),
            P("Beef Diced Casserole", "500g diced chuck steak for slow cooking", "PM-BO-002", cat("Beef Other Cuts"), brand("Primo Cuts"), 16.00m, 7m, 60, 10, true, false, 0.5m),
            P("Beef Stir-Fry Strips", "400g thinly sliced rump strips", "PM-BO-003", cat("Beef Other Cuts"), brand("Primo Cuts"), 17.00m, 8m, 50, 10, true, false, 0.4m),
            P("Osso Buco (Beef Shin)", "1kg cross-cut beef shin on the bone", "PM-BO-004", cat("Beef Other Cuts"), brand("Primo Cuts"), 22.00m, 11m, 25, 5, true, false, 1.0m),
            P("Corned Silverside", "1.5kg cured silverside for classic corned beef", "PM-BO-005", cat("Beef Other Cuts"), brand("Primo Cuts"), 19.99m, 10m, 20, 4, true, false, 1.5m),
            P("Beef Marrow Bones", "1kg split marrow bones for roasting or bone broth", "PM-BO-006", cat("Beef Other Cuts"), brand("Primo Cuts"), 8.99m, 3m, 30, 5, true, false, 1.0m),
            P("Beef Tongue", "Whole beef tongue, ideal for slow braising or smoking", "PM-BO-007", cat("Beef Other Cuts"), brand("Primo Cuts"), 24.99m, 12m, 10, 2, true, false, 1.2m),
            P("Beef Jerky", "250g house-made smoky beef jerky, high protein snack", "PM-BO-008", cat("Beef Other Cuts"), brand("Grill Master"), 16.99m, 7m, 40, 8, true, true, 0.25m),

            // ── Veal Steaks & Cutlets (6) ────────────────────────
            P("Veal Scallopini", "400g thinly sliced veal leg, pan-ready", "PM-VS-001", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 22.00m, 12m, 25, 5, true, false, 0.4m),
            P("Veal Rib Eye", "350g veal rib eye steak, tender and mild", "PM-VS-002", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 20.00m, 11m, 20, 4, true, true, 0.35m),
            P("Veal Chops (Crumbed)", "4-pack hand-crumbed veal chops", "PM-VS-003", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 14.00m, 7m, 30, 5, true, false, 0.5m),
            P("Veal Schnitzel", "4-pack golden-crumbed veal schnitzel, pan-ready", "PM-VS-004", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 18.99m, 9m, 30, 5, true, true, 0.5m),
            P("Veal Rump Steak", "350g lean veal rump steak", "PM-VS-005", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 19.99m, 10m, 20, 4, true, false, 0.35m),
            P("Veal Loin Steak", "300g veal loin steak, delicate flavour and tender texture", "PM-VS-006", cat("Veal Steaks & Cutlets"), brand("Primo Cuts"), 24.99m, 13m, 15, 3, true, false, 0.3m),

            // ── Veal Specialty (6) ───────────────────────────────
            P("Veal Osso Buco", "1kg cross-cut veal shank, 3cm thick", "PM-VX-001", cat("Veal Specialty"), brand("Primo Cuts"), 34.99m, 18m, 20, 4, true, true, 1.0m),
            P("Veal Involtini", "4-pack veal rolls stuffed with prosciutto and sage", "PM-VX-002", cat("Veal Specialty"), brand("Artisan Kitchen"), 8.00m, 4m, 30, 5, true, true, 0.5m),
            P("Veal Cutlets (Bone-In)", "4-pack bone-in veal rib cutlets", "PM-VX-003", cat("Veal Specialty"), brand("Primo Cuts"), 38.99m, 22m, 15, 3, true, false, 0.5m),
            P("Veal Shoulder Rolled", "1.5kg boneless rolled veal shoulder for slow roasting", "PM-VX-004", cat("Veal Specialty"), brand("Primo Cuts"), 42.00m, 22m, 12, 3, true, false, 1.5m),
            P("Veal Shanks", "1kg veal shanks (2 pieces), braising cut", "PM-VX-005", cat("Veal Specialty"), brand("Primo Cuts"), 28.99m, 15m, 18, 4, true, false, 1.0m),
            P("Veal Meatballs", "500g hand-rolled veal meatballs with parmesan and herbs", "PM-VX-006", cat("Veal Specialty"), brand("Artisan Kitchen"), 16.99m, 8m, 25, 5, true, false, 0.5m),

            // ── Lamb Chops & Cutlets (9) ─────────────────────────
            P("Lamb Rack", "Full 8-point rack, frenched and cap on", "PM-LC-001", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 40.00m, 22m, 15, 3, true, true, 0.6m),
            P("Lamb Chops (Marinated)", "6-pack herb and garlic marinated chops", "PM-LC-002", cat("Lamb Chops & Cutlets"), brand("Grill Master"), 17.50m, 9m, 35, 5, true, false, 0.6m),
            P("Lamb Cutlets (French Trimmed)", "4-pack frenched lamb cutlets", "PM-LC-003", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 28.99m, 15m, 30, 5, true, true, 0.4m),
            P("Lamb Loin Chops", "4-pack thick-cut loin chops", "PM-LC-004", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 22.99m, 12m, 40, 8, true, false, 0.5m),
            P("Lamb Backstrap", "400g trimmed lamb backstrap fillet", "PM-LC-005", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 32.00m, 17m, 20, 4, true, false, 0.4m),
            P("Lamb Forequarter Chops", "6-pack forequarter chops, great value for grilling", "PM-LC-006", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 14.99m, 7m, 45, 8, true, false, 0.7m),
            P("Lamb T-Bone Chops", "4-pack lamb T-bone chops, loin and fillet in one", "PM-LC-007", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 26.99m, 14m, 30, 5, true, false, 0.5m),
            P("Lamb Rump Steak", "350g boneless lamb rump, versatile and flavourful", "PM-LC-008", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 21.99m, 11m, 25, 5, true, false, 0.35m),
            P("Lamb Souvlaki Skewers", "8-pack Greek-style marinated lamb skewers", "PM-LC-009", cat("Lamb Chops & Cutlets"), brand("Grill Master"), 19.99m, 10m, 30, 5, true, true, 0.6m),

            // ── Lamb Roasting Joints (8) ─────────────────────────
            P("Lamb Leg (Easy Carve)", "2kg butterflied and deboned leg for easy carving", "PM-LR-001", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 85.00m, 46m, 12, 3, true, true, 2.0m),
            P("Lamb Leg (Bone In)", "2kg whole leg of lamb for traditional roasting", "PM-LR-002", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 48.99m, 26m, 15, 3, true, false, 2.0m),
            P("Lamb Shoulder (Boneless Rolled)", "1.5kg boneless rolled shoulder, slow-roast favourite", "PM-LR-003", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 55.00m, 28m, 18, 4, true, true, 1.5m),
            P("Lamb Shanks", "1kg lamb shanks (2 pieces), perfect for braising", "PM-LR-004", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 18.00m, 9m, 30, 5, true, false, 1.0m),
            P("Lamb Shoulder (Bone In)", "2.5kg whole bone-in shoulder for low and slow roasting", "PM-LR-005", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 45.00m, 22m, 12, 3, true, false, 2.5m),
            P("Mini Lamb Roast", "1kg boneless mini lamb roast, perfect for two", "PM-LR-006", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 32.00m, 16m, 20, 4, true, false, 1.0m),
            P("Lamb Crown Roast", "1.5kg crown roast of lamb, frenched and tied", "PM-LR-007", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 65.00m, 35m, 8, 2, true, true, 1.5m),
            P("Argentinian Lamb Shoulder", "2kg lamb shoulder with chimichurri marinade", "PM-LR-008", cat("Lamb Roasting Joints"), brand("Grill Master"), 55.00m, 28m, 10, 3, true, true, 2.0m),

            // ── Lamb Other Cuts (6) ──────────────────────────────
            P("Lamb Mince", "500g lean lamb mince for kofta, moussaka and more", "PM-LO-001", cat("Lamb Other Cuts"), brand("Valley Fresh"), 14.99m, 7m, 50, 10, true, false, 0.5m),
            P("Lamb Diced", "500g diced lamb shoulder for curries and stews", "PM-LO-002", cat("Lamb Other Cuts"), brand("Valley Fresh"), 17.99m, 9m, 40, 8, true, false, 0.5m),
            P("Lamb Kofta", "8-pack spiced lamb kofta skewers", "PM-LO-003", cat("Lamb Other Cuts"), brand("Grill Master"), 18.99m, 9m, 30, 5, true, true, 0.5m),
            P("Lamb Backstrap Marinated", "400g backstrap in rosemary and garlic marinade", "PM-LO-004", cat("Lamb Other Cuts"), brand("Grill Master"), 34.99m, 18m, 20, 4, true, false, 0.4m),
            P("Lamb Neck Fillets", "500g boneless lamb neck fillets, rich and succulent", "PM-LO-005", cat("Lamb Other Cuts"), brand("Valley Fresh"), 16.99m, 8m, 25, 5, true, false, 0.5m),
            P("Lamb Stir-Fry Strips", "400g thinly sliced lamb strips for quick cooking", "PM-LO-006", cat("Lamb Other Cuts"), brand("Valley Fresh"), 19.99m, 10m, 30, 5, true, false, 0.4m),

            // ── Pork Steaks & Chops (7) ──────────────────────────
            P("Pork Loin Chops", "4-pack bone-in loin chops, 2cm thick", "PM-PS-001", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 16.99m, 8m, 50, 10, true, false, 0.6m),
            P("Pork Scotch Fillet Steak", "2-pack pork scotch fillet steaks", "PM-PS-002", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 14.99m, 7m, 45, 8, true, false, 0.4m),
            P("Pork Cutlets (Crumbed)", "4-pack hand-crumbed pork cutlets", "PM-PS-003", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 18.99m, 9m, 30, 5, true, false, 0.5m),
            P("Pork Medallions", "4-pack lean pork loin medallions, pan-ready", "PM-PS-004", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 17.99m, 9m, 35, 5, true, false, 0.4m),
            P("Pork Tenderloin", "500g whole pork tenderloin, leanest pork cut", "PM-PS-005", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 16.99m, 8m, 30, 5, true, true, 0.5m),
            P("Pork Schnitzel", "4-pack golden-crumbed pork schnitzel", "PM-PS-006", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 19.99m, 10m, 30, 5, true, false, 0.5m),
            P("Pork T-Bone Steak", "2-pack thick-cut pork T-bone steaks", "PM-PS-007", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 15.99m, 8m, 25, 5, true, false, 0.5m),

            // ── Pork Roasting Joints (6) ─────────────────────────
            P("Porchetta", "1.8kg Italian-style rolled pork belly with herbs", "PM-PR-001", cat("Pork Roasting Joints"), brand("Artisan Kitchen"), 50.00m, 26m, 15, 3, true, true, 1.8m),
            P("Pork Rack (Trimmed)", "1kg 4-cutlet frenched pork rack", "PM-PR-002", cat("Pork Roasting Joints"), brand("Primo Cuts"), 32.99m, 18m, 15, 3, true, false, 1.0m),
            P("Pork Shoulder (Bone In)", "2.5kg whole pork shoulder for pulled pork", "PM-PR-003", cat("Pork Roasting Joints"), brand("Primo Cuts"), 45.00m, 22m, 18, 4, true, true, 2.5m),
            P("Pork Leg (Bone In)", "3kg whole bone-in pork leg with scored rind", "PM-PR-004", cat("Pork Roasting Joints"), brand("Primo Cuts"), 55.00m, 28m, 10, 3, true, false, 3.0m),
            P("Pork Belly Roast Roll", "1.5kg rolled pork belly with herb stuffing", "PM-PR-005", cat("Pork Roasting Joints"), brand("Artisan Kitchen"), 38.99m, 20m, 12, 3, true, true, 1.5m),
            P("Pork Ribs Whole Rack", "1.5kg full rack of pork spare ribs", "PM-PR-006", cat("Pork Roasting Joints"), brand("Grill Master"), 34.99m, 17m, 20, 4, true, false, 1.5m),

            // ── Pork Other Cuts (7) ──────────────────────────────
            P("Pork Belly (Scored)", "1.5kg scored pork belly for perfect crackling", "PM-PO-001", cat("Pork Other Cuts"), brand("Primo Cuts"), 24.99m, 12m, 20, 4, true, true, 1.5m),
            P("Pork Belly Strips", "500g sliced pork belly strips for grilling or stir-fry", "PM-PO-002", cat("Pork Other Cuts"), brand("Primo Cuts"), 15.00m, 7m, 35, 5, true, false, 0.5m),
            P("Pork Leg (Boneless)", "2kg boneless pork leg roast", "PM-PO-003", cat("Pork Other Cuts"), brand("Primo Cuts"), 45.00m, 24m, 15, 3, true, false, 2.0m),
            P("Bacon", "500g thick-cut smoked bacon rashers", "PM-PO-004", cat("Pork Other Cuts"), brand("Primo Cuts"), 12.50m, 5m, 60, 10, true, true, 0.5m),
            P("Pork Mince", "500g lean pork mince for Asian dishes and meatballs", "PM-PO-005", cat("Pork Other Cuts"), brand("Primo Cuts"), 12.99m, 5m, 40, 8, true, false, 0.5m),
            P("Pork Spare Ribs", "1kg individual pork spare ribs for smoking or braising", "PM-PO-006", cat("Pork Other Cuts"), brand("Grill Master"), 22.99m, 11m, 25, 5, true, false, 1.0m),
            P("Pork Hock Smoked", "Whole smoked pork hock, ready for soups and stews", "PM-PO-007", cat("Pork Other Cuts"), brand("Primo Cuts"), 14.99m, 6m, 20, 4, true, false, 0.8m),

            // ── Chicken Portions (9) ─────────────────────────────
            P("Chicken Breast Fillets", "1kg skinless chicken breast fillets (3-4 pieces)", "PM-CP-001", cat("Chicken Portions"), brand("Valley Fresh"), 14.99m, 7m, 80, 15, true, false, 1.0m),
            P("Chicken Thigh Fillets", "1kg boneless skinless thigh fillets", "PM-CP-002", cat("Chicken Portions"), brand("Valley Fresh"), 12.99m, 6m, 70, 12, true, false, 1.0m),
            P("Chicken Drumsticks", "1kg free-range drumsticks (6-8 pieces)", "PM-CP-003", cat("Chicken Portions"), brand("Valley Fresh"), 9.99m, 4m, 60, 10, true, false, 1.0m),
            P("Chicken Wings", "1kg chicken wings, perfect for marinating", "PM-CP-004", cat("Chicken Portions"), brand("Valley Fresh"), 8.99m, 4m, 50, 10, true, false, 1.0m),
            P("Chicken Skewers (Marinated)", "8-pack satay marinated chicken skewers", "PM-CP-005", cat("Chicken Portions"), brand("Grill Master"), 16.99m, 8m, 40, 8, true, true, 0.6m),
            P("Chicken Tenderloins", "500g chicken tenderloins, kids favourite", "PM-CP-006", cat("Chicken Portions"), brand("Valley Fresh"), 13.99m, 6m, 50, 10, true, false, 0.5m),
            P("Chicken Marylands", "4-pack whole chicken marylands (leg and thigh)", "PM-CP-007", cat("Chicken Portions"), brand("Valley Fresh"), 14.99m, 7m, 40, 8, true, false, 0.8m),
            P("Chicken Kiev", "2-pack garlic butter chicken kievs, crumbed", "PM-CP-008", cat("Chicken Portions"), brand("Artisan Kitchen"), 16.99m, 8m, 30, 5, true, true, 0.4m),
            P("Chicken Schnitzel", "4-pack golden-crumbed chicken breast schnitzel", "PM-CP-009", cat("Chicken Portions"), brand("Artisan Kitchen"), 19.99m, 10m, 35, 5, true, false, 0.5m),

            // ── Whole Birds (7) ──────────────────────────────────
            P("Whole Chicken (Boned & Rolled)", "1.8kg whole free-range chicken, boned and rolled", "PM-WB-001", cat("Whole Birds"), brand("Valley Fresh"), 60.00m, 32m, 15, 3, true, true, 1.8m),
            P("Whole Chicken (Butterflied)", "1.5kg spatchcocked and flattened chicken", "PM-WB-002", cat("Whole Birds"), brand("Valley Fresh"), 22.00m, 11m, 20, 4, true, false, 1.5m),
            P("Whole Chicken (Butterflied & Marinated)", "1.5kg butterflied chicken in lemon herb marinade", "PM-WB-003", cat("Whole Birds"), brand("Grill Master"), 28.50m, 14m, 20, 4, true, true, 1.5m),
            P("Duck (Whole)", "2.2kg whole Pekin duck", "PM-WB-004", cat("Whole Birds"), brand("Valley Fresh"), 32.99m, 18m, 10, 2, true, false, 2.2m),
            P("Spatchcock", "2-pack baby chicken spatchcock, tender and quick to cook", "PM-WB-005", cat("Whole Birds"), brand("Valley Fresh"), 18.99m, 9m, 15, 3, true, false, 0.6m),
            P("Whole Chicken (Free Range)", "1.8kg whole free-range chicken", "PM-WB-006", cat("Whole Birds"), brand("Valley Fresh"), 18.99m, 9m, 25, 5, true, false, 1.8m),
            P("Roasting Chicken (Stuffed)", "2kg whole chicken with sage and onion stuffing", "PM-WB-007", cat("Whole Birds"), brand("Artisan Kitchen"), 34.99m, 18m, 12, 3, true, true, 2.0m),

            // ── Turkey & Game (5) ────────────────────────────────
            P("Turkey (Whole)", "5kg whole free-range turkey for celebrations", "PM-TG-001", cat("Turkey & Game"), brand("Valley Fresh"), 95.00m, 52m, 8, 2, true, true, 5.0m),
            P("Turkey Breast (Boned & Rolled)", "2.5kg boneless rolled turkey breast", "PM-TG-002", cat("Turkey & Game"), brand("Valley Fresh"), 90.00m, 48m, 8, 2, true, true, 2.5m),
            P("Turkey Buffe", "3kg turkey buffe (bone-in breast and wing)", "PM-TG-003", cat("Turkey & Game"), brand("Valley Fresh"), 100.00m, 55m, 6, 2, true, false, 3.0m),
            P("Turkey Butterflied", "4kg whole turkey butterflied for even roasting", "PM-TG-004", cat("Turkey & Game"), brand("Valley Fresh"), 115.00m, 62m, 5, 2, true, false, 4.0m),
            P("Duck Breast Fillets", "2-pack Pekin duck breast fillets with skin", "PM-TG-005", cat("Turkey & Game"), brand("Valley Fresh"), 24.99m, 13m, 15, 3, true, true, 0.5m),

            // ── Cured Meats (8) ──────────────────────────────────
            P("Prosciutto di Parma (Sliced)", "150g thinly sliced aged prosciutto", "PM-CM-001", cat("Cured Meats"), brand("Artisan Kitchen"), 14.99m, 7m, 40, 8, true, true, 0.15m),
            P("Bresaola", "150g air-dried beef bresaola, lean and peppery", "PM-CM-002", cat("Cured Meats"), brand("Artisan Kitchen"), 16.99m, 8m, 30, 5, true, false, 0.15m),
            P("Pancetta (Diced)", "200g Italian pancetta diced for cooking", "PM-CM-003", cat("Cured Meats"), brand("Artisan Kitchen"), 9.99m, 4m, 35, 5, true, false, 0.2m),
            P("Soppressa Salami", "250g traditional soppressa-style salami", "PM-CM-004", cat("Cured Meats"), brand("Artisan Kitchen"), 13.99m, 6m, 30, 5, true, false, 0.25m),
            P("Cacciatore Salami", "200g small format hunter-style cacciatore", "PM-CM-005", cat("Cured Meats"), brand("Artisan Kitchen"), 11.99m, 5m, 35, 5, true, false, 0.2m),
            P("Pepperoni", "200g spicy pepperoni, perfect for pizza and snacking", "PM-CM-006", cat("Cured Meats"), brand("Artisan Kitchen"), 8.99m, 3m, 40, 8, true, false, 0.2m),
            P("Mortadella (Sliced)", "300g classic Italian mortadella with pistachios", "PM-CM-007", cat("Cured Meats"), brand("Artisan Kitchen"), 12.99m, 5m, 25, 5, true, true, 0.3m),
            P("Coppa", "150g dry-cured pork neck coppa", "PM-CM-008", cat("Cured Meats"), brand("Artisan Kitchen"), 15.99m, 7m, 20, 4, true, false, 0.15m),

            // ── Prepared Deli (8) ────────────────────────────────
            P("Ham (Half Leg)", "3kg honey-glazed half leg ham", "PM-PD-001", cat("Prepared Deli"), brand("Artisan Kitchen"), 110.00m, 60m, 10, 2, true, true, 3.0m),
            P("Ham (Whole Leg)", "6kg whole glazed leg ham, feeds a crowd", "PM-PD-002", cat("Prepared Deli"), brand("Artisan Kitchen"), 220.00m, 120m, 5, 2, true, true, 6.0m),
            P("Chorizo (Small)", "300g spicy Spanish-style chorizo", "PM-PD-003", cat("Prepared Deli"), brand("Artisan Kitchen"), 12.50m, 5m, 40, 8, true, false, 0.3m),
            P("Salami Milano", "200g traditional Milano-style salami", "PM-PD-004", cat("Prepared Deli"), brand("Artisan Kitchen"), 12.99m, 6m, 35, 8, true, false, 0.2m),
            P("Antipasto Platter", "500g mixed cured meats, olives and pickled vegetables", "PM-PD-005", cat("Prepared Deli"), brand("Artisan Kitchen"), 24.00m, 12m, 20, 4, true, true, 0.5m),
            P("Marinated Olives", "300g mixed marinated olives with herbs and chilli", "PM-PD-006", cat("Prepared Deli"), brand("Rustic Pantry"), 8.99m, 3m, 40, 8, true, false, 0.3m),
            P("Stuffed Capsicums", "300g capsicums stuffed with ricotta and herbs", "PM-PD-007", cat("Prepared Deli"), brand("Artisan Kitchen"), 12.99m, 5m, 25, 5, true, false, 0.3m),
            P("Semi-Dried Tomatoes", "250g slow-roasted semi-dried tomatoes in oil", "PM-PD-008", cat("Prepared Deli"), brand("Rustic Pantry"), 9.99m, 4m, 30, 5, true, false, 0.25m),

            // ── Italian Classics (7) ─────────────────────────────
            P("Beef Lasagne", "1kg family-size beef and bechamel lasagne", "PM-RM-001", cat("Italian Classics"), brand("Artisan Kitchen"), 24.99m, 12m, 20, 4, true, true, 1.0m),
            P("Arancini (6 pack)", "6 golden-crumbed risotto balls with lamb ragù", "PM-RM-002", cat("Italian Classics"), brand("Artisan Kitchen"), 24.00m, 10m, 25, 5, true, true, 0.45m),
            P("Bolognese Sauce", "750g slow-cooked traditional Bolognese meat sauce", "PM-RM-003", cat("Italian Classics"), brand("Artisan Kitchen"), 18.00m, 8m, 30, 5, true, false, 0.75m),
            P("Chicken Parmigiana", "2-pack crumbed chicken parmigiana with napoli and cheese", "PM-RM-004", cat("Italian Classics"), brand("Artisan Kitchen"), 22.99m, 11m, 20, 4, true, true, 0.6m),
            P("Cannelloni", "1kg spinach and ricotta cannelloni in napoli sauce", "PM-RM-005", cat("Italian Classics"), brand("Artisan Kitchen"), 22.00m, 10m, 18, 4, true, false, 1.0m),
            P("Meatballs Italian-Style", "12-pack beef and pork meatballs in sugo", "PM-RM-006", cat("Italian Classics"), brand("Artisan Kitchen"), 19.99m, 9m, 25, 5, true, false, 0.6m),
            P("Eggplant Parmigiana", "800g layered eggplant, napoli and parmesan bake", "PM-RM-007", cat("Italian Classics"), brand("Artisan Kitchen"), 18.99m, 8m, 15, 3, true, false, 0.8m),

            // ── Sausages & Burgers (10) ──────────────────────────
            P("Beef BBQ Sausages", "6-pack classic thick beef sausages", "PM-SB-001", cat("Sausages & Burgers"), brand("Grill Master"), 14.00m, 5m, 80, 15, true, false, 0.6m),
            P("Pork Sausages - Italian (Thin)", "6-pack thin Italian-style pork sausages", "PM-SB-002", cat("Sausages & Burgers"), brand("Grill Master"), 14.00m, 6m, 60, 10, true, true, 0.5m),
            P("Lamb & Rosemary Sausages", "6-pack lamb sausages with fresh rosemary", "PM-SB-003", cat("Sausages & Burgers"), brand("Grill Master"), 16.00m, 7m, 50, 10, true, false, 0.6m),
            P("Gourmet Beef Burgers", "4-pack 150g premium beef burger patties", "PM-SB-004", cat("Sausages & Burgers"), brand("Grill Master"), 4.00m, 2m, 60, 10, true, true, 0.6m),
            P("Pork & Fennel Sausages", "6-pack Italian-style pork and fennel", "PM-SB-005", cat("Sausages & Burgers"), brand("Grill Master"), 14.99m, 6m, 50, 10, true, false, 0.6m),
            P("Chicken Sausages", "6-pack lean chicken sausages with herbs", "PM-SB-006", cat("Sausages & Burgers"), brand("Grill Master"), 13.00m, 5m, 40, 8, true, false, 0.5m),
            P("Chorizo Sausages", "6-pack spicy smoked chorizo sausages", "PM-SB-007", cat("Sausages & Burgers"), brand("Grill Master"), 16.99m, 8m, 35, 5, true, false, 0.6m),
            P("Veal & Pork Sausages", "6-pack traditional Italian veal and pork", "PM-SB-008", cat("Sausages & Burgers"), brand("Grill Master"), 15.99m, 7m, 30, 5, true, false, 0.6m),
            P("Lamb Burgers", "4-pack 150g spiced lamb burger patties with herbs", "PM-SB-009", cat("Sausages & Burgers"), brand("Grill Master"), 16.99m, 8m, 35, 5, true, false, 0.6m),
            P("Merguez Sausages", "6-pack North African spiced lamb merguez", "PM-SB-010", cat("Sausages & Burgers"), brand("Grill Master"), 17.99m, 9m, 25, 5, true, true, 0.5m),

            // ── Curries & Stews (6) ──────────────────────────────
            P("Slow-Cooked Beef Ragù", "750g rich beef ragù with red wine and herbs", "PM-CS-001", cat("Curries & Stews"), brand("Artisan Kitchen"), 22.99m, 10m, 20, 4, true, false, 0.75m),
            P("Lamb Massaman Curry", "700g tender lamb in massaman curry sauce", "PM-CS-002", cat("Curries & Stews"), brand("Artisan Kitchen"), 24.99m, 12m, 18, 4, true, false, 0.7m),
            P("Chicken Cacciatore", "750g braised chicken in tomato, olive and herb sauce", "PM-CS-003", cat("Curries & Stews"), brand("Artisan Kitchen"), 21.99m, 10m, 18, 4, true, true, 0.75m),
            P("Braised Lamb Shanks", "1kg pre-braised lamb shanks in red wine jus", "PM-CS-004", cat("Curries & Stews"), brand("Artisan Kitchen"), 32.99m, 16m, 12, 3, true, true, 1.0m),
            P("Osso Buco alla Milanese", "1kg veal osso buco in saffron and gremolata sauce", "PM-CS-005", cat("Curries & Stews"), brand("Artisan Kitchen"), 36.99m, 18m, 10, 3, true, false, 1.0m),
            P("Beef Stroganoff", "700g creamy beef stroganoff with mushrooms", "PM-CS-006", cat("Curries & Stews"), brand("Artisan Kitchen"), 22.99m, 10m, 15, 4, true, false, 0.7m),

            // ── BBQ Ready (6) ────────────────────────────────────
            P("BBQ Platter Mixed", "1.5kg mixed BBQ platter with sausages, steaks and chicken", "PM-BB-001", cat("BBQ Ready"), brand("Grill Master"), 45.00m, 22m, 15, 3, true, true, 1.5m),
            P("Marinated Chicken Wings", "1kg smoky BBQ marinated chicken wings", "PM-BB-002", cat("BBQ Ready"), brand("Grill Master"), 14.99m, 6m, 40, 8, true, false, 1.0m),
            P("Beef Kebabs", "8-pack marinated beef kebabs with capsicum and onion", "PM-BB-003", cat("BBQ Ready"), brand("Grill Master"), 22.99m, 11m, 30, 5, true, true, 0.6m),
            P("Lamb Kebabs", "8-pack marinated lamb kebabs with Mediterranean herbs", "PM-BB-004", cat("BBQ Ready"), brand("Grill Master"), 24.99m, 12m, 25, 5, true, false, 0.6m),
            P("BBQ Pork Ribs Marinated", "1.2kg American-style BBQ marinated pork ribs", "PM-BB-005", cat("BBQ Ready"), brand("Grill Master"), 29.99m, 14m, 18, 4, true, true, 1.2m),
            P("Surf & Turf Pack", "1kg combination of beef fillet medallions and prawns", "PM-BB-006", cat("BBQ Ready"), brand("Grill Master"), 49.99m, 26m, 10, 3, true, true, 1.0m),

            // ── Pasta & Sauces (8) ──────────────────────────────
            P("Rustichella Spaghetti", "500g bronze-cut Italian dried spaghetti", "PM-PT-001", cat("Pasta & Sauces"), brand("Rustic Pantry"), 9.95m, 4m, 80, 15, true, false, 0.5m),
            P("Arrabiata Pasta Sauce", "500ml Rustichella d'Abruzzo arrabiata sauce", "PM-PT-002", cat("Pasta & Sauces"), brand("Rustic Pantry"), 18.50m, 8m, 50, 10, true, true, 0.5m),
            P("Sugo di Pomodoro", "500ml slow-cooked tomato pasta sauce", "PM-PT-003", cat("Pasta & Sauces"), brand("Rustic Pantry"), 8.99m, 3m, 80, 15, true, false, 0.5m),
            P("Penne Rigate", "500g Italian dried bronze-cut penne", "PM-PT-004", cat("Pasta & Sauces"), brand("Rustic Pantry"), 6.99m, 2.5m, 100, 20, true, false, 0.5m),
            P("Fusilli", "500g Italian dried bronze-cut fusilli spirals", "PM-PT-005", cat("Pasta & Sauces"), brand("Rustic Pantry"), 6.99m, 2.5m, 80, 15, true, false, 0.5m),
            P("Rigatoni", "500g Italian dried bronze-cut rigatoni tubes", "PM-PT-006", cat("Pasta & Sauces"), brand("Rustic Pantry"), 7.49m, 3m, 70, 12, true, false, 0.5m),
            P("Pesto alla Genovese", "190g traditional basil pesto from Genoa", "PM-PT-007", cat("Pasta & Sauces"), brand("Rustic Pantry"), 11.99m, 5m, 40, 8, true, true, 0.19m),
            P("Napolitana Sauce", "500ml classic slow-cooked napolitana pasta sauce", "PM-PT-008", cat("Pasta & Sauces"), brand("Rustic Pantry"), 9.50m, 4m, 60, 10, true, false, 0.5m),

            // ── Oils & Seasonings (8) ────────────────────────────
            P("Extra Virgin Olive Oil", "500ml cold-pressed Sicilian olive oil", "PM-OS-001", cat("Oils & Seasonings"), brand("Rustic Pantry"), 18.99m, 8m, 50, 10, true, true, 0.5m),
            P("Murray River Pink Salt Flakes", "200g premium Australian pink salt", "PM-OS-002", cat("Oils & Seasonings"), brand("Rustic Pantry"), 9.95m, 4m, 60, 10, true, false, 0.2m),
            P("Chimichurri Marinade", "250ml fresh herb and garlic chimichurri", "PM-OS-003", cat("Oils & Seasonings"), brand("Grill Master"), 9.99m, 3m, 60, 10, true, false, 0.25m),
            P("Smoky BBQ Rub", "150g house-blend smoky barbecue spice rub", "PM-OS-004", cat("Oils & Seasonings"), brand("Grill Master"), 7.99m, 2m, 80, 15, true, true, 0.15m),
            P("Balsamic Vinegar di Modena", "250ml aged balsamic vinegar from Modena, Italy", "PM-OS-005", cat("Oils & Seasonings"), brand("Rustic Pantry"), 14.99m, 6m, 40, 8, true, false, 0.25m),
            P("Truffle Oil", "100ml black truffle infused extra virgin olive oil", "PM-OS-006", cat("Oils & Seasonings"), brand("Rustic Pantry"), 19.99m, 9m, 25, 5, true, true, 0.1m),
            P("Italian Herb Mix", "100g dried Italian herb blend for seasoning", "PM-OS-007", cat("Oils & Seasonings"), brand("Rustic Pantry"), 6.99m, 2m, 50, 10, true, false, 0.1m),
            P("Garlic & Herb Butter", "250g compound butter with roasted garlic and parsley", "PM-OS-008", cat("Oils & Seasonings"), brand("Artisan Kitchen"), 8.99m, 3m, 30, 5, true, false, 0.25m),

            // ── Condiments & Preserves (6) ───────────────────────
            P("Wholegrain Mustard", "200g stone-ground wholegrain mustard", "PM-CN-001", cat("Condiments & Preserves"), brand("Rustic Pantry"), 7.99m, 3m, 40, 8, true, false, 0.2m),
            P("Fig & Walnut Paste", "150g artisan fig and walnut paste for cheese boards", "PM-CN-002", cat("Condiments & Preserves"), brand("Rustic Pantry"), 12.99m, 5m, 25, 5, true, true, 0.15m),
            P("Caramelised Onion Jam", "300g slow-cooked caramelised onion jam", "PM-CN-003", cat("Condiments & Preserves"), brand("Rustic Pantry"), 9.99m, 4m, 30, 5, true, false, 0.3m),
            P("Pickled Vegetables", "350g Italian-style giardiniera pickled vegetables", "PM-CN-004", cat("Condiments & Preserves"), brand("Rustic Pantry"), 8.99m, 3m, 35, 5, true, false, 0.35m),
            P("Australian Honey", "500g raw unfiltered Australian bush honey", "PM-CN-005", cat("Condiments & Preserves"), brand("Rustic Pantry"), 14.99m, 6m, 30, 5, true, true, 0.5m),
            P("Tomato Chutney", "280g spiced tomato chutney, pairs with cold meats", "PM-CN-006", cat("Condiments & Preserves"), brand("Rustic Pantry"), 8.99m, 3m, 35, 5, true, false, 0.28m),
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Add variants for Beef BBQ Sausages (flavour packs)
        var sausages = products.First(p => p.SKU == "PM-SB-001");
        var sausageFlavours = new[] { "Original", "Pepper & Onion", "Smoky BBQ" };
        foreach (var flavour in sausageFlavours)
            context.ProductVariants.Add(new ProductVariant { ProductId = sausages.Id, VariantName = flavour, SKU = $"PM-SB-001-{flavour[..3].ToUpper()}", Price = 14.00m, StockQuantity = 25, Attributes = $"{{\"flavour\":\"{flavour}\"}}", CreatedBy = "system" });

        // Add variants for Gourmet Beef Burgers (size)
        var burgers = products.First(p => p.SKU == "PM-SB-004");
        context.ProductVariants.Add(new ProductVariant { ProductId = burgers.Id, VariantName = "Single", SKU = "PM-SB-004-1PK", Price = 4.00m, StockQuantity = 80, Attributes = "{\"size\":\"single\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = burgers.Id, VariantName = "4-pack", SKU = "PM-SB-004-4PK", Price = 14.00m, StockQuantity = 40, Attributes = "{\"size\":\"4-pack\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = burgers.Id, VariantName = "8-pack", SKU = "PM-SB-004-8PK", Price = 26.00m, StockQuantity = 20, Attributes = "{\"size\":\"8-pack\"}", CreatedBy = "system" });

        // Add variants for Porchetta (size)
        var porchetta = products.First(p => p.SKU == "PM-PR-001");
        context.ProductVariants.Add(new ProductVariant { ProductId = porchetta.Id, VariantName = "Small (1kg)", SKU = "PM-PR-001-SM", Price = 50.00m, StockQuantity = 10, Attributes = "{\"size\":\"1kg\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = porchetta.Id, VariantName = "Large (2kg)", SKU = "PM-PR-001-LG", Price = 90.00m, StockQuantity = 8, Attributes = "{\"size\":\"2kg\"}", CreatedBy = "system" });

        // Add variants for Turkey Whole (size)
        var turkey = products.First(p => p.SKU == "PM-TG-001");
        context.ProductVariants.Add(new ProductVariant { ProductId = turkey.Id, VariantName = "Small (4kg)", SKU = "PM-TG-001-SM", Price = 85.00m, StockQuantity = 5, Attributes = "{\"size\":\"4kg\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = turkey.Id, VariantName = "Medium (5kg)", SKU = "PM-TG-001-MD", Price = 95.00m, StockQuantity = 5, Attributes = "{\"size\":\"5kg\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = turkey.Id, VariantName = "Large (7kg)", SKU = "PM-TG-001-LG", Price = 135.00m, StockQuantity = 3, Attributes = "{\"size\":\"7kg\"}", CreatedBy = "system" });

        // Add variants for Ham Whole Leg (size)
        var ham = products.First(p => p.SKU == "PM-PD-002");
        context.ProductVariants.Add(new ProductVariant { ProductId = ham.Id, VariantName = "Half Leg (3kg)", SKU = "PM-PD-002-HL", Price = 110.00m, StockQuantity = 8, Attributes = "{\"size\":\"3kg\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = ham.Id, VariantName = "Whole Leg (6kg)", SKU = "PM-PD-002-WL", Price = 220.00m, StockQuantity = 4, Attributes = "{\"size\":\"6kg\"}", CreatedBy = "system" });

        await context.SaveChangesAsync();
    }

    private static Product P(string name, string desc, string sku, Category cat, Brand brand, decimal price, decimal cost, int stock, int min, bool active, bool featured, decimal weight)
        => new() { Name = name, Description = desc, SKU = sku, CategoryId = cat.Id, BrandId = brand.Id, BasePrice = price, CostPrice = cost, StockQuantity = stock, MinStockLevel = min, IsActive = active, IsFeatured = featured, Weight = weight, CreatedBy = "system" };

    private static async Task SeedProductImagesAsync(AppDbContext context)
    {
        if (await context.ProductImages.AnyAsync()) return;
        var products = await context.Products.ToListAsync();

        var imageMap = new Dictionary<string, string[]>
        {
            // Beef Steaks
            ["PM-BS-001"] = new[] { "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=800", "https://images.unsplash.com/photo-1558030006-450675393462?w=800" },
            ["PM-BS-002"] = new[] { "https://images.unsplash.com/photo-1588168333986-5078d3ae3976?w=800" },
            ["PM-BS-003"] = new[] { "https://images.unsplash.com/photo-1551028150-64b9f398f678?w=800" },
            ["PM-BS-004"] = new[] { "https://images.unsplash.com/photo-1603048297172-c92544798d5a?w=800" },
            ["PM-BS-005"] = new[] { "https://images.unsplash.com/photo-1544025162-d76694265947?w=800" },
            ["PM-BS-006"] = new[] { "https://images.unsplash.com/photo-1615937722923-67f6deaf2cc9?w=800", "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?w=800" },
            ["PM-BS-007"] = new[] { "https://images.unsplash.com/photo-1529694157872-4e0c0f3b238b?w=800" },
            ["PM-BS-008"] = new[] { "https://images.unsplash.com/photo-1546964124-0cce460f38ef?w=800" },
            ["PM-BS-009"] = new[] { "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800" },
            ["PM-BS-010"] = new[] { "https://images.unsplash.com/photo-1594041680534-e8c8cdebd659?w=800" },
            ["PM-BS-011"] = new[] { "https://images.unsplash.com/photo-1558030006-450675393462?w=800" },
            ["PM-BS-012"] = new[] { "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800" },
            // Beef Roasting Joints
            ["PM-BR-001"] = new[] { "https://images.unsplash.com/photo-1588347818481-0e7b4e5f4e94?w=800" },
            ["PM-BR-002"] = new[] { "https://images.unsplash.com/photo-1560781290-7dc94c0f8f4f?w=800" },
            ["PM-BR-003"] = new[] { "https://images.unsplash.com/photo-1529694157872-4e0c0f3b238b?w=800" },
            ["PM-BR-004"] = new[] { "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?w=800" },
            ["PM-BR-005"] = new[] { "https://images.unsplash.com/photo-1558030006-450675393462?w=800" },
            ["PM-BR-006"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-BR-007"] = new[] { "https://images.unsplash.com/photo-1544025162-d76694265947?w=800" },
            ["PM-BR-008"] = new[] { "https://images.unsplash.com/photo-1551135049-8a33b5883817?w=800" },
            ["PM-BR-009"] = new[] { "https://images.unsplash.com/photo-1609167830220-7164aa7bf827?w=800" },
            // Beef Other
            ["PM-BO-001"] = new[] { "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800" },
            ["PM-BO-002"] = new[] { "https://images.unsplash.com/photo-1551135049-8a33b5883817?w=800" },
            ["PM-BO-003"] = new[] { "https://images.unsplash.com/photo-1609167830220-7164aa7bf827?w=800" },
            ["PM-BO-004"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-BO-005"] = new[] { "https://images.unsplash.com/photo-1588347818481-0e7b4e5f4e94?w=800" },
            ["PM-BO-006"] = new[] { "https://images.unsplash.com/photo-1560781290-7dc94c0f8f4f?w=800" },
            ["PM-BO-007"] = new[] { "https://images.unsplash.com/photo-1551028150-64b9f398f678?w=800" },
            ["PM-BO-008"] = new[] { "https://images.unsplash.com/photo-1594041680534-e8c8cdebd659?w=800" },
            // Veal Steaks & Cutlets
            ["PM-VS-001"] = new[] { "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800" },
            ["PM-VS-002"] = new[] { "https://images.unsplash.com/photo-1619221882266-14ef84780a0e?w=800" },
            ["PM-VS-003"] = new[] { "https://images.unsplash.com/photo-1610540881590-e9f5e1d11d63?w=800" },
            ["PM-VS-004"] = new[] { "https://images.unsplash.com/photo-1585325701165-351af305e480?w=800" },
            ["PM-VS-005"] = new[] { "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800" },
            ["PM-VS-006"] = new[] { "https://images.unsplash.com/photo-1619221882266-14ef84780a0e?w=800" },
            // Veal Specialty
            ["PM-VX-001"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-VX-002"] = new[] { "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=800" },
            ["PM-VX-003"] = new[] { "https://images.unsplash.com/photo-1619221882266-14ef84780a0e?w=800" },
            ["PM-VX-004"] = new[] { "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800" },
            ["PM-VX-005"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-VX-006"] = new[] { "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=800" },
            // Lamb Chops & Cutlets
            ["PM-LC-001"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-LC-002"] = new[] { "https://images.unsplash.com/photo-1598515214211-89d3c73ae83b?w=800" },
            ["PM-LC-003"] = new[] { "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=800" },
            ["PM-LC-004"] = new[] { "https://images.unsplash.com/photo-1514516345957-556ca7d90a29?w=800" },
            ["PM-LC-005"] = new[] { "https://images.unsplash.com/photo-1608877907149-a206d75ba011?w=800" },
            ["PM-LC-006"] = new[] { "https://images.unsplash.com/photo-1598515214211-89d3c73ae83b?w=800" },
            ["PM-LC-007"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-LC-008"] = new[] { "https://images.unsplash.com/photo-1608877907149-a206d75ba011?w=800" },
            ["PM-LC-009"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            // Lamb Roasting Joints
            ["PM-LR-001"] = new[] { "https://images.unsplash.com/photo-1608877907149-a206d75ba011?w=800" },
            ["PM-LR-002"] = new[] { "https://images.unsplash.com/photo-1574484284002-952d92456975?w=800" },
            ["PM-LR-003"] = new[] { "https://images.unsplash.com/photo-1606728035253-49e8a23146de?w=800" },
            ["PM-LR-004"] = new[] { "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=800" },
            ["PM-LR-005"] = new[] { "https://images.unsplash.com/photo-1574484284002-952d92456975?w=800" },
            ["PM-LR-006"] = new[] { "https://images.unsplash.com/photo-1606728035253-49e8a23146de?w=800" },
            ["PM-LR-007"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-LR-008"] = new[] { "https://images.unsplash.com/photo-1514516345957-556ca7d90a29?w=800" },
            // Lamb Other Cuts
            ["PM-LO-001"] = new[] { "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800" },
            ["PM-LO-002"] = new[] { "https://images.unsplash.com/photo-1551135049-8a33b5883817?w=800" },
            ["PM-LO-003"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-LO-004"] = new[] { "https://images.unsplash.com/photo-1608877907149-a206d75ba011?w=800" },
            ["PM-LO-005"] = new[] { "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=800" },
            ["PM-LO-006"] = new[] { "https://images.unsplash.com/photo-1609167830220-7164aa7bf827?w=800" },
            // Pork Steaks & Chops
            ["PM-PS-001"] = new[] { "https://images.unsplash.com/photo-1623174479650-562c9a8af8fa?w=800" },
            ["PM-PS-002"] = new[] { "https://images.unsplash.com/photo-1606568218095-54b5f6b6e1a8?w=800" },
            ["PM-PS-003"] = new[] { "https://images.unsplash.com/photo-1610540881590-e9f5e1d11d63?w=800" },
            ["PM-PS-004"] = new[] { "https://images.unsplash.com/photo-1623174479650-562c9a8af8fa?w=800" },
            ["PM-PS-005"] = new[] { "https://images.unsplash.com/photo-1606568218095-54b5f6b6e1a8?w=800" },
            ["PM-PS-006"] = new[] { "https://images.unsplash.com/photo-1585325701165-351af305e480?w=800" },
            ["PM-PS-007"] = new[] { "https://images.unsplash.com/photo-1623174479650-562c9a8af8fa?w=800" },
            // Pork Roasting Joints
            ["PM-PR-001"] = new[] { "https://images.unsplash.com/photo-1592686092538-a77869c15422?w=800", "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            ["PM-PR-002"] = new[] { "https://images.unsplash.com/photo-1590779033100-9f60a05a013d?w=800" },
            ["PM-PR-003"] = new[] { "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800" },
            ["PM-PR-004"] = new[] { "https://images.unsplash.com/photo-1606568218095-54b5f6b6e1a8?w=800" },
            ["PM-PR-005"] = new[] { "https://images.unsplash.com/photo-1592686092538-a77869c15422?w=800" },
            ["PM-PR-006"] = new[] { "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            // Pork Other Cuts
            ["PM-PO-001"] = new[] { "https://images.unsplash.com/photo-1592686092538-a77869c15422?w=800" },
            ["PM-PO-002"] = new[] { "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            ["PM-PO-003"] = new[] { "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800" },
            ["PM-PO-004"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            ["PM-PO-005"] = new[] { "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800" },
            ["PM-PO-006"] = new[] { "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            ["PM-PO-007"] = new[] { "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800" },
            // Chicken Portions
            ["PM-CP-001"] = new[] { "https://images.unsplash.com/photo-1604503468506-a8da13d82f2b?w=800" },
            ["PM-CP-002"] = new[] { "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800" },
            ["PM-CP-003"] = new[] { "https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=800" },
            ["PM-CP-004"] = new[] { "https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=800" },
            ["PM-CP-005"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-CP-006"] = new[] { "https://images.unsplash.com/photo-1604503468506-a8da13d82f2b?w=800" },
            ["PM-CP-007"] = new[] { "https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=800" },
            ["PM-CP-008"] = new[] { "https://images.unsplash.com/photo-1585325701165-351af305e480?w=800" },
            ["PM-CP-009"] = new[] { "https://images.unsplash.com/photo-1610540881590-e9f5e1d11d63?w=800" },
            // Whole Birds
            ["PM-WB-001"] = new[] { "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800" },
            ["PM-WB-002"] = new[] { "https://images.unsplash.com/photo-1501200291289-c5a76c232e5f?w=800" },
            ["PM-WB-003"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-WB-004"] = new[] { "https://images.unsplash.com/photo-1574653853027-5382a3d23a15?w=800" },
            ["PM-WB-005"] = new[] { "https://images.unsplash.com/photo-1501200291289-c5a76c232e5f?w=800" },
            ["PM-WB-006"] = new[] { "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800" },
            ["PM-WB-007"] = new[] { "https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=800" },
            // Turkey & Game
            ["PM-TG-001"] = new[] { "https://images.unsplash.com/photo-1574653853027-5382a3d23a15?w=800" },
            ["PM-TG-002"] = new[] { "https://images.unsplash.com/photo-1606728035253-49e8a23146de?w=800" },
            ["PM-TG-003"] = new[] { "https://images.unsplash.com/photo-1574653853027-5382a3d23a15?w=800" },
            ["PM-TG-004"] = new[] { "https://images.unsplash.com/photo-1501200291289-c5a76c232e5f?w=800" },
            ["PM-TG-005"] = new[] { "https://images.unsplash.com/photo-1574653853027-5382a3d23a15?w=800" },
            // Cured Meats
            ["PM-CM-001"] = new[] { "https://images.unsplash.com/photo-1626200419199-391ae4be7a41?w=800" },
            ["PM-CM-002"] = new[] { "https://images.unsplash.com/photo-1541014741259-de529411b96a?w=800" },
            ["PM-CM-003"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            ["PM-CM-004"] = new[] { "https://images.unsplash.com/photo-1541014741259-de529411b96a?w=800" },
            ["PM-CM-005"] = new[] { "https://images.unsplash.com/photo-1626200419199-391ae4be7a41?w=800" },
            ["PM-CM-006"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            ["PM-CM-007"] = new[] { "https://images.unsplash.com/photo-1524438418049-ab2acb7aa48f?w=800" },
            ["PM-CM-008"] = new[] { "https://images.unsplash.com/photo-1626200419199-391ae4be7a41?w=800" },
            // Prepared Deli
            ["PM-PD-001"] = new[] { "https://images.unsplash.com/photo-1524438418049-ab2acb7aa48f?w=800" },
            ["PM-PD-002"] = new[] { "https://images.unsplash.com/photo-1544025162-d76694265947?w=800" },
            ["PM-PD-003"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            ["PM-PD-004"] = new[] { "https://images.unsplash.com/photo-1541014741259-de529411b96a?w=800" },
            ["PM-PD-005"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-PD-006"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-PD-007"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-PD-008"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            // Italian Classics
            ["PM-RM-001"] = new[] { "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=800" },
            ["PM-RM-002"] = new[] { "https://images.unsplash.com/photo-1595295333158-4742f28fbd85?w=800" },
            ["PM-RM-003"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-RM-004"] = new[] { "https://images.unsplash.com/photo-1585325701165-351af305e480?w=800" },
            ["PM-RM-005"] = new[] { "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=800" },
            ["PM-RM-006"] = new[] { "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=800" },
            ["PM-RM-007"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            // Sausages & Burgers
            ["PM-SB-001"] = new[] { "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800", "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800" },
            ["PM-SB-002"] = new[] { "https://images.unsplash.com/photo-1627309302198-09a50ae3d566?w=800" },
            ["PM-SB-003"] = new[] { "https://images.unsplash.com/photo-1587536849024-daaa4a417b16?w=800" },
            ["PM-SB-004"] = new[] { "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=800", "https://images.unsplash.com/photo-1550547660-d9450f859349?w=800" },
            ["PM-SB-005"] = new[] { "https://images.unsplash.com/photo-1627309302198-09a50ae3d566?w=800" },
            ["PM-SB-006"] = new[] { "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800" },
            ["PM-SB-007"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            ["PM-SB-008"] = new[] { "https://images.unsplash.com/photo-1627309302198-09a50ae3d566?w=800" },
            ["PM-SB-009"] = new[] { "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=800" },
            ["PM-SB-010"] = new[] { "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800" },
            // Curries & Stews
            ["PM-CS-001"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-CS-002"] = new[] { "https://images.unsplash.com/photo-1455619452474-d2be8b1e70cd?w=800" },
            ["PM-CS-003"] = new[] { "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=800" },
            ["PM-CS-004"] = new[] { "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=800" },
            ["PM-CS-005"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-CS-006"] = new[] { "https://images.unsplash.com/photo-1455619452474-d2be8b1e70cd?w=800" },
            // BBQ Ready
            ["PM-BB-001"] = new[] { "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800" },
            ["PM-BB-002"] = new[] { "https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=800" },
            ["PM-BB-003"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-BB-004"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-BB-005"] = new[] { "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            ["PM-BB-006"] = new[] { "https://images.unsplash.com/photo-1558030006-450675393462?w=800" },
            // Pasta & Sauces
            ["PM-PT-001"] = new[] { "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=800" },
            ["PM-PT-002"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            ["PM-PT-003"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            ["PM-PT-004"] = new[] { "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=800" },
            ["PM-PT-005"] = new[] { "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=800" },
            ["PM-PT-006"] = new[] { "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=800" },
            ["PM-PT-007"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-PT-008"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            // Oils & Seasonings
            ["PM-OS-001"] = new[] { "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800" },
            ["PM-OS-002"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-OS-003"] = new[] { "https://images.unsplash.com/photo-1628557044797-f21a177c37ec?w=800" },
            ["PM-OS-004"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-OS-005"] = new[] { "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800" },
            ["PM-OS-006"] = new[] { "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800" },
            ["PM-OS-007"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-OS-008"] = new[] { "https://images.unsplash.com/photo-1628557044797-f21a177c37ec?w=800" },
            // Condiments & Preserves
            ["PM-CN-001"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-CN-002"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            ["PM-CN-003"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-CN-004"] = new[] { "https://images.unsplash.com/photo-1628557044797-f21a177c37ec?w=800" },
            ["PM-CN-005"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-CN-006"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
        };

        var images = new List<ProductImage>();
        foreach (var product in products)
        {
            if (imageMap.TryGetValue(product.SKU, out var urls))
            {
                for (int i = 0; i < urls.Length; i++)
                {
                    images.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = urls[i],
                        AltText = product.Name,
                        SortOrder = i,
                        IsPrimary = i == 0,
                        CreatedBy = "system"
                    });
                }
            }
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
            new() { Code = "BBQ20", Name = "BBQ Season 20%", Description = "20% off all sausages and burgers", DiscountType = DiscountType.Percentage, Value = 20, MinOrderAmount = 25, MaxDiscountAmount = 30, StartDate = now.AddDays(-5), EndDate = now.AddDays(45), IsActive = true, UsageLimit = 500, UsedCount = 38, CreatedBy = "system" },
            new() { Code = "ROAST10", Name = "Sunday Roast $10 Off", Description = "$10 off any roasting joint over $40", DiscountType = DiscountType.Fixed, Value = 10, MinOrderAmount = 40, StartDate = now.AddDays(-10), EndDate = now.AddDays(60), IsActive = true, UsageLimit = 300, UsedCount = 67, CreatedBy = "system" },
            new() { Code = "FAMILY25", Name = "Family Pack 25% Off", Description = "25% off orders over $100", DiscountType = DiscountType.Percentage, Value = 25, MinOrderAmount = 100, MaxDiscountAmount = 60, StartDate = now, EndDate = now.AddDays(30), IsActive = true, UsageLimit = 200, UsedCount = 12, CreatedBy = "system" },
            new() { Code = "FREEDELIVERY", Name = "Free Delivery", Description = "Free delivery on orders over $50", DiscountType = DiscountType.Fixed, Value = 12.99m, MinOrderAmount = 50, StartDate = now.AddDays(-30), EndDate = now.AddDays(60), IsActive = true, UsageLimit = 1000, UsedCount = 189, CreatedBy = "system" },
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
            new() { Code = "MEAT25", Description = "$25 gift voucher", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 25, StartDate = now.AddDays(-30), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
            new() { Code = "MEAT50", Description = "$50 gift voucher", VoucherType = VoucherType.Gift, ValueType = VoucherValueType.Fixed, Value = 50, StartDate = now.AddDays(-15), ExpiryDate = now.AddDays(180), IsActive = true, MaxRedemptions = 1, CurrentRedemptions = 0, CreatedBy = "system" },
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
            new() { Name = "Weekend BBQ Pack", Description = "Save on sausages, burgers and marinades this weekend", OfferType = OfferType.Bundle, StartDate = now, EndDate = now.AddDays(3), IsActive = true, CreatedBy = "system" },
            new() { Name = "Steak Night Special", Description = "Premium steaks at flash sale prices", OfferType = OfferType.Flash, StartDate = now.AddDays(-1), EndDate = now.AddDays(7), IsActive = true, CreatedBy = "system" },
            new() { Name = "Winter Roast Season", Description = "15% off all roasting joints for winter", OfferType = OfferType.Seasonal, StartDate = now, EndDate = now.AddDays(60), IsActive = true, CreatedBy = "system" },
        };
        context.SpecialOffers.AddRange(offers);
        await context.SaveChangesAsync();

        // Link products to offers
        var bbqProducts = products.Where(p => p.SKU.StartsWith("PM-SB") || p.SKU.StartsWith("PM-OS")).ToList();
        foreach (var p in bbqProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[0].Id, ProductId = p.Id, CreatedBy = "system" });

        var steakProducts = products.Where(p => p.SKU.StartsWith("PM-BS")).ToList();
        foreach (var p in steakProducts)
            context.SpecialOfferProducts.Add(new SpecialOfferProduct { SpecialOfferId = offers[1].Id, ProductId = p.Id, CreatedBy = "system" });

        var roastProducts = products.Where(p => p.SKU.StartsWith("PM-BR") || p.SKU.StartsWith("PM-LR") || p.SKU.StartsWith("PM-PR")).ToList();
        foreach (var p in roastProducts)
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
            ("Absolutely tender!", "The scotch fillet was perfectly marbled. Best steak I've had at home."),
            ("Family favourite", "Our kids love the sausages. We order every week now."),
            ("Restaurant quality", "The dry-aged ribeye was incredible. Worth every cent."),
            ("Great for meal prep", "Chicken breast fillets are always fresh and well-trimmed."),
            ("Sunday roast sorted", "The lamb leg was perfect. Fell off the bone after slow roasting."),
            ("Delicious deli meats", "The prosciutto is thinly sliced and has great flavour."),
            ("Easy weeknight dinner", "The beef lasagne is generous and tastes homemade."),
            ("Best burgers ever", "These gourmet patties are juicy and hold together perfectly on the grill."),
            ("Premium quality", "You can really taste the difference with free-range chicken."),
            ("Pantry staple", "The olive oil is top quality. Use it on everything now."),
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
            new() { Key = "Store.Name", Value = "Primo Meats", Group = "General", Description = "Store display name", CreatedBy = "system" },
            new() { Key = "Store.Currency", Value = "AUD", Group = "General", Description = "Default currency", CreatedBy = "system" },
            new() { Key = "Store.TaxRate", Value = "10", Group = "General", Description = "GST rate percentage", CreatedBy = "system" },
            new() { Key = "Store.DeliveryFee", Value = "12.99", Group = "Delivery", Description = "Default delivery fee", CreatedBy = "system" },
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
