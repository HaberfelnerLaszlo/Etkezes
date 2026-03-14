using Etkezes_Ellenor.Data;
using Etkezes_Models;
using Etkezes_Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace Etkezes_Ellenor.Services
{
    public class SyncService(ApiHelper apiHelper, EtkezesDBcontext dbContext, ILogger<SyncService> logger)
    {
        public string ErrorMessage { get; private set; } = string.Empty;
        public async Task<bool> GetSyncDates()
        {
            try
            {
                Console.WriteLine("Fetching sync dates from server...");
                var result = await apiHelper.Get<SyncDatesView>("/sync/dates");

                if (result == null) { return false; }

                return await SyncCoordinator(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching sync dates.");
                ErrorMessage = "Failed to fetch sync dates.";
                return false;
            }
        }
        internal async Task<bool> SyncCoordinator(SyncDatesView syncDates)
        {
            Console.WriteLine("Starting synchronization process...");
            DateTime loginUserSyncDateUp = DateTime.MinValue,
                lastUserSyncDateUp = DateTime.MinValue,
                lastLoginUserSyncDateDown = DateTime.MinValue,
                lastUserSyncDateDown = DateTime.MinValue;
            if (await dbContext.LoginUsers.AnyAsync())
            {
                loginUserSyncDateUp = await dbContext.LoginUsers.MaxAsync(sd => sd.UpdatedAt);
            }
            if(await dbContext.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Up && sd.Table == "User"))
            {
                lastUserSyncDateUp = await dbContext.SyncDatas.Where(sd => sd.Type == SyncType.Up && sd.Table == "User").MaxAsync(sd => sd.SyncDate);
            }
            if(await dbContext.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Down && sd.Table == "LoginUser"))
            {
                lastLoginUserSyncDateDown = await dbContext.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "LoginUser").MaxAsync(sd => sd.SyncDate);
            }
            if(await dbContext.SyncDatas.AnyAsync(sd => sd.Type == SyncType.Down && sd.Table == "User"))
            {
                lastUserSyncDateDown = await dbContext.SyncDatas.Where(sd => sd.Type == SyncType.Down && sd.Table == "User").MaxAsync(sd => sd.SyncDate);
            }
            bool IsSyncSuccessful = false;
            #region down
            try
            {
                if (syncDates.LoginUserSyncDateDown)
                {
                    var loginUsersToUpdate = await apiHelper.Get<List<LoginUser>>($"/sync/loginusers/{lastLoginUserSyncDateDown:O}");
                    if (loginUsersToUpdate != null)
                    {
                        dbContext.LoginUsers.UpdateRange(loginUsersToUpdate);
                        await dbContext.SaveChangesAsync();
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "LoginUser",
                            Type = SyncType.Down,
                            SyncDate = DateTime.UtcNow
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = true;
                    }
                    else
                    {
                        ErrorMessage = "Nem jött a login user adat a szerverről.";
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "LoginUser",
                            IsSuccess = false,
                            Type = SyncType.Down,
                            SyncDate = DateTime.UtcNow,
                            Description = "Nem jött a login user adat a szerverről."
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = false;
                    }
                }
                if (syncDates.UserSyncDateDown)
                {
                    var usersToUpdate = await apiHelper.Get<List<User>>($"/sync/users/{lastUserSyncDateDown:O}");
                    if (usersToUpdate != null)
                    {
                        dbContext.Users.UpdateRange(usersToUpdate);
                        await dbContext.SaveChangesAsync();
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "User",
                            Type = SyncType.Down,
                            SyncDate = DateTime.UtcNow
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = true;
                    }
                    else
                    {
                        ErrorMessage = "Nem jött a user adat a szerverről.";
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "User",
                            IsSuccess = false,
                            Type = SyncType.Down,
                            SyncDate = DateTime.UtcNow,
                            Description = "Nem jött a user adat a szerverről."
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = false;
                    }
                }
                IsSyncSuccessful = await SyncEtkezokByNow();
                dbContext.SyncDatas.Add(new SyncData
                {
                    Table = "Etkezes",
                    IsSuccess = IsSyncSuccessful,
                    Type = SyncType.Down,
                    SyncDate = DateTime.UtcNow,
                    Description = IsSyncSuccessful ? null : ErrorMessage
                });
                await dbContext.SaveChangesAsync();
#endregion
                #region up
                if (syncDates.LoginUserSyncDateUp < loginUserSyncDateUp)
                {
                    var loginUsersToSync = await dbContext.LoginUsers.Where(lu => lu.UpdatedAt > loginUserSyncDateUp).AsNoTracking().ToListAsync();
                    if (await SyncLoginUserToServer(loginUsersToSync))
                    {
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "LoginUser",
                            Type = SyncType.Up,
                            IsSuccess = true,
                            SyncDate = DateTime.UtcNow
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = true;
                    }
                    else
                    {
                        dbContext.SyncDatas.Add(new SyncData
                        {
                            Table = "LoginUser",
                            Type = SyncType.Up,
                            IsSuccess = false,
                            SyncDate = DateTime.UtcNow,
                            Description = "Failed to sync login users to server."
                        });
                        await dbContext.SaveChangesAsync();
                        IsSyncSuccessful = false;
                    }
                    if (syncDates.UserSyncDateUp < lastUserSyncDateUp)
                    {
                        if (await SyncUserToServer(await dbContext.Users.Where(u => u.Updated > lastUserSyncDateUp).AsNoTracking().ToListAsync()))
                        {
                            dbContext.SyncDatas.Add(new SyncData
                            {
                                Table = "User",
                                Type = SyncType.Up,
                                IsSuccess = true,
                                SyncDate = DateTime.UtcNow
                            });
                            await dbContext.SaveChangesAsync();
                            IsSyncSuccessful = true;
                        }
                        else
                        {
                            dbContext.SyncDatas.Add(new SyncData
                            {
                                Table = "User",
                                Type = SyncType.Up,
                                IsSuccess = false,
                                SyncDate = DateTime.UtcNow,
                                Description = "Failed to sync users to server."
                            });
                            await dbContext.SaveChangesAsync();
                            IsSyncSuccessful = false;
                        }
                    }
                #endregion
                }
                Console.WriteLine("Synchronization process completed.");
                return IsSyncSuccessful;
           }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing login users from server.");
                ErrorMessage = "Failed to sync login users from server.";
                return false;
            }
            
        }
        private async Task<bool> SyncEtkezokByNow() 
        {
            try
            {
                var etkezesekToUpdate = await apiHelper.Get<List<EtkezokView>>($"/etkezesek/{DateTime.Today:O}");
                if (etkezesekToUpdate != null)
                {
                    foreach (var etkezes in etkezesekToUpdate)
                    {
                        var localEtkezes = await dbContext.Etkezesek.FindAsync(etkezes.UserId);
                        if (localEtkezes != null)
                        {
                            localEtkezes.Name = etkezes.Name;
                            localEtkezes.Menu = etkezes.Menu;
                            localEtkezes.Adag = etkezes.Adag;
                            localEtkezes.Darab = etkezes.Darab;
                            localEtkezes.Updated = DateTime.UtcNow;
                            dbContext.Etkezesek.Update(localEtkezes);
                        }
                        else
                        {
                            dbContext.Etkezesek.Add(new Etkezok
                            {
                                UserId = etkezes.UserId,
                                Name = etkezes.Name,
                                Menu = etkezes.Menu,
                                Adag = etkezes.Adag,
                                Darab = etkezes.Darab,
                                Updated = DateTime.UtcNow
                            });
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    return true;
                }
                ErrorMessage = "Nem jött az etkezesek adat a szerverről.";
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing etkezesek from server.");
                ErrorMessage = "Failed to sync etkezesek from server.";
                return false;
            }

            }
        internal async Task<bool> SyncLoginUserToServer(List<LoginUser> loginUsers)
        {
            try
            {
                var result = await apiHelper.Post<List<LoginUser>>("/sync/loginusers", loginUsers);
                return result != null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing login users to server.");
                ErrorMessage = "Failed to sync login users to server.";
                return false;
            }
        }
        internal async Task<bool> SyncUserToServer(List<User> users)
        {
            try
            {
                var result = await apiHelper.Post<List<User>>("/sync/users", users);
                return result != null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing users to server.");
                ErrorMessage = "Failed to sync users to server.";
                return false;
            }
        }
        internal async Task<bool> SyncEtkezesToServer(List<Etkezes> etkezesek)
        {
            try
            {
                var result = await apiHelper.Post<List<Etkezes>>("/sync/etkezesek", etkezesek);
                return result != null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing etkezesek to server.");
                ErrorMessage = "Failed to sync etkezesek to server.";
                return false;
            }
        }
    }
}
