# Flotsam Plus

Polskie menu **F8**: ustawienia statku, potrzeby mieszkańców, szybkie budowanie i badania oraz dodawanie surowców.

**[⬇ Pobierz Flotsam Plus 1.1.0](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.1.0.zip)** · [English](README.en.md) · [Instrukcja](docs/INSTALL_PL.md)

Windows x64 · BepInEx 5.4.x · Wersja 1.1.0

## Funkcje

| Zakładka | Opcje |
|---|---|
| Statek | Prędkość podróży ×1–×5, zwrotność ×1–×5, przyspieszenie i hamowanie ×1–×5, podróż bez zużywania energii |
| Mieszkańcy | Osobne przełączniki głodu, pragnienia i nocnego snu |
| Budowa i badania | Natychmiastowe nowe budowy i ulepszenia bez surowców; badania ×1–×100 |
| Surowce | Wyszukiwarka przedmiotów i dodawanie 1–5000 sztuk do pasujących magazynów |

Suwaki są niezależne. Przycisk przywraca normalne ustawienia statku; drugi reset wyłącza wszystkie ułatwienia. Ustawienia są zapamiętywane. Nowe opcje początkowo są wyłączone lub ustawione na ×1; wcześniejsze ustawienia głodu i pragnienia zostają zachowane.

Prędkość i skręt obejmują sterowanie ręczne oraz podróż po trasie. Przyspieszenie zmienia reakcję w trybie fizycznego napędu; bezpośredni ruch bez bezwładności nie wymaga rozpędzania. Hamowanie ogranicza dryf po puszczeniu sterowania.

Badania nadal wymagają punktów i stacji. Rozpoczęty wcześniej sen kończy się normalnie. Istniejące place budowy zachowują swoje zadania; opcja natychmiastowej budowy obejmuje nowe zlecenia.

## Aktualizacja z 1.0.0

1. Zamknij grę.
2. Wypakuj folder `BepInEx` z nowego ZIP-a do katalogu z `Flotsam.exe` i **nadpisz dotychczasową DLL**.
3. Wczytaj zapis i naciśnij **F8**.

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

Nie zostawiaj drugiej kopii tego samego pluginu pod inną nazwą. Przy pierwszej instalacji potrzebny jest [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21).

## Sprawdzenie wersji 1.1.0

Kompilacja: 0 błędów i 0 ostrzeżeń. Przeszło 21 testów kolejki/mnożników/resetu oraz 43 kontrole API i kodu IL względem dostarczonej DLL Flotsam. **Nowej wersji nie sprawdzono jeszcze w uruchomionej grze.**

[Poprzednia paczka 1.0.0](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.0.0.zip) pozostaje dostępna; użytkownik potwierdził działanie tej wersji.

[Obsługa i diagnoza](docs/INSTALL_PL.md) · [Rozwój i kompilacja](docs/DEVELOPMENT.md) · [Historia zmian](CHANGELOG.md)

Licencja [MIT](LICENSE). Zależności i autorzy: [NOTICE.md](NOTICE.md).
