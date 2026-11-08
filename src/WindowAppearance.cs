using System;
using UnityEngine;

namespace Puchatek.FlotsamPlus
{
    // Private skin and six reusable one-pixel textures; no game assets are edited.
    internal sealed class WindowAppearance : IDisposable
    {
        private GUISkin _skin, _source;
        private Texture2D _panel, _control, _hover, _selected, _thumb, _track;
        private float _r, _g, _b, _opacity;
        private int _font;
        private bool _prepared;
        internal GUISkin Skin => _skin;

        internal bool Ensure(GUISkin source, WindowPreferences p)
        {
            if (_skin == null || _source != source)
            {
                Dispose();
                _source = source;
                _skin = UnityEngine.Object.Instantiate(source);
                _skin.hideFlags = HideFlags.HideAndDontSave;
                _skin.window = new GUIStyle(source.window);
                _skin.label = new GUIStyle(source.label);
                _skin.button = new GUIStyle(source.button);
                _skin.box = new GUIStyle(source.box);
                _skin.textField = new GUIStyle(source.textField);
                _skin.toggle = new GUIStyle(source.toggle);
                _skin.horizontalSlider = new GUIStyle(source.horizontalSlider);
                _skin.horizontalSliderThumb = new GUIStyle(source.horizontalSliderThumb);
                _skin.verticalScrollbar = new GUIStyle(source.verticalScrollbar);
                _skin.verticalScrollbarThumb = new GUIStyle(source.verticalScrollbarThumb);
                _panel = Create(); _control = Create(); _hover = Create();
                _selected = Create(); _thumb = Create(); _track = Create();
            }
            if (_prepared && _r == p.Red && _g == p.Green && _b == p.Blue &&
                _opacity == p.Opacity && _font == p.FontSize) return false;
            _r = p.Red; _g = p.Green; _b = p.Blue; _opacity = p.Opacity; _font = p.FontSize;
            var palette = new MenuPalette(p);
            Fill(_panel, palette.Background); Fill(_control, palette.Control);
            Fill(_hover, palette.Hover); Fill(_selected, palette.Selected);
            Fill(_thumb, palette.Text); Fill(_track, palette.Control);
            Paint(_skin.window, _panel, _panel, _panel, palette.Text, palette.Text, palette.Text);
            _skin.window.fontSize = p.FontSize + 2;
            _skin.window.fontStyle = FontStyle.Bold;
            _skin.window.alignment = TextAnchor.UpperCenter;
            _skin.window.padding = new RectOffset(8, 8, 5, 5);
            _skin.window.contentOffset = Vector2.zero;
            Paint(_skin.label, null, null, null, palette.Text, palette.Text, palette.Text);
            _skin.label.fontSize = p.FontSize;
            _skin.label.wordWrap = true;
            _skin.label.alignment = TextAnchor.MiddleLeft;
            _skin.label.padding = new RectOffset(0, 0, 2, 2);
            foreach (GUIStyle style in new[] { _skin.button, _skin.box, _skin.textField })
            {
                Paint(style, _control, _hover, _selected, palette.ControlText,
                    palette.HoverText, palette.SelectedText);
                style.fontSize = p.FontSize;
                style.fixedHeight = 0f;
                style.fixedWidth = 0f;
                style.padding = new RectOffset(8, 8, 5, 5);
                style.wordWrap = true;
            }
            _skin.button.alignment = TextAnchor.MiddleCenter;
            _skin.box.alignment = TextAnchor.MiddleLeft;
            _skin.textField.wordWrap = false;
            // Preserve the native checkbox icons instead of replacing them with
            // a solid rectangle; only its text and sizing are changed.
            foreach (GUIStyleState state in States(_skin.toggle)) state.textColor = Convert(palette.Text);
            _skin.toggle.fontSize = p.FontSize;
            _skin.toggle.wordWrap = true;
            _skin.toggle.fixedHeight = 0f;
            Paint(_skin.horizontalSlider, _track, _track, _track,
                palette.Text, palette.Text, palette.Text);
            _skin.horizontalSlider.fixedHeight = 8f;
            _skin.horizontalSlider.margin = new RectOffset(4, 4, 9, 9);
            _skin.horizontalSlider.padding = new RectOffset();
            Paint(_skin.horizontalSliderThumb, _thumb, _thumb, _thumb,
                palette.Text, palette.Text, palette.Text);
            _skin.horizontalSliderThumb.fixedWidth = 18f;
            _skin.horizontalSliderThumb.fixedHeight = 22f;
            _skin.horizontalSliderThumb.padding = new RectOffset();
            Paint(_skin.verticalScrollbar, _track, _track, _track,
                palette.Text, palette.Text, palette.Text);
            _skin.verticalScrollbar.fixedWidth = 18f;
            Paint(_skin.verticalScrollbarThumb, _thumb, _thumb, _thumb,
                palette.Text, palette.Text, palette.Text);
            _skin.verticalScrollbarThumb.fixedWidth = 18f;
            _prepared = true;
            return true;
        }

        private static GUIStyleState[] States(GUIStyle s) => new[] {
            s.normal, s.hover, s.active, s.focused, s.onNormal, s.onHover, s.onActive, s.onFocused };

        private static void Paint(GUIStyle style, Texture2D normal, Texture2D hover,
            Texture2D selected, MenuColor text, MenuColor hoverText, MenuColor selectedText)
        {
            style.border = new RectOffset();
            Set(style.normal, normal, text); Set(style.focused, normal, text);
            Set(style.hover, hover, hoverText); Set(style.active, selected, selectedText);
            Set(style.onNormal, selected, selectedText); Set(style.onFocused, selected, selectedText);
            Set(style.onHover, selected, selectedText); Set(style.onActive, selected, selectedText);
        }

        private static void Set(GUIStyleState state, Texture2D texture, MenuColor text)
        {
            state.background = texture;
            state.textColor = Convert(text);
        }

        private static Texture2D Create()
        {
            return new Texture2D(1, 1, TextureFormat.RGBA32, false) {
                hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private static Color Convert(MenuColor c) => new Color(c.R, c.G, c.B, c.A);
        private static void Fill(Texture2D texture, MenuColor color)
        {
            texture.SetPixel(0, 0, Convert(color));
            texture.Apply(false, false);
        }

        public void Dispose()
        {
            foreach (Texture2D texture in new[] { _panel, _control, _hover, _selected, _thumb, _track })
                if (texture != null) UnityEngine.Object.Destroy(texture);
            if (_skin != null) UnityEngine.Object.Destroy(_skin);
            _skin = _source = null;
            _panel = _control = _hover = _selected = _thumb = _track = null;
            _prepared = false;
        }
    }
}

