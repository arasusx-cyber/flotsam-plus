using System;
using Puchatek.FlotsamPlus;

internal static class WindowChecks
{
    internal static void Run(Action<bool, string> check)
    {
        var p = new WindowPreferences();
        var normal = WindowLayout.Fit(p, 1680, 1050);
        check(normal.Width == 760f && normal.Height == 780f &&
            normal.X == 460f && normal.Y == 135f, "Default window is larger and centered");
        var moved = new WindowPreferences { X = 1800f, Y = 1000f, Width = 1000f, Height = 900f };
        var fit = WindowLayout.Fit(moved, 1280, 720);
        check(fit.X >= 0 && fit.Y >= 0 && fit.X + fit.Width <= 1280 && fit.Y + fit.Height <= 720,
            "A moved or oversized window remains entirely on-screen");
        float desiredWidth = moved.Width, desiredHeight = moved.Height;
        WindowLayout.Fit(moved, 640, 480);
        var expanded = WindowLayout.Fit(moved, 2560, 1440);
        check(moved.Width == desiredWidth && moved.Height == desiredHeight &&
            expanded.Width == desiredWidth && expanded.Height == desiredHeight,
            "A temporary resolution reduction preserves the chosen window size");

        bool allFit = true;
        int[,] screens = { { 1, 1 }, { 20, 40 }, { 320, 240 }, { 640, 480 },
            { 800, 600 }, { 1280, 720 }, { 1680, 1050 }, { 1920, 1080 }, { 3840, 2160 } };
        foreach (float width in new[] { -5f, 380f, 760f, 9000f, float.NaN, float.PositiveInfinity })
            for (int i = 0; i < screens.GetLength(0); i++)
            {
                var prefs = new WindowPreferences { Width = width, Height = width, X = float.NaN, Y = 9000f };
                WindowBounds bounds = WindowLayout.Fit(prefs, screens[i, 0], screens[i, 1]);
                allFit &= !float.IsNaN(bounds.X) && !float.IsNaN(bounds.Y) &&
                    !float.IsInfinity(bounds.Width) && !float.IsInfinity(bounds.Height) &&
                    bounds.Width > 0 && bounds.Height > 0 && bounds.X >= 0 && bounds.Y >= 0 &&
                    bounds.X + bounds.Width <= screens[i, 0] + .01f &&
                    bounds.Y + bounds.Height <= screens[i, 1] + .01f;
            }
        check(allFit, "Resolution and malformed-config cases cannot strand the window outside the display");
        var invalid = new WindowPreferences { Red = float.NaN, Green = -5f, Blue = 8f,
            Opacity = float.PositiveInfinity, Width = float.NaN, FontSize = int.MaxValue };
        invalid.Sanitize();
        check(invalid.Red == .07f && invalid.Green == 0f && invalid.Blue == 1f &&
            invalid.Opacity == .96f && invalid.Width == 760f && invalid.FontSize == 20,
            "Corrupt appearance values have finite, bounded fallbacks");
        check(WindowLayout.Columns(285f, 80f, 5) == 3 && WindowLayout.Rows(5, 3) == 2 &&
            WindowLayout.Columns(40f, 80f, 5) == 1,
            "Preset buttons wrap instead of forcing a wider window");
        check(WindowLayout.TabColumns(900f, 120f, 6) == 6 &&
            WindowLayout.TabColumns(500f, 120f, 6) == 3 &&
            WindowLayout.TabColumns(300f, 120f, 6) == 2,
            "Navigation uses balanced one-, two- and three-row arrangements");
        int rejected = 0;
        try { WindowLayout.Columns(200, 50, 0); } catch (ArgumentOutOfRangeException) { ++rejected; }
        try { WindowLayout.Rows(5, 0); } catch (ArgumentOutOfRangeException) { ++rejected; }
        check(rejected == 2, "Empty or zero-column layouts are rejected");

        var transparent = new MenuPalette(new WindowPreferences { Opacity = 0f });
        var opaque = new MenuPalette(new WindowPreferences { Opacity = 1f });
        check(transparent.Background.A == 0f && opaque.Background.A == 1f &&
            transparent.Control.A == 1f && transparent.Text.A == 1f &&
            transparent.Hover.A == 1f && transparent.Selected.A == 1f,
            "Transparency changes only the panel; text and controls retain full alpha");
        var light = new MenuPalette(new WindowPreferences { Red = 1f, Green = 1f, Blue = 1f });
        var dark = new MenuPalette(new WindowPreferences { Red = 0f, Green = 0f, Blue = 0f });
        check(light.Text.R == 0f && dark.Text.R == 1f,
            "Light backgrounds use dark text and dark backgrounds use light text");
        bool contrast = true;
        foreach (float r in new[] { 0f, .25f, .5f, .75f, 1f })
            foreach (float g in new[] { 0f, .25f, .5f, .75f, 1f })
                foreach (float b in new[] { 0f, .25f, .5f, .75f, 1f })
                {
                    var palette = new MenuPalette(new WindowPreferences { Red = r, Green = g, Blue = b });
                    contrast &= Contrast(palette.Background, palette.Text) >= 4.5 &&
                        Contrast(palette.Control, palette.ControlText) >= 4.5 &&
                        Contrast(palette.Hover, palette.HoverText) >= 4.5 &&
                        Contrast(palette.Selected, palette.SelectedText) >= 4.5;
                }
        check(contrast, "Foreground choices maintain contrast against each solid palette state");
        check(WindowLayout.CapturesInput(true, true, false, false, false, false, false) &&
            WindowLayout.CapturesInput(false, false, true, false, false, false, false),
            "Resizing and its mouse release stay captured outside or after closing the window");
        check(!WindowLayout.CapturesInput(true, false, false, false, false, true, true) &&
            WindowLayout.CapturesInput(true, false, false, false, true, true, true),
            "An outside click leaves the menu while an inside click stays captured");
        check(WindowLayout.CapturesInput(true, false, false, false, false, true, false) &&
            !WindowLayout.CapturesInput(false, false, false, false, false, true, false),
            "Only an open menu's text editing captures game keyboard input");
        var reset = new WindowPreferences();
        check(reset.X == -1f && reset.Y == -1f && reset.Opacity >= .95f && reset.FontSize == 14,
            "Emergency appearance reset restores a visible, centered default");
    }

    private static double Linear(float c) =>
        c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4);

    private static double Contrast(MenuColor a, MenuColor b)
    {
        double x = .2126 * Linear(a.R) + .7152 * Linear(a.G) + .0722 * Linear(a.B);
        double y = .2126 * Linear(b.R) + .7152 * Linear(b.G) + .0722 * Linear(b.B);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }
}

