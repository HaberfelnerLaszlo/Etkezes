# Étkezés

## Étkezés adatbázis

Két adatbázis mely szinkronban van.
Az egyik a `etkezes_local` melyben a napi étkezéseket tároljuk, a másik pedig a `etkezesDB` melyben az étkezési adatokat tároljuk.
Az `etkezes_local` adatbázisban minden nap indításkor lefut egy szkript mely a napi étkezéseket lekéri az `etkezesDB` adatbázisból és elmenti az `etkezes_local` adatbázisba.
A napi étkezések felülírják a régi adatokat, így mindig csak a napi étkezések vannak az `etkezes_local` adatbázisban.
A diákok adatainak csak a frissítése történik meg, új diákok a szinkronnal kerülnek be az `etkezes_local` adatbázisba.
A loginUser táblában a felhasználók adatai vannak tárolva, melyek a szinkron során frissülnek az `etkezesDB` adatbázisba. Itt a etkezes_local az elsõdleges adatbázis, így a szinkron során az `etkezes_local` adatbázisban lévõ adatok kerülnek frissítésre az `etkezesDB` adatbázisban.
Azonnali szinkronizáció történik a két adatbázis között, ha:
  1. setSyncLoginUser(List[LoginUser]):bool //új felhasználó vagy update
  2. deletedSyncLoginUser(List[LoginUser]):bool //törlés

A User táblában a diákok adatai vannak tárolva, melyek a szinkron során frissülnek az `etkezes_local` adatbázisba. Itt az `etkezesDB` adatbázis az elsõdleges adatbázis, így a szinkron során az `etkezesDB` adatbázisban lévõ adatok kerülnek frissítésre az `etkezes_local` adatbázisban.
### A szinkron menete:
  1. getSyncLoginUser(syncDatum) :List[LoginUser] //update password
  2. getSyncUser(syncDatum) :List[User] //update és insert
  3. deletedSyncUser(syncDatum):List[User]
  4. setSyncUser(List[User]):bool //ujjlenyomat frissítés
  5. getEtkezes(date):List[Etkezes]

Félóránként lefut `getEtkezes(syncTime)` mely a napi étkezés¡változásokat lekéri az `etkezesDB` adatbázisból és elmenti az `etkezes_local` adatbázisba.

## Étkezés nyilvantartás
### Layout
  3 részbõl áll
  - Fejléc
    - Alkalmazás neve
    - Felhasználó neve / Belépés gomb
  - Törzs
  - Lábléc
    - Verzió száma, dátuma
### Lapok:
- Kezdõlap
  - Belépés - Azonosítás nélkül nem látható semmi
  - Menü választás
    - Diák nyilvántartó
    - Étkezés nyilvántartó
    - Felhasználó nyilvántartó
- Diákok nyilvántartása /User tábla
    - Új diák
    - Diákok feltöltése
    - Osztály szûrõ
    - Diákok litázása
        - Módosítás
        - Törlés
        - Étkezik-Nem étkezik
    - Mentés
    - Vissza
- Étkezések nyilvántartása /Etkezes tábla 
  - Étkezõk feltöltése - napi
  - Étkezõk feltöltése - havi
  - Egyenkénti feltöltés - Osztály szûrõ
  - Dátum szûrõ - aktuális napra
  - Étkezõk listázása (Név, osztály?, adag, menü, darab)
    - Étkezik - Nem étkezik (Ez a törlés)
    - Szerkesztés
  - Mentés
  - Vissza
- Felhasználók nyilvántartása /LoginUser tábla (Csak admin felhasználónál látszik)
    - Új felhasználó
    - Felhasználók listázása (Név, UserName, Szerepkör)
      - Szerkeztés
      - Törlés
    - Mentés
    - Vissza