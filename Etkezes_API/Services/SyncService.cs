using Etkezes_API.Data;

using Etkezes_Models;
using Etkezes_Models.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace Etkezes_API.Services
{
    public class SyncService(EtkezesDbContext context)
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public async Task<List<LoginUser>?> SyncLoginUser(DateTime syncDate)
           {
                var loginUsers = await context.LoginUsers.Where(lu => lu.UpdatedAt > syncDate).AsNoTracking().ToListAsync();
            if (loginUsers == null || loginUsers.Count == 0)
            {
                return null;
            }
            context.SyncDatas.Add(new SyncData
            {
                Table = "LoginUser",
                Type = SyncType.Down,
                SyncDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return loginUsers;
        }
        public async Task<List<User>?> SyncUser(DateTime syncDate)
        {
            var users = await context.Users.Where(u => u.Updated > syncDate).AsNoTracking().ToListAsync();
            if (users == null || users.Count == 0)
            {
                ErrorMessage = "Nincsenek új adatok a szinkronizáláshoz.";
                return null;
            }
            context.SyncDatas.Add(new SyncData
            {
                Table = "User",
                Type = SyncType.Down,
                SyncDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return users;
        }
        public async Task<List<Etkezes>?> SyncEtkezes(DateTime syncDate)
        {
            var etkezesek = await context.Etkezesek.Where(e => e.Updated > syncDate).AsNoTracking().ToListAsync();
            if (etkezesek == null || etkezesek.Count == 0)
            {
                ErrorMessage = "Nincsenek új adatok a szinkronizáláshoz.";
                return null;
            }
            context.SyncDatas.Add(new SyncData
            {
                Table = "Etkezes",
                Type = SyncType.Down,
                SyncDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return etkezesek;
        }
        public async Task<bool> SyncLoginUserToServer(List<LoginUser> loginUsers)
        {
            try
            {
                foreach (var loginUser in loginUsers)
                {
                    var existingLoginUser = await context.LoginUsers.FindAsync(loginUser.Id);
                    if (existingLoginUser == null)
                    {
                        context.LoginUsers.Add(loginUser);
                    }
                    else
                    {
                        context.Entry(existingLoginUser).CurrentValues.SetValues(loginUser);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "LoginUser",
                    Type = SyncType.Up,
                    SyncDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed   
                ErrorMessage = ex.Message;
                return false;
            }
        }
         public async Task<bool> SyncUserToServer(List<User> users)
        {
            try
            {
                foreach (var user in users)
                {
                    var existingUser = await context.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                        context.Users.Add(user);
                    }
                    else
                    {
                        context.Entry(existingUser).CurrentValues.SetValues(user);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "User",
                    Type = SyncType.Up,
                    SyncDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public async Task<bool> SyncEtkezesToServer(List<Etkezes> etkezesek)
        {
            try
            {
                foreach (var etkezes in etkezesek)
                {
                    var existingEtkezes = await context.Etkezesek.FindAsync(etkezes.Id);
                    if (existingEtkezes == null)
                    {
                        context.Etkezesek.Add(etkezes);
                    }
                    else
                    {
                        context.Entry(existingEtkezes).CurrentValues.SetValues(etkezes);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "Etkezes",
                    Type = SyncType.Up,
                    SyncDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public async Task<SyncDatesView> GetLastSyncDates()
        {
            var lastLoginUserSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "LoginUser" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);

            var lastUserSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "User" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            var lastEtkezesSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "Etkezes" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            var lastLoginUserSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "User" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            var lastLoginUserSyncDown = await context.LoginUsers.AnyAsync(lu=>lu.UpdatedAt>lastLoginUserSyncDateDown);
            var lastUserSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "User"&& sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            var lastUserSyncDown = await context.Users.AnyAsync(u => u.Updated > lastUserSyncDateDown);
            var lastEtkezesSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "Etkezes" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            var lastEtkezesSyncDown = await context.Etkezesek.AnyAsync(e => e.Updated > lastEtkezesSyncDateDown);
            return new SyncDatesView
            {
                LoginUserSyncDateUp = lastLoginUserSyncDateUp,
                UserSyncDateUp = lastUserSyncDateUp,
                EtkezesSyncDateUp = lastEtkezesSyncDateUp,
                LoginUserSyncDateDown = lastLoginUserSyncDown,
                UserSyncDateDown = lastUserSyncDown,
                EtkezesSyncDateDown = lastEtkezesSyncDown
            };
        }
    }
}
