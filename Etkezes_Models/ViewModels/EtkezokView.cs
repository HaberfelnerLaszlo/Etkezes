using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public class EtkezokView
    {
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Menu { get; set; } = string.Empty;
        public string Adag { get; set; } = string.Empty;
        public int Darab { get; set; }
    }
}
