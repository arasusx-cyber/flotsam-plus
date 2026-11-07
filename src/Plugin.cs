using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PajamaLlama.Fltsm;
using UnityEngine;

namespace Puchatek.FlotsamPlus
{
    [BepInPlugin(Guid, "Flotsam: surowce i potrzeby", "1.0.0")]
    [BepInProcess("Flotsam.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "pl.puchatek.flotsam.surowce-potrzeby";
        internal static Plugin Instance;
        internal ConfigEntry<bool> NoHunger;
        internal ConfigEntry<bool> NoThirst;
        private ConfigEntry<KeyCode> _menuKey;
        private Harmony _harmony;
        private bool _open;
        private bool _patchesReady;
        private Rect _window = new Rect(30, 55, 590, 650);
        private Vector2 _scroll;
        private GUIStyle _headerStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _messageStyle;
        private string _search = "";
        private string _filter = "";
        private string _amount = "100";
        private string _message = "Wybierz przedmiot, ustaw ilość i kliknij Dodaj.";
        private string _lastError;
        private readonly List<ResourceEntry> _items = new List<ResourceEntry>();
        private readonly List<ResourceEntry> _filtered = new List<ResourceEntry>();
        private ResourceEntry _selected;
        private Community _community;
        private Community _batchCommunity;
        private ResourceEntry _batchItem;
        private SpawnBatch _batch;
        private float _nextRefresh;
        private float _nextCountRefresh;
        private int _selectedCount;

        private sealed class ResourceEntry
        {
            public ItemProperties Properties;
            public string Name;
            public string Category;
            public string Search;
        }

        private void Awake()
        {
            Instance = this;
            NoHunger = Config.Bind("Potrzeby", "WylaczGlod", false,
                "Mieszkańcy Twojej osady nie potrzebują jedzenia.");
            NoThirst = Config.Bind("Potrzeby", "WylaczPragnienie", false,
                "Mieszkańcy Twojej osady nie potrzebują wody.");
            _menuKey = Config.Bind("Menu", "Klawisz", KeyCode.F8,
                "Klawisz otwierający i zamykający menu dodatku.");
            _harmony = new Harmony(Guid);
            try
            {
                _harmony.PatchAll(typeof(Plugin).Assembly);
                _patchesReady = true;
                Logger.LogInfo("Flotsam Plus 1.0.0 gotowy. Menu: " + _menuKey.Value);
            }
            catch (Exception error)
            {
                _harmony.UnpatchSelf();
                _message = "Nie udało się włączyć obsługi potrzeb. Sprawdź LogOutput.log.";
                Logger.LogError(error);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(_menuKey.Value))
            {
                _open = !_open;
                if (_open) _nextRefresh = 0;
            }
            Community current = Community.PlayerCommunity;
            if (!ReferenceEquals(current, _community))
            {
                if (_batch != null && !_batch.Finished)
                    _batch.Cancel("Zmieniono osadę lub zamknięto zapis.");
                _community = current;
                _items.Clear();
                _filtered.Clear();
                _selected = null;
                _nextRefresh = 0;
                if (current != null && _patchesReady) ApplyNeedsNow();
            }
            if (_open && current != null && Time.unscaledTime >= _nextRefresh)
            {
                if (_items.Count == 0) RefreshItems();
                _nextRefresh = Time.unscaledTime + 2f;
            }
            if (_batch != null && !_batch.Finished)
            {
                if (current == null || !ReferenceEquals(current, _batchCommunity))
                    _batch.Cancel("Zmieniono osadę lub zamknięto zapis.");
                else
                    _batch.Step(TryAddOne, 24);
                UpdateBatchMessage();
            }
            if (_open && _selected != null && current != null &&
                Time.unscaledTime >= _nextCountRefresh)
            {
                try { _selectedCount = current.Inventory.ReturnStoredItemCount(_selected.Properties, true); }
                catch { _selectedCount = 0; }
                _nextCountRefresh = Time.unscaledTime + .4f;
            }
        }

        internal static bool BlockGameplayInput()
        {
            Plugin self = Instance;
            if (self == null || !self._open) return false;
            return GUIUtility.keyboardControl != 0 || self._window.Contains(
                new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        }

        private void RefreshItems()
        {
            try
            {
                GameSettings settings = GameManager.Settings;
                if (settings == null || settings.ItemSettings == null ||
                    settings.ItemSettings.ItemProperties == null) return;
                ItemProperties previous = _selected == null ? null : _selected.Properties;
                _items.Clear();
                var seen = new HashSet<int>();
                foreach (ItemProperties properties in settings.ItemSettings.ItemProperties)
                {
                    if (properties == null || properties.Id < 0 || properties.IsQuestItem ||
                        !seen.Add(properties.Id)) continue;
                    string name;
                    try { name = properties.LocalizedName; }
                    catch { name = properties.name; }
                    if (string.IsNullOrWhiteSpace(name)) name = properties.name;
                    string category;
                    try { category = properties.ReturnCategory(); }
                    catch { category = ""; }
                    _items.Add(new ResourceEntry {
                        Properties = properties, Name = name, Category = category ?? "",
                        Search = Normalize(name + " " + properties.name + " " + category + " " + properties.Id)
                    });
                }
                _items.Sort((a, b) => string.Compare(a.Name, b.Name,
                    StringComparison.CurrentCultureIgnoreCase));
                _selected = _items.Find(i => ReferenceEquals(i.Properties, previous));
                Refilter();
                _lastError = null;
            }
            catch (Exception error) { ReportError("Nie udało się odczytać listy surowców.", error); }
        }

        private static string Normalize(string text)
        {
            string normalized = (text ?? "").Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c == 'ł' || c == 'Ł' ? 'l' : char.ToLowerInvariant(c));
            }
            return builder.ToString();
        }

        private void Refilter()
        {
            _filter = Normalize(_search);
            _filtered.Clear();
            foreach (ResourceEntry item in _items)
                if (_filter.Length == 0 || item.Search.Contains(_filter)) _filtered.Add(item);
            _scroll = Vector2.zero;
        }

        private bool TryAddOne()
        {
            if (_batchCommunity == null || _batchItem == null || GameManager.ResourceManager == null)
                return false;
            Item item = new Item(_batchItem.Properties);
            // Follow the game's own ItemDevTools route, with an extra capacity
            // check and an actual insertion check before reporting success.
            foreach (Storage storage in _batchCommunity.Storages)
            {
                if (storage == null || storage.Buildable == null ||
                    storage.Buildable.BuildPhase != BuildPhase.Finished || !storage.FitsItem(item))
                    continue;
                Inventory inventory = storage.Buildable.Inventory;
                if (inventory == null || !inventory.FitsInInventory(item, SubInventoryType.Storage))
                    continue;
                GameManager.ResourceManager.SpawnItemToInventory(inventory, item);
                if (ReferenceEquals(item.Inventory, inventory)) return true;
                // Stop after a failed native insertion rather than dispatching
                // the same object repeatedly to other storages.
                throw new InvalidOperationException("Gra odrzuciła przedmiot. Sprawdź filtry magazynu.");
            }
            return false;
        }

        private void BeginBatch()
        {
            if (_community == null || _selected == null || (_batch != null && !_batch.Finished)) return;
            int amount;
            if (!int.TryParse(_amount, out amount) || amount < 1 || amount > 5000)
            {
                _message = "Wpisz ilość od 1 do 5000.";
                return;
            }
            _batchCommunity = _community;
            _batchItem = _selected;
            _batch = new SpawnBatch(amount);
            _nextCountRefresh = 0;
            UpdateBatchMessage();
        }

        private void UpdateBatchMessage()
        {
            if (_batch == null || _batchItem == null) return;
            string prefix = _batch.Finished ? "Dodano " : "Dodawanie: ";
            _message = prefix + _batch.Added + " / " + _batch.Requested + " × " + _batchItem.Name;
            if (!string.IsNullOrEmpty(_batch.Failure)) _message += ". " + _batch.Failure;
        }

        internal static bool Suppressed(Vitals vitals, VitalType type)
        {
            Plugin self = Instance;
            if (self == null || !self._patchesReady || vitals == null || vitals.Agent == null) return false;
            if (!vitals.Agent.IsAlive || !ReferenceEquals(vitals.Agent.Community, Community.PlayerCommunity))
                return false;
            return (type == VitalType.Hunger && self.NoHunger.Value) ||
                   (type == VitalType.Thirst && self.NoThirst.Value);
        }

        internal static bool Suppressed(DietVital vital)
        {
            return vital != null && Suppressed(vital.Vitals, vital.VitalType);
        }

        internal static void Satisfy(DietVital vital, bool clearReservation)
        {
            if (vital == null) return;
            vital.Reset();
            if (clearReservation && vital.Diet != null) vital.Diet.ClearItemToConsume();
        }

        internal static void SatisfyEnabled(Vitals vitals)
        {
            if (vitals == null) return;
            if (Suppressed(vitals, VitalType.Hunger)) Satisfy(vitals.Hunger, false);
            if (Suppressed(vitals, VitalType.Thirst)) Satisfy(vitals.Thirst, false);
        }

        private void ApplyNeedsNow()
        {
            if (_community == null) return;
            try
            {
                foreach (Agent agent in _community.Agents)
                {
                    if (agent == null || !agent.IsAlive || agent.Vitals == null) continue;
                    if (NoHunger.Value) Satisfy(agent.Vitals.Hunger, true);
                    if (NoThirst.Value) Satisfy(agent.Vitals.Thirst, true);
                }
            }
            catch (Exception error) { ReportError("Nie udało się odświeżyć potrzeb.", error); }
        }

        private void ReportError(string message, Exception error)
        {
            _message = message + " Szczegóły: BepInEx/LogOutput.log.";
            if (_lastError != error.ToString()) Logger.LogError(error);
            _lastError = error.ToString();
        }

        private void OnGUI()
        {
            if (!_open) return;
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
                _smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
                _messageStyle = new GUIStyle(GUI.skin.box) { wordWrap = true, alignment = TextAnchor.MiddleLeft };
            }
            _window.width = Mathf.Min(590, Mathf.Max(320, Screen.width - 20));
            _window.height = Mathf.Min(650, Mathf.Max(310, Screen.height - 30));
            _window.x = Mathf.Clamp(_window.x, 0, Mathf.Max(0, Screen.width - _window.width));
            _window.y = Mathf.Clamp(_window.y, 0, Mathf.Max(0, Screen.height - _window.height));
            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color;
            try
            {
                GUI.enabled = true;
                GUI.color = Color.white;
                _window = GUI.Window(7408162, _window, DrawWindow, "Flotsam: surowce i potrzeby");
            }
            finally { GUI.enabled = oldEnabled; GUI.color = oldColor; }
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Menu: " + _menuKey.Value + "  |  Poprzedni trainer: F9", _smallStyle);
            if (GUILayout.Button("Zamknij", GUILayout.Width(78))) _open = false;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Potrzeby mieszkańców", _headerStyle);
            GUI.enabled = _patchesReady;
            bool noHunger = GUILayout.Toggle(NoHunger.Value, " Wyłącz głód");
            bool noThirst = GUILayout.Toggle(NoThirst.Value, " Wyłącz pragnienie");
            if (noHunger != NoHunger.Value || noThirst != NoThirst.Value)
            {
                NoHunger.Value = noHunger;
                NoThirst.Value = noThirst;
                ApplyNeedsNow();
            }
            GUI.enabled = true;
            GUILayout.Label(_patchesReady
                ? "Włączone opcje od razu zaspokajają potrzebę i zatrzymują jej narastanie."
                : "Obsługa potrzeb nie uruchomiła się. Sprawdź LogOutput.log.", _smallStyle);
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dodawanie surowców", _headerStyle);
            GUI.enabled = _community != null;
            if (GUILayout.Button("Odśwież", GUILayout.Width(80))) RefreshItems();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (_community == null)
            {
                GUILayout.Box("Wczytaj zapis lub rozpocznij grę, aby dodać surowce.", _messageStyle,
                    GUILayout.ExpandHeight(true));
                GUI.DragWindow(new Rect(0, 0, 5000, 20));
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Szukaj:", GUILayout.Width(46));
            string search = GUILayout.TextField(_search);
            if (search != _search) { _search = search; Refilter(); }
            if (GUILayout.Button("×", GUILayout.Width(26))) { _search = ""; Refilter(); }
            GUILayout.EndHorizontal();
            GUILayout.Label("Przedmioty: " + _filtered.Count + " / " + _items.Count, _smallStyle);
            _scroll = GUILayout.BeginScrollView(_scroll, GUI.skin.box, GUILayout.ExpandHeight(true));
            foreach (ResourceEntry entry in _filtered)
            {
                Color before = GUI.backgroundColor;
                if (ReferenceEquals(_selected, entry)) GUI.backgroundColor = new Color(.45f, .85f, .9f);
                string label = entry.Name + (string.IsNullOrEmpty(entry.Category) ? "" : "  ·  " + entry.Category);
                if (GUILayout.Button(label, GUILayout.Height(27)))
                {
                    _selected = entry;
                    _nextCountRefresh = 0;
                }
                GUI.backgroundColor = before;
            }
            if (_filtered.Count == 0) GUILayout.Label("Brak pasujących przedmiotów.");
            GUILayout.EndScrollView();
            GUILayout.Label(_selected == null ? "Wybierz przedmiot z listy."
                : "Wybrano: " + _selected.Name + "  |  W magazynach: " + _selectedCount, _smallStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ilość:", GUILayout.Width(40));
            _amount = GUILayout.TextField(_amount, 5, GUILayout.Width(65));
            foreach (int preset in new[] { 1, 10, 100, 1000 })
                if (GUILayout.Button(preset.ToString(), GUILayout.Width(49))) _amount = preset.ToString();
            GUILayout.FlexibleSpace();
            bool running = _batch != null && !_batch.Finished;
            GUI.enabled = _selected != null && !running;
            if (GUILayout.Button("Dodaj", GUILayout.Width(76), GUILayout.Height(28))) BeginBatch();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Box(_message, _messageStyle, GUILayout.MinHeight(48));
            if (running && GUILayout.Button("Przerwij dodawanie"))
            {
                _batch.Cancel("Dodawanie przerwane przez użytkownika.");
                UpdateBatchMessage();
            }
            GUILayout.Label("Limit jednej operacji: 5000. Surowce trafiają do pasujących magazynów z wolnym miejscem.", _smallStyle);
            GUI.DragWindow(new Rect(0, 0, 5000, 20));
        }

        private void OnDestroy()
        {
            if (_batch != null) _batch.Cancel("Dodatek wyłączony.");
            if (_harmony != null) _harmony.UnpatchSelf();
            if (ReferenceEquals(Instance, this)) Instance = null;
        }
    }

    [HarmonyPatch(typeof(DietVital), "IncreaseAmount")]
    internal static class HungerThirstIncreasePatch
    {
        private static bool Prefix(DietVital __instance)
        {
            if (!Plugin.Suppressed(__instance)) return true;
            Plugin.Satisfy(__instance, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(Vitals), "LateUpdate")]
    internal static class LoadedNeedsPatch
    {
        private static void Prefix(Vitals __instance) { Plugin.SatisfyEnabled(__instance); }
    }

    [HarmonyPatch(typeof(Vitals), "TryReserveItemToConsume")]
    internal static class ReserveConsumablesPatch
    {
        private static bool Prefix(Vitals __instance, VitalType vitalType, ref bool __result)
        {
            if (!Plugin.Suppressed(__instance, vitalType)) return true;
            Plugin.Satisfy(vitalType == VitalType.Hunger ? (DietVital)__instance.Hunger : __instance.Thirst, true);
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vitals), "TryAddConsumeProject")]
    internal static class StartConsumeProjectPatch
    {
        private static bool Prefix(Vitals __instance, VitalType vitalType, ref bool __result)
        {
            if (!Plugin.Suppressed(__instance, vitalType)) return true;
            Plugin.Satisfy(vitalType == VitalType.Hunger ? (DietVital)__instance.Hunger : __instance.Thirst, true);
            // The need is satisfied. Reporting success avoids the game's
            // failed-project list, hunger penalties and failure notifications.
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(DietVital), "RetryInstantiateProject")]
    internal static class RetryConsumeProjectPatch
    {
        private static bool Prefix(DietVital __instance, ref bool __result)
        {
            if (!Plugin.Suppressed(__instance)) return true;
            Plugin.Satisfy(__instance, true);
            __result = false;
            return false;
        }
    }

    // Prevent menu clicks and typed searches from triggering construction or
    // selection in the world. The addon hotkey uses Unity Input directly.
    [HarmonyPatch]
    internal static class MenuInputPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "GetButton", "GetButtonDown", "GetButtonUp", "GetButtonRepeating" })
                yield return AccessTools.Method(typeof(FlotsamInputManager), name, new[] { typeof(int) });
        }

        private static bool Prefix(ref bool __result)
        {
            if (!Plugin.BlockGameplayInput()) return true;
            __result = false;
            return false;
        }
    }
}
