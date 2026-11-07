# Kompilacja i rozwój

## Środowisko

.NET SDK 8, Python 3.9+, lokalna instalacja Flotsam z BepInEx 5.4.x x64. Potrzebne są biblioteki z `Flotsam_Data/Managed` i `BepInEx/core`, w tym Mono.Cecil do kontroli API.

```powershell
python scripts/build.py --game-dir "D:/Steam/steamapps/common/Flotsam"
```

Opcja `--dotnet` wskazuje SDK w niestandardowej lokalizacji. Bezpośrednia kompilacja:

```powershell
dotnet build src/FlotsamPlus.csproj -c Release '-p:GameDir=D:/Steam/steamapps/common/Flotsam'
```

Alternatywne ścieżki referencji: `GameManagedDir`, `UnityManagedDir`, `BepInExCoreDir`. DLL: `src/bin/Release/netstandard2.1/FlotsamPuchatekPlus.dll`. Referencje mają `Private=false`; nie trafiają do paczki.

## Co sprawdza build.py

1. Obecność bibliotek, w tym UnityEngine.Physics2DModule.
2. 21 testów kolejki, mnożników i przywracania wartości.
3. Kompilację pluginu.
4. 43 kontrole API/IL względem lokalnej DLL gry i zbudowanego pluginu.
5. SHA-256 źródeł, DLL i zawartości ZIP-a.

```shell
dotnet run --project tests/SpawnBatchTests.csproj -c Release
python scripts/verify_repo.py
python scripts/package_release.py
```

Osobna kontrola DLL gry:

```powershell
dotnet run --project tests/GameApiChecks.csproj -c Release '-p:GameDir=D:/Steam/steamapps/common/Flotsam' -- "D:/Steam/steamapps/common/Flotsam/Flotsam_Data/Managed/Assembly-CSharp.dll" "src/bin/Release/netstandard2.1/FlotsamPuchatekPlus.dll"
```

Kontrola korzysta z Mono.Cecil jako czytnika metadanych; nie uruchamia gry. Sprawdza sygnatury patchowanych metod, pola wstrzykiwane do patchy, osiem ścieżek flagi InstantBuild, trzy odczyty pól w korutynie podróży i jeden odczyt deltaTime w badaniach. Sama obecność tych punktów nie jest testem działania Harmony w grze.

## Organizacja

| Plik | Rola |
|---|---|
| Plugin.cs | Konfiguracja, cztery zakładki, zasoby i dotychczasowa obsługa potrzeb |
| GameplayPatches.cs | Prędkość/skręt obu ścieżek ruchu, energia, budowa, badania i nocny sen |
| ShipPhysics.cs | Przyspieszenie, tłumienie ruchu i przywracanie przechwyconych wartości |
| Tuning.cs | Ograniczenia mnożników i obsługa własności nadpisanych wartości |
| SpawnBatch.cs | Ograniczona kolejka dodawania surowców |

Mnożniki prędkości i skrętu nie zapisują wartości w WorldMap ani globalnych assetach. Korutyna tras czyta pola bezpośrednio, dlatego ma osobny transpiler.

Przyspieszenie mnoży siłę i liniowe tłumienie ruchu w trybie Force, zachowując proporcję siła/tłumienie. To przybliżenie odpowiedzi fizycznej — nie deklaracja identycznej prędkości w każdej sytuacji kolizji. Hamowanie zwiększa tłumienie bez aktywnego sterowania. Każda klatka najpierw odtwarza bazę, więc mnożniki nie narastają. Reset, zmiana osady i wyłączenie pluginu odtwarzają bazę; późniejsze zmiany innych modów są respektowane.

Parametry Rigidbody2D mają nazwy linearDamping/angularDamping w Unity 6 oraz drag/angularDrag we wcześniejszych wersjach. Wybór nazw następuje w czasie działania. Nie modyfikujemy globalnego BuildingDevTools.InstantBuild; transpiler uwzględnia ustawienie pluginu na odczytach flagi.

Nocny sen: blokowane jest przydzielanie nowego zadania Rest. Nie zrywamy aktywnej korutyny snu, aby zachować naturalne sprzątanie łóżka, rodzica obiektu i ikon.

Grupy patchy uruchamiają się osobno. Częściowo zainstalowana grupa pozostaje nieaktywna dzięki sprawdzaniu flagi gotowości w callbackach.

## Zgodność 1.1.0

- Assembly-CSharp.dll: SHA-256 `5b5c9f730ed71d5f66c0267064fc57667d2ff5644a018e9189b140ce025c1cd8`.
- Kompilacja: .NET SDK 8.0.427, BepInEx 5.4.21, referencje UnityEngine.Modules 2021.3.33.
- 21 testów logiki i 43 kontroli API/IL przeszło; kompilacja bez błędów i ostrzeżeń.
- **Test w uruchomionej grze: jeszcze niewykonany.** Inne wersje gry wymagają własnego sprawdzenia.
- Działanie poprzedniej DLL 1.0.0 użytkownik potwierdził 7 listopada 2026.

## GitHub Actions i wersjonowanie

CI uruchamia testy logiki, sprawdza manifest i pakuje dołączoną DLL. Nie ma bibliotek gry, więc nie kompiluje pluginu ani nie wykonuje kontroli API gry. Wynik udostępnia jako artefakt. Tag wersji uruchamia istniejący workflow publikacji wydania.

Przy nowej wersji zmień projekt i BepInPlugin, dokumentację i release notes. Uruchom build.py; zatwierdź źródła, testy, manifest, DLL i ZIP. Manifest obejmuje wszystkie pliki C# z katalogu src.
