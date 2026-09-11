using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync())
            return;

        var categories = new[]
        {
            new Category { Name = "إلكترونيات", Slug = "electronics" },
            new Category { Name = "أزياء", Slug = "fashion" },
            new Category { Name = "منزل ومكتب", Slug = "home-office" },
            new Category { Name = "إكسسوارات", Slug = "accessories" }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var products = new[]
        {
            Product("سماعات عزل الضوضاء Pro", "صوت نقي وبطارية تدوم حتى 30 ساعة مع تصميم مريح للاستخدام اليومي.", 129.99m, 42, "SoundMax", categories[0], 4.8m, "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=900"),
            Product("ساعة ذكية Active X", "تابع نشاطك وصحتك وإشعارات هاتفك بشاشة عالية الوضوح ومقاومة للماء.", 89.50m, 28, "Pulse", categories[0], 4.6m, "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=900"),
            Product("حقيبة ظهر Urban اليومية", "حقيبة عملية مقاومة للماء، مناسبة للعمل والجامعة مع مساحة للابتوب.", 54.00m, 65, "Northline", categories[1], 4.7m, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=900"),
            Product("حذاء رياضي خفيف", "راحة وحركة طوال اليوم بنعل مرن وتصميم عصري يناسب كل إطلالة.", 74.99m, 34, "Stride", categories[1], 4.5m, "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=900"),
            Product("مصباح مكتب ذكي", "إضاءة قابلة للتعديل مع شحن لاسلكي وتصميم أنيق لمساحة عملك.", 39.90m, 19, "Luma", categories[2], 4.4m, "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=900"),
            Product("كرسي مكتب مريح", "دعم قطني ومسند رأس وارتفاع قابل للتعديل لجلسات العمل الطويلة.", 179.00m, 12, "Ergo", categories[2], 4.9m, "https://images.unsplash.com/photo-1505843490701-5be5d9f7d0d5?w=900"),
            Product("نظارة شمسية كلاسيكية", "عدسات UV400 وإطار خفيف بتصميم خالد يناسب مختلف الوجوه.", 32.00m, 51, "Vista", categories[3], 4.3m, "https://images.unsplash.com/photo-1511499767150-a48a237f0083?w=900"),
            Product("زجاجة حرارية ذكية", "تحافظ على حرارة مشروبك لساعات، مصنوعة من الستانلس ستيل الآمن.", 24.95m, 73, "Thermo", categories[3], 4.6m, "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=900")
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private static Product Product(string name, string description, decimal price, int stock, string brand,
        Category category, decimal rating, string imageUrl)
    {
        var product = new Product
        {
            Name = name, Description = description, Price = price, Stock = stock, Brand = brand,
            Category = category, AverageRating = rating, ReviewCount = Random.Shared.Next(8, 96),
            LowStockAlert = stock <= 5,
            Tags = new List<ProductTag> { new() { Tag = brand.ToLowerInvariant() }, new() { Tag = "featured" } },
            Images = new List<ProductImage> { new() { Url = imageUrl, IsMain = true } }
        };
        return product;
    }
}
