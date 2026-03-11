using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public class SyncDatesView
    {
        public DateTime UserSyncDateUp { get; set; }
        public DateTime LoginUserSyncDateUp { get; set; }
        public DateTime EtkezesSyncDateUp { get; set; } = DateTime.UtcNow;
        public bool UserSyncDateDown { get; set; }
        public bool LoginUserSyncDateDown { get; set; }
        public bool EtkezesSyncDateDown { get; set; }

    }
}
