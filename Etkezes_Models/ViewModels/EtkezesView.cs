using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Etkezes_Models.ViewModels
{
    public class EtkezesView
    {
       public int Id { get; set; }
        [Required]
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Osztaly { get; set; } = string.Empty;
 /// <summary>
        /// Étkezés dátuma. Az érték alapértelmezés szerint a mai nap lesz.
        /// </summary>
        [Required]
        public DateTime Datum { get; set; } = DateTime.Today;
        /// <summary>
        /// Menü típusa, amelyet a felhasználó elfogyaszt. Az értékek a következők lehetnek:
        /// - A
        /// - B
        /// </summary>
        public string Menu { get; set; } = "A";
        /// <summary>
        /// Kor alapján a következő értékeket veheti fel:
        /// - 0-14 év: kis adag
        /// - 15-64 év: normál adag
        /// </summary>
        public string Adag { get; set; } = "kis adag";
        /// <summary>
        /// Étkezés darabszáma egy napon belül, amelyet a felhasználó elfogyaszt.
        /// </summary>
        public int Darab { get; set; } = 1;
        /// <summary>
        /// Megtörtént az étel elfogyasztása vagy sem.
        /// </summary>
        public bool Elfogyasztva { get; set; } = false;
    }
}
