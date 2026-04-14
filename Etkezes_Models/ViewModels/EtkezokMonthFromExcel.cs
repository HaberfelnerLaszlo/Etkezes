using System;
using System.Collections.Generic;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public record EtkezokMonthFromExcel
    {
        public string Nev { get; init; }
        public string Azonosito { get; init; }
        public string Osztaly { get; init; }
        public string EtkTipusa { get; init; }

        public string Honap { get; init; }
        public int[] Etkezesek { get; init; } = new int[32];
        public EtkezokMonthFromExcel(string nev, string azonosito,string osztaly, string tipus, string ho, int[] etkezesek)
        {
            Nev = nev;
            Azonosito = azonosito;
            Osztaly = osztaly;
            EtkTipusa = tipus;
            Etkezesek = etkezesek;
            Honap = ho;
        }
    }
}
