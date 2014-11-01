using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ApiFinal.Datasource
{
    public class SimpleDatasourceContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasKey(k => k.UserName);
        }
    }

    public class User
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string HashSalt { get; set; }
        public bool Active { get; set; }
        public string Roles { get; set; }
    }

    public enum ApplicationTypes
    {
        WebClient,
        NativeClient
    }

    public class Client
    {
        public string Id { get; set; }
        public string Secret { get; set; }
        public string Name { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }

        public string AllowedOrigin { get; set; }
    }

    public class RefreshToken
    {

        public string Id { get; set; }
        public string Subject { get; set; }
        public string ClientId { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string ProtectedTicket { get; set; }
    }

    public static class SeedHelper
    {
        public static void SeedDatabase(SimpleDatasourceContext context)
        {
            SeedUsers(context);
            SeedClients(context);
        }

        private static void SeedUsers(SimpleDatasourceContext context)
        {
            context.Users.AddOrUpdate(GenerateUser("Admin", "123abc", "admin"));
            context.Users.AddOrUpdate(GenerateUser("User01", "123abc", "RoleA;RoleD"));
            context.Users.AddOrUpdate(GenerateUser("User02", "123abc", "RoleB;RoleC"));
        }

        private static void SeedClients(SimpleDatasourceContext context)
        {
            context.Clients.AddOrUpdate(new Client
            {
                Id = "WebClient01",
                Secret = PasswordHelper.HashString("webclientsecret"),
                Name = "Web Client 01",
                ApplicationType = ApplicationTypes.WebClient,
                Active = true,
                RefreshTokenLifeTime = 3,
                AllowedOrigin = "*"
            });

            context.Clients.AddOrUpdate(new Client
            {
                Id = "NativeClient01",
                Secret = PasswordHelper.HashString("nativeclientsecret"),
                Name = "Web Client 01",
                ApplicationType = ApplicationTypes.NativeClient,
                Active = true,
                RefreshTokenLifeTime = 3,
                AllowedOrigin = "*"
            });
        }

        private static User GenerateUser(string userName, string password, string roles)
        {
            var salt = PasswordHelper.GenerateRandomSalt(16);

            return new User
            {
                UserName = userName,
                HashSalt = salt,
                PasswordHash = PasswordHelper.HashPassword(password, salt),
                Active = true,
                Roles = roles
            };
        }
    }

    public static class PasswordHelper
    {
        public static string GenerateRandomSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[size];

            rng.GetBytes(buffer);

            return Convert.ToBase64String(buffer);
        }

        public static string HashPassword(string password, string salt)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}", password, salt));
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public static string HashString(string secret)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(secret);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }

    public class SimpleIdentityManager
    {
        public async static Task<UserInfo> ValidateUserAsync(string userName, string password)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Active);
                if (user != null)
                {
                    var hashPassword = PasswordHelper.HashPassword(password, user.HashSalt);
                    if (hashPassword == user.PasswordHash)
                    {
                        return UserInfo.FromUser(user);
                    }
                }
            }

            return null;
        }

        public async static Task<UserInfo> FindUserAsync(string userName)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Active);
                if (user != null)
                {
                    return UserInfo.FromUser(user);
                }
            }

            return null;
        }

        public static async Task<Client> FindClientAsync(string clientId)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                return await context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
            }
        }

        public async static Task<bool> StoreRefreshToken(RefreshToken refreshToken)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                var oldToken =
                    context.RefreshTokens.SingleOrDefault(
                        rt => rt.Subject == refreshToken.Subject && rt.ClientId == refreshToken.ClientId);

                if (oldToken != null)
                {
                    context.RefreshTokens.Remove(oldToken);
                }

                context.RefreshTokens.Add(refreshToken);

                return await context.SaveChangesAsync() > 0;
            }
        }

        public async static Task<RefreshToken> FindRefreshToken(string id)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                return await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == id);
            }
        }

        public async static Task<bool> DeleteRefreshToken(string id)
        {
            using (SimpleDatasourceContext context = new SimpleDatasourceContext())
            {
                var refreshToken = await context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Id == id);

                if (refreshToken != null)
                {
                    context.RefreshTokens.Remove(refreshToken);
                    return await context.SaveChangesAsync() > 0;
                }
            }

            return false;
        }
    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public bool Active { get; set; }
        public IList<string> Roles { get; set; }

        public static UserInfo FromUser(User user)
        {
            return new UserInfo
            {
                UserName = user.UserName,
                Active = user.Active,
                Roles = user.Roles.Split(';').ToList()
            };
        }
    }

}