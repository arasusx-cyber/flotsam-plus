using System;
using BepInEx.Configuration;
using UnityEngine;
using static Puchatek.FlotsamPlus.I18n;

namespace Puchatek.FlotsamPlus
{
    public sealed partial class Plugin
    {
        private WindowPreferences _windowPreferences;
        private WindowAppearance _appearance;
        private ConfigEntry<float> _windowWidth, _windowHeight, _windowX, _windowY;
        private ConfigEntry<float> _backgroundRed, _backgroundGreen, _backgroundBlue, _backgroundOpacity;
        private ConfigEntry<int> _windowFont;
        private bool _windowDirty, _resizing, _captureMouseRelease;
        private bool _editingMenuText, _releaseMenuFocus;
        private float _windowSaveAfter, _contentHeight;
        private int _blockInputUntilFrame = -1;
        private Vector2 _resizePointer, _resizeSize;
        private int MenuFontSize => _smallStyle == null ? 14 : _smallStyle.fontSize;

        private void InitializeWindowPreferences()
        {
            _windowWidth = BindWindowFloat("Szerokosc", 760f, 380f, 8192f);
            _windowHeight = BindWindowFloat("Wysokosc", 780f, 360f, 8192f);
            _windowX = BindWindowFloat("PozycjaX", -1f, -1f, 65536f);
            _windowY = BindWindowFloat("PozycjaY", -1f, -1f, 65536f);
            _backgroundRed = BindWindowFloat("KolorTlaR", .07f, 0f, 1f);
            _backgroundGreen = BindWindowFloat("KolorTlaG", .10f, 0f, 1f);
            _backgroundBlue = BindWindowFloat("KolorTlaB", .15f, 0f, 1f);
            _backgroundOpacity = BindWindowFloat("NieprzezroczystoscTla", .96f, 0f, 1f);
            _windowFont = Config.Bind("Okno", "RozmiarTekstu", 14,
                new ConfigDescription("Menu text size in pixels.", new AcceptableValueRange<int>(12, 20)));
            _windowPreferences = new WindowPreferences {
                Width = _windowWidth.Value, Height = _windowHeight.Value,
                X = _windowX.Value, Y = _windowY.Value,
                Red = _backgroundRed.Value, Green = _backgroundGreen.Value, Blue = _backgroundBlue.Value,
                Opacity = _backgroundOpacity.Value, FontSize = _windowFont.Value
            };
            _windowPreferences.Sanitize();
            _appearance = new WindowAppearance();
        }

        private ConfigEntry<float> BindWindowFloat(string key, float value, float minimum, float maximum)
        {
            return Config.Bind("Okno", key, value, new ConfigDescription(
                "Menu appearance/layout. Saved independently of gameplay options.",
                new AcceptableValueRange<float>(minimum, maximum)));
        }

        private static Vector2 ScreenPointer() =>
            new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        private void MarkWindowDirty()
        {
            _windowDirty = true;
            _windowSaveAfter = Time.unscaledTime + .4f;
        }

        private void SaveWindowPreferences()
        {
            if (!_windowDirty || _windowPreferences == null) return;
            try
            {
                _windowPreferences.Sanitize();
                _windowWidth.Value = _windowPreferences.Width; _windowHeight.Value = _windowPreferences.Height;
                _windowX.Value = _windowPreferences.X; _windowY.Value = _windowPreferences.Y;
                _backgroundRed.Value = _windowPreferences.Red; _backgroundGreen.Value = _windowPreferences.Green;
                _backgroundBlue.Value = _windowPreferences.Blue; _backgroundOpacity.Value = _windowPreferences.Opacity;
                _windowFont.Value = _windowPreferences.FontSize;
                _windowDirty = false;
            }
            catch (Exception error)
            {
                _windowSaveAfter = Time.unscaledTime + 5f;
                Logger.LogError("Could not save window preferences: " + error);
            }
        }

        private void ResetWindowPreferences()
        {
            _windowPreferences = new WindowPreferences();
            if (_resizing) _captureMouseRelease = true;
            _resizing = false;
            _blockInputUntilFrame = Time.frameCount;
            MarkWindowDirty();
            SaveWindowPreferences();
        }

        private void UpdateWindowPreferences()
        {
            if (_editingMenuText && (!_open || (Input.GetMouseButtonDown(0) &&
                !_window.Contains(ScreenPointer())))) _releaseMenuFocus = true;
            if (!_open && _resizing) { _resizing = false; _captureMouseRelease = true; }
            if (_captureMouseRelease && !Input.GetMouseButton(0))
            {
                _captureMouseRelease = false;
                _blockInputUntilFrame = Time.frameCount;
            }
            if (_open)
            {
                Vector2 pointer = ScreenPointer();
                if (_resizing)
                {
                    _windowPreferences.Width = WindowLayout.Value(
                        _resizeSize.x + pointer.x - _resizePointer.x, 760f, 380f, Math.Max(380f, Screen.width - 16f));
                    _windowPreferences.Height = WindowLayout.Value(
                        _resizeSize.y + pointer.y - _resizePointer.y, 780f, 360f, Math.Max(360f, Screen.height - 16f));
                    MarkWindowDirty();
                    if (!Input.GetMouseButton(0))
                    {
                        _resizing = false;
                        _blockInputUntilFrame = Time.frameCount;
                    }
                }
                else if (Input.GetMouseButtonDown(0) &&
                    new Rect(_window.xMax - 26, _window.yMax - 26, 26, 26).Contains(pointer))
                {
                    _resizing = true; _resizePointer = pointer; _resizeSize = _window.size;
                }
            }
            if (_windowDirty && (!_open || (!Input.GetMouseButton(0) && Time.unscaledTime >= _windowSaveAfter)))
                SaveWindowPreferences();
        }

        private bool CapturesWindowInput()
        {
            return WindowLayout.CapturesInput(_open, _resizing, _captureMouseRelease,
                Time.frameCount <= _blockInputUntilFrame, _window.Contains(ScreenPointer()),
                _editingMenuText, Input.GetMouseButtonDown(0));
        }

        private void OnGUI()
        {
            if (_releaseMenuFocus)
            {
                GUI.FocusControl(null);
                _releaseMenuFocus = _editingMenuText = false;
            }
            if (!_open) return;
            GUISkin oldSkin = GUI.skin;
            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color, oldBackground = GUI.backgroundColor, oldContent = GUI.contentColor;
            Matrix4x4 oldMatrix = GUI.matrix;
            try
            {
                // Apply layout/style changes at Layout so a mouse event cannot
                // change the number of GUILayout rows halfway through a frame.
                if (_appearance.Skin == null || Event.current.type == EventType.Layout)
                {
                    WindowBounds bounds = WindowLayout.Fit(_windowPreferences, Screen.width, Screen.height);
                    _window = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    if (_appearance.Ensure(oldSkin, _windowPreferences) || _headerStyle == null)
                    {
                        _headerStyle = new GUIStyle(_appearance.Skin.label) {
                            fontSize = _windowPreferences.FontSize + 2, fontStyle = FontStyle.Bold };
                        _smallStyle = new GUIStyle(_appearance.Skin.label) { wordWrap = true };
                        _messageStyle = new GUIStyle(_appearance.Skin.box) {
                            wordWrap = true, alignment = TextAnchor.MiddleLeft };
                    }
                }
                GUI.skin = _appearance.Skin;
                GUI.enabled = true;
                GUI.color = GUI.backgroundColor = GUI.contentColor = Color.white;
                GUI.matrix = Matrix4x4.identity;
                Rect before = _window;
                _window = GUI.Window(7408162, _window, DrawWindow, "Flotsam Plus 1.4.0");
                if (_window.x != before.x || _window.y != before.y)
                {
                    _windowPreferences.X = _window.x; _windowPreferences.Y = _window.y;
                    MarkWindowDirty();
                }
            }
            finally
            {
                GUI.skin = oldSkin; GUI.enabled = oldEnabled; GUI.color = oldColor;
                GUI.backgroundColor = oldBackground; GUI.contentColor = oldContent; GUI.matrix = oldMatrix;
            }
        }

        private void DrawWindow(int id)
        {
            float inner = Math.Max(1f, _window.width - 24f);
            float closeWidth = Math.Max(78f, MenuFontSize * 5f);
            float hintHeight = _smallStyle.CalcHeight(new GUIContent(
                F("Menu: {0}  |  Poprzedni trainer: F9", _menuKey.Value)), Math.Max(1f, inner - closeWidth - 12f));
            float rowHeight = Math.Max(MenuFontSize + 12f, hintHeight);
            float titleStrip = MenuFontSize + 18f;
            float headerHeight = rowHeight + MenuFontSize + 32f;
            GUILayout.BeginArea(new Rect(12, titleStrip, inner, headerHeight));
            GUILayout.BeginHorizontal();
            GUILayout.Label(F("Menu: {0}  |  Poprzedni trainer: F9", _menuKey.Value), _smallStyle);
            if (GUILayout.Button(T("Zamknij"), GUILayout.Width(closeWidth)))
            {
                _open = false; _blockInputUntilFrame = Time.frameCount;
                if (_editingMenuText) GUI.FocusControl(null);
                _editingMenuText = false;
                SaveWindowPreferences();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("Język:"), GUILayout.Width(MenuFontSize * 5f));
            int language = _language.Value == UiLanguage.English ? 1 : 0;
            int nextLanguage = GUILayout.Toolbar(language, new[] { "PL", "EN" }, GUILayout.Width(112));
            if (nextLanguage != language)
            {
                _language.Value = nextLanguage == 1 ? UiLanguage.English : UiLanguage.Polski;
                I18n.Language = _language.Value;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(T("Okno"), GUILayout.Width(MenuFontSize * 5f)))
            { _tab = 5; _settingsScroll = Vector2.zero; }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            var titles = new GUIContent[Tabs.Length];
            float cell = 1f;
            for (int i = 0; i < titles.Length; i++)
            {
                string title = _window.width < 620f && i == 2 ? T("Budowa") : T(Tabs[i]);
                titles[i] = new GUIContent(title, T(Tabs[i]));
                cell = Math.Max(cell, GUI.skin.button.CalcSize(titles[i]).x + 6f);
            }
            int columns = WindowLayout.TabColumns(inner, cell, titles.Length);
            float tabHeight = WindowLayout.Rows(titles.Length, columns) * (MenuFontSize + 16f);
            float top = titleStrip + headerHeight + 4f;
            int tab = GUI.SelectionGrid(new Rect(12, top, inner, tabHeight), _tab, titles, columns, GUI.skin.button);
            if (tab != _tab) { _tab = tab; _settingsScroll = Vector2.zero; }
            top += tabHeight + 8f;
            _contentHeight = Math.Max(1f, _window.height - top - 27f);
            GUILayout.BeginArea(new Rect(12, top, inner, _contentHeight));
            _settingsScroll = GUILayout.BeginScrollView(_settingsScroll, false, true);
            if (_tab == 0) DrawShip();
            else if (_tab == 1) DrawResidents();
            else if (_tab == 2) DrawBuildingAndResearch();
            else if (_tab == 3) DrawResources();
            else if (_tab == 4) DrawMap();
            else DrawWindowSettings();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.enabled = true;
            GUI.Label(new Rect(_window.width - 25, _window.height - 25, 24, 24), "◢", GUI.skin.button);
            GUI.DragWindow(new Rect(0, 0, _window.width, titleStrip - 2f));
            _editingMenuText = (GUI.GetNameOfFocusedControl() ?? "").StartsWith("FlotsamPlus.",
                StringComparison.Ordinal);
        }

        private int DrawButtonGrid(string[] labels, float minimumWidth = 62f)
        {
            int columns = WindowLayout.Columns(Math.Max(1f, _window.width - 66f), minimumWidth, labels.Length);
            return GUILayout.SelectionGrid(-1, labels, columns, GUI.skin.button,
                GUILayout.Height(WindowLayout.Rows(labels.Length, columns) * (MenuFontSize + 14f)));
        }

        private void DrawQuantity(ref string value, string label, int length, int[] presets)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(MenuFontSize * 5f));
            GUI.SetNextControlName("FlotsamPlus.Quantity");
            value = GUILayout.TextField(value, length, GUILayout.Width(90));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            int chosen = DrawButtonGrid(Array.ConvertAll(presets, p => p.ToString()),
                MenuFontSize * 3f + 22f);
            if (chosen >= 0) value = presets[chosen].ToString();
        }

        private void DrawWindowSettings()
        {
            GUILayout.Label(T("Wygląd i rozmiar okna"), _headerStyle);
            GUILayout.Label(F("Awaryjny reset okna: Ctrl + {0}.", _menuKey.Value), _smallStyle);
            float transparency = (1f - _windowPreferences.Opacity) * 100f;
            GUILayout.Label(F("Przezroczystość tła: {0:0}% (0 = pełne tło)", transparency), _smallStyle);
            float next = Mathf.Round(GUILayout.HorizontalSlider(transparency, 0f, 100f));
            string[] levels = { "0%", "25%", "50%", "75%", "100%" };
            int level = DrawButtonGrid(levels, 70);
            if (level >= 0) next = level * 25f;
            SetWindowValue(ref _windowPreferences.Opacity, 1f - next / 100f);
            GUILayout.Label(T("Przezroczystość dotyczy tylko tła, nie tekstu i przycisków."), _smallStyle);
            GUILayout.Space(10);
            GUILayout.Label(T("Kolor tła"), _headerStyle);
            string[] paletteNames = { T("Grafit"), T("Granat"), T("Zieleń"), T("Bordo"), T("Jasne") };
            int color = DrawButtonGrid(paletteNames, MenuFontSize * 6f + 20f);
            if (color >= 0)
            {
                float[,] colors = { { .09f, .10f, .12f }, { .07f, .10f, .15f },
                    { .06f, .16f, .11f }, { .19f, .07f, .09f }, { .88f, .90f, .93f } };
                SetWindowValue(ref _windowPreferences.Red, colors[color, 0]);
                SetWindowValue(ref _windowPreferences.Green, colors[color, 1]);
                SetWindowValue(ref _windowPreferences.Blue, colors[color, 2]);
            }
            GUILayout.Space(10);
            GUILayout.Label(T("Rozmiar okna"), _headerStyle);
            string[] sizes = { T("Małe"), T("Standardowe"), T("Duże"), T("Dopasuj do ekranu") };
            int size = DrawButtonGrid(sizes, MenuFontSize * 8f + 22f);
            if (size >= 0)
            {
                float[] widths = { 520f, 760f, 1000f, Math.Max(380f, Screen.width - 16f) };
                float[] heights = { 600f, 780f, 900f, Math.Max(360f, Screen.height - 16f) };
                SetWindowValue(ref _windowPreferences.Width, widths[size]);
                SetWindowValue(ref _windowPreferences.Height, heights[size]);
            }
            DrawWindowDimension(ref _windowPreferences.Width, "Szerokość: {0:0} px",
                _window.width, Math.Min(380f, Screen.width - 16f), Math.Max(1f, Screen.width - 16f));
            DrawWindowDimension(ref _windowPreferences.Height, "Wysokość: {0:0} px",
                _window.height, Math.Min(360f, Screen.height - 16f), Math.Max(1f, Screen.height - 16f));
            GUILayout.Label(T("Przeciągnij prawy dolny róg, aby zmienić rozmiar. Przesuwaj okno za pasek tytułu."), _smallStyle);
            GUILayout.Space(8);
            GUILayout.Label(F("Rozmiar tekstu: {0} px", _windowPreferences.FontSize), _smallStyle);
            int font = Mathf.RoundToInt(GUILayout.HorizontalSlider(_windowPreferences.FontSize, 12f, 20f));
            if (font != _windowPreferences.FontSize) { _windowPreferences.FontSize = font; MarkWindowDirty(); }
            GUILayout.Space(10);
            GUILayout.Label(T("Własny kolor (RGB)"), _headerStyle);
            DrawColorChannel(ref _windowPreferences.Red, "Czerwony: {0:0}");
            DrawColorChannel(ref _windowPreferences.Green, "Zielony: {0:0}");
            DrawColorChannel(ref _windowPreferences.Blue, "Niebieski: {0:0}");
            GUILayout.Space(10);
            if (GUILayout.Button(T("Przywróć wygląd i położenie okna"), GUILayout.MinHeight(32)))
                ResetWindowPreferences();
            GUILayout.Label(T("Ustawienia okna są zapamiętywane. Reset wyglądu nie zmienia ułatwień ani języka."), _smallStyle);
        }

        private void SetWindowValue(ref float current, float value)
        {
            if (Math.Abs(current - value) <= .0001f) return;
            current = value;
            MarkWindowDirty();
        }

        private void DrawColorChannel(ref float channel, string label)
        {
            GUILayout.Label(F(label, channel * 255f), _smallStyle);
            float current = channel * 255f;
            float value = GUILayout.HorizontalSlider(current, 0f, 255f);
            if (Math.Abs(value - current) > .001f)
                SetWindowValue(ref channel, Mathf.Round(value) / 255f);
        }

        private void DrawWindowDimension(ref float requested, string label, float actual, float minimum, float maximum)
        {
            GUILayout.Label(F(label, actual), _smallStyle);
            float value = Mathf.Round(GUILayout.HorizontalSlider(actual, Math.Max(1f, minimum), maximum));
            if (Math.Abs(value - actual) > .5f) SetWindowValue(ref requested, value);
        }

        private void DisposeWindowPreferences()
        {
            SaveWindowPreferences();
            _appearance?.Dispose();
        }
    }
}

