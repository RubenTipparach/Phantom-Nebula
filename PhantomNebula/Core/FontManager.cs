using System;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Font manager - loads and manages fonts for the game
/// Generates font atlas and provides default font
/// </summary>
public static class FontManager
{
    private static Font defaultFont;
    private static bool fontLoaded = false;

    public static Font DefaultFont => defaultFont;

    /// <summary>
    /// Initialize font manager and load default font
    /// </summary>
    public static void Initialize()
    {
        if (fontLoaded)
            return;

        try
        {
            string fontPath = "Fonts/Aldrich-Regular.ttf";

            unsafe
            {
                fixed (byte* path = System.Text.Encoding.UTF8.GetBytes(fontPath + "\0"))
                {
                    if (FileExists((sbyte*)path))
                    {
                        Console.WriteLine($"[FontManager] Loading font from: {fontPath}");

                        // Load font with specified size and generate font atlas
                        // Size 32 creates a good quality atlas for various text sizes
                        defaultFont = LoadFontEx((sbyte*)path, 32, null, 0);

                        // Set texture filter to bilinear for smooth scaling
                        SetTextureFilter(defaultFont.Texture, TextureFilter.Bilinear);

                        fontLoaded = true;
                        Console.WriteLine("[FontManager] Font loaded successfully");
                        Console.WriteLine($"[FontManager] Font atlas size: {defaultFont.Texture.Width}x{defaultFont.Texture.Height}");
                        Console.WriteLine($"[FontManager] Glyph count: {defaultFont.GlyphCount}");
                    }
                    else
                    {
                        Console.WriteLine($"[FontManager] ERROR: Font file not found at {fontPath}");
                        Console.WriteLine("[FontManager] Using default Raylib font");
                        defaultFont = GetFontDefault();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FontManager] Failed to load font: {ex.Message}");
            Console.WriteLine("[FontManager] Using default Raylib font");
            defaultFont = GetFontDefault();
        }
    }

    /// <summary>
    /// Draw text using the default font
    /// </summary>
    public static void DrawText(string text, int posX, int posY, int fontSize, Color color)
    {
        if (!fontLoaded)
            Initialize();

        DrawTextEx(defaultFont, text, new System.Numerics.Vector2(posX, posY), fontSize, 1.0f, color);
    }

    /// <summary>
    /// Measure text width using the default font
    /// </summary>
    public static int MeasureText(string text, int fontSize)
    {
        if (!fontLoaded)
            Initialize();

        var size = MeasureTextEx(defaultFont, text, fontSize, 1.0f);
        return (int)size.X;
    }

    /// <summary>
    /// Unload font resources
    /// </summary>
    public static void Unload()
    {
        if (fontLoaded && defaultFont.Texture.Id != GetFontDefault().Texture.Id)
        {
            UnloadFont(defaultFont);
            fontLoaded = false;
            Console.WriteLine("[FontManager] Font unloaded");
        }
    }
}
