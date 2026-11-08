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
2. 58 testów kolejki, mnożników, punktów, języków, udźwigu, układu, kontrastu i przechwytywania wejścia.
3. Kompilację pluginu.
4. 92 kontrole API/IL względem lokalnej DLL gry i zbudowanego pluginu.
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
| Plugin.cs | Konfiguracja, przełącznik PL/EN, pięć zakładek, udźwig, mieszkańcy i zasoby |
| WindowUi.cs | Konfiguracja wyglądu, zakładka Okno, prostokąty przewijanej zawartości, zmiana rozmiaru i reset |
| WindowLayout.cs | Projekcja okna na ekran, układ przycisków, kolory i polityka przechwytywania wejścia |
| WindowAppearance.cs | Własny GUISkin i sześć tekstur 1×1; odświeżanie i sprzątanie |
| MenuAxisInputPatch.cs | Neutralizuje osie i zoom gry podczas obsługi menu |
| Localization.cs | 118 par tłumaczeń; leniwe formatowanie komunikatów i kultura liczb |
| WeightCapacity.cs | Mnożnik udźwigu i skończone wartości bez przepełnienia |
| WeightPatches.cs | Wynik ReturnTownTugCapacity i sama kontrola wagi w CanTug |
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

Kontrole automatyczne nie uruchamiają gry. Użytkownik potwierdził działanie wydania 1.2.0 8 listopada 2026. Przy kolejnych zmianach sprawdzać menu, chód/pływanie/wyspy, rozdanie punktów, zapis i ponowne wczytanie oraz odkrywanie nowego obszaru podczas podróży.

## Wygląd okna w 1.4.0

Nowa część partial Plugin mieści UI w WindowUi.cs. Okno ma prywatny GUISkin i sześć ponownie używanych tekstur 1×1; sam panel dostaje ustawione alpha, a kontrolki i tekst pozostają nieprzezroczyste. Style/tekstury zmieniają się przy zmianie preferencji. OnDestroy niszczy skórkę i tekstury. OnGUI przywraca poprzednie GUI.skin, matrix, color, backgroundColor, contentColor i enabled w finally.

WindowLayout.Fit projektuje rozmiar i pozycję na aktualny ekran bez nadpisywania większych preferencji użytkownika. Układ stosuje zmianę przy EventType.Layout. Nagłówek i zakładki mają osobne obszary, a zawartość każdej zakładki jest przycięta i przewijana. Przyciski gotowych wartości mieszczą się w siatkach; ilość i akcja są w osobnych rzędach.

Sekcja konfiguracji [Okno] przechowuje Szerokosc, Wysokosc, PozycjaX/Y, KolorTlaR/G/B, NieprzezroczystoscTla i RozmiarTekstu. Zmiany są zapisywane po .4 s bez przeciągania, a także przy zamknięciu i wyłączeniu. Ctrl + klawisz menu przywraca sam wygląd. Reset rozgrywki i reset okna są niezależne.

Przeciąganie rozmiaru korzysta z pozycji myszy względem ekranu i zachowuje przechwycenie do zwolnienia przycisku, także poza oknem. Tekstowe pola moda mają własne nazwy, aby rozpoznawać ich fokus. Kliknięcie poza oknem pozwala wrócić do gry. MenuAxisInputPatch obejmuje GetAxis(int), GetAxisRaw(int), GetCameraZoom() i GetCameraRotation(); getter zoomu musi być osobno objęty, ponieważ czyta Rewired bezpośrednio.

58 testów logiki i 92 kontrole API/IL przechodzi. Testy geometrii/kontrastu i kompilacja nie uruchamiają Unity i nie potwierdzają wyglądu w grze. Do ręcznej kontroli: kolory ciemny/jasny/RGB, alpha 0/50/100%, tekst 12/20 px, mały/duży rozmiar, przeciąganie poza krawędź, zmiana rozdzielczości, Ctrl+F8, zapis/restart i kółko bez zoomowania gry.

## Język i udźwig w 1.3.0

`Menu.Jezyk` przechowuje enum `Polski`/`English`. Dotychczasowe sekcje i klucze konfiguracji pozostają identyczne; nie są tłumaczone. Komunikaty przechowują klucz źródłowy i argumenty zamiast gotowego tekstu. Dzięki temu przełączenie języka zmienia także istniejący wynik dodawania punktów, odkrywania mapy i zakończonej kolejki zasobów. Nazwy przedmiotów i kategorii są nadal pobierane z natywnej lokalizacji gry. Testy sprawdzają zgodność argumentów obu katalogów i zmianę języka gotowych wyników; verify_repo.py sprawdza użyte klucze.

`Statek.Udzwig` jest mnożnikiem ×1–×100, a `Statek.BezLimituWagi` niezależnym przełącznikiem. Postfix `Engine.ReturnTownTugCapacity` skaluje wyłącznie świeżo obliczony wynik silnika osady gracza. Natywne `LateUpdate` odświeża cache, wolny udźwig, procent wagi i zdarzenia. Nie zmieniamy assetów silników, rzeczywistej wagi osady ani pojemności magazynów.

Tryb bez limitu pozostawia skończony zapas udźwigu co najmniej 1e9 dla natywnego licznika i omija samą kontrolę wagi w `Engine.CanTug`. Prefix wymaga istniejącego silnika osady gracza i poprawnie uruchomionej grupy patchy. Pozostałe wymagania w konstruktorach i podglądzie stawiania wykonuje gra. Skończone nasycanie obliczeń zapobiega Infinity/NaN. Zmiana mnożnika, reset i wyłączenie grupy uwzględniane są przy najbliższym natywnym przeliczeniu; wartości nie narastają między klatkami.

1.3.0: 43 testy logiki i 82 kontrole API/IL, kompilacja bez błędów i ostrzeżeń. Działanie nowych opcji w uruchomionej grze wymaga potwierdzenia użytkownika.

## Zgodność 1.2.0

- Assembly-CSharp.dll: SHA-256 `5b5c9f730ed71d5f66c0267064fc57667d2ff5644a018e9189b140ce025c1cd8`.
- Kompilacja: .NET SDK 8.0.427, BepInEx 5.4.21, referencje UnityEngine.Modules 2021.3.33.
- 27 testów logiki i 68 kontroli API/IL przeszło; kompilacja bez błędów i ostrzeżeń.
- **Działanie 1.2.0 w uruchomionej grze użytkownik potwierdził 8 listopada 2026.** Jest to zgłoszenie użytkownika, odrębne od kontroli automatycznych.
- Działanie poprzedniej DLL 1.1.0 użytkownik potwierdził 8 listopada 2026; paczki 1.0.0 i 1.1.0 pozostają dostępne.

## GitHub Actions i wersjonowanie

CI uruchamia testy logiki, sprawdza manifest i pakuje dołączoną DLL. Nie ma bibliotek gry, więc nie kompiluje pluginu ani nie wykonuje kontroli API gry. Wynik udostępnia jako artefakt. Tag wersji uruchamia istniejący workflow publikacji wydania.

Przy nowej wersji zmień projekt i BepInPlugin, dokumentację i release notes. Uruchom build.py; zatwierdź źródła, testy, manifest, DLL i ZIP. Manifest obejmuje wszystkie pliki C# z katalogu src.
