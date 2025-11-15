using Raylib_cs;
using PhantomNebula.Scenes;

namespace PhantomNebula;

class Program
{
    static void Main()
    {
        // Initialize window
        const int screenWidth = 1280;
        const int screenHeight = 720;

        Raylib.InitWindow(screenWidth, screenHeight, "Phantom Nebula - Starfield Scene");
        Raylib.SetTargetFPS(60);

        // Initialize scene
        var scene = new StarfieldScene();

        // Game loop
        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();

            scene.Update(deltaTime);

            Raylib.BeginDrawing();
            scene.Draw();
            Raylib.EndDrawing();
        }

        scene.Dispose();
        Raylib.CloseWindow();
    }
}
