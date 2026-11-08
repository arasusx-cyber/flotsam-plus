using System;
using System.Collections.Generic;
using System.Globalization;

namespace Puchatek.FlotsamPlus
{
    internal enum UiLanguage { Polski, English }

    // Stable Polish source keys keep UI text separate from the persisted config
    // keys. Messages retain their arguments so language changes also update old
    // status/results, without repeating an action.
    internal static class I18n
    {
        internal static UiLanguage Language = UiLanguage.Polski;
        internal static CultureInfo Culture => CultureInfo.GetCultureInfo(
            Language == UiLanguage.English ? "en-US" : "pl-PL");
        internal static readonly IReadOnlyDictionary<string, string> English =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Statek"] = "Ship",
            ["Mieszkańcy"] = "Residents",
            ["Budowa i badania"] = "Build & research",
            ["Surowce"] = "Resources",
            ["Mapa"] = "Map",
            ["Menu: {0}  |  Poprzedni trainer: F9"] = "Menu: {0}  |  Legacy trainer: F9",
            ["Zamknij"] = "Close",
            ["Język:"] = "Language:",
            ["Okno"] = "Window",
            ["Wygląd i rozmiar okna"] = "Window appearance and size",
            ["Awaryjny reset okna: Ctrl + {0}."] = "Emergency window reset: Ctrl + {0}.",
            ["Przezroczystość tła: {0:0}% (0 = pełne tło)"] = "Background transparency: {0:0}% (0 = solid background)",
            ["Przezroczystość dotyczy tylko tła, nie tekstu i przycisków."] = "Transparency affects only the background, not text or buttons.",
            ["Kolor tła"] = "Background color",
            ["Grafit"] = "Graphite",
            ["Granat"] = "Navy",
            ["Zieleń"] = "Green",
            ["Bordo"] = "Burgundy",
            ["Jasne"] = "Light",
            ["Rozmiar okna"] = "Window size",
            ["Małe"] = "Small",
            ["Standardowe"] = "Standard",
            ["Duże"] = "Large",
            ["Dopasuj do ekranu"] = "Fit to screen",
            ["Szerokość: {0:0} px"] = "Width: {0:0} px",
            ["Wysokość: {0:0} px"] = "Height: {0:0} px",
            ["Przeciągnij prawy dolny róg, aby zmienić rozmiar. Przesuwaj okno za pasek tytułu."] = "Drag the bottom-right corner to resize. Drag the title bar to move the window.",
            ["Rozmiar tekstu: {0} px"] = "Text size: {0} px",
            ["Własny kolor (RGB)"] = "Custom color (RGB)",
            ["Czerwony: {0:0}"] = "Red: {0:0}",
            ["Zielony: {0:0}"] = "Green: {0:0}",
            ["Niebieski: {0:0}"] = "Blue: {0:0}",
            ["Przywróć wygląd i położenie okna"] = "Reset window appearance and position",
            ["Ustawienia okna są zapamiętywane. Reset wyglądu nie zmienia ułatwień ani języka."] = "Window settings are saved. Resetting appearance keeps gameplay options and language.",
            ["Statek i podróż"] = "Ship and travel",
            ["Prędkość podróży"] = "Travel speed",
            ["Zwrotność / szybkość skrętu"] = "Turning speed",
            ["Przyspieszenie"] = "Acceleration",
            ["Hamowanie / mniejsza bezwładność"] = "Braking / reduced coasting",
            [" Podróż bez zużywania energii"] = " Travel without using energy",
            ["Prędkość i skręt są niezależne. Hamowanie ogranicza dryf po puszczeniu sterowania."] = "Speed and turning are independent. Braking reduces coasting after releasing the controls.",
            ["Przywróć normalne ustawienia statku"] = "Restore normal ship settings",
            ["Udźwig osady"] = "Town weight capacity",
            [" Wyłącz limit wagi osady"] = " Disable town weight limit",
            ["Waga osady: {0:0}  |  Udźwig: {1}"] = "Town weight: {0:0}  |  Capacity: {1}",
            ["bez limitu"] = "unlimited",
            ["Zwiększa udźwig, aby można było dalej budować. Nie powiększa magazynów. Zmiana działa po najbliższej klatce gry."] = "Raises the weight capacity so you can keep building. Storage space stays separate. Changes apply on the next game frame.",
            ["Wczytaj zapis, aby zobaczyć wagę i udźwig."] = "Load a save to see the town weight and capacity.",
            ["Potrzeby mieszkańców"] = "Resident needs",
            [" Wyłącz głód"] = " Disable hunger",
            [" Wyłącz pragnienie"] = " Disable thirst",
            [" Wyłącz nocny sen / odpoczynek"] = " Disable nighttime sleep / rest",
            ["Opcje dotyczą mieszkańców Twojej osady. Nie rozpoczynają nocnego snu. Rozpoczęty wcześniej odpoczynek kończy się normalnie."] = "These options affect your town's residents. They do not start nighttime sleep. Rest already in progress finishes normally.",
            ["Szybkość chodzenia i pływania"] = "Walking and swimming speed",
            ["Dotyczy żywych mieszkańców Twojej osady, także podczas chodzenia po wyspach."] = "Affects your town's living residents, including movement on islands.",
            ["Punkty umiejętności"] = "Skill points",
            ["Odśwież listę"] = "Refresh list",
            ["Wczytaj zapis, aby wybrać mieszkańca."] = "Load a save to select a resident.",
            ["{0}  |  poziom {1}  |  punkty: {2}"] = "{0}  |  level {1}  |  points: {2}",
            ["Brak dostępnych mieszkańców."] = "No residents available.",
            ["Wybrano: {0}  |  do rozdania: {1}"] = "Selected: {0}  |  unspent points: {1}",
            ["Wybierz mieszkańca z listy."] = "Select a resident from the list.",
            ["Punkty:"] = "Points:",
            ["Dodaj punkty"] = "Add points",
            ["Wybierz mieszkańca, aby dodać punkty umiejętności."] = "Select a resident to add skill points.",
            ["Od 1 do 100 punktów na kliknięcie. Rozdaj je w zwykłym panelu postaci. Poziom i XP pozostają bez zmian; nadal obowiązują limity umiejętności gry."] = "Add 1–100 points per click and spend them in the normal character panel. Level, XP and native skill caps stay unchanged.",
            ["Wpisz od 1 do 100 punktów."] = "Enter between 1 and 100 points.",
            ["Dodano {0} punktów dla: {1}. Do rozdania: {2}."] = "Added {0} points to {1}. Unspent points: {2}.",
            ["Nie udało się dodać punktów umiejętności."] = "Could not add skill points.",
            ["Odkrywanie mapy"] = "Map discovery",
            ["Odsłania mgłę i istniejące miejsca we wszystkich wygenerowanych częściach bieżącego świata."] = "Reveals fog and existing locations across all generated parts of the current world.",
            ["Odkryj całą dostępną mapę"] = "Reveal the entire available map",
            [" Automatycznie odkrywaj także nowe obszary"] = " Automatically reveal newly generated areas",
            ["Odkrywanie obejmuje wszystkie wygenerowane części bieżącego świata."] = "Discovery covers all generated parts of the current world.",
            ["Świat powstaje etapami. Automatyczne odkrywanie obejmie kolejne obszary, gdy pojawią się podczas podróży. Zablokowane miejsca fabularne pozostają pod kontrolą gry."] = "The world is generated in stages. Automatic discovery reveals new areas as they appear during travel. Locked story locations remain controlled by the game.",
            ["Odkryte miejsca i dodane punkty umiejętności zostają w zapisie. Wyłączenie opcji ani reset ich nie cofa."] = "Discovered locations and granted skill points stay in the save. Disabling options or resetting settings does not undo them.",
            ["Najpierw wczytaj zapis gry."] = "Load a game save first.",
            ["Nie udało się odkryć mapy."] = "Could not reveal the map.",
            ["Odkryto nowe miejsca: {0}. Odsłonięte fragmenty mapy: {1}. Dostępne części świata: {2}."] = "New locations revealed: {0}. Fog sections cleared: {1}. Available world tiles: {2}.",
            ["Budowa"] = "Construction",
            [" Natychmiastowa budowa i ulepszenia bez surowców"] = " Instant construction and upgrades without resources",
            ["Dotyczy nowych budynków, dekoracji, łodzi i rozpoczynanych ulepszeń. Już istniejące place budowy kończą pracę normalnie."] = "Affects new buildings, decorations, boats and newly started upgrades. Existing construction jobs finish normally.",
            ["Badania"] = "Research",
            ["Szybkość badań"] = "Research speed",
            ["Przyspiesza pracę naukowców. Nadal potrzebne są punkty badań i aktywna stacja."] = "Speeds up researchers. Research points and an active station are still required.",
            ["Wyłącz wszystkie ułatwienia i przywróć mnożniki ×1"] = "Disable all conveniences and reset multipliers to ×1",
            ["Reset nie cofa dodanych surowców i punktów umiejętności, odkrytej mapy, ukończonych budynków ani badań."] = "Reset does not undo added resources or skill points, map discovery, completed buildings or research.",
            ["Część opcji nie uruchomiła się. Szczegóły w BepInEx/LogOutput.log."] = "Some options failed to initialize. See BepInEx/LogOutput.log.",
            ["Dodawanie surowców"] = "Add resources",
            ["Odśwież"] = "Refresh",
            ["Wczytaj zapis lub rozpocznij grę, aby dodać surowce."] = "Load a save or start a game to add resources.",
            ["Szukaj:"] = "Search:",
            ["Przedmioty: {0} / {1}"] = "Items: {0} / {1}",
            ["Brak pasujących przedmiotów."] = "No matching items.",
            ["Wybierz przedmiot z listy."] = "Select an item from the list.",
            ["Wybrano: {0}  |  W magazynach: {1}"] = "Selected: {0}  |  In storage: {1}",
            ["Ilość:"] = "Amount:",
            ["Dodaj"] = "Add",
            ["Przerwij dodawanie"] = "Stop adding",
            ["Limit jednej operacji: 5000. Surowce trafiają do pasujących magazynów z wolnym miejscem."] = "Limit per operation: 5000. Items go into compatible storage with available space.",
            ["Nazwy surowców i kategorii pochodzą z języka gry."] = "Item and category names follow the game's language.",
            ["Wybierz przedmiot, ustaw ilość i kliknij Dodaj."] = "Select an item, set the amount and click Add.",
            ["Zmieniono osadę lub zamknięto zapis."] = "The town changed or the save was closed.",
            ["Dodawanie przerwane przez użytkownika."] = "Adding cancelled by the user.",
            ["Dodatek wyłączony."] = "Plugin disabled.",
            ["Wpisz ilość od 1 do 5000."] = "Enter an amount between 1 and 5000.",
            ["Dodano {0} / {1} × {2}."] = "Added {0} / {1} × {2}.",
            ["Dodawanie: {0} / {1} × {2}."] = "Adding: {0} / {1} × {2}.",
            ["{0} {1}"] = "{0} {1}",
            ["{0} Szczegóły: BepInEx/LogOutput.log."] = "{0} Details: BepInEx/LogOutput.log.",
            ["Brak wolnego miejsca albo odpowiedniego magazynu."] = "No available space or compatible storage.",
            ["Dodawanie przerwane: {0}"] = "Adding stopped: {0}",
            ["Nie udało się odczytać listy surowców."] = "Could not read the item list.",
            ["Nie udało się odświeżyć potrzeb."] = "Could not refresh resident needs.",
            ["Gra odrzuciła przedmiot. Sprawdź filtry magazynu."] = "The game rejected the item. Check the storage filters.",
            ["Nieznana wersja mapy i mgły wojny."] = "Unsupported map or fog-of-war version.",
            ["Nieznany licznik punktów umiejętności."] = "Unsupported skill-point counter.",
            ["Wybierz żywego mieszkańca bieżącej osady."] = "Select a living resident of the current town.",
        };

        internal static string T(string source)
        {
            return Language == UiLanguage.English && English.TryGetValue(source, out string text)
                ? text : source;
        }

        internal static string F(string source, params object[] args)
        {
            return string.Format(Culture, T(source), args);
        }

        internal static LocalizedMessage M(string source, params object[] args)
        {
            return new LocalizedMessage(source, args);
        }
    }

    internal sealed class LocalizedMessage
    {
        private readonly string _source;
        private readonly object[] _args;

        internal LocalizedMessage(string source, object[] args)
        {
            _source = source;
            _args = (object[])args.Clone();
        }

        public override string ToString() => I18n.F(_source, _args);
    }
}

