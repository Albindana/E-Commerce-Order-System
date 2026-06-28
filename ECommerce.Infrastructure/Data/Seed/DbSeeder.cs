using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data.Seed;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbSeeder(AppDbContext context, UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedCategoriesAndProductsAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.FindByEmailAsync("admin@shop.com") == null)
        {
            var admin = new AppUser
            {
                UserName = "admin@shop.com",
                Email = "admin@shop.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(admin, "Admin123!");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await _userManager.FindByEmailAsync("customer@shop.com") == null)
        {
            var customer = new AppUser
            {
                UserName = "customer@shop.com",
                Email = "customer@shop.com",
                FirstName = "Jane",
                LastName = "Doe",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(customer, "Customer123!");
            await _userManager.AddToRoleAsync(customer, "Customer");
        }
    }

    private async Task SeedCategoriesAndProductsAsync()
    {
        if (await _context.Categories.AnyAsync()) return;

        var categories = new[]
        {
            new Category { Name = "Electronics",   Description = "Gadgets and devices" },
            new Category { Name = "Clothing",      Description = "Apparel and fashion" },
            new Category { Name = "Books",         Description = "Books and literature" },
            new Category { Name = "Home & Garden", Description = "Home improvement and garden" },
            new Category { Name = "Sports",        Description = "Sports and outdoor equipment" }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        var electronics = categories[0];
        var clothing    = categories[1];
        var books       = categories[2];
        var home        = categories[3];
        var sports      = categories[4];

        var products = new[]
        {
            new Product { Name = "Wireless Headphones",    Description = "Noise-cancelling over-ear headphones",          Price = 149.99m, StockQuantity = 50,  CategoryId = electronics.Id },
            new Product { Name = "Mechanical Keyboard",    Description = "RGB backlit mechanical gaming keyboard",         Price = 89.99m,  StockQuantity = 30,  CategoryId = electronics.Id },
            new Product { Name = "USB-C Hub",              Description = "7-in-1 USB-C hub with HDMI and PD charging",     Price = 39.99m,  StockQuantity = 100, CategoryId = electronics.Id },
            new Product { Name = "Smartphone Stand",       Description = "Adjustable aluminium desk phone stand",          Price = 24.99m,  StockQuantity = 200, CategoryId = electronics.Id },

            new Product { Name = "Classic White T-Shirt",  Description = "100% cotton crew-neck tee",                     Price = 19.99m,  StockQuantity = 150, CategoryId = clothing.Id },
            new Product { Name = "Slim Fit Jeans",         Description = "Stretch denim slim fit trousers",                Price = 49.99m,  StockQuantity = 80,  CategoryId = clothing.Id },
            new Product { Name = "Running Jacket",         Description = "Lightweight water-resistant running jacket",     Price = 69.99m,  StockQuantity = 40,  CategoryId = clothing.Id },
            new Product { Name = "Leather Wallet",         Description = "Slim genuine leather bifold wallet",             Price = 34.99m,  StockQuantity = 60,  CategoryId = clothing.Id },

            new Product { Name = "Clean Code",             Description = "A handbook of agile software craftsmanship",     Price = 34.99m,  StockQuantity = 45,  CategoryId = books.Id },
            new Product { Name = "The Pragmatic Programmer", Description = "Your journey to mastery",                      Price = 39.99m,  StockQuantity = 35,  CategoryId = books.Id },
            new Product { Name = "Design Patterns",        Description = "Elements of reusable object-oriented software",  Price = 44.99m,  StockQuantity = 25,  CategoryId = books.Id },
            new Product { Name = "Domain-Driven Design",   Description = "Tackling complexity in the heart of software",   Price = 49.99m,  StockQuantity = 20,  CategoryId = books.Id },

            new Product { Name = "Indoor Plant Pot Set",   Description = "Set of 3 ceramic plant pots with drainage",     Price = 29.99m,  StockQuantity = 70,  CategoryId = home.Id },
            new Product { Name = "LED Desk Lamp",          Description = "Adjustable colour temperature desk lamp",        Price = 54.99m,  StockQuantity = 55,  CategoryId = home.Id },
            new Product { Name = "Kitchen Knife Set",      Description = "Professional 5-piece stainless steel knife set", Price = 79.99m,  StockQuantity = 30,  CategoryId = home.Id },
            new Product { Name = "Bamboo Cutting Board",   Description = "Large eco-friendly bamboo chopping board",       Price = 22.99m,  StockQuantity = 90,  CategoryId = home.Id },

            new Product { Name = "Yoga Mat",               Description = "Non-slip 6mm thick exercise yoga mat",           Price = 29.99m,  StockQuantity = 120, CategoryId = sports.Id },
            new Product { Name = "Resistance Bands Set",   Description = "Set of 5 latex resistance bands",               Price = 18.99m,  StockQuantity = 150, CategoryId = sports.Id },
            new Product { Name = "Water Bottle",           Description = "Insulated stainless steel 750ml bottle",         Price = 24.99m,  StockQuantity = 200, CategoryId = sports.Id },
            new Product { Name = "Running Shoes",          Description = "Lightweight breathable trail running shoes",     Price = 89.99m,  StockQuantity = 45,  CategoryId = sports.Id }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }
}
