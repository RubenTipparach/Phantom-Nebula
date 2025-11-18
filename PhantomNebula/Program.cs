using Raylib_cs;
using PhantomNebula.Core;
using PhantomNebula.Scenes;
using System.Runtime.InteropServices.JavaScript;

namespace PhantomNebula
{
    public class Program
{
    // Game state - kept as static for WASM/JavaScript access
    private static StarfieldScene? scene;
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;

#if !WASM
    // Desktop entry point
    static void Main()
    {
        // Desktop initialization
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Phantom Nebula - Starfield Scene");
        Raylib.SetTargetFPS(60);

        // Initialize font manager
        FontManager.Initialize();

        // Initialize scene
        scene = new StarfieldScene();

        // Game loop (desktop only)
        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();

            scene.Update(deltaTime);

            Raylib.BeginDrawing();
            scene.Draw();
            Raylib.EndDrawing();
        }

        scene?.Dispose();
        FontManager.Unload();
        Raylib.CloseWindow();
    }
#else
    // WASM entry point - called once at startup
    public static void Main()
    {
        // Initialize Raylib window (safe on WASM startup)
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Phantom Nebula - Starfield Scene");
        Raylib.SetTargetFPS(60);

        // Initialize font manager
        FontManager.Initialize();

        // Initialize scene
        scene = new StarfieldScene();
    }

    // This is called from JavaScript in a game loop
    [JSExport]
    public static void UpdateFrame()
    {
        if (scene == null)
        {
            return;
        }

        float deltaTime = Raylib.GetFrameTime();
        scene.Update(deltaTime);

        Raylib.BeginDrawing();
        scene.Draw();
        Raylib.EndDrawing();
    }

    [JSExport]
    public static string GetGameStatus()
    {
        return scene != null ? "Game running" : "Game initializing...";
    }
#endif
    }
}
