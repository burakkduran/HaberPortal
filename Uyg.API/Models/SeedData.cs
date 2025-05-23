using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Uyg.API.Models
{
    public static class SeedData
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            try
            {
                // Rol yöneticisi
                var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
                // Kullanıcı yöneticisi
                var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
                var logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();

                logger.LogInformation("Seed işlemi başlatılıyor...");

                // Rolleri oluştur
                string[] roleNames = { "Admin", "Editor", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new AppRole { Name = roleName });
                        logger.LogInformation($"{roleName} rolü oluşturuldu.");
                    }
                }

                // Admin kullanıcısını oluştur
                var adminUser = await userManager.FindByEmailAsync("admin@haberportal.com");
                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = "admin@haberportal.com",
                        Email = "admin@haberportal.com",
                        EmailConfirmed = true,
                        FullName = "Admin User",
                        PhotoUrl = "default.jpg"
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin kullanıcısı oluşturuldu.");
                    }
                    else
                    {
                        logger.LogError($"Admin kullanıcısı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Editor kullanıcısını oluştur
                var editorUser = await userManager.FindByEmailAsync("editor@haberportal.com");
                if (editorUser == null)
                {
                    editorUser = new AppUser
                    {
                        UserName = "editor@haberportal.com",
                        Email = "editor@haberportal.com",
                        EmailConfirmed = true,
                        FullName = "Editor User",
                        PhotoUrl = "default.jpg"
                    };

                    var result = await userManager.CreateAsync(editorUser, "Editor123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(editorUser, "Editor");
                        logger.LogInformation("Editor kullanıcısı oluşturuldu.");
                    }
                    else
                    {
                        logger.LogError($"Editor kullanıcısı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Normal kullanıcı oluştur
                var normalUser = await userManager.FindByEmailAsync("user@haberportal.com");
                if (normalUser == null)
                {
                    normalUser = new AppUser
                    {
                        UserName = "user@haberportal.com",
                        Email = "user@haberportal.com",
                        EmailConfirmed = true,
                        FullName = "Normal User",
                        PhotoUrl = "default.jpg"
                    };

                    var result = await userManager.CreateAsync(normalUser, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(normalUser, "User");
                        logger.LogInformation("Normal kullanıcı oluşturuldu.");
                    }
                    else
                    {
                        logger.LogError($"Normal kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Örnek kategoriler, makaleler ve yorumlar oluştur
                await SeedCategoriesArticlesAndCommentsAsync(serviceProvider, adminUser, editorUser, normalUser);

                logger.LogInformation("Seed işlemi tamamlandı.");
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();
                logger.LogError(ex, "Seed işlemi sırasında bir hata oluştu.");
                throw;
            }
        }

        private static async Task SeedCategoriesArticlesAndCommentsAsync(IServiceProvider serviceProvider, AppUser adminUser, AppUser editorUser, AppUser normalUser)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();

            // Kategorileri oluştur
            var categories = new List<Category>
            {
                new Category { Name = "Teknoloji", IsActive = true },
                new Category { Name = "Spor", IsActive = true },
                new Category { Name = "Ekonomi", IsActive = true },
                new Category { Name = "Sağlık", IsActive = true }
            };

            foreach (var category in categories)
            {
                if (!context.Categories.Any(c => c.Name == category.Name))
                {
                    await context.Categories.AddAsync(category);
                    logger.LogInformation($"{category.Name} kategorisi oluşturuldu.");
                }
            }
            await context.SaveChangesAsync();

            // Makaleleri oluştur
            var technologyCategory = await context.Categories.FirstAsync(c => c.Name == "Teknoloji");
            var sportsCategory = await context.Categories.FirstAsync(c => c.Name == "Spor");
            var economyCategory = await context.Categories.FirstAsync(c => c.Name == "Ekonomi");
            var healthCategory = await context.Categories.FirstAsync(c => c.Name == "Sağlık");

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Yapay Zeka ve Geleceğimiz",
                    Content = "Yapay zeka teknolojileri hayatımızı nasıl değiştirecek? Detaylı bir analiz...",
                    Summary = "Yapay zeka teknolojilerinin geleceğe etkileri",
                    ImageUrl = "ai-future.jpg",
                    CategoryId = technologyCategory.Id,
                    UserId = adminUser.Id,
                    PublishDate = DateTime.Now,
                    IsPublished = true,
                    ViewCount = 0
                },
                new Article
                {
                    Title = "Süper Lig'de Son Durum",
                    Content = "Süper Lig'de son hafta maçlarının detaylı analizi ve puan durumu...",
                    Summary = "Süper Lig'de son hafta değerlendirmesi",
                    ImageUrl = "superlig.jpg",
                    CategoryId = sportsCategory.Id,
                    UserId = editorUser.Id,
                    PublishDate = DateTime.Now,
                    IsPublished = true,
                    ViewCount = 0
                },
                new Article
                {
                    Title = "Ekonomide Yeni Dönem",
                    Content = "Merkez Bankası'nın yeni faiz kararı ve piyasalara etkileri...",
                    Summary = "Merkez Bankası faiz kararı analizi",
                    ImageUrl = "economy.jpg",
                    CategoryId = economyCategory.Id,
                    UserId = editorUser.Id,
                    PublishDate = DateTime.Now,
                    IsPublished = true,
                    ViewCount = 0
                },
                new Article
                {
                    Title = "Sağlıklı Beslenme İpuçları",
                    Content = "Günlük hayatta sağlıklı beslenme için öneriler ve püf noktaları...",
                    Summary = "Sağlıklı beslenme rehberi",
                    ImageUrl = "health.jpg",
                    CategoryId = healthCategory.Id,
                    UserId = adminUser.Id,
                    PublishDate = DateTime.Now,
                    IsPublished = true,
                    ViewCount = 0
                }
            };

            foreach (var article in articles)
            {
                if (!context.Articles.Any(a => a.Title == article.Title))
                {
                    await context.Articles.AddAsync(article);
                    logger.LogInformation($"{article.Title} makalesi oluşturuldu.");
                }
            }
            await context.SaveChangesAsync();

            // Yorumları oluştur
            var aiArticle = await context.Articles.FirstAsync(a => a.Title == "Yapay Zeka ve Geleceğimiz");
            var superLigArticle = await context.Articles.FirstAsync(a => a.Title == "Süper Lig'de Son Durum");

            var comments = new List<Comment>
            {
                new Comment
                {
                    Content = "Çok bilgilendirici bir makale olmuş, teşekkürler.",
                    ArticleId = aiArticle.Id,
                    UserId = normalUser.Id
                },
                new Comment
                {
                    Content = "Yapay zeka konusunda daha detaylı bilgi verilebilirdi.",
                    ArticleId = aiArticle.Id,
                    UserId = editorUser.Id
                },
                new Comment
                {
                    Content = "Takımımızın performansı gerçekten etkileyici!",
                    ArticleId = superLigArticle.Id,
                    UserId = normalUser.Id
                },
                new Comment
                {
                    Content = "Haftaya çok önemli bir maç var, heyecan dorukta.",
                    ArticleId = superLigArticle.Id,
                    UserId = adminUser.Id
                }
            };

            foreach (var comment in comments)
            {
                if (!context.Comments.Any(c => c.Content == comment.Content && c.ArticleId == comment.ArticleId))
                {
                    await context.Comments.AddAsync(comment);
                    logger.LogInformation($"Yeni yorum eklendi: {comment.Content}");
                }
            }
            await context.SaveChangesAsync();
        }
    }
} 