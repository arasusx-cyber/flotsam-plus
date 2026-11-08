using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Puchatek.FlotsamPlus;

internal static class LocalizationCapacityChecks
{
    internal static void Run(Action<bool, string> check)
    {
        UiLanguage previous = I18n.Language;
        try
        {
            I18n.Language = UiLanguage.Polski;
            var result = I18n.M("Dodano {0} punktów dla: {1}. Do rozdania: {2}.", 5, "Ada", 12);
            string polish = result.ToString();
            I18n.Language = UiLanguage.English;
            check(polish == "Dodano 5 punktów dla: Ada. Do rozdania: 12." &&
                result.ToString() == "Added 5 points to Ada. Unspent points: 12.",
                "A completed action changes language without being executed again");
            check(I18n.T("Statek") == "Ship" && I18n.T("Mieszkańcy") == "Residents" &&
                I18n.T("Surowce") == "Resources", "English navigation labels are available");
            check(I18n.T("Unknown native item") == "Unknown native item",
                "Native names and unknown keys are preserved");
            var full = new SpawnBatch(5);
            full.Step(() => false, 24);
            string englishFailure = full.Failure;
            I18n.Language = UiLanguage.Polski;
            check(englishFailure == "No available space or compatible storage." &&
                full.Failure == "Brak wolnego miejsca albo odpowiedniego magazynu.",
                "An existing full-storage result changes language live");
            var failed = new SpawnBatch(2);
            failed.Step(() => { throw new InvalidOperationException(
                "Gra odrzuciła przedmiot. Sprawdź filtry magazynu."); }, 24);
            var nested = I18n.M("{0} {1}", I18n.M("Dodano {0} / {1} × {2}.", 0, 2, "Wood"),
                failed.FailureMessage);
            I18n.Language = UiLanguage.English;
            check(nested.ToString().Contains("Added 0 / 2") &&
                nested.ToString().Contains("The game rejected the item."),
                "Nested batch results and known insertion errors are translated together");
            var cancelled = new SpawnBatch(4);
            cancelled.Step(() => true, 1);
            cancelled.Cancel("Zmieniono osadę lub zamknięto zapis.");
            check(cancelled.Failure == "The town changed or the save was closed." &&
                cancelled.Added == 1 && cancelled.Cancelled,
                "A translated cancellation preserves the completed count");
            bool formats = true, placeholders = true;
            foreach (var entry in I18n.English)
            {
                placeholders &= Fields(entry.Key).SequenceEqual(Fields(entry.Value));
                foreach (UiLanguage language in new[] { UiLanguage.Polski, UiLanguage.English })
                {
                    I18n.Language = language;
                    try { I18n.F(entry.Key, 3.5, 8.0, 12.0); }
                    catch (FormatException) { formats = false; }
                }
            }
            check(placeholders && formats, "Both language catalogs preserve every format argument");
            I18n.Language = UiLanguage.Polski;
            string plNumber = 2.5f.ToString("0.0", I18n.Culture);
            I18n.Language = UiLanguage.English;
            check(plNumber == "2,5" && 2.5f.ToString("0.0", I18n.Culture) == "2.5",
                "Number formatting follows the selected UI language");
            I18n.Language = (UiLanguage)99;
            check(I18n.T("Statek") == "Statek", "Invalid language values fall back to Polish");

            check(WeightCapacity.Calculate(2880f, 1f, false) == 2880f &&
                WeightCapacity.Calculate(float.MaxValue, 1f, false) == float.MaxValue,
                "The default preserves the exact native weight capacity");
            check(WeightCapacity.Calculate(2880f, 5f, false) == 14400f &&
                WeightCapacity.Calculate(2880f, 100f, false) == 288000f,
                "Town capacity supports fivefold and hundredfold limits");
            check(WeightCapacity.Calculate(2880f, float.NaN, false) == 2880f &&
                WeightCapacity.Calculate(2880f, float.PositiveInfinity, false) == 2880f &&
                WeightCapacity.Calculate(2880f, -2f, false) == 2880f &&
                WeightCapacity.Calculate(2880f, 1000f, false) == 288000f,
                "Malformed capacity multipliers are bounded safely");
            float unlimited = WeightCapacity.Calculate(2880f, 1f, true);
            check(!float.IsInfinity(unlimited) && !float.IsNaN(unlimited) && unlimited >= 1000000000f,
                "Unlimited mode supplies finite headroom for the native weight UI");
            check(WeightCapacity.Calculate(float.MaxValue, 100f, true) == float.MaxValue &&
                WeightCapacity.Calculate(0f, 1f, true) == WeightCapacity.UnlimitedReserve,
                "Large capacities saturate instead of overflowing and zero capacity stays finite");
            check(WeightCapacity.Calculate(2880f, 5f, true) >= unlimited &&
                WeightCapacity.Calculate(2880f, 5f, false) == 14400f &&
                WeightCapacity.Calculate(2880f, 1f, false) == 2880f,
                "Disabling unlimited or resetting computes from the native baseline");
            check(float.IsNaN(WeightCapacity.Calculate(float.NaN, 5f, false)) &&
                WeightCapacity.Calculate(-1f, 5f, true) == -1f,
                "Invalid native capacity is not disguised by a fabricated valid value");
        }
        finally { I18n.Language = previous; }
    }

    private static IEnumerable<string> Fields(string template)
    {
        return Regex.Matches(template, @"\{(\d+)(?:[^}]*)\}")
            .Cast<Match>().Select(m => m.Groups[1].Value).OrderBy(x => x);
    }
}

