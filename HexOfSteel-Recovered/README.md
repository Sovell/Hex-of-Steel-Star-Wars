# Hex of Steel — odzyskany kod i analiza mechanizmów

Analiza wykonana 9 października 2026 na lokalnej instalacji z `C:\Program Files (x86)\Steam\steamapps\common\Hex of Steel`.

**Wynik: obie biblioteki C# zostały zdekompilowane, a ich odzyskany kod daje się ponownie skompilować.** Nie uruchomiono przebudowanych bibliotek w grze. Jest to materiał do analizy i dalszego rozwoju, a nie kompletny projekt Unity ani samodzielna gra.

**Co znajduje się w tym katalogu**

| Katalog / plik | Zawartość |
|---|---|
| `source/Assembly-CSharp/` | 271 plików C#, 142 658 linii; mechanika, AI, interfejs, edytory, lokalizacje, integracje |
| `source/Assembly-CSharp-firstpass/` | 30 plików C#, 9 181 linii; głównie LeanTween i dodatkowe komponenty |
| `references/Managed/` | Kopie 106 bibliotek z instalacji; zależności i materiał źródłowy dekompilacji |
| `verification/` | Przebudowane biblioteki i pliki odpowiedzi kompilatora |
| `logs/` | Logi dekompilacji i kompilacji, statystyki oraz sumy SHA-256 skopiowanych bibliotek |
| `tools/` | Lokalnie zainstalowany ILSpy CLI 11.1.0.9782 i jego zależności |
| `Build-Analysis.ps1` | Powtarzalna kompilacja do `verification/`, bez podmiany plików gry |

Katalog leży poza instalacją Steam. Nie skopiowano wykonywalnej gry, scen, tekstur ani pozostałych zasobów Unity. Nie modyfikowano plików instalacji.

**Sposób odzyskania i weryfikacja**

Instalacja wykorzystuje Unity **2022.3.62f3**, Mono oraz kod zarządzany .NET. Wersję Unity odczytano z `globalgamemanagers`. Nazwy klas i metod pozostają czytelne w sprawdzonych mechanizmach.

Biblioteki zdekompilowano ILSpy CLI z opcjami `--disable-updatecheck -p -lv CSharp7_3`, wskazując kopię `references/Managed` jako katalog zależności. Oba wywołania zwróciły kod 0; logi są puste. W odzyskanych plikach nie znaleziono komunikatów typu `DecompilerException`, `Error decompiling`, `Could not decompile` ani `Invalid IL code`.

Kompilacja: Roslyn z lokalnego .NET SDK 10.0.302, język C# 7.3, `/nostdlib+`, referencje do bibliotek dostarczonych z grą. Najpierw skompilowano `Assembly-CSharp-firstpass`, potem bibliotekę główną odwołującą się do **przebudowanego** firstpass. W referencjach kompilacji nie użyto oryginalnego `Assembly-CSharp.dll`.

| Biblioteka | Błędy | Ostrzeżenia | Wynik |
|---|---:|---:|---|
| Assembly-CSharp-firstpass | 0 | 12 | DLL utworzona |
| Assembly-CSharp | 0 | 651 | DLL utworzona |

W głównej bibliotece 621 ostrzeżeń dotyczy pól bez przypisania w kodzie (CS0649); wiele takich pól jest przypisywanych przez serializację Unity. Pozostałe dotyczą m.in. nieużywanych pól, przestarzałych elementów, wywołań asynchronicznych bez `await`, porównań referencji i przypisań zmiennej do siebie. Nie poprawiano ich w odzyskanym kodzie. Ostrzeżenia i udana kompilacja nie dowodzą zgodności działania z oryginałem.

Sumy SHA-256 wszystkich 106 kopii DLL zgadzają się z bibliotekami w instalacji po zakończeniu kompilacji. Dane audytowe: `logs/assembly-hashes.csv`, `logs/source-statistics.json`.

Skrypt `Build-Analysis.ps1` można uruchamiać z PowerShell. Wymaga zainstalowanego .NET SDK; korzysta z lokalnych referencji, bez pobierania pakietów. Automatycznie wybiera najnowszy dostępny kompilator z katalogu SDK. Wygenerowane przez dekompilator pliki `.csproj` wskazują `net40`; nie weryfikowano ich przez `dotnet build`. Potwierdzona ścieżka kompilacji to dołączony skrypt.

**Mapa mechanizmów — od czego zacząć czytanie**

Podane numery linii dotyczą niezmienionego eksportu znajdującego się w `source/Assembly-CSharp`.

| Mechanizm | Miejsce w kodzie | Ustalenie |
|---|---|---|
| Model jednostki | `Unit.cs:204` i dalej | Nazwa, kraj, typ, filtr, HP, pancerz, punkty ruchu, trzy rodzaje obrażeń, zasięg, amunicja, autonomia, koszt i cechy |
| Kalkulator obrażeń | `Unit.cs:1557`, `BattleContext.cs` | `GetPotentialDamage` oblicza obrażenia i ich rozbicie na kolejne modyfikatory |
| Dobór rodzaju obrażeń | `Unit.cs:1775`, `DamageTypes.cs` | SOFT, HARD i AIR; wybór powiązany z obecnymi typami jednostek |
| Wykonanie ataku | `UnitGO.cs:2452`, `UnitGO.cs:2890` | Stan jednostek, dźwięki, animacja, morale, zużycie zasobów i integracja z multiplayerem |
| Wsparcie / odwet / odwrót | `UnitGO.cs:3080`, `UnitGO.cs:3365`, `UnitGO.cs:3457` | Oddzielne etapy obsługi starcia |
| Wyznaczanie drogi | `UnitGO.cs:5640` | A*: kolejka priorytetowa, koszt ruchu i heurystyka odległości; oddzielne reguły dla lądu, samolotów i statków |
| Wykonanie ruchu | `UnitGO.cs:1420`, `UnitGO.cs:1432` | Przejście po wyznaczonej ścieżce; m.in. strefy kontroli i zasadzki |
| Strefy kontroli | `Unit.cs:3245` | Ustalanie, czy jednostka wywiera strefę kontroli na inną jednostkę |
| Zaopatrzenie | `UnitGO.cs:859`, `Tile.cs:536`, `Tile.cs:577` | Źródła zaopatrzenia, trasy, koszty terenu i zagrożenia; samoloty mają odrębną obsługę |
| Główna pętla AI | `AI.cs:1011` | Gra jednostkami w korutynie, z osobnymi etapami decyzji |
| Cel ruchu i droga AI | `AI.cs:2357`, `AI.cs:4303` | Wyznaczanie celów i własna rozbudowana logika planowania ruchu |
| Wybór ataku AI | `AI.cs:5537`, `AI.cs:10177` | Prognozy obrażeń, odwet, wsparcie, koszt jednostek, punkty zwycięstwa i relacja ekonomiczna |
| Zakupy AI | `AI.cs:6410`, `AI.cs:7746` | Wybór zakupów i warunki dostępności jednostek |
| Tury i zdarzenia | `TurnManager.cs:262`, `TurnManager.cs:762` | Zmiana gracza, pogoda, badania i zdarzenia scenariusza |
| Mapa | `Map.cs`, `Tile.cs:220` | Siatka `Tile[,]`; heks posiada osobne pola `GroundUnit` i `AirUnit` |
| Mody z kodem | `ModsManager.cs:484`, `Mod.cs:32`, `GameModification.cs:24` | Ładowanie DLL z folderu Libraries i tworzenie klas dziedziczących po GameModification |
| Dane i zapis | `OfficialUnits.cs:41`, `OfficialUnits.cs:50`, `GameData.cs:318`, `GameData.cs:339` | Definicje jednostek i zapis stanu wykorzystują BinaryFormatter; zapis gry dodatkowo kompresję Deflate |

**Walka**

`GetPotentialDamage` wybiera bazowe obrażenia i stosuje modyfikatory zdrowia/doświadczenia, pancerza, bohatera, polityk, morale, pogody, terenu, rozpoznania, wsparcia dowódcy, połączonych rodzajów wojsk, okopania i okrążenia. Są także szczegółowe reguły II wojny światowej, np. niszczyciele przeciw okrętom podwodnym, ataki na barki desantowe i torpedy. Kolejność modyfikatorów oraz zaokrąglenia są istotne; kalkulator nie jest prostym wzorem „atak minus obrona”.

AI i wykonanie ataku korzystają z tego samego kalkulatora prognozowanych obrażeń. To dobre miejsce do dodania osłon energetycznych czy nowych rodzajów uzbrojenia, ale trzeba też dostosować wykonanie ataku, podgląd statystyk, wybór celów, odwet i zużycie zasobów. Samo dodanie pola Shield do Unit nie tworzy działającej mechaniki osłon.

**Ruch i mapa**

Koszt drogi uwzględnia teren, zajętość pól, mgłę wojny, sojusze, strefy kontroli i właściwości jednostki. Są osobne ograniczenia dla samolotów, statków, pociągów i jednostek lądowych. Dołożenie repulsorów powinno obejmować wyszukiwanie drogi, podgląd zasięgu ruchu oraz wykonanie ruchu. AI ma dodatkową własną logikę tras, którą też trzeba przejrzeć.

Model heksa ma dwa sloty jednostek: lądowy i powietrzny. Floty na orbicie, transport pomiędzy planetami lub wiele warstw mapy wymagają zaprojektowania zmian w danych, interfejsie, regułach ruchu, celowaniu i zapisie. Przemianowanie Boat na statek kosmiczny pozostawiłoby ograniczenia związane z morzem, portami i istniejącymi kategoriami.

**AI**

Odzyskane AI jest systemem reguł i heurystyk. Obsługuje m.in. ruch do celów, ataki, obronę granic, zaopatrzenie, zakupy, rozmieszczanie jednostek, badania i polityki. Plik AI.cs ma ponad 10 tys. linii. Jest silnie powiązany ze stanem gry i obiektami Unity; jego wydzielenie do własnego silnika wymaga pracy.

W kodzie pozostają wyjątki historyczne: nazwy krajów, daty, kontrola desantów i zachowanie związane z historycznym trybem gry. Przykłady: `AI.cs:868`, `AI.cs:5178`, `AI.cs:5872`; `TurnManager.cs:1512`, `TurnManager.cs:1573`, `TurnManager.cs:1615`. Do scenariuszy Gwiezdnych Wojen trzeba sprawdzić warunki ich uruchamiania i usunąć lub zastąpić niepasujące reguły. Nie potwierdzono, że samo wyłączenie trybu historycznego eliminuje wszystkie takie zależności.

**Wbudowane rozszerzenia C#**

Gra już ładuje biblioteki DLL z katalogu `Libraries` moda. Wyszukuje konkretne klasy dziedziczące po `GameModification`, tworzy je z argumentem `Mod` i wywołuje `OnModInitialization`. Przy wyłączeniu moda wywołuje `OnModUnloaded`. Są też struktury własnych jednostek, kafli, krajów, bohaterów i modyfikacji danych bazowych, a także ładowanie grafik i dźwięków.

To potwierdzona w kodzie droga do rozszerzeń programistycznych. Nie oznacza, że API udostępnia gotowe punkty rozszerzenia dla każdego etapu walki i ruchu. Dla głębokich zmian nadal może być potrzebna modyfikacja biblioteki głównej lub dodatkowe techniki rozszerzania kodu. Nie utworzono ani nie załadowano testowego moda.

**Ocena dla wargame w klimacie Gwiezdnych Wojen**

| Pomysł | Ocena na podstawie kodu |
|---|---|
| Republika, Separatyści, Imperium, Rebelia; nowe jednostki, grafiki i dźwięki | Dobre dopasowanie do istniejących danych i systemu modów; wymaga zbudowania treści i sprawdzenia zależności |
| Bitwy lądowe z piechotą, droidami, pojazdami i lotnictwem | Najbliższe obecnej architekturze; sensowny pierwszy prototyp |
| AT-AT / AT-ST jako pojazdy o własnych cechach | Możliwe przypisanie do istniejącego typu pojazdu; odrębne reguły ruchu kroczącego wymagają kodu |
| Repulsory, osłony, Moc, umiejętności aktywne | Rozszerzenie mechaniki; potrzebne zmiany w AI, UI i zapisie zależnie od pomysłu |
| Walki kosmiczne i floty | Znaczna przebudowa ruchu, typów, terenu, celowania oraz AI |
| Kampania galaktyczna i przejścia powierzchnia–orbita | Nowy system strategiczny i powiązanie kilku warstw/scenariuszy; nie wynika z samej dekompilacji |

Moja propozycja kolejnego etapu: kopia uruchamialnej instalacji z osobnymi zapisami i jeden mały scenariusz lądowy. Najpierw potwierdzić zachowanie przebudowanej biblioteki bez zmian, potem wdrożyć jedną konkretną mechanikę i sprawdzić reakcję AI. Dopiero po takim prototypie decydować o pełnym odtworzeniu projektu Unity lub architekturze kampanii galaktycznej.

**Granice wykonanej pracy**

Nie odzyskano scen, prefabów, tekstur, dźwięków ani ustawień projektu Unity. Nie sprawdzono działania przebudowanych bibliotek, multiplayera ani zgodności zapisu. Nie naprawiano ostrzeżeń ani nie wprowadzano zmian w mechanice.

Dokumentacja użytego narzędzia: [ILSpy](https://github.com/icsharpcode/ILSpy), [ILSpy CLI w NuGet](https://www.nuget.org/packages/ilspycmd/11.1.0.9782). Ustalenia o Hex of Steel pochodzą z lokalnych plików i odzyskanego kodu.
