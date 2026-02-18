using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Etkezes_Ellenor.Data
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "A név mező maximum 100 karakter hosszú lehet.")]
        public string Name { get; set; } = string.Empty;
        [Range(0, int.MaxValue, ErrorMessage = "A FpId mezőnek pozitív egész számnak kell lennie.")]
        public int FpId { get; set; }    = 0;
        public string FingerPrint1 { get; set; } = string.Empty;
        public string FingerPrint2 { get; set; } = string.Empty;
        /// <summary>
        /// Osztály: Ez egy szöveges mező, amely a felhasználó osztályát vagy csoportját jelöli. Ez az információ hasznos lehet például egy iskolai rendszerben, ahol a diákokat különböző osztályokba sorolják, vagy egy vállalati rendszerben, ahol a munkavállalókat különböző osztályokba vagy részlegekbe sorolják. Az osztály mező segít azonosítani, hogy melyik csoporthoz tartozik a felhasználó, és lehetővé teszi a csoportos műveletek végrehajtását a rendszerben.
        /// </summary>
        [Required]
        [StringLength(5, ErrorMessage = "Az osztály mező maximum 5 karakter hosszú lehet.")]
        public string Osztaly { get; set; } = string.Empty;
        /// <summary>
        /// Étkezik: Ez egy logikai érték (boolean), amely jelzi, hogy a felhasználó jelenleg étkezik-e vagy sem. Ha az érték true, akkor a felhasználó étkezik, ha false, akkor nem étkezik. Ez az információ hasznos lehet például egy étkezési rendszerben, ahol nyomon kell követni, hogy mely felhasználók vesznek részt az étkezésben.
        /// </summary>
        public bool Etkezik { get; set; }
        /// <summary>
        /// Létrehozva: Az az időpont, amikor a felhasználó adatai először létre lettek hozva a rendszerben. Ez általában akkor történik, amikor egy új felhasználó regisztrál vagy amikor egy adminisztrátor létrehoz egy új felhasználói fiókot. Ez az időpont segít nyomon követni, hogy mikor került be a rendszerbe az adott felhasználó.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;
        /// <summary>
        /// Feltöltve: Az az időpont, amikor a felhasználó adatai utoljára feltöltésre kerültek a rendszerbe. Ez lehet manuális vagy automatikus feltöltés eredménye is, és segít nyomon követni, hogy mikor történt az utolsó adatfrissítés a felhasználóval kapcsolatban.
        /// </summary>
        public DateTime Uploaded { get; set; }
        /// <summary>
        /// Frissítve: Az az időpont, amikor a felhasználó adatai utoljára módosításra kerültek a rendszerben. Ez lehet manuális vagy automatikus frissítés eredménye is, és segít nyomon követni, hogy mikor történt az utolsó adatváltoztatás a felhasználóval kapcsolatban.
        /// </summary>
        public DateTime Updated { get; set; }  = DateTime.Now;

    }
}