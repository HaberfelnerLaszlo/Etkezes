using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public record EtkezokFromExcel
    {
        public string Nev { get; init; }
        public string Azonosito { get; init; }
        public string Osztaly { get; init; }
        public string EtkTipusa { get; init; }

        public string Honap { get; init; }
        public EtkezokFromExcel(string nev, string azonosito,string osztaly, string tipus, string ho)
        {
            Nev = nev;
            Azonosito = azonosito;
            Osztaly = osztaly;
            EtkTipusa = tipus;
            Honap = ho;
        }
    }
}
