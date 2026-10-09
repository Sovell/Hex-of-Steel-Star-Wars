# Hex of Steel — Star Wars prototype

Prototyp lądowego wargame Imperium–Rebelia, rozwijany na bazie lokalnej kopii Hex of Steel. Pierwszy scenariusz ma 12 × 9 heksów i 11 oddziałów. Grafiki i dźwięki jednostek pozostają tymczasowe.

## Zawartość repozytorium

- `StarWars-Ground-Prototype/development/source` — źródła zmienionej wersji, nad którymi dalej pracujemy.
- `StarWars-Ground-Prototype/development/Build-Analysis.ps1` — kompilacja bibliotek do katalogu weryfikacji.
- `StarWars-Ground-Prototype/development/Build-Prototype.ps1` — kompilacja i aktualizacja bibliotek w lokalnej kopii gry.
- `StarWars-Ground-Prototype/Start-Prototype.cmd` — rozpoczęcie nowej bitwy.
- `StarWars-Ground-Prototype/Start-Menu.cmd` — menu, wczytywanie zapisów i edytor.
- `HexOfSteel-Recovered/source` — pierwotny eksport dekompilacji, zachowany jako punkt odniesienia.

Instrukcja prototypu: [README](StarWars-Ground-Prototype/README.md). Analiza mechanizmów: [README](HexOfSteel-Recovered/README.md).

## Praca lokalna

Bieżący katalog projektu: `C:\Prywatne\Hex of Stell SW`.

Repozytorium zawiera źródła i dokumentację. Instalacja gry (`Game`), biblioteki referencyjne (`references/Managed`), narzędzia dekompilacji, wyniki kompilacji, profil gracza oraz logi pozostają lokalne i są pomijane przez Git. Samo sklonowanie repozytorium nie daje uruchamialnej gry. Do kompilacji potrzebne są .NET SDK i zgodne biblioteki z lokalnej instalacji; do uruchomienia także pozostałe pliki gry.

W bieżącym katalogu wszystkie te lokalne zależności są już przygotowane. Po zamknięciu prototypu uruchom w PowerShell:

```powershell
& '.\StarWars-Ground-Prototype\development\Build-Prototype.ps1'
```

Skrypt aktualizuje wyłącznie kopię w `StarWars-Ground-Prototype/Game`. Dane rozgrywki są kierowane do `StarWars-Ground-Prototype/Profile`. Steam Cloud i integracja konta są wyłączone w tej kopii.

Pierwszy prototyp przeszedł sprawdzenie uruchomienia z grafiką, wyznaczania drogi, ruchu, ataku, zapisu i odczytu stanu oraz tury AI. Projekt nie jest pełnym projektem edytora Unity. Szczegóły ograniczeń i pochodzenia kodu znajdują się w dokumentacji obu katalogów.
