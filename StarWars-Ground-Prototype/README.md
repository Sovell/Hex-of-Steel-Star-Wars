# Prototyp lądowy: Imperium kontra Rebelia

**Uruchom `Start-Prototype.cmd`, aby rozpocząć nową bitwę.** Plik `Start-Menu.cmd` otwiera menu gry — tam można wczytać zapis lub wejść do edytora map. Nie uruchamiaj obu kopii prototypu równocześnie, ponieważ korzystają z tego samego lokalnego profilu.

Scenariusz **„Placowka przekaznikowa — prototyp SW”** ma 12 × 9 heksów. Gracz prowadzi Imperium Galaktyczne, a komputer Sojusz Rebeliantów. Imperium ma sześć oddziałów, Rebelia pięć. Zdobądź główną bazę przeciwnika i zachowaj własną. Centralny przekaźnik jest dodatkowym punktem strategicznym. Scenariusz nie ma limitu tur.

Oddziały Imperium: trzy drużyny szturmowców, patrol AT-ST, bateria wsparcia E-Web i saperzy. Oddziały Rebelii: trzy drużyny piechoty, czołg zwiadowczy i bateria blasterów. Definicje mają własne nazwy, statystyki i frakcje; grafiki oraz dźwięki jednostek pochodzą z istniejącej gry i są tymczasowe. AT-ST korzysta obecnie z zasad pojazdu pancernego. Nie dodano jeszcze osłon, Mocy, ruchu repulsorowego ani animacji charakterystycznych dla Gwiezdnych Wojen.

Na tym etapie wyłączono mgłę wojny, pogodę, badania, polityki, partyzantów, historyczny tryb AI, złożone zaopatrzenie, manpower i utrzymanie. Amunicja i paliwo pozostają skończone. Bitwa ma służyć sprawdzeniu podstaw ruchu, walki, zakupów i zachowania AI. Kamera otwiera widok całej małej mapy. Interfejs pozostaje interfejsem Hex of Steel.

**Zapisy i ustawienia**

| Lokalizacja | Zawartość |
|---|---|
| `Game/` | Osobna kopia uruchamialnej instalacji |
| `Profile/Maps/` | Wygenerowany scenariusz `.sav` |
| `Profile/Saves/` | Zapisy i automatyczne zapisy prototypu |
| `Profile/playerSettings.sav` | Oddzielne ustawienia |
| `Profile/MODS/` | Oddzielny katalog modów |
| `Profile/Screenshots/` | Zrzuty wykonywane przez grę |
| `Logs/` | Logi i dowody testów |
| `development/` | Źródła zmienionej wersji, referencje i skrypty kompilacji |

Kod zapisów, ustawień, edytorów, modów i zrzutów przekierowano do `Profile/`. Wyłączono migrację starych danych. Preferencje selektora kolorów zapisują się do pliku profilu zamiast współdzielonego rejestru. Dodatkowo tożsamość kopii w ustawieniach silnika Unity zmieniono na `Local Prototypes / SW Prototype`, aby oddzielić również jego własne preferencje. Zmieniono wyłącznie dwie identycznej długości wartości w `globalgamemanagers` oraz `app.info`; kopia oryginalnego pliku i lista przesunięć są zachowane. W tej kopii wyłączono integrację ze Steam Cloud, osiągnięciami i rankingami, a także czat publiczny. Nie zmieniano konfiguracji konta Steam ani plików oryginalnej instalacji.

Cały folder `StarWars-Ground-Prototype` można przenieść w inne miejsce: profil jest wyznaczany względem katalogu kopii gry. Przenosząc tylko `Game/`, pomijasz profil i skrypty uruchamiania.

**Dalsza edycja**

`development/source/Assembly-CSharp/PrototypeBootstrap.cs` zawiera definicje oddziałów, ustawienia prototypu, generator mapy i jawny tryb testowy. Ścieżki definiuje `development/source/Assembly-CSharp-firstpass/PrototypePaths.cs`. Pozostałe źródła zachowują odzyskaną mechanikę gry.

Po zmianie kodu zamknij kopię gry i uruchom w PowerShell `development/Build-Prototype.ps1`. Skrypt kompiluje biblioteki i podmienia je wyłącznie w `Game/`. Wymaga .NET SDK. Referencje do bibliotek są dołączone, więc kompilacja nie wymaga NuGet ani Unity Editor. To nadal nie jest pełny projekt Unity.

Generator tworzy mapę tylko wtedy, gdy plik scenariusza nie istnieje. Zmiany dokonane w edytorze nie są automatycznie nadpisywane. Ponowne generowanie można wymusić argumentem `-prototype-regenerate`; wtedy zostanie nadpisany wyłącznie scenariusz prototypu, a nie zapisy rozgrywki. `Start-Prototype.cmd` zawsze zaczyna świeżą bitwę; aby kontynuować, użyj `Start-Menu.cmd` i wczytaj zapis.

**Weryfikacja**

Przebudowane biblioteki skompilowano bez błędów. W trybie testowym gra uruchomiła scenę, stworzyła 11 jednostek z grafiką, wyznaczyła drogę, wykonała ruch oraz atak z utratą HP, zapisała i odczytała stan, a następnie wykonała turę AI. Szczegóły: `Logs/smoke-player.log` i `Logs/smoke-result.txt`. Testowe zapisy przeniesiono do `Logs/TestSaves/`, aby nie mieszały się z rozgrywką użytkownika.

Dodatkowo sprawdzono normalne uruchomienie z grafiką oraz zrzut planszy `Logs/scenario-preview.png`. Testy nie obejmują pełnej rozgrywki do zwycięstwa ani multiplayera. Pierwszy balans oddziałów jest roboczy.

Kod prototypu pochodzi z eksportu przygotowanego w poprzednim etapie. Audyt wykazał, że obecna biblioteka `Assembly-CSharp.dll` w instalacji Steam różni się od biblioteki użytej do tamtej dekompilacji; nie ustalano przyczyny ani nie przenoszono zmian pomiędzy tymi wersjami. Źródła i sumy SHA-256 zapisano w `Logs/verification.json`. Sprawdzenie działania dotyczy przygotowanej kopii prototypu. Porównanie wszystkich 351 plików instalacji z kopią wykazało tylko cztery zamierzone różnice: obie przebudowane biblioteki oraz ustawienia tożsamości Unity w `globalgamemanagers` i `app.info`.
