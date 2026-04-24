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
            await SeedProductImagesAsync(context);
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
            new() { Name = "Beef", Description = "Premium grain-fed and grass-fed beef cuts", ImageUrl = "https://images.unsplash.com/photo-1588168333986-5078d3ae3976?w=800", SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lamb", Description = "Tender spring lamb sourced from local farms", ImageUrl = "https://images.unsplash.com/photo-1603048297172-c92544798d5a?w=800", SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pork", Description = "Free-range pork from trusted producers", ImageUrl = "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800", SortOrder = 3, IsActive = true, CreatedBy = "system" },
            new() { Name = "Poultry", Description = "Free-range chicken, duck and turkey", ImageUrl = "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800", SortOrder = 4, IsActive = true, CreatedBy = "system" },
            new() { Name = "Veal", Description = "Ethically raised premium veal", SortOrder = 5, IsActive = true, CreatedBy = "system" },
            new() { Name = "Sausages & Burgers", Description = "Handmade sausages and gourmet burger patties", ImageUrl = "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800", SortOrder = 6, IsActive = true, CreatedBy = "system" },
            new() { Name = "Deli & Cold Cuts", Description = "Cured meats, salumi, and antipasto platters", ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=800", SortOrder = 7, IsActive = true, CreatedBy = "system" },
            new() { Name = "Ready Meals", Description = "Chef-prepared meals ready to heat and serve", SortOrder = 8, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pantry", Description = "Artisan pasta, sauces, oils and condiments", SortOrder = 9, IsActive = true, CreatedBy = "system" },
            new() { Name = "Marinades & Rubs", Description = "House-made marinades, rubs and seasonings", SortOrder = 10, IsActive = true, CreatedBy = "system" },
        };
        context.Categories.AddRange(topLevel);
        await context.SaveChangesAsync();

        var subCats = new List<Category>
        {
            // Beef subs
            new() { Name = "Beef Steaks", Description = "Premium steak cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Beef Roasting Joints", Description = "Slow roast and Sunday roast cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            new() { Name = "Beef Other Cuts", Description = "Mince, diced, strips and specialty cuts", ParentCategoryId = topLevel[0].Id, SortOrder = 3, IsActive = true, CreatedBy = "system" },
            // Lamb subs
            new() { Name = "Lamb Chops & Cutlets", Description = "Chops, cutlets and racks", ParentCategoryId = topLevel[1].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Lamb Roasting Joints", Description = "Leg, shoulder and rolled roasts", ParentCategoryId = topLevel[1].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            // Pork subs
            new() { Name = "Pork Steaks & Chops", Description = "Loin chops, steaks and cutlets", ParentCategoryId = topLevel[2].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Pork Roasting Joints", Description = "Rolled pork, rack and belly roasts", ParentCategoryId = topLevel[2].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            // Poultry subs
            new() { Name = "Chicken Breast & Thigh", Description = "Skinless breast, thigh fillets and tenderloins", ParentCategoryId = topLevel[3].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Whole Birds", Description = "Whole chicken, spatchcock and duck", ParentCategoryId = topLevel[3].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
            // Ready Meals subs
            new() { Name = "Italian Classics", Description = "Lasagne, arancini, involtini and more", ParentCategoryId = topLevel[7].Id, SortOrder = 1, IsActive = true, CreatedBy = "system" },
            new() { Name = "Curries & Stews", Description = "Slow-cooked curries and hearty stews", ParentCategoryId = topLevel[7].Id, SortOrder = 2, IsActive = true, CreatedBy = "system" },
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
            // Beef Steaks (5)
            P("Scotch Fillet Steak", "300g premium grain-fed scotch fillet, beautifully marbled", "PM-BS-001", cat("Beef Steaks"), brand("Primo Cuts"), 32.99m, 18m, 40, 5, true, true, 0.3m),
            P("Eye Fillet Steak", "250g centre-cut eye fillet, the tenderest steak cut", "PM-BS-002", cat("Beef Steaks"), brand("Primo Cuts"), 38.99m, 22m, 30, 5, true, true, 0.25m),
            P("T-Bone Steak", "450g bone-in T-bone with fillet and sirloin", "PM-BS-003", cat("Beef Steaks"), brand("Primo Cuts"), 29.99m, 16m, 35, 5, true, false, 0.45m),
            P("Porterhouse Steak", "350g thick-cut porterhouse, perfect for grilling", "PM-BS-004", cat("Beef Steaks"), brand("Primo Cuts"), 27.99m, 14m, 45, 8, true, false, 0.35m),
            P("Dry-Aged Ribeye 45 Day", "400g 45-day dry-aged ribeye, intense beefy flavour", "PM-BS-005", cat("Beef Steaks"), brand("Heritage Reserve"), 54.99m, 30m, 12, 3, true, true, 0.4m),

            // Beef Roasting Joints (4)
            P("Beef Sirloin Roast", "1.2kg boneless sirloin roast, ideal for Sunday lunch", "PM-BR-001", cat("Beef Roasting Joints"), brand("Primo Cuts"), 42.99m, 24m, 20, 4, true, true, 1.2m),
            P("Standing Rib Roast", "2kg bone-in rib roast for special occasions", "PM-BR-002", cat("Beef Roasting Joints"), brand("Heritage Reserve"), 89.99m, 50m, 10, 2, true, true, 2.0m),
            P("Beef Brisket", "1.5kg whole brisket for low and slow cooking", "PM-BR-003", cat("Beef Roasting Joints"), brand("Primo Cuts"), 28.99m, 14m, 25, 5, true, false, 1.5m),
            P("Beef Fillet Roast", "1kg whole eye fillet, trimmed and tied", "PM-BR-004", cat("Beef Roasting Joints"), brand("Heritage Reserve"), 69.99m, 40m, 12, 3, true, true, 1.0m),

            // Beef Other Cuts (4)
            P("Premium Beef Mince", "500g lean premium mince, less than 10% fat", "PM-BO-001", cat("Beef Other Cuts"), brand("Primo Cuts"), 12.99m, 6m, 80, 15, true, false, 0.5m),
            P("Beef Diced Casserole", "500g diced chuck steak for slow cooking", "PM-BO-002", cat("Beef Other Cuts"), brand("Primo Cuts"), 14.99m, 7m, 60, 10, true, false, 0.5m),
            P("Beef Stir-Fry Strips", "400g thinly sliced rump strips", "PM-BO-003", cat("Beef Other Cuts"), brand("Primo Cuts"), 15.99m, 8m, 50, 10, true, false, 0.4m),
            P("Osso Buco (Beef Shin)", "1kg cross-cut beef shin on the bone", "PM-BO-004", cat("Beef Other Cuts"), brand("Primo Cuts"), 22.99m, 11m, 25, 5, true, false, 1.0m),

            // Lamb Chops & Cutlets (4)
            P("Lamb Cutlets (French Trimmed)", "4-pack frenched lamb cutlets", "PM-LC-001", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 28.99m, 15m, 30, 5, true, true, 0.4m),
            P("Lamb Loin Chops", "4-pack thick-cut loin chops", "PM-LC-002", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 22.99m, 12m, 40, 8, true, false, 0.5m),
            P("Lamb Rack (8 Point)", "Full 8-point rack, frenched and cap on", "PM-LC-003", cat("Lamb Chops & Cutlets"), brand("Valley Fresh"), 42.99m, 24m, 15, 3, true, true, 0.6m),
            P("Marinated Lamb Chops", "6-pack herb and garlic marinated chops", "PM-LC-004", cat("Lamb Chops & Cutlets"), brand("Grill Master"), 24.99m, 13m, 35, 5, true, false, 0.6m),

            // Lamb Roasting Joints (3)
            P("Lamb Leg (Bone In)", "2kg whole leg of lamb, perfect for roasting", "PM-LR-001", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 48.99m, 26m, 15, 3, true, true, 2.0m),
            P("Lamb Shoulder (Boneless Rolled)", "1.5kg boneless rolled shoulder", "PM-LR-002", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 36.99m, 20m, 18, 4, true, false, 1.5m),
            P("Lamb Easy Carve Leg", "1.8kg butterflied and deboned leg", "PM-LR-003", cat("Lamb Roasting Joints"), brand("Valley Fresh"), 52.99m, 28m, 12, 3, true, false, 1.8m),

            // Pork Steaks & Chops (3)
            P("Pork Loin Chops", "4-pack bone-in loin chops, 2cm thick", "PM-PS-001", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 16.99m, 8m, 50, 10, true, false, 0.6m),
            P("Pork Scotch Fillet Steak", "2-pack pork scotch fillet steaks", "PM-PS-002", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 14.99m, 7m, 45, 8, true, false, 0.4m),
            P("Pork Cutlets (Crumbed)", "4-pack hand-crumbed pork cutlets", "PM-PS-003", cat("Pork Steaks & Chops"), brand("Primo Cuts"), 18.99m, 9m, 30, 5, true, false, 0.5m),

            // Pork Roasting Joints (3)
            P("Pork Belly (Scored)", "1.5kg scored pork belly with crackling", "PM-PR-001", cat("Pork Roasting Joints"), brand("Primo Cuts"), 24.99m, 12m, 20, 4, true, true, 1.5m),
            P("Pork Rack (4 Cutlet)", "4-cutlet frenched pork rack", "PM-PR-002", cat("Pork Roasting Joints"), brand("Primo Cuts"), 32.99m, 18m, 15, 3, true, false, 0.8m),
            P("Pork Shoulder (Bone In)", "2.5kg whole pork shoulder for pulling", "PM-PR-003", cat("Pork Roasting Joints"), brand("Primo Cuts"), 29.99m, 14m, 18, 4, true, false, 2.5m),

            // Chicken Breast & Thigh (4)
            P("Chicken Breast Fillets", "1kg skinless chicken breast fillets (3-4 pieces)", "PM-CB-001", cat("Chicken Breast & Thigh"), brand("Valley Fresh"), 14.99m, 7m, 80, 15, true, false, 1.0m),
            P("Chicken Thigh Fillets", "1kg boneless skinless thigh fillets", "PM-CB-002", cat("Chicken Breast & Thigh"), brand("Valley Fresh"), 12.99m, 6m, 70, 12, true, false, 1.0m),
            P("Chicken Tenderloins", "500g tender inner breast fillets", "PM-CB-003", cat("Chicken Breast & Thigh"), brand("Valley Fresh"), 11.99m, 5m, 60, 10, true, false, 0.5m),
            P("Chicken Skewers (Marinated)", "8-pack satay marinated chicken skewers", "PM-CB-004", cat("Chicken Breast & Thigh"), brand("Grill Master"), 16.99m, 8m, 40, 8, true, true, 0.6m),

            // Whole Birds (3)
            P("Whole Free-Range Chicken", "1.8kg whole free-range chicken", "PM-WB-001", cat("Whole Birds"), brand("Valley Fresh"), 16.99m, 8m, 25, 5, true, false, 1.8m),
            P("Butterflied Chicken", "1.5kg spatchcocked and flattened chicken", "PM-WB-002", cat("Whole Birds"), brand("Valley Fresh"), 18.99m, 9m, 20, 4, true, false, 1.5m),
            P("Duck (Whole)", "2.2kg whole Pekin duck", "PM-WB-003", cat("Whole Birds"), brand("Valley Fresh"), 32.99m, 18m, 10, 2, true, true, 2.2m),

            // Veal (3)
            P("Veal Scallopini", "400g thinly sliced veal leg steaks", "PM-VL-001", cat("Veal"), brand("Primo Cuts"), 26.99m, 14m, 25, 5, true, false, 0.4m),
            P("Veal Osso Buco", "1kg cross-cut veal shank, 3cm thick", "PM-VL-002", cat("Veal"), brand("Primo Cuts"), 34.99m, 18m, 20, 4, true, true, 1.0m),
            P("Veal Cutlets", "4-pack bone-in veal rib cutlets", "PM-VL-003", cat("Veal"), brand("Primo Cuts"), 38.99m, 22m, 15, 3, true, false, 0.5m),

            // Sausages & Burgers (5)
            P("Beef BBQ Sausages", "6-pack classic thick beef sausages", "PM-SB-001", cat("Sausages & Burgers"), brand("Grill Master"), 12.99m, 5m, 80, 15, true, false, 0.6m),
            P("Pork & Fennel Sausages", "6-pack Italian-style pork and fennel", "PM-SB-002", cat("Sausages & Burgers"), brand("Grill Master"), 14.99m, 6m, 60, 10, true, true, 0.6m),
            P("Lamb & Rosemary Sausages", "6-pack lamb sausages with rosemary", "PM-SB-003", cat("Sausages & Burgers"), brand("Grill Master"), 15.99m, 7m, 50, 10, true, false, 0.6m),
            P("Gourmet Beef Burgers", "4-pack 150g premium beef burger patties", "PM-SB-004", cat("Sausages & Burgers"), brand("Grill Master"), 16.99m, 8m, 60, 10, true, true, 0.6m),
            P("Chorizo Sausages", "4-pack spicy Spanish-style chorizo", "PM-SB-005", cat("Sausages & Burgers"), brand("Grill Master"), 13.99m, 6m, 40, 8, true, false, 0.4m),

            // Deli & Cold Cuts (4)
            P("Prosciutto (Sliced)", "150g thinly sliced aged prosciutto", "PM-DL-001", cat("Deli & Cold Cuts"), brand("Artisan Kitchen"), 14.99m, 7m, 40, 8, true, true, 0.15m),
            P("Salami Milano", "200g traditional Milano-style salami", "PM-DL-002", cat("Deli & Cold Cuts"), brand("Artisan Kitchen"), 12.99m, 6m, 35, 8, true, false, 0.2m),
            P("Antipasto Platter", "500g mixed cured meats, olives and pickled vegetables", "PM-DL-003", cat("Deli & Cold Cuts"), brand("Artisan Kitchen"), 29.99m, 14m, 20, 4, true, true, 0.5m),
            P("Leg Ham (Sliced)", "200g honey-glazed leg ham slices", "PM-DL-004", cat("Deli & Cold Cuts"), brand("Artisan Kitchen"), 9.99m, 4m, 50, 10, true, false, 0.2m),

            // Italian Classics (3)
            P("Beef Lasagne", "1kg family-size beef and bechamel lasagne", "PM-RM-001", cat("Italian Classics"), brand("Artisan Kitchen"), 24.99m, 12m, 20, 4, true, true, 1.0m),
            P("Lamb Arancini (6 pack)", "6 golden-crumbed risotto balls with lamb ragu", "PM-RM-002", cat("Italian Classics"), brand("Artisan Kitchen"), 18.99m, 8m, 25, 5, true, false, 0.45m),
            P("Veal Involtini", "4-pack veal rolls stuffed with prosciutto and sage", "PM-RM-003", cat("Italian Classics"), brand("Artisan Kitchen"), 28.99m, 14m, 15, 3, true, true, 0.5m),

            // Curries & Stews (2)
            P("Slow-Cooked Beef Ragu", "750g rich beef ragu with red wine and herbs", "PM-CS-001", cat("Curries & Stews"), brand("Artisan Kitchen"), 22.99m, 10m, 20, 4, true, false, 0.75m),
            P("Lamb Massaman Curry", "700g tender lamb in massaman curry sauce", "PM-CS-002", cat("Curries & Stews"), brand("Artisan Kitchen"), 24.99m, 12m, 18, 4, true, false, 0.7m),

            // Pantry (3)
            P("Bronze-Cut Penne Rigate", "500g Italian dried bronze-cut penne", "PM-PT-001", cat("Pantry"), brand("Rustic Pantry"), 6.99m, 2.5m, 100, 20, true, false, 0.5m),
            P("Sugo di Pomodoro", "500ml slow-cooked tomato pasta sauce", "PM-PT-002", cat("Pantry"), brand("Rustic Pantry"), 8.99m, 3m, 80, 15, true, false, 0.5m),
            P("Extra Virgin Olive Oil", "500ml cold-pressed Sicilian olive oil", "PM-PT-003", cat("Pantry"), brand("Rustic Pantry"), 18.99m, 8m, 50, 10, true, true, 0.5m),

            // Marinades & Rubs (3)
            P("Chimichurri Marinade", "250ml fresh herb and garlic chimichurri", "PM-MR-001", cat("Marinades & Rubs"), brand("Grill Master"), 9.99m, 3m, 60, 10, true, false, 0.25m),
            P("Smoky BBQ Rub", "150g house-blend smoky barbecue spice rub", "PM-MR-002", cat("Marinades & Rubs"), brand("Grill Master"), 7.99m, 2m, 80, 15, true, true, 0.15m),
            P("Lemon & Herb Marinade", "250ml zesty lemon and herb marinade for poultry", "PM-MR-003", cat("Marinades & Rubs"), brand("Grill Master"), 8.99m, 3m, 60, 10, true, false, 0.25m),
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Add variants for Beef BBQ Sausages (flavour packs)
        var sausages = products.First(p => p.SKU == "PM-SB-001");
        var sausageFlavours = new[] { "Original", "Pepper & Onion", "Smoky BBQ" };
        foreach (var flavour in sausageFlavours)
            context.ProductVariants.Add(new ProductVariant { ProductId = sausages.Id, VariantName = flavour, SKU = $"PM-SB-001-{flavour[..3].ToUpper()}", Price = 12.99m, StockQuantity = 25, Attributes = $"{{\"flavour\":\"{flavour}\"}}", CreatedBy = "system" });

        // Add variants for Gourmet Beef Burgers (size)
        var burgers = products.First(p => p.SKU == "PM-SB-004");
        context.ProductVariants.Add(new ProductVariant { ProductId = burgers.Id, VariantName = "4-pack", SKU = "PM-SB-004-4PK", Price = 16.99m, StockQuantity = 40, Attributes = "{\"size\":\"4-pack\"}", CreatedBy = "system" });
        context.ProductVariants.Add(new ProductVariant { ProductId = burgers.Id, VariantName = "8-pack", SKU = "PM-SB-004-8PK", Price = 29.99m, StockQuantity = 20, Attributes = "{\"size\":\"8-pack\"}", CreatedBy = "system" });

        await context.SaveChangesAsync();
    }

    private static Product P(string name, string desc, string sku, Category cat, Brand brand, decimal price, decimal cost, int stock, int min, bool active, bool featured, decimal weight)
        => new() { Name = name, Description = desc, SKU = sku, CategoryId = cat.Id, BrandId = brand.Id, BasePrice = price, CostPrice = cost, StockQuantity = stock, MinStockLevel = min, IsActive = active, IsFeatured = featured, Weight = weight, CreatedBy = "system" };

    private static async Task SeedProductImagesAsync(AppDbContext context)
    {
        if (await context.ProductImages.AnyAsync()) return;
        var products = await context.Products.ToListAsync();

        // Map SKU prefixes to Unsplash photo URLs (royalty-free)
        var imageMap = new Dictionary<string, string[]>
        {
            // Beef Steaks
            ["PM-BS-001"] = new[] { "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=800", "https://images.unsplash.com/photo-1558030006-450675393462?w=800" },
            ["PM-BS-002"] = new[] { "https://images.unsplash.com/photo-1588168333986-5078d3ae3976?w=800" },
            ["PM-BS-003"] = new[] { "https://images.unsplash.com/photo-1551028150-64b9f398f678?w=800" },
            ["PM-BS-004"] = new[] { "https://images.unsplash.com/photo-1603048297172-c92544798d5a?w=800" },
            ["PM-BS-005"] = new[] { "https://images.unsplash.com/photo-1615937722923-67f6deaf2cc9?w=800", "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?w=800" },
            // Beef Roasting
            ["PM-BR-001"] = new[] { "https://images.unsplash.com/photo-1588347818481-0e7b4e5f4e94?w=800" },
            ["PM-BR-002"] = new[] { "https://images.unsplash.com/photo-1544025162-d76694265947?w=800" },
            ["PM-BR-003"] = new[] { "https://images.unsplash.com/photo-1529694157872-4e0c0f3b238b?w=800" },
            ["PM-BR-004"] = new[] { "https://images.unsplash.com/photo-1560781290-7dc94c0f8f4f?w=800" },
            // Beef Other
            ["PM-BO-001"] = new[] { "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800" },
            ["PM-BO-002"] = new[] { "https://images.unsplash.com/photo-1551135049-8a33b5883817?w=800" },
            ["PM-BO-003"] = new[] { "https://images.unsplash.com/photo-1609167830220-7164aa7bf827?w=800" },
            ["PM-BO-004"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            // Lamb
            ["PM-LC-001"] = new[] { "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=800" },
            ["PM-LC-002"] = new[] { "https://images.unsplash.com/photo-1514516345957-556ca7d90a29?w=800" },
            ["PM-LC-003"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-LC-004"] = new[] { "https://images.unsplash.com/photo-1598515214211-89d3c73ae83b?w=800" },
            ["PM-LR-001"] = new[] { "https://images.unsplash.com/photo-1608877907149-a206d75ba011?w=800" },
            ["PM-LR-002"] = new[] { "https://images.unsplash.com/photo-1574484284002-952d92456975?w=800" },
            ["PM-LR-003"] = new[] { "https://images.unsplash.com/photo-1606728035253-49e8a23146de?w=800" },
            // Pork
            ["PM-PS-001"] = new[] { "https://images.unsplash.com/photo-1623174479650-562c9a8af8fa?w=800" },
            ["PM-PS-002"] = new[] { "https://images.unsplash.com/photo-1606568218095-54b5f6b6e1a8?w=800" },
            ["PM-PS-003"] = new[] { "https://images.unsplash.com/photo-1610540881590-e9f5e1d11d63?w=800" },
            ["PM-PR-001"] = new[] { "https://images.unsplash.com/photo-1592686092538-a77869c15422?w=800", "https://images.unsplash.com/photo-1625938393824-e9ace9a7c8ff?w=800" },
            ["PM-PR-002"] = new[] { "https://images.unsplash.com/photo-1590779033100-9f60a05a013d?w=800" },
            ["PM-PR-003"] = new[] { "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800" },
            // Chicken
            ["PM-CB-001"] = new[] { "https://images.unsplash.com/photo-1604503468506-a8da13d82f2b?w=800" },
            ["PM-CB-002"] = new[] { "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800" },
            ["PM-CB-003"] = new[] { "https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=800" },
            ["PM-CB-004"] = new[] { "https://images.unsplash.com/photo-1599921841143-819065a55cc6?w=800" },
            ["PM-WB-001"] = new[] { "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=800" },
            ["PM-WB-002"] = new[] { "https://images.unsplash.com/photo-1501200291289-c5a76c232e5f?w=800" },
            ["PM-WB-003"] = new[] { "https://images.unsplash.com/photo-1574653853027-5382a3d23a15?w=800" },
            // Veal
            ["PM-VL-001"] = new[] { "https://images.unsplash.com/photo-1607116667981-68bd72c3a0ef?w=800" },
            ["PM-VL-002"] = new[] { "https://images.unsplash.com/photo-1612487439139-c2dea1a345c7?w=800" },
            ["PM-VL-003"] = new[] { "https://images.unsplash.com/photo-1619221882266-14ef84780a0e?w=800" },
            // Sausages & Burgers
            ["PM-SB-001"] = new[] { "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800", "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800" },
            ["PM-SB-002"] = new[] { "https://images.unsplash.com/photo-1627309302198-09a50ae3d566?w=800" },
            ["PM-SB-003"] = new[] { "https://images.unsplash.com/photo-1587536849024-daaa4a417b16?w=800" },
            ["PM-SB-004"] = new[] { "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=800", "https://images.unsplash.com/photo-1550547660-d9450f859349?w=800" },
            ["PM-SB-005"] = new[] { "https://images.unsplash.com/photo-1606851091519-5d0b4e4a0069?w=800" },
            // Deli
            ["PM-DL-001"] = new[] { "https://images.unsplash.com/photo-1626200419199-391ae4be7a41?w=800" },
            ["PM-DL-002"] = new[] { "https://images.unsplash.com/photo-1541014741259-de529411b96a?w=800" },
            ["PM-DL-003"] = new[] { "https://images.unsplash.com/photo-1432139555190-58524dae6a55?w=800" },
            ["PM-DL-004"] = new[] { "https://images.unsplash.com/photo-1524438418049-ab2acb7aa48f?w=800" },
            // Ready Meals
            ["PM-RM-001"] = new[] { "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=800" },
            ["PM-RM-002"] = new[] { "https://images.unsplash.com/photo-1595295333158-4742f28fbd85?w=800" },
            ["PM-RM-003"] = new[] { "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=800" },
            ["PM-CS-001"] = new[] { "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=800" },
            ["PM-CS-002"] = new[] { "https://images.unsplash.com/photo-1455619452474-d2be8b1e70cd?w=800" },
            // Pantry
            ["PM-PT-001"] = new[] { "https://images.unsplash.com/photo-1551462147-37885acc36f1?w=800" },
            ["PM-PT-002"] = new[] { "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?w=800" },
            ["PM-PT-003"] = new[] { "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800" },
            // Marinades
            ["PM-MR-001"] = new[] { "https://images.unsplash.com/photo-1628557044797-f21a177c37ec?w=800" },
            ["PM-MR-002"] = new[] { "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=800" },
            ["PM-MR-003"] = new[] { "https://images.unsplash.com/photo-1621955964441-c173e01c6668?w=800" },
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
        var bbqProducts = products.Where(p => p.SKU.StartsWith("PM-SB") || p.SKU.StartsWith("PM-MR")).ToList();
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
}
