# Kompilacja i rozwój

## Wymagania

- .NET SDK 8 i Python 3.9+.
- Własna instalacja Flotsam na Windows z BepInEx 5.4.x x64.
- Biblioteki referencyjne z `Flotsam_Data/Managed` oraz `BepInEx/core`.

## Kompilacja

Z katalogu repozytorium uruchom:

```powershell
python scripts/build.py --game-dir "D:/Steam/steamapps/common/Flotsam"
```

Skrypt sprawdza zależności, uruchamia testy, buduje plugin i aktualizuje DLL
oraz manifest w `binaries/<wersja>`. Gotowy ZIP powstaje w `dist`.
Parametr `--dotnet` pozwala wskazać konkretny plik wykonywalny SDK.

Bezpośrednia kompilacja:

```powershell
dotnet build src/FlotsamPlus.csproj -c Release '-p:GameDir=D:/Steam/steamapps/common/Flotsam'
```

Osobne lokalizacje referencji można podać przez właściwości MSBuild
`GameManagedDir`, `UnityManagedDir` i `BepInExCoreDir`.
Wynik: `src/bin/Release/netstandard2.1/FlotsamPuchatekPlus.dll`.
Zależności mają `Private=false`; nie są kopiowane do paczki pluginu.

## Testy i pakowanie

```shell
dotnet run --project tests/SpawnBatchTests.csproj -c Release
python scripts/verify_repo.py
python scripts/package_release.py
```

Osiem testów kolejki obejmuje limit operacji na klatkę, pełne i częściowe
dodawanie, brak miejsca, anulowanie, błąd wstawienia oraz ograniczenia ilości.
Testy używają klasy `SpawnBatch` z pluginu. Interfejs i działanie Harmony
wymagają osobnej próby w uruchomionej grze.

Manifest zapisuje SHA-256 DLL i plików źródłowych pluginu. Po zmianie kodu
uruchom ponownie `build.py`, aby zaktualizować binarkę i manifest.
Pakowanie zatrzyma się, jeśli źródła lub DLL nie pasują do manifestu.

## GitHub Actions

Workflow sprawdza manifest, uruchamia testy i pakuje dołączoną DLL.
Gotowy ZIP udostępnia jako artefakt. Pełna kompilacja pluginu odbywa się
lokalnie z bibliotekami gry.

Wypchnięcie tagu odpowiadającego wersji projektu, np. `v1.0.0`, uruchamia
publikację wydania z ZIP-em i sumą SHA-256. Zwykły commit nie publikuje wydania.

## Kolejna wersja

1. Zmień wersję w `src/FlotsamPlus.csproj` i atrybucie `BepInPlugin`.
2. Zaktualizuj README, CHANGELOG i `.github/RELEASE_NOTES.md`.
3. Uruchom `build.py` i sprawdź nową DLL w grze.
4. Zatwierdź źródła, binarkę, manifest oraz paczkę w `dist`.
5. Wyślij commit i odpowiadający mu tag wersji.

## Zgodność wersji 1.0.0

Pierwotną DLL zbudowano dla `netstandard2.1` z BepInEx 5.4.21 i referencjami
wspólnych API Unity 2021.3.33. Kompilacja zakończyła się bez błędów i ostrzeżeń;
wykonano osiem testów kolejki oraz próbę działania w jednej instalacji gry.
Inne wersje Flotsam wymagają osobnego sprawdzenia.

## Materiały

- [BepInEx: tworzenie pluginów](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html)
- [Harmony: prefix patches](https://harmony.pardeike.net/articles/patching-prefix.html)
