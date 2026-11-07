# Instalacja Flotsam Plus 1.0.0

## Wymagania

- Flotsam na Windows, wersja Mono z katalogiem `Flotsam_Data/Managed`.
- BepInEx 5.4.x x64, użyty przy pierwszej kompilacji: 5.4.21.

## Instalowanie

1. Zamknij grę.
2. Wypakuj folder `BepInEx` z paczki moda do katalogu z `Flotsam.exe`.
3. Docelowy plik to:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

4. Uruchom grę, wczytaj zapis i naciśnij **F8**.

Jeśli wcześniej nie było BepInEx, najpierw zainstaluj
[BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21),
uruchom grę raz i zamknij ją. Gotowa DLL nie wymaga kompilacji.

## Obsługa

- F8 pokazuje i chowa polskie menu dodatku.
- „Wyłącz głód” i „Wyłącz pragnienie” to osobne, zapamiętywane przełączniki.
- Domyślnie oba są wyłączone. Włączenie zaspokaja odpowiednią potrzebę.
- Zatrzymuje też nowe zadania jedzenia lub picia. Zadanie rozpoczęte
  wcześniej może jeszcze zostać dokończone.
- Wybierz przedmiot, podaj ilość od 1 do 5000 i kliknij „Dodaj”.
- Przyciski 1, 10, 100 i 1000 szybko ustawiają ilość.
- Wyszukiwarka obsługuje nazwę w języku gry, nazwę wewnętrzną, kategorię i ID.
- Przedmioty fabularne są pomijane.
- Przy pełnych magazynach operacja zatrzymuje się i pokazuje częściowy wynik.
- Woda wymaga odpowiedniego magazynu na płyny.
- Zmiana zapisu lub osady przerywa trwającą operację dodawania.

Jeśli używasz też trainera Hansjoy, jego menu nadal jest pod F9.

## Zapisy i odinstalowanie

Przed pierwszym użyciem skopiuj katalog zapisów:

```text
%USERPROFILE%/AppData/LocalLow/Pajama Llama Games/Flotsam/Saves
```

Dodane surowce i zaspokojone potrzeby mogą zostać utrwalone przy zapisie gry.
Odinstalowanie dodatku nie usuwa już dodanych przedmiotów.

Aby odinstalować, zamknij grę i usuń folder:

```text
BepInEx/plugins/FlotsamPuchatekPlus
```

## Konfiguracja i diagnoza

Konfiguracja powstaje po pierwszym uruchomieniu:

```text
BepInEx/config/pl.puchatek.flotsam.surowce-potrzeby.cfg
```

Można w niej zmienić klawisz menu oraz oba przełączniki potrzeb.
Jeśli F8 nie działa, sprawdź ścieżkę DLL i plik `BepInEx/LogOutput.log`.
W logu szukaj „Flotsam Plus” lub identyfikatora pluginu.
