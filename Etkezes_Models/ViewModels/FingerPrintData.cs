using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public record FingerPrintData
    {
        public int FpId { get; set; }
        public string FingerTemplate1 { get; set; } = string.Empty;
        public string FingerTemplate2 { get; set; } = string.Empty;
    }
}
