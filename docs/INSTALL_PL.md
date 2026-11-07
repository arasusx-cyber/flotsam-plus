# Instalacja Flotsam Plus 1.1.0

## Instalacja i aktualizacja

1. Zamknij grę.
2. Wypakuj folder `BepInEx` z ZIP-a do katalogu z `Flotsam.exe`.
3. Przy aktualizacji nadpisz dotychczasową DLL w tym samym miejscu:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

4. Wczytaj zapis i naciśnij **F8**.

Nie zostawiaj dodatkowej kopii DLL pluginu w innym folderze BepInEx. Konfiguracja z 1.0.0 nadal jest używana.

Pierwsza instalacja wymaga Flotsam na Windows z `Flotsam_Data/Managed` i BepInEx 5.4.x x64. Jeśli go nie masz, zainstaluj [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21), uruchom grę raz i zamknij ją. Paczka pluginu nie zawiera loadera ani bibliotek gry.

## Statek

| Ustawienie | Zakres / działanie |
|---|---|
| Prędkość | ×1–×5; niezależna od skrętu |
| Zwrotność | ×1–×5; szybkość obrotu |
| Przyspieszenie | ×1–×5; mocniejsza reakcja fizycznego napędu |
| Hamowanie | ×1–×5; mniejszy dryf po puszczeniu sterowania |
| Podróż bez energii | Nie pobiera energii za ruch; urządzenia nadal mają swoje koszty |
| Przywróć normalne ustawienia statku | Cztery mnożniki ×1 i normalne koszty podróży |

Prędkość i skręcanie obejmują sterowanie ręczne oraz trasę wyznaczoną na mapie. Gra ma też bezpośrednie tryby ruchu bez fazy rozpędzania: mnożnik przyspieszenia działa w trybie fizycznego napędu. Hamowanie dotyczy fizycznego dryfu po puszczeniu sterowania; ukończona trasa zatrzymuje się zgodnie z zasadami gry.

Opcja energii nie napełnia magazynów energii i nie zmienia działania generatorów. Zachowuje przeszkody, blokady fabularne i pozostałe ograniczenia świata.

## Mieszkańcy

Głód i pragnienie działają osobno. Włączenie od razu zaspokaja potrzebę i zatrzymuje jej narastanie. Rozpoczęte wcześniej jedzenie lub picie może zostać dokończone.

„Wyłącz nocny sen / odpoczynek” zapobiega nowym nocnym projektom snu. W tej wersji gry odpoczynek jest zadaniem nocnym, bez osobnego aktywnego licznika zmęczenia. Już śpiący mieszkańcy kończą sen normalnie, aby zachować prawidłowe wyjście z łóżek i domów.

Opcje obejmują mieszkańców osady gracza.

## Budowa i badania

Natychmiastowa budowa wykorzystuje wbudowane ścieżki gry dla nowych budynków, dekoracji, łodzi i ulepszeń. Nie pobiera materiałów i uwzględnia sprawdzanie ich dostępności przy stawianiu.

Nie przerywa istniejących zleceń budowy, transportu ani rozbiórki. Nowe zlecenia korzystają z ustawienia obowiązującego w momencie ich rozpoczęcia.

Szybkość badań: ×1–×100. Zmienia tempo pracy badacza w stacji. Potrzebne są normalne punkty badań, pracownik i aktywna stacja. Nie odblokowuje automatycznie całego drzewa.

## Surowce

Wybierz przedmiot, wpisz 1–5000 i kliknij „Dodaj”. Wyszukiwanie obejmuje nazwę w języku gry, nazwę wewnętrzną, kategorię i ID. Przedmioty fabularne są pomijane.

Surowce trafiają do pasujących magazynów z wolną pojemnością. Woda wymaga magazynu na płyny. Przy braku miejsca dodawanie zatrzymuje się i pokazuje rzeczywistą ilość. Zmiana zapisu lub osady przerywa operację.

## Reset i zapis

W zakładce „Budowa i badania” można wyłączyć wszystkie ułatwienia i przywrócić mnożniki ×1. Reset nie usuwa dodanych przedmiotów, nie rozbiera ukończonych budynków i nie cofa badań.

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

Wersja 1.1.0 jest skompilowana i sprawdzona automatycznie; nie wykonano jeszcze próby w uruchomionej grze. Paczka 1.0.0 pozostaje w repozytorium.

Aby odinstalować, zamknij grę i usuń `BepInEx/plugins/FlotsamPuchatekPlus`.
