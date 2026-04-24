using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public class NewLoginUser:LoginUser
    {
        public string NewPassword { get; set; }  = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
