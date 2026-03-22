using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public class UserEtkezoView
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public required string Name { get; set; }
        public required string Osztaly { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsSaved { get; set; } = false;
        public string Adag { get; set; } = string.Empty;
        public string Menu { get; set; } = string.Empty;
        public int Darab { get; set; } = 0;
        public DateTime Date { get; set; }
    }
}
