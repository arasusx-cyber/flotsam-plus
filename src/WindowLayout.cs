using System;

namespace Puchatek.FlotsamPlus
{
    internal struct WindowBounds
    {
        internal float X, Y, Width, Height;
        internal WindowBounds(float x, float y, float width, float height)
        { X = x; Y = y; Width = width; Height = height; }
    }

    internal sealed class WindowPreferences
    {
        internal float Width = 760f, Height = 780f, X = -1f, Y = -1f;
        internal float Red = .07f, Green = .10f, Blue = .15f, Opacity = .96f;
        internal int FontSize = 14;

        internal void Sanitize()
        {
            Width = WindowLayout.Value(Width, 760f, 380f, 8192f);
            Height = WindowLayout.Value(Height, 780f, 360f, 8192f);
            X = WindowLayout.Value(X, -1f, -1f, 65536f);
            Y = WindowLayout.Value(Y, -1f, -1f, 65536f);
            Red = WindowLayout.Value(Red, .07f, 0f, 1f);
            Green = WindowLayout.Value(Green, .10f, 0f, 1f);
            Blue = WindowLayout.Value(Blue, .15f, 0f, 1f);
            Opacity = WindowLayout.Value(Opacity, .96f, 0f, 1f);
            FontSize = Math.Max(12, Math.Min(20, FontSize));
        }
    }

    internal static class WindowLayout
    {
        internal static float Value(float value, float fallback, float minimum, float maximum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) value = fallback;
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        // Projection does not overwrite the user's preferred size or position.
        // A temporary resolution reduction therefore does not shrink saved settings.
        internal static WindowBounds Fit(WindowPreferences p, int screenWidth, int screenHeight)
        {
            float sw = Math.Max(1, screenWidth), sh = Math.Max(1, screenHeight);
            float margin = Math.Min(8f, (Math.Min(sw, sh) - 1f) / 2f);
            float availableW = sw - margin * 2f, availableH = sh - margin * 2f;
            float width = Value(p.Width, 760f, Math.Min(380f, availableW), availableW);
            float height = Value(p.Height, 780f, Math.Min(360f, availableH), availableH);
            float x = p.X < 0f ? (sw - width) / 2f : p.X;
            float y = p.Y < 0f ? (sh - height) / 2f : p.Y;
            x = Value(x, (sw - width) / 2f, margin, sw - width - margin);
            y = Value(y, (sh - height) / 2f, margin, sh - height - margin);
            return new WindowBounds(x, y, width, height);
        }

        internal static bool CapturesInput(bool open, bool resizing, bool releasePending,
            bool releaseFrame, bool inside, bool editing, bool mouseDown)
        {
            return resizing || releasePending || releaseFrame ||
                (open && (inside || (editing && !mouseDown)));
        }

        internal static int Columns(float available, float minimum, int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            float width = Value(available, 1f, 1f, 65536f);
            float cell = Value(minimum, 80f, 1f, 65536f);
            return Math.Max(1, Math.Min(count, (int)(width / cell)));
        }

        internal static int TabColumns(float available, float minimum, int count)
        {
            int columns = Columns(available, minimum, count);
            if (columns >= count) return count;
            return columns >= 3 ? 3 : columns >= 2 ? 2 : 1;
        }

        internal static int Rows(int count, int columns)
        {
            if (count < 1 || columns < 1) throw new ArgumentOutOfRangeException();
            return (count - 1) / columns + 1;
        }
    }

    internal readonly struct MenuColor
    {
        internal readonly float R, G, B, A;
        internal MenuColor(float r, float g, float b, float a = 1f)
        { R = r; G = g; B = b; A = a; }

        internal static MenuColor Blend(MenuColor a, MenuColor b, float mix)
        {
            return new MenuColor(a.R + (b.R - a.R) * mix,
                a.G + (b.G - a.G) * mix, a.B + (b.B - a.B) * mix);
        }

        private static double Linear(float channel)
        {
            return channel <= .04045f ? channel / 12.92 : Math.Pow((channel + .055) / 1.055, 2.4);
        }

        internal static MenuColor TextFor(MenuColor background)
        {
            double luminance = .2126 * Linear(background.R) +
                .7152 * Linear(background.G) + .0722 * Linear(background.B);
            bool black = (luminance + .05) / .05 >= 1.05 / (luminance + .05);
            return black ? new MenuColor(0f, 0f, 0f) : new MenuColor(1f, 1f, 1f);
        }
    }

    internal readonly struct MenuPalette
    {
        internal readonly MenuColor Background, Control, Hover, Selected;
        internal readonly MenuColor Text, ControlText, HoverText, SelectedText;
        internal MenuPalette(WindowPreferences preferences)
        {
            var p = new WindowPreferences { Red = preferences.Red, Green = preferences.Green,
                Blue = preferences.Blue, Opacity = preferences.Opacity };
            p.Sanitize();
            Background = new MenuColor(p.Red, p.Green, p.Blue, p.Opacity);
            Text = MenuColor.TextFor(Background);
            var toward = Text.R == 0f ? new MenuColor(1f, 1f, 1f) : new MenuColor(0f, 0f, 0f);
            Control = MenuColor.Blend(Background, toward, .15f);
            Hover = MenuColor.Blend(Control, Text, .12f);
            Selected = MenuColor.Blend(Control, Text, .25f);
            ControlText = MenuColor.TextFor(Control);
            HoverText = MenuColor.TextFor(Hover);
            SelectedText = MenuColor.TextFor(Selected);
        }
    }
}

