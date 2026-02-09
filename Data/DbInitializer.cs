using TodoApi.Models;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using BCrypt.Net; // Tilføj denne!

namespace TodoApi.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context, IConfiguration configuration)
        {
            context.Database.EnsureCreated();

            // 1. Tjek om admin-brugeren findes (vi tjekker på navnet fra config)
            var seedName = configuration["SEED_USER_NAME"] ?? "Far";
            if (context.Users.Any(u => u.Username == seedName)) return;

            // 2. Hent password fra Render/Config
            var seedPass = configuration["SEED_USER_PASS"] ?? "1234";

            // 3. HASH PASSWORDET HER
            // Dette forvandler "1234" til noget i stil med "$2a$11$iv3S..."
            string hashedContext = BCrypt.Net.BCrypt.HashPassword(seedPass);

            var adminUser = new User
            {
                Username = seedName,
                PasswordHash = hashedContext, // Gem den hashede version!
                Role = "Admin",
                FamilyId = 1,
                FamilyName = "Bang",
                TotalPoints = 0,
                SavingsBalance = 0
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}