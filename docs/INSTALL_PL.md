# Instalacja Flotsam Plus 1.4.0

## Instalacja i aktualizacja

1. Zamknij grę.
2. Wypakuj folder `BepInEx` z ZIP-a do katalogu z `Flotsam.exe`.
3. Przy aktualizacji nadpisz dotychczasową DLL w tym samym miejscu:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

4. Wczytaj zapis i naciśnij **F8**.

Nie zostawiaj dodatkowej kopii DLL pluginu w innym folderze BepInEx. Konfiguracja z 1.0.0, 1.1.0, 1.2.0 i 1.3.0 nadal jest używana. Nowe opcje początkowo są wyłączone lub ustawione na ×1.

Pierwsza instalacja wymaga Flotsam na Windows z `Flotsam_Data/Managed` i BepInEx 5.4.x x64. Jeśli go nie masz, zainstaluj [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21), uruchom grę raz i zamknij ją. Paczka pluginu nie zawiera loadera ani bibliotek gry.

## Język menu

W górnej części menu F8 wybierz **PL** lub **EN**. Zmiana jest natychmiastowa i zapamiętywana w istniejącym pliku konfiguracji: sekcja `[Menu]`, `Jezyk = Polski` lub `Jezyk = English`. Domyślnie pozostaje polski. Reset ułatwień nie zmienia języka.

Język obejmuje przyciski, zakładki, opisy i komunikaty, także wynik wcześniej wykonanej operacji. Nazwy surowców, kategorii i mieszkańców pochodzą z gry; język moda nie zmienia ustawienia języka samego Flotsam.

## Okno: kolor, prześwit i wielkość

Otwórz **F8 → Okno** albo kliknij „Okno” obok wyboru języka.

- **Przezroczystość tła:** 0% oznacza pełne tło, 100% całkowicie przezroczyste. Dostępne są suwak i gotowe wartości. Tekst i przyciski nie są przygaszane razem z tłem.
- **Kolor tła:** wybierz grafit, granat, zieleń, bordo lub jasne tło. Niżej ustawisz własny kolor suwakami RGB. Tekst automatycznie staje się jasny lub ciemny.
- **Rozmiar:** wybierz małe, standardowe lub duże okno albo dopasuj je do ekranu. Szerokość i wysokość mają osobne suwaki. Możesz też przeciągnąć znacznik w prawym dolnym rogu.
- **Przesuwanie:** przeciągaj pasek tytułu. Wielkość i położenie są ograniczane do aktualnego ekranu. Po zmianie rozdzielczości okno pozostaje w zasięgu; zapamiętany większy rozmiar wraca po powrocie do większej rozdzielczości.
- **Tekst:** zakres 12–20 px. Zakładki i gotowe wartości układają się w więcej rzędów w węższym oknie. Wszystkie zakładki można przewijać.

**Ctrl+F8** otwiera menu i przywraca domyślne tło, tekst, wielkość oraz środek ekranu. W „Okno” jest też przycisk resetu. Ułatwienia rozgrywki i język pozostają ustawione. Przy zmienionym klawiszu menu skrót to Ctrl + ten klawisz.

Wygląd i położenie zapisują się po zakończeniu regulacji, przy zamknięciu menu i przy wyłączeniu pluginu. Zwykły reset ułatwień nie resetuje wyglądu. Przewijanie nad menu blokuje jednoczesny zoom gry; kliknięcie poza oknem kończy wpisywanie w polu menu.

## Statek

| Ustawienie | Zakres / działanie |
|---|---|
| Udźwig osady | ×1–×100; zwiększa limit całkowitej wagi |
| Wyłącz limit wagi osady | Pozwala dalej budować mimo przekroczenia wagi |
| Prędkość | ×1–×5; niezależna od skrętu |
| Zwrotność | ×1–×5; szybkość obrotu |
| Przyspieszenie | ×1–×5; mocniejsza reakcja fizycznego napędu |
| Hamowanie | ×1–×5; mniejszy dryf po puszczeniu sterowania |
| Podróż bez energii | Nie pobiera energii za ruch; urządzenia nadal mają swoje koszty |
| Przywróć normalne ustawienia statku | Mnożniki ruchu i udźwigu ×1, limit wagi włączony, normalne koszty podróży |

Aby odblokować rozbudowę zablokowaną wagą, otwórz **F8 → Statek** i włącz **Wyłącz limit wagi osady** albo zwiększ **Udźwig osady**. Mnożnik jest nieaktywny, gdy działa opcja bez limitu; jego ustawienie pozostaje zapamiętane. Gra przelicza udźwig w najbliższej klatce, także po zmianie zapisu i po resecie.

Dotyczy to wagi całej osady, nie liczby miejsc w magazynach. Filtry magazynów, wymagania podłączenia, geometrii i odblokowania budynków pozostają pod kontrolą gry. Wyłączenie limitu nie zeruje rzeczywistej wagi osady i nie usuwa zbudowanych budynków; po powrocie do zwykłego udźwigu osada może znów być przeciążona.

Prędkość i skręcanie obejmują sterowanie ręczne oraz trasę wyznaczoną na mapie. Gra ma też bezpośrednie tryby ruchu bez fazy rozpędzania: mnożnik przyspieszenia działa w trybie fizycznego napędu. Hamowanie dotyczy fizycznego dryfu po puszczeniu sterowania; ukończona trasa zatrzymuje się zgodnie z zasadami gry.

Opcja energii nie napełnia magazynów energii i nie zmienia działania generatorów. Zachowuje przeszkody, blokady fabularne i pozostałe ograniczenia świata.

## Mieszkańcy

Głód i pragnienie działają osobno. Włączenie od razu zaspokaja potrzebę i zatrzymuje jej narastanie. Rozpoczęte wcześniej jedzenie lub picie może zostać dokończone.

„Wyłącz nocny sen / odpoczynek” zapobiega nowym nocnym projektom snu. W tej wersji gry odpoczynek jest zadaniem nocnym, bez osobnego aktywnego licznika zmęczenia. Już śpiący mieszkańcy kończą sen normalnie, aby zachować prawidłowe wyjście z łóżek i domów.

Opcje obejmują żywych mieszkańców osady gracza.

Szybkość chodzenia i pływania: ×1–×5. Obejmuje też chodzenie po wyspach. Zmiana nie podnosi atrybutu Athletics i nie zmienia prędkości statku ani samych łodzi.

W sekcji „Punkty umiejętności” wybierz mieszkańca z listy, wpisz 1–100 i kliknij „Dodaj punkty”. Licznik to prawdziwe punkty do rozdania w zwykłym panelu postaci. Poziom, XP i punkty badań pozostają bez zmian. Obowiązują normalne limity poziomu poszczególnych umiejętności. Dodane punkty zapisują się razem z grą.

Lista odświeża się co dwie sekundy, gdy menu jest otwarte; można odświeżyć ją ręcznie. Zmiana osady czy zapisu czyści wybór postaci. Martwy lub przeniesiony mieszkaniec nie otrzyma punktów.

## Budowa i badania

Natychmiastowa budowa wykorzystuje wbudowane ścieżki gry dla nowych budynków, dekoracji, łodzi i ulepszeń. Nie pobiera materiałów i uwzględnia sprawdzanie ich dostępności przy stawianiu.

Nie przerywa istniejących zleceń budowy, transportu ani rozbiórki. Nowe zlecenia korzystają z ustawienia obowiązującego w momencie ich rozpoczęcia.

Szybkość badań: ×1–×100. Zmienia tempo pracy badacza w stacji. Potrzebne są normalne punkty badań, pracownik i aktywna stacja. Nie odblokowuje automatycznie całego drzewa.

## Surowce

Wybierz przedmiot, wpisz 1–5000 i kliknij „Dodaj”. Wyszukiwanie obejmuje nazwę w języku gry, nazwę wewnętrzną, kategorię i ID. Przedmioty fabularne są pomijane.

Surowce trafiają do pasujących magazynów z wolną pojemnością. Woda wymaga magazynu na płyny. Przy braku miejsca dodawanie zatrzymuje się i pokazuje rzeczywistą ilość. Zmiana zapisu lub osady przerywa operację.

## Mapa

Przycisk „Odkryj całą dostępną mapę” odsłania mgłę i istniejące miejsca we wszystkich wygenerowanych częściach świata. Opcja „Automatycznie odkrywaj także nowe obszary” powtarza odkrywanie podczas podróży i obejmuje nowe obszary po ich wygenerowaniu.

Świat powstaje etapami. Funkcja nie wymusza generowania przyszłych biomów, wejścia do regionów ani wykonywania zadań zwiadowczych. Zablokowane miejsca fabularne pozostają pod kontrolą gry. Nie przesuwa osady i nie kończy aktywnych zadań.

Odkryta mgła i stany istniejących miejsc korzystają ze zwykłego zapisu gry. Wyłączenie automatycznego odkrywania nie ukrywa ich ponownie.

## Reset i zapis

W zakładce „Budowa i badania” można wyłączyć wszystkie ułatwienia i przywrócić mnożniki ×1. Reset nie usuwa dodanych przedmiotów i punktów umiejętności, nie ukrywa odkrytej mapy, nie rozbiera ukończonych budynków i nie cofa badań.

Przed zmianami zachowaj kopię zapisu. Katalog zapisów:

```text
%USERPROFILE%/AppData/LocalLow/Pajama Llama Games/Flotsam/Saves
```

## Konfiguracja, F9 i odinstalowanie

Ustawienia:

```text
BepInEx/config/pl.puchatek.flotsam.surowce-potrzeby.cfg
```

F8 obsługuje Flotsam Plus. Jeśli używasz trainera Hansjoy, jego F9 pozostaje dostępne. Reset Flotsam Plus nie wyłącza opcji ustawionych przez inny trainer.

Przy problemie sprawdź `BepInEx/LogOutput.log` i wpisy „Flotsam Plus”. W przypadku niezgodnej wersji gry jedna grupa opcji może pozostać niedostępna, a pozostałe działać dalej.

Wersja 1.4.0 przechodzi kompilację, 58 testów logiki i 92 kontrole API/IL. Nowe opcje wymagają jeszcze potwierdzenia w grze. Działanie wersji 1.2.0 i 1.1.0 użytkownik potwierdził 8 listopada 2026. Poprzednie paczki pozostają w repozytorium.

Aby odinstalować, zamknij grę i usuń `BepInEx/plugins/FlotsamPuchatekPlus`.
