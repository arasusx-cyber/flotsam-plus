# Kompilacja i rozwój

## Środowisko

.NET SDK 8, Python 3.9+, lokalna instalacja Flotsam z BepInEx 5.4.x x64. Potrzebne są biblioteki z `Flotsam_Data/Managed` i `BepInEx/core`, w tym Mono.Cecil do kontroli API oraz UnityEngine.AIModule do nawigacji po wyspach.

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
2. 27 testów kolejki, mnożników, przywracania wartości i przyznawania punktów.
3. Kompilację pluginu.
4. 68 kontroli API/IL względem lokalnej DLL gry i zbudowanego pluginu.
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
| Plugin.cs | Konfiguracja, pięć zakładek, wybór mieszkańca, zasoby i obsługa potrzeb |
| GameplayPatches.cs | Prędkość/skręt obu ścieżek ruchu, energia, budowa, badania i nocny sen |
| ShipPhysics.cs | Przyspieszenie, tłumienie ruchu i przywracanie przechwyconych wartości |
| Tuning.cs | Ograniczenia mnożników i obsługa własności nadpisanych wartości |
| SpawnBatch.cs | Ograniczona kolejka dodawania surowców |
| ResidentTools.cs | Ruch mieszkańców, odświeżanie NavMesh oraz licznik punktów i zdarzenie panelu postaci |
| PointGrant.cs | Walidacja przyrostu punktów, zakres i ochrona przed przepełnieniem |
| MapTools.cs | Odkrywanie istniejących miejsc i mgły we wszystkich dostępnych częściach świata |

Mnożniki prędkości i skrętu nie zapisują wartości w WorldMap ani globalnych assetach. Korutyna tras czyta pola bezpośrednio, dlatego ma osobny transpiler.

Przyspieszenie mnoży siłę i liniowe tłumienie ruchu w trybie Force, zachowując proporcję siła/tłumienie. To przybliżenie odpowiedzi fizycznej — nie deklaracja identycznej prędkości w każdej sytuacji kolizji. Hamowanie zwiększa tłumienie bez aktywnego sterowania. Każda klatka najpierw odtwarza bazę, więc mnożniki nie narastają. Reset, zmiana osady i wyłączenie pluginu odtwarzają bazę; późniejsze zmiany innych modów są respektowane.

Parametry Rigidbody2D mają nazwy linearDamping/angularDamping w Unity 6 oraz drag/angularDrag we wcześniejszych wersjach. Wybór nazw następuje w czasie działania. Nie modyfikujemy globalnego BuildingDevTools.InstantBuild; transpiler uwzględnia ustawienie pluginu na odczytach flagi.

Nocny sen: blokowane jest przydzielanie nowego zadania Rest. Nie zrywamy aktywnej korutyny snu, aby zachować naturalne sprzątanie łóżka, rodzica obiektu i ikon.

Grupy patchy uruchamiają się osobno. Częściowo zainstalowana grupa pozostaje nieaktywna dzięki sprawdzaniu flagi gotowości w callbackach.

## Mieszkańcy, punkty i mapa

Mnożnik ruchu jest nakładany raz, na wynik Navigator.ReturnSpeed(TerrainType). Przeciążenie bez argumentów deleguje do tej metody. Sprawdzany jest żywy, zainicjalizowany Agent z bieżącej osady gracza. Łodzie mają osobnego właściciela nawigacji i nie otrzymują mnożnika mieszkańców.

UnityNavMeshNavigator buforuje speed przy zmianie terenu. Osobny prefix Update odczytuje aktualny wynik tej samej metody, zamiast mnożyć poprzednio zapamiętaną prędkość. Reset i wyłączenie pluginu odświeżają bufor z normalnym mnożnikiem.

Punkty są dodawane do prywatnego pola SpendablePoints konkretnego DrifterAttributes. Wywoływane jest AvailableSpendingPointsUpdatedEvent, którego słuchają licznik i przyciski w panelu postaci. Nie wywołujemy AddLevel ani AddTotalExperience. Natywne PersistentData zapisuje i odtwarza ten licznik. Przyznawanie sprawdza bieżącą osadę, listę żywych mieszkańców, zakres 1–100 i przepełnienie int.

Mapa podnosi ScoutingState istniejących, aktywnych spawnerów do Scouted. Nie wywołuje Region.Scout, StartQuest ani generowania następnych części świata; nie ustawia flag wykonania zwiadu. Zablokowane spawners fabularne są pomijane. Kolory siatki mgły są zerowane w tej samej tablicy _colors, którą czyta natywne ReturnAlphas przy zapisie. Operacja obejmuje tylko instancje mgły należące do części bieżącego świata. Automatyczna opcja ponawia odkrywanie raz na sekundę, również po inicjalizacji nowych siatek.

Te kontrole nie zastępują testu w grze: trzeba sprawdzić menu, chód/pływanie/wyspy, rozdanie punktów, zapis i ponowne wczytanie oraz odkrywanie nowego obszaru podczas podróży.

## Zgodność 1.2.0

- Assembly-CSharp.dll: SHA-256 `5b5c9f730ed71d5f66c0267064fc57667d2ff5644a018e9189b140ce025c1cd8`.
- Kompilacja: .NET SDK 8.0.427, BepInEx 5.4.21, referencje UnityEngine.Modules 2021.3.33.
- 27 testów logiki i 68 kontroli API/IL przeszło; kompilacja bez błędów i ostrzeżeń.
- **Test w uruchomionej grze: jeszcze niewykonany.** Inne wersje gry wymagają własnego sprawdzenia.
- Działanie poprzedniej DLL 1.1.0 użytkownik potwierdził 8 listopada 2026; paczki 1.0.0 i 1.1.0 pozostają dostępne.

## GitHub Actions i wersjonowanie

CI uruchamia testy logiki, sprawdza manifest i pakuje dołączoną DLL. Nie ma bibliotek gry, więc nie kompiluje pluginu ani nie wykonuje kontroli API gry. Wynik udostępnia jako artefakt. Tag wersji uruchamia istniejący workflow publikacji wydania.

Przy nowej wersji zmień projekt i BepInPlugin, dokumentację i release notes. Uruchom build.py; zatwierdź źródła, testy, manifest, DLL i ZIP. Manifest obejmuje wszystkie pliki C# z katalogu src.
