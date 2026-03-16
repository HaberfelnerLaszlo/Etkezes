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
        public async Task<List<LoginUser>?> SyncLoginUserToServer(List<LoginUser> loginUsers)
        {
            try
            {
                List<LoginUser> loginUsersToUpdate = new List<LoginUser>();
                foreach (var loginUser in loginUsers)
                {
                    var existingLoginUser = await context.LoginUsers.FindAsync(loginUser.Id);
                    if (existingLoginUser == null)
                    {
                        loginUsersToUpdate.Add( context.LoginUsers.Add(loginUser).Entity);
                    }
                    else
                    {
                        context.Entry(existingLoginUser).CurrentValues.SetValues(loginUser);
                        loginUsersToUpdate.Add(existingLoginUser);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "LoginUser",
                    Type = SyncType.Up,
                    IsSuccess = true,
                    SyncDate = DateTime.UtcNow,
                    Description = $"{loginUsersToUpdate.Count} LoginUser rekord szinkronizálva."
                });
                await context.SaveChangesAsync();
                return loginUsersToUpdate;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed   
                ErrorMessage = ex.Message;
                context.SyncDatas.Add(new SyncData
                {
                    Table = "LoginUser",
                    Type = SyncType.Up,
                    IsSuccess = false,
                    SyncDate = DateTime.UtcNow,
                    Description = $"Hiba történt a szinkronizálás során: {ex.Message}"
                });
                await context.SaveChangesAsync();
                return null;
            }
        }
         public async Task<List<User>?> SyncUserToServer(List<User> users)
        {
            try
            {
                List<User> usersToUpdate = new List<User>();
                foreach (var user in users)
                {
                    var existingUser = await context.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                        user.Uploaded = DateTime.UtcNow;
                        usersToUpdate.Add(context.Users.Add(user).Entity);
                    }
                    else
                    {
                        user.Uploaded = DateTime.UtcNow;
                        context.Entry(existingUser).CurrentValues.SetValues(user);
                        usersToUpdate.Add(existingUser);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "User",
                    Type = SyncType.Up,
                    IsSuccess = true,
                    SyncDate = DateTime.UtcNow,
                    Description = $"{usersToUpdate.Count} User rekord szinkronizálva."
                });
                await context.SaveChangesAsync();
                return usersToUpdate;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                ErrorMessage = ex.Message;
                context.SyncDatas.Add(new SyncData
                {
                    Table = "User",
                    Type = SyncType.Up,
                    IsSuccess = false,
                    SyncDate = DateTime.UtcNow,
                    Description = $"Hiba történt a szinkronizálás során: {ex.Message}"
                });
                return null;
            }
        }
        public async Task<List<Etkezes>?> SyncEtkezesToServer(List<Etkezes> etkezesek)
        {
            try
            {
                List<Etkezes> etkezesekToUpdate = new List<Etkezes>();
                foreach (var etkezes in etkezesek)
                {
                    var existingEtkezes = await context.Etkezesek.FindAsync(etkezes.Id);
                    if (existingEtkezes == null)
                    {
                        etkezesekToUpdate.Add(context.Etkezesek.Add(etkezes).Entity);
                    }
                    else
                    {
                        context.Entry(existingEtkezes).CurrentValues.SetValues(etkezes);
                        etkezesekToUpdate.Add(existingEtkezes);
                    }
                }
                await context.SaveChangesAsync();
                context.SyncDatas.Add(new SyncData
                {
                    Table = "Etkezes",
                    Type = SyncType.Up,
                    IsSuccess = true,
                    SyncDate = DateTime.UtcNow,
                    Description = $"{etkezesekToUpdate.Count} Etkezes rekord szinkronizálva."
                });
                await context.SaveChangesAsync();
                return etkezesekToUpdate;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                ErrorMessage = ex.Message;
                context.SyncDatas.Add(new SyncData
                {
                    Table = "Etkezes",
                    Type = SyncType.Up,
                    IsSuccess = false,
                    SyncDate = DateTime.UtcNow,
                    Description = $"Hiba történt a szinkronizálás során: {ex.Message}"
                });
                return null;
            }
        }
        public async Task<SyncDatesView> GetLastSyncDates()
        {
            DateTime lastLoginUserSyncDateUp = DateTime.MinValue;
            DateTime lastUserSyncDateUp = DateTime.MinValue;
            DateTime lastEtkezesSyncDateUp = DateTime.MinValue;
            bool lastLoginUserSyncDown = false;
            bool lastUserSyncDown = false;
            bool lastEtkezesSyncDown = false;
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Up && sd.Table == "LoginUser" && sd.IsSuccess))
            {
                lastLoginUserSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "LoginUser" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            }
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Up && sd.Table == "User" && sd.IsSuccess))
            {
                lastUserSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "User" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            }
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Up && sd.Table == "Etkezes" && sd.IsSuccess))
            {
                lastEtkezesSyncDateUp = await context.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "Etkezes" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
            }
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Down && sd.Table == "LoginUser" && sd.IsSuccess))
            {
                var lastLoginUserSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "LoginUser" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
                lastLoginUserSyncDown = await context.LoginUsers.AnyAsync(lu=>lu.UpdatedAt>=lastLoginUserSyncDateDown);
            }
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Down && sd.Table == "User" && sd.IsSuccess))
            {
                var lastUserSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "User" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
                lastUserSyncDown = await context.Users.AnyAsync(u => u.Updated >= lastUserSyncDateDown);
            }
            if (await context.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Down && sd.Table == "Etkezes" && sd.IsSuccess))
            {
                var lastEtkezesSyncDateDown = await context.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "Etkezes" && sd.IsSuccess).MaxAsync(sd => sd.SyncDate);
                lastEtkezesSyncDown = await context.Etkezesek.AnyAsync(e => e.Updated >= lastEtkezesSyncDateDown);
            }
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
