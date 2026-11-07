# Flotsam Plus

**Surowce na życzenie, osobne przełączniki głodu i pragnienia oraz polskie menu w grze.**
![Menu Flotsam Plus](docs/images/menu.png)

Dodatek do **Flotsam** na Windows, działający przez **BepInEx 5.4.x**.
Menu otwierasz klawiszem **F8**. Może działać razem z Flotsam Trainer
autorstwa Hansjoy, którego menu pozostaje pod F9.

[English](README.en.md) · [Pobierz ZIP 1.0.0](dist/Flotsam_Plus_1.0.0.zip) · [Instalacja](docs/INSTALL_PL.md) · [Kompilacja](docs/DEVELOPMENT.md)

## Funkcje

| Opcja | Działanie |
| --- | --- |
| Wyłącz głód | Zaspokaja bieżący głód i zatrzymuje jego narastanie. |
| Wyłącz pragnienie | Zaspokaja bieżące pragnienie i zatrzymuje jego narastanie. |
| Dodawanie surowców | Wybór przedmiotu, wyszukiwanie i ilość od 1 do 5000. |
| Właściwe magazyny | Uwzględnia filtry, ukończenie budynku oraz wolną pojemność. |
| Częściowe dodawanie | Pokazuje rzeczywiście dodaną liczbę i zatrzymuje się po zapełnieniu magazynów. |
| Ustawienia | Zapamiętuje oba przełączniki; klawisz menu można zmienić w konfiguracji. |

Opcje potrzeb dotyczą żywych mieszkańców osady gracza. Domyślnie są
wyłączone. Przedmioty korzystają z nazw w języku gry; wyszukiwarka rozpoznaje
też nazwy wewnętrzne i ID. Przedmioty fabularne są pomijane.

## Szybka instalacja

1. Zainstaluj [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21), uruchom grę raz i zamknij ją.
2. Pobierz [Flotsam_Plus_1.0.0.zip](dist/Flotsam_Plus_1.0.0.zip).
3. Wypakuj folder `BepInEx` z paczki do katalogu zawierającego `Flotsam.exe`.
4. Uruchom grę, wczytaj zapis i naciśnij **F8**.

Docelowa ścieżka pliku:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

Zrób kopię zapisu przed pierwszą próbą. Dodane surowce mogą zostać zapisane
w stanie gry. Szczegóły i rozwiązywanie problemów: [instrukcja](docs/INSTALL_PL.md).

## Stan wersji 1.0.0

- Kompilacja zakończona bez błędów i ostrzeżeń względem dostarczonej biblioteki gry.
- Osiem testów kolejki dodawania zakończonych powodzeniem.
- Użytkownik potwierdził działanie w swojej grze 7 listopada 2026 r.
- Załączona DLL jest dokładnie tą samą binarką, której działanie potwierdzono.
- Inne wersje gry wymagają osobnego sprawdzenia.

## Dla programistów

Wymagane: **.NET SDK 8** i **Python 3.9+**. Pełna kompilacja pluginu korzysta
z bibliotek własnej instalacji Flotsam i BepInEx.

```powershell
python scripts/build.py --game-dir "D:/Steam/steamapps/common/Flotsam"
```

Skrypt uruchamia testy, buduje DLL, aktualizuje jej manifest i tworzy ZIP w
`dist/`. Parametr `--dotnet` pozwala wskazać konkretny plik wykonywalny SDK.
Testy samej kolejki nie wymagają zainstalowanej gry:

```powershell
dotnet run --project tests/SpawnBatchTests.csproj -c Release
```

Pakowanie istniejącej, sprawdzonej DLL:

```powershell
python scripts/verify_repo.py
python scripts/package_release.py
```

[Dokumentacja rozwoju projektu](docs/DEVELOPMENT.md) opisuje zależności,
manifest binarki i przygotowanie następnej wersji.

## GitHub Actions

Po wysłaniu zmian workflow uruchamia testy kolejki, sprawdza zgodność źródeł
z manifestem binarki i przygotowuje ZIP do pobrania jako artefakt.
CI pakuje dołączoną DLL; pełna kompilacja pluginu odbywa się lokalnie z
bibliotekami gry. Po wypchnięciu tagu, np. `v1.0.0`, drugi etap workflow
publikuje GitHub Release z ZIP-em i sumami SHA-256.

## Licencja i zależności

Kod tego projektu: [MIT](LICENSE). Flotsam, BepInEx, Unity, Harmony oraz
oryginalny Flotsam Trainer zachowują własne licencje. Zobacz [NOTICE.md](NOTICE.md).
