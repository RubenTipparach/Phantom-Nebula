using System;
using System.Numerics;
using PhantomNebula.Game;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Renders health bars for ITarget entities
/// Converts 3D world position to 2D screen position and renders floating health bar
/// </summary>
public class HealthBarRenderer
{
    private const float BarWidth = 60f;
    private const float BarHeight = 8f;
    private const float BarOffsetY = -15f; // Distance above the entity in screen space

    /// <summary>
    /// Draw health bar for a target above its position in world space
    /// </summary>
    public void DrawHealthBar(ITarget target, Camera3D camera, int screenWidth, int screenHeight)
    {
        // Only draw if entity is not dead
        if (target.IsDead)
            return;

        // Convert 3D world position to 2D screen position
        Vector3 targetWorldPos = target.Position;
        Vector2 screenPos = GetWorldToScreen(targetWorldPos, camera);

        // Check if position is on screen (with some margin)
        if (screenPos.X < -BarWidth || screenPos.X > screenWidth + BarWidth ||
            screenPos.Y < -BarHeight * 2 || screenPos.Y > screenHeight + BarHeight)
            return;

        // Offset above the entity
        screenPos.Y += BarOffsetY;

        // Draw background (red for low health)
        DrawHealthBarBackground(screenPos);

        // Draw health bar (green/yellow based on health)
        DrawHealthBarFill(screenPos, target.HealthPercent);

        // Draw border
        DrawHealthBarBorder(screenPos);

        // Draw target name and health text
        DrawHealthBarText(screenPos, target);
    }

    private void DrawHealthBarBackground(Vector2 position)
    {
        // Dark background
        Rectangle bgRect = new(
            position.X - BarWidth / 2,
            position.Y - BarHeight / 2,
            BarWidth,
            BarHeight
        );
        DrawRectangleRec(bgRect, Color.DarkGray);
    }

    private void DrawHealthBarFill(Vector2 position, float healthPercent)
    {
        // Clamp health percent to 0-1
        healthPercent = float.Clamp(healthPercent, 0f, 1f);

        // Determine color based on health
        Color healthColor = healthPercent > 0.5f
            ? Color.Green
            : healthPercent > 0.25f
                ? Color.Yellow
                : Color.Red;

        // Draw health bar fill
        Rectangle healthRect = new(
            position.X - BarWidth / 2,
            position.Y - BarHeight / 2,
            BarWidth * healthPercent,
            BarHeight
        );
        DrawRectangleRec(healthRect, healthColor);
    }

    private void DrawHealthBarBorder(Vector2 position)
    {
        // Draw border rectangle
        Rectangle borderRect = new(
            position.X - BarWidth / 2,
            position.Y - BarHeight / 2,
            BarWidth,
            BarHeight
        );
        DrawRectangleLinesEx(borderRect, 1.0f, Color.White);
    }

    private void DrawHealthBarText(Vector2 position, ITarget target)
    {
        // Display target name above health bar
        string nameText = target.TargetName;
        Vector2 namePos = new(position.X, position.Y - BarHeight / 2 - 20);

        // Approximate text width for centering (very rough estimate)
        float textWidth = nameText.Length * 5f;

        // Draw name text in white
        DrawText(nameText, (int)(namePos.X - textWidth / 2), (int)namePos.Y, 10, Color.White);

        // Display health values below bar
        string healthText = $"{target.CurrentHealth:F0}/{target.StartingHealth:F0}";
        Vector2 healthPos = new(position.X, position.Y + BarHeight / 2 + 2);

        // Approximate text width for centering
        float healthTextWidth = healthText.Length * 4f;

        // Draw health text in light gray
        DrawText(healthText, (int)(healthPos.X - healthTextWidth / 2), (int)healthPos.Y, 8, Color.LightGray);
    }
}
