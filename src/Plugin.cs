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
using static Puchatek.FlotsamPlus.I18n;

namespace Puchatek.FlotsamPlus
{
    [BepInPlugin(Guid, "Flotsam Plus", "1.3.0")]
    [BepInProcess("Flotsam.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "pl.puchatek.flotsam.surowce-potrzeby";
        internal static Plugin Instance;
        internal ConfigEntry<bool> NoHunger;
        internal ConfigEntry<bool> NoThirst;
        internal ConfigEntry<bool> NoRest;
        internal ConfigEntry<bool> FreeTravel;
        internal ConfigEntry<bool> InstantBuild;
        internal ConfigEntry<float> ShipSpeed;
        internal ConfigEntry<float> ShipTurning;
        internal ConfigEntry<float> ShipAcceleration;
        internal ConfigEntry<float> ShipBraking;
        internal ConfigEntry<float> ResearchSpeed;
        internal ConfigEntry<float> ResidentSpeed;
        internal ConfigEntry<bool> AutoReveal;
        internal ConfigEntry<float> TownCapacity;
        internal ConfigEntry<bool> NoWeightLimit;
        internal bool CapacityReady;
        private ConfigEntry<UiLanguage> _language;
        internal bool ShipReady;
        internal bool PhysicsReady;
        internal bool EnergyReady;
        internal bool BuildingReady;
        internal bool ResearchReady;
        internal bool RestReady;
        internal bool ResidentsReady;
        private bool _pointsReady;
        private bool _mapReady;
        private ConfigEntry<KeyCode> _menuKey;
        private Harmony _harmony;
        private bool _open;
        private bool _patchesReady;
        private Rect _window = new Rect(30, 55, 590, 650);
        private Vector2 _scroll;
        private Vector2 _settingsScroll;
        private int _tab;
        private static readonly string[] Tabs = { "Statek", "Mieszkańcy", "Budowa i badania", "Surowce", "Mapa" };
        private GUIStyle _headerStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _messageStyle;
        private string _search = "";
        private string _filter = "";
        private string _amount = "100";
        private LocalizedMessage _message = M("Wybierz przedmiot, ustaw ilość i kliknij Dodaj.");
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
        private readonly List<Agent> _residents = new List<Agent>();
        private Agent _selectedResident;
        private Vector2 _residentsScroll;
        private string _pointAmount = "5";
        private LocalizedMessage _residentMessage = M("Wybierz mieszkańca, aby dodać punkty umiejętności.");
        private LocalizedMessage _mapMessage = M("Odkrywanie obejmuje wszystkie wygenerowane części bieżącego świata.");
        private float _nextMapReveal;

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
            NoRest = Config.Bind("Potrzeby", "WylaczSen", false,
                "Wyłącza nocne projekty snu mieszkańców Twojej osady.");
            FreeTravel = Config.Bind("Statek", "PodrozBezEnergii", false,
                "Ruch osady nie zużywa energii; pozostałe urządzenia działają normalnie.");
            InstantBuild = Config.Bind("Budowa", "NatychmiastowaBudowa", false,
                "Nowe budynki, łodzie, dekoracje i ulepszenia powstają od razu, bez surowców.");
            ShipSpeed = BindMultiplier("Statek", "Predkosc", 5f);
            ShipTurning = BindMultiplier("Statek", "Zwrotnosc", 5f);
            ShipAcceleration = BindMultiplier("Statek", "Przyspieszenie", 5f);
            ShipBraking = BindMultiplier("Statek", "Hamowanie", 5f);
            ResearchSpeed = BindMultiplier("Badania", "SzybkoscBadan", 100f);
            ResidentSpeed = BindMultiplier("Mieszkancy", "PredkoscRuchu", 5f);
            AutoReveal = Config.Bind("Mapa", "AutomatyczneOdkrywanie", false,
                "Odkrywa dostępną mapę oraz kolejne części świata po ich wygenerowaniu.");
            // Keep all existing section/key names stable across UI languages.
            TownCapacity = BindMultiplier("Statek", "Udzwig", WeightCapacity.MaximumMultiplier);
            NoWeightLimit = Config.Bind("Statek", "BezLimituWagi", false,
                "Disable the town weight limit. Storage space and other placement rules stay native.");
            _language = Config.Bind("Menu", "Jezyk", UiLanguage.Polski,
                "Menu language: Polski (PL) or English (EN).");
            I18n.Language = _language.Value;
            _menuKey = Config.Bind("Menu", "Klawisz", KeyCode.F8,
                "Klawisz otwierający i zamykający menu dodatku.");
            _harmony = new Harmony(Guid);
            _patchesReady = PatchGroup("Potrzeby", typeof(HungerThirstIncreasePatch),
                typeof(LoadedNeedsPatch), typeof(ReserveConsumablesPatch),
                typeof(StartConsumeProjectPatch), typeof(RetryConsumeProjectPatch));
            PatchGroup("Menu", typeof(MenuInputPatch));
            ShipReady = PatchGroup("Prędkość i skręt", typeof(ShipSpeedPatch),
                typeof(ShipTurningPatch), typeof(PathMovementPatch));
            EnergyReady = PatchGroup("Energia podróży", typeof(TravelEnergyPatch),
                typeof(TravelCostPatch), typeof(TravelRangePatch), typeof(TravelCooldownPatch));
            PhysicsReady = PatchGroup("Przyspieszenie i hamowanie",
                typeof(ShipPhysicsPatch), typeof(ShipForcePatch));
            BuildingReady = PatchGroup("Budowanie", typeof(InstantBuildingPatch));
            ResearchReady = PatchGroup("Badania", typeof(ResearchSpeedPatch));
            RestReady = PatchGroup("Sen", typeof(RestNightPatch));
            ResidentsReady = PatchGroup("Ruch mieszkańców", typeof(ResidentSpeedPatch),
                typeof(ResidentNavMeshSpeedPatch));
            CapacityReady = PatchGroup("Town weight capacity", typeof(TownCapacityPatch),
                typeof(UnlimitedTownWeightPatch));
            _pointsReady = InitializeAction("Punkty umiejętności", ResidentTools.InitializePoints);
            _mapReady = InitializeAction("Odkrywanie mapy", MapTools.Initialize);
            Logger.LogInfo("Flotsam Plus 1.3.0 ready. Menu: " + _menuKey.Value);
        }

        private bool InitializeAction(string name, Action initialize)
        {
            try { initialize(); return true; }
            catch (Exception error)
            {
                Logger.LogError("Nie uruchomiono grupy: " + name + ". " + error);
                return false;
            }
        }

        private ConfigEntry<float> BindMultiplier(string section, string key, float maximum)
        {
            return Config.Bind(section, key, 1f, new ConfigDescription(
                "Mnożnik. 1 = normalna wartość gry.", new AcceptableValueRange<float>(1f, maximum)));
        }

        private bool PatchGroup(string name, params Type[] patchTypes)
        {
            try
            {
                foreach (Type type in patchTypes) _harmony.CreateClassProcessor(type).Patch();
                return true;
            }
            catch (Exception error)
            {
                // Partially installed groups remain inert because each callback
                // checks its group's ready flag. Other groups keep working.
                Logger.LogError("Nie uruchomiono grupy: " + name + ". " + error);
                return false;
            }
        }

        internal static bool IsPlayer(Community community)
        {
            return community != null && ReferenceEquals(community, Community.PlayerCommunity);
        }

        internal static bool IsPlayerEngine(Engine engine)
        {
            return engine != null && engine.Buildable != null && IsPlayer(engine.Buildable.Community);
        }

        internal static bool SuppressRest(Vital vital)
        {
            Plugin self = Instance;
            return self != null && self.RestReady && self.NoRest.Value && vital != null &&
                vital.VitalType == VitalType.Rest && vital.Agent != null &&
                vital.Agent.IsAlive && IsPlayer(vital.Agent.Community);
        }

        private void ResetShipSettings()
        {
            ShipSpeed.Value = ShipTurning.Value = ShipAcceleration.Value = ShipBraking.Value = 1f;
            FreeTravel.Value = false;
            TownCapacity.Value = 1f;
            NoWeightLimit.Value = false;
            ShipPhysics.RestoreAll();
        }

        private void Update()
        {
            I18n.Language = _language.Value;
            if (Input.GetKeyDown(_menuKey.Value))
            {
                _open = !_open;
                if (_open) _nextRefresh = 0;
            }
            Community current = Community.PlayerCommunity;
            if (!ReferenceEquals(current, _community))
            {
                ShipPhysics.RestoreAll();
                if (_batch != null && !_batch.Finished)
                    _batch.Cancel("Zmieniono osadę lub zamknięto zapis.");
                _community = current;
                _items.Clear();
                _filtered.Clear();
                _selected = null;
                _residents.Clear();
                _selectedResident = null;
                _residentMessage = M("Wybierz mieszkańca, aby dodać punkty umiejętności.");
                _mapMessage = M("Odkrywanie obejmuje wszystkie wygenerowane części bieżącego świata.");
                _nextMapReveal = 0;
                _nextRefresh = 0;
                if (current != null) ApplyNeedsNow();
            }
            if (_open && current != null && Time.unscaledTime >= _nextRefresh)
            {
                if (_items.Count == 0) RefreshItems();
                RefreshResidents();
                _nextRefresh = Time.unscaledTime + 2f;
            }
            if (_mapReady && AutoReveal.Value && current != null &&
                Time.unscaledTime >= _nextMapReveal && GameManager.WorldManager != null &&
                GameManager.WorldManager.World != null)
            {
                _nextMapReveal = Time.unscaledTime + 1f;
                RevealMap(true);
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
                _message = M("Wpisz ilość od 1 do 5000.");
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
            _message = M(_batch.Finished ? "Dodano {0} / {1} × {2}." : "Dodawanie: {0} / {1} × {2}.",
                _batch.Added, _batch.Requested, _batchItem.Name);
            if (_batch.FailureMessage != null)
                _message = M("{0} {1}", _message, _batch.FailureMessage);
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
                    if (_patchesReady && NoHunger.Value) Satisfy(agent.Vitals.Hunger, true);
                    if (_patchesReady && NoThirst.Value) Satisfy(agent.Vitals.Thirst, true);
                }
            }
            catch (Exception error) { ReportError("Nie udało się odświeżyć potrzeb.", error); }
        }

        private void ReportError(string message, Exception error)
        {
            _message = M("{0} Szczegóły: BepInEx/LogOutput.log.", M(message));
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
            _window.width = Mathf.Min(690, Mathf.Max(320, Screen.width - 20));
            _window.height = Mathf.Min(690, Mathf.Max(310, Screen.height - 30));
            _window.x = Mathf.Clamp(_window.x, 0, Mathf.Max(0, Screen.width - _window.width));
            _window.y = Mathf.Clamp(_window.y, 0, Mathf.Max(0, Screen.height - _window.height));
            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color;
            try
            {
                GUI.enabled = true;
                GUI.color = Color.white;
                _window = GUI.Window(7408162, _window, DrawWindow, "Flotsam Plus 1.3.0");
            }
            finally { GUI.enabled = oldEnabled; GUI.color = oldColor; }
        }


        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(F("Menu: {0}  |  Poprzedni trainer: F9", _menuKey.Value), _smallStyle);
            if (GUILayout.Button(T("Zamknij"), GUILayout.Width(78))) _open = false;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Język:"), GUILayout.Width(65));
            int currentLanguage = _language.Value == UiLanguage.English ? 1 : 0;
            int nextLanguage = GUILayout.Toolbar(currentLanguage, new[] { "PL", "EN" }, GUILayout.Width(110));
            if (nextLanguage != currentLanguage)
            {
                _language.Value = nextLanguage == 1 ? UiLanguage.English : UiLanguage.Polski;
                I18n.Language = _language.Value;
            }
            GUILayout.EndHorizontal();
            int nextTab = GUILayout.Toolbar(_tab, Array.ConvertAll(Tabs, T), GUILayout.Height(30));
            if (nextTab != _tab) { _tab = nextTab; _settingsScroll = Vector2.zero; }
            GUILayout.Space(8);
            if (_tab == 3) DrawResources();
            else
            {
                _settingsScroll = GUILayout.BeginScrollView(_settingsScroll, GUILayout.ExpandHeight(true));
                if (_tab == 0) DrawShip();
                else if (_tab == 1) DrawResidents();
                else if (_tab == 4) DrawMap();
                else DrawBuildingAndResearch();
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(new Rect(0, 0, 5000, 20));
        }

        private void DrawShip()
        {
            GUILayout.Label(T("Statek i podróż"), _headerStyle);
            DrawToggle(NoWeightLimit, T(" Wyłącz limit wagi osady"), CapacityReady);
            DrawMultiplier(TownCapacity, T("Udźwig osady"), WeightCapacity.MaximumMultiplier,
                CapacityReady && !NoWeightLimit.Value);
            if (IsPlayer(_community) && _community.Engine != null)
                GUILayout.Label(F("Waga osady: {0:0}  |  Udźwig: {1}", Engine.TownWeight,
                    CapacityReady && NoWeightLimit.Value ? T("bez limitu") :
                    Engine.TownTugCapacity.ToString("0", I18n.Culture)), _smallStyle);
            else GUILayout.Label(T("Wczytaj zapis, aby zobaczyć wagę i udźwig."), _smallStyle);
            GUILayout.Label(T("Zwiększa udźwig, aby można było dalej budować. Nie powiększa magazynów. Zmiana działa po najbliższej klatce gry."), _smallStyle);
            GUILayout.Space(10);
            DrawMultiplier(ShipSpeed, T("Prędkość podróży"), 5f, ShipReady);
            DrawMultiplier(ShipTurning, T("Zwrotność / szybkość skrętu"), 5f, ShipReady);
            DrawMultiplier(ShipAcceleration, T("Przyspieszenie"), 5f, PhysicsReady);
            DrawMultiplier(ShipBraking, T("Hamowanie / mniejsza bezwładność"), 5f, PhysicsReady);
            DrawToggle(FreeTravel, T(" Podróż bez zużywania energii"), EnergyReady);
            GUILayout.Space(8);
            GUILayout.Label(T("Prędkość i skręt są niezależne. Hamowanie ogranicza dryf po puszczeniu sterowania."),
                _smallStyle);
            if (GUILayout.Button(T("Przywróć normalne ustawienia statku"), GUILayout.Height(30)))
                ResetShipSettings();
            DrawAvailability(ShipReady && PhysicsReady && EnergyReady && CapacityReady);
        }

        private void DrawResidents()
        {
            GUILayout.Label(T("Potrzeby mieszkańców"), _headerStyle);
            bool changed = DrawToggle(NoHunger, T(" Wyłącz głód"), _patchesReady);
            changed |= DrawToggle(NoThirst, T(" Wyłącz pragnienie"), _patchesReady);
            changed |= DrawToggle(NoRest, T(" Wyłącz nocny sen / odpoczynek"), RestReady);
            if (changed) ApplyNeedsNow();
            GUILayout.Space(8);
            GUILayout.Label(T("Opcje dotyczą mieszkańców Twojej osady. Nie rozpoczynają nocnego snu. Rozpoczęty wcześniej odpoczynek kończy się normalnie."),
                _smallStyle);
            GUILayout.Space(10);
            DrawMultiplier(ResidentSpeed, T("Szybkość chodzenia i pływania"), 5f, ResidentsReady);
            GUILayout.Label(T("Dotyczy żywych mieszkańców Twojej osady, także podczas chodzenia po wyspach."), _smallStyle);
            DrawAvailability(_patchesReady && RestReady && ResidentsReady);
            GUILayout.Space(12);
            DrawResidentPoints();
        }

        private void RefreshResidents()
        {
            _residents.Clear();
            if (!IsPlayer(_community)) return;
            foreach (Agent agent in _community.Agents)
                if (ResidentTools.IsResident(agent, _community) && agent.Attributes != null)
                    _residents.Add(agent);
            _residents.Sort((a, b) => string.Compare(a.Name, b.Name,
                StringComparison.CurrentCultureIgnoreCase));
            if (!_residents.Contains(_selectedResident)) _selectedResident = null;
        }

        private void DrawResidentPoints()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Punkty umiejętności"), _headerStyle);
            GUI.enabled = IsPlayer(_community);
            if (GUILayout.Button(T("Odśwież listę"), GUILayout.Width(110))) RefreshResidents();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (!IsPlayer(_community))
            {
                GUILayout.Label(T("Wczytaj zapis, aby wybrać mieszkańca."), _smallStyle);
                return;
            }
            _residentsScroll = GUILayout.BeginScrollView(_residentsScroll, GUI.skin.box,
                GUILayout.Height(125));
            foreach (Agent resident in _residents)
            {
                if (!ResidentTools.IsResident(resident, _community) || resident.Attributes == null) continue;
                Color before = GUI.backgroundColor;
                if (ReferenceEquals(resident, _selectedResident))
                    GUI.backgroundColor = new Color(.45f, .85f, .9f);
                if (GUILayout.Button(F("{0}  |  poziom {1}  |  punkty: {2}", resident.Name, resident.Attributes.Level,
                    resident.Attributes.SpendablePoints), GUILayout.Height(26)))
                    _selectedResident = resident;
                GUI.backgroundColor = before;
            }
            if (_residents.Count == 0) GUILayout.Label(T("Brak dostępnych mieszkańców."));
            GUILayout.EndScrollView();
            bool valid = ResidentTools.IsResident(_selectedResident, _community) &&
                _selectedResident.Attributes != null;
            GUILayout.Label(valid ? F("Wybrano: {0}  |  do rozdania: {1}", _selectedResident.Name,
                _selectedResident.Attributes.SpendablePoints) : T("Wybierz mieszkańca z listy."), _smallStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Punkty:"), GUILayout.Width(50));
            _pointAmount = GUILayout.TextField(_pointAmount, 3, GUILayout.Width(45));
            foreach (int amount in new[] { 1, 5, 10, 50, 100 })
                if (GUILayout.Button(amount.ToString(), GUILayout.Width(38))) _pointAmount = amount.ToString();
            GUI.enabled = _pointsReady && valid;
            if (GUILayout.Button(T("Dodaj punkty"), GUILayout.Height(28))) AddResidentPoints();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Box(_residentMessage.ToString(), _messageStyle);
            GUILayout.Label(T("Od 1 do 100 punktów na kliknięcie. Rozdaj je w zwykłym panelu postaci. Poziom i XP pozostają bez zmian; nadal obowiązują limity umiejętności gry."), _smallStyle);
            DrawAvailability(_pointsReady);
        }

        private void AddResidentPoints()
        {
            if (!_pointsReady) return;
            if (!int.TryParse(_pointAmount, out int amount) || amount < 1 || amount > PointGrant.Maximum)
            {
                _residentMessage = M("Wpisz od 1 do 100 punktów.");
                return;
            }
            try
            {
                int total = ResidentTools.AddPoints(_selectedResident, _community, amount);
                _residentMessage = M("Dodano {0} punktów dla: {1}. Do rozdania: {2}.", amount,
                    _selectedResident.Name, total);
            }
            catch (Exception error)
            {
                ReportError("Nie udało się dodać punktów umiejętności.", error);
                _residentMessage = _message;
            }
        }

        private void DrawMap()
        {
            GUILayout.Label(T("Odkrywanie mapy"), _headerStyle);
            GUILayout.Label(T("Odsłania mgłę i istniejące miejsca we wszystkich wygenerowanych częściach bieżącego świata."),
                _smallStyle);
            GUILayout.Space(8);
            GUI.enabled = _mapReady && IsPlayer(_community) && GameManager.WorldManager != null &&
                GameManager.WorldManager.World != null;
            if (GUILayout.Button(T("Odkryj całą dostępną mapę"), GUILayout.Height(34))) RevealMap(false);
            GUI.enabled = true;
            if (DrawToggle(AutoReveal, T(" Automatycznie odkrywaj także nowe obszary"), _mapReady))
                _nextMapReveal = 0;
            GUILayout.Space(10);
            GUILayout.Box(_mapMessage.ToString(), _messageStyle, GUILayout.MinHeight(50));
            GUILayout.Label(T("Świat powstaje etapami. Automatyczne odkrywanie obejmie kolejne obszary, gdy pojawią się podczas podróży. Zablokowane miejsca fabularne pozostają pod kontrolą gry."), _smallStyle);
            GUILayout.Space(8);
            GUILayout.Label(T("Odkryte miejsca i dodane punkty umiejętności zostają w zapisie. Wyłączenie opcji ani reset ich nie cofa."),
                _smallStyle);
            DrawAvailability(_mapReady);
        }

        private void RevealMap(bool automatic)
        {
            if (!_mapReady || !IsPlayer(_community)) return;
            try
            {
                if (GameManager.WorldManager == null || GameManager.WorldManager.World == null)
                    throw new InvalidOperationException(T("Najpierw wczytaj zapis gry."));
                _mapMessage = MapTools.Reveal(GameManager.WorldManager.World);
            }
            catch (Exception error)
            {
                if (automatic) AutoReveal.Value = false;
                ReportError("Nie udało się odkryć mapy.", error);
                _mapMessage = _message;
            }
        }

        private void DrawBuildingAndResearch()
        {
            GUILayout.Label(T("Budowa"), _headerStyle);
            DrawToggle(InstantBuild, T(" Natychmiastowa budowa i ulepszenia bez surowców"), BuildingReady);
            GUILayout.Label(T("Dotyczy nowych budynków, dekoracji, łodzi i rozpoczynanych ulepszeń. Już istniejące place budowy kończą pracę normalnie."), _smallStyle);
            GUILayout.Space(12);
            GUILayout.Label(T("Badania"), _headerStyle);
            DrawMultiplier(ResearchSpeed, T("Szybkość badań"), 100f, ResearchReady);
            GUILayout.Label(T("Przyspiesza pracę naukowców. Nadal potrzebne są punkty badań i aktywna stacja."),
                _smallStyle);
            DrawAvailability(BuildingReady && ResearchReady);
            GUILayout.Space(16);
            if (GUILayout.Button(T("Wyłącz wszystkie ułatwienia i przywróć mnożniki ×1"), GUILayout.Height(30)))
            {
                ResetShipSettings();
                NoHunger.Value = NoThirst.Value = NoRest.Value = InstantBuild.Value = false;
                ResearchSpeed.Value = 1f;
                ResidentSpeed.Value = 1f;
                AutoReveal.Value = false;
                ResidentTools.RefreshNavMeshes(_community);
            }
            GUILayout.Label(T("Reset nie cofa dodanych surowców i punktów umiejętności, odkrytej mapy, ukończonych budynków ani badań."),
                _smallStyle);
        }

        private bool DrawToggle(ConfigEntry<bool> setting, string label, bool ready)
        {
            GUI.enabled = ready;
            bool value = GUILayout.Toggle(setting.Value, label);
            GUI.enabled = true;
            if (!ready || value == setting.Value) return false;
            setting.Value = value;
            return true;
        }

        private void DrawMultiplier(ConfigEntry<float> setting, string label, float maximum, bool ready)
        {
            float current = Tuning.Multiplier(setting.Value, maximum);
            GUILayout.Space(5);
            GUILayout.Label(label + ": ×" + current.ToString("0.0", I18n.Culture), _smallStyle);
            GUI.enabled = ready;
            float value = GUILayout.HorizontalSlider(current, 1f, maximum);
            value = Mathf.Round(value * 10f) / 10f;
            GUILayout.BeginHorizontal();
            int[] presets = maximum > 5f ? new[] { 1, 5, 10, 50, 100 } : new[] { 1, 2, 3, 5 };
            foreach (int preset in presets)
                if (GUILayout.Button("×" + preset, GUILayout.Width(52))) value = preset;
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            if (ready && value != setting.Value) setting.Value = value;
        }

        private void DrawAvailability(bool ready)
        {
            if (!ready)
                GUILayout.Label(T("Część opcji nie uruchomiła się. Szczegóły w BepInEx/LogOutput.log."), _smallStyle);
        }

        private void DrawResources()
        {
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Dodawanie surowców"), _headerStyle);
            GUI.enabled = _community != null;
            if (GUILayout.Button(T("Odśwież"), GUILayout.Width(80))) RefreshItems();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (_community == null)
            {
                GUILayout.Box(T("Wczytaj zapis lub rozpocznij grę, aby dodać surowce."), _messageStyle,
                    GUILayout.ExpandHeight(true));
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Szukaj:"), GUILayout.Width(46));
            string search = GUILayout.TextField(_search);
            if (search != _search) { _search = search; Refilter(); }
            if (GUILayout.Button("×", GUILayout.Width(26))) { _search = ""; Refilter(); }
            GUILayout.EndHorizontal();
            GUILayout.Label(F("Przedmioty: {0} / {1}", _filtered.Count, _items.Count), _smallStyle);
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
            if (_filtered.Count == 0) GUILayout.Label(T("Brak pasujących przedmiotów."));
            GUILayout.EndScrollView();
            GUILayout.Label(_selected == null ? T("Wybierz przedmiot z listy.")
                : F("Wybrano: {0}  |  W magazynach: {1}", _selected.Name, _selectedCount), _smallStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Ilość:"), GUILayout.Width(62));
            _amount = GUILayout.TextField(_amount, 5, GUILayout.Width(65));
            foreach (int preset in new[] { 1, 10, 100, 1000 })
                if (GUILayout.Button(preset.ToString(), GUILayout.Width(49))) _amount = preset.ToString();
            GUILayout.FlexibleSpace();
            bool running = _batch != null && !_batch.Finished;
            GUI.enabled = _selected != null && !running;
            if (GUILayout.Button(T("Dodaj"), GUILayout.Width(76), GUILayout.Height(28))) BeginBatch();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Box(_message.ToString(), _messageStyle, GUILayout.MinHeight(48));
            if (running && GUILayout.Button(T("Przerwij dodawanie")))
            {
                _batch.Cancel("Dodawanie przerwane przez użytkownika.");
                UpdateBatchMessage();
            }
            GUILayout.Label(T("Limit jednej operacji: 5000. Surowce trafiają do pasujących magazynów z wolnym miejscem."), _smallStyle);
            GUILayout.Label(T("Nazwy surowców i kategorii pochodzą z języka gry."), _smallStyle);
        }

        private void OnDestroy()
        {
            CapacityReady = false;
            ResidentsReady = false;
            try { ResidentTools.RefreshNavMeshes(_community); }
            catch (Exception error) { Logger.LogError("Nie przywrócono szybkości nawigacji: " + error); }
            ShipPhysics.RestoreAll();
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
