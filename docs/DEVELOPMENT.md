# Kompilacja i wydania

## Pełna kompilacja pluginu

Zainstaluj .NET SDK 8, Python 3.9+ oraz BepInEx we własnej instalacji gry.
Ze źródłowego katalogu repozytorium uruchom:

```powershell
python scripts/build.py --game-dir "D:/Steam/steamapps/common/Flotsam"
```

Skrypt sprawdza wymagane biblioteki, uruchamia osiem testów kolejki,
buduje plugin i zapisuje nową DLL oraz manifest w `binaries/<wersja>`.
Następnie tworzy ZIP w `dist`. Wybrany SDK można wskazać przez `--dotnet`.

Bezpośrednia kompilacja z osobnymi katalogami referencji:

```powershell
dotnet build src/FlotsamPlus.csproj -c Release `
  '-p:GameManagedDir=D:/Games/Flotsam/Flotsam_Data/Managed' `
  '-p:UnityManagedDir=D:/Games/Flotsam/Flotsam_Data/Managed' `
  '-p:BepInExCoreDir=D:/Games/Flotsam/BepInEx/core'
```

Zależności projektu mają `Private=false`. Plugin używa bibliotek dostarczonych
z grą i BepInEx; skrypt pakowania dodaje wyłącznie DLL tego projektu.

## Testy i manifest

```shell
dotnet run --project tests/SpawnBatchTests.csproj -c Release
python scripts/verify_repo.py
python scripts/package_release.py
```

Testy obejmują limit operacji na klatkę, pełną i częściową operację, brak
magazynu, anulowanie, wyjątek przy wstawieniu oraz ograniczenia ilości.
Wykonują rzeczywistą klasę `SpawnBatch` ze źródeł pluginu.

Manifest zawiera sumę SHA-256 DLL oraz trzech plików źródłowych projektu.
Pakowanie odrzuca binarkę z inną sumą lub nieaktualnymi źródłami.
Po zmianie pluginu uruchom ponownie `build.py`; sama zmiana manifestu nie
zastępuje kompilacji. Testy kolejki nie sprawdzają interfejsu ani działania
Harmony w uruchomionej grze.

## Co robi CI

Dołączony GitHub Actions workflow:

1. Weryfikuje manifest i pliki repozytorium.
2. Uruchamia osiem testów kolejki na .NET 8.
3. Tworzy powtarzalny ZIP z dołączonej DLL i instrukcją.
4. Udostępnia ZIP jako artefakt workflow.
5. Przy tagu `v<wersja>` publikuje GitHub Release z ZIP-em i SHA-256.

Pełna kompilacja wymaga lokalnej instalacji gry. CI korzysta z gotowej DLL
w `binaries`, której zgodność ze źródłami opisuje manifest.

## Następna wersja

1. Zmień wersję w `src/FlotsamPlus.csproj` i atrybucie `BepInPlugin`.
2. Zaktualizuj README, CHANGELOG, instrukcję i `.github/RELEASE_NOTES.md`.
3. Uruchom `python scripts/build.py --game-dir "..."`.
4. Sprawdź plugin w grze, potem zatwierdź kod, nową binarkę i manifest.
5. Wyślij commit oraz odpowiadający mu tag, np. `v1.0.1`.

## Pierwotna wersja 1.0.0

Pierwotną DLL zbudowano z BepInEx 5.4.21, `netstandard2.1`, dostarczoną
biblioteką gry i referencjami wspólnych API Unity 2021.3.33.
SHA-256 biblioteki gry użytej do tej kompilacji:

```text
5b5c9f730ed71d5f66c0267064fc57667d2ff5644a018e9189b140ce025c1cd8
```

Kompilacja przeszła bez błędów i ostrzeżeń. Użytkownik potwierdził działanie
tej DLL w swojej grze 2026-11-07. Nie jest to sprawdzenie wszystkich wersji gry.

## Materiały

- https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html
- https://harmony.pardeike.net/articles/patching-prefix.html
- https://github.com/actions/checkout
- https://github.com/actions/setup-dotnet
- https://github.com/actions/upload-artifact
- https://cli.github.com/manual/gh_release_create
