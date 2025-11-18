using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using PhantomNebula.Core;
using PhantomNebula.Game;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Renders targeting UI including bounding boxes, distance indicators, and target information.
/// </summary>
public class TargetingUIRenderer
{
    private const float BORDER_WIDTH = 2f;
    private const float CORNER_LENGTH = 15f;

    /// <summary>
    /// Renders the targeting UI for all targets, showing bounding boxes for all valid targets.
    /// Only draws UI if targets are in front of the camera.
    /// </summary>
    public void Draw(TargetingSystem targetingSystem, Vector3 playerPosition, Camera3D camera, List<ITarget> allTargets)
    {
        // Calculate camera forward vector once
        Vector3 cameraForward = Vector3.Normalize(camera.Target - camera.Position);

        // Get the current target (hovered or selected)
        ITarget? currentTarget = targetingSystem.HoveredTarget ?? targetingSystem.SelectedTarget;

        // Draw all targets with their bounding boxes
        foreach (ITarget target in allTargets)
        {
            // Skip dead targets
            if (target.IsDead)
                continue;

            // Get target screen position and bounds
            Vector2 targetScreenPos = GetWorldToScreen(target.Position, camera);
            const float boxWidth = 80f;
            const float boxHeight = 100f;

            Rectangle screenBounds = new(
                targetScreenPos.X - boxWidth / 2,
                targetScreenPos.Y - boxHeight / 2,
                boxWidth,
                boxHeight
            );

            // Check if target is behind the camera
            Vector3 dirToTarget = target.Position - camera.Position;
            if (Vector3.Dot(dirToTarget, cameraForward) < 0)
            {
                // Target is behind camera, skip
                continue;
            }

            // Determine if this target is hovered or selected
            bool isHighlighted = target == currentTarget;

            if (isHighlighted)
            {
                // Draw bright cyan highlight when hovered or selected
                DrawHighlightedTargetBoxForTarget(target, screenBounds, playerPosition);
            }
            else
            {
                // Draw dark blue box when not highlighted
                DrawTargetBoxForTarget(target, screenBounds, playerPosition);
            }

            // Draw health bar and name above target if hovered or selected
            if (isHighlighted)
            {
                DrawTargetHealthBar(target, screenBounds);
                DrawTargetName(target, screenBounds);
            }
        }
    }

    /// <summary>
    /// Draws the target bounding box in dark blue (when not highlighted).
    /// </summary>
    private void DrawTargetBoxForTarget(ITarget target, Rectangle bounds, Vector3 playerPosition)
    {
        Color boxColor = new(25, 50, 100, 200); // Dark blue

        // Draw corner brackets
        DrawTargetCorners(bounds, boxColor);

        // Draw distance text at the bottom of the box
        Vector3 dirToTarget = target.Position - playerPosition;
        float distance = dirToTarget.Length();
        string distanceText = $"{distance:F1}m";
        int textWidth = MeasureText(distanceText, 12);
        int textX = (int)(bounds.X + bounds.Width / 2 - textWidth / 2);
        int textY = (int)(bounds.Y + bounds.Height + 5);

        DrawText(distanceText, textX, textY, 12, boxColor);
    }

    /// <summary>
    /// Draws the target bounding box in bright cyan (when highlighted/hovered/selected).
    /// </summary>
    private void DrawHighlightedTargetBoxForTarget(ITarget target, Rectangle bounds, Vector3 playerPosition)
    {
        Color boxColor = new(0, 255, 255, 255); // Bright cyan

        // Draw corner brackets
        DrawTargetCorners(bounds, boxColor);

        // Draw distance text at the bottom of the box
        Vector3 dirToTarget = target.Position - playerPosition;
        float distance = dirToTarget.Length();
        string distanceText = $"{distance:F1}m";
        int textWidth = MeasureText(distanceText, 12);
        int textX = (int)(bounds.X + bounds.Width / 2 - textWidth / 2);
        int textY = (int)(bounds.Y + bounds.Height + 5);

        DrawText(distanceText, textX, textY, 12, boxColor);
    }

    /// <summary>
    /// Draws the target corner brackets to outline the bounding box.
    /// </summary>
    private void DrawTargetCorners(Rectangle bounds, Color color)
    {
        float x = bounds.X;
        float y = bounds.Y;
        float w = bounds.Width;
        float h = bounds.Height;

        // Top-left corner
        DrawLineEx(new Vector2(x, y), new Vector2(x + CORNER_LENGTH, y), BORDER_WIDTH, color);
        DrawLineEx(new Vector2(x, y), new Vector2(x, y + CORNER_LENGTH), BORDER_WIDTH, color);

        // Top-right corner
        DrawLineEx(new Vector2(x + w, y), new Vector2(x + w - CORNER_LENGTH, y), BORDER_WIDTH, color);
        DrawLineEx(new Vector2(x + w, y), new Vector2(x + w, y + CORNER_LENGTH), BORDER_WIDTH, color);

        // Bottom-left corner
        DrawLineEx(new Vector2(x, y + h), new Vector2(x + CORNER_LENGTH, y + h), BORDER_WIDTH, color);
        DrawLineEx(new Vector2(x, y + h), new Vector2(x, y + h - CORNER_LENGTH), BORDER_WIDTH, color);

        // Bottom-right corner
        DrawLineEx(new Vector2(x + w, y + h), new Vector2(x + w - CORNER_LENGTH, y + h), BORDER_WIDTH, color);
        DrawLineEx(new Vector2(x + w, y + h), new Vector2(x + w, y + h - CORNER_LENGTH), BORDER_WIDTH, color);
    }

    /// <summary>
    /// Draws detailed information about the selected target.
    /// Shows target name, health bar, and distance above the bounding box.
    /// </summary>
    private void DrawSelectedTargetInfo(TargetingSystem targetingSystem, Vector3 playerPosition)
    {
        ITarget? target = targetingSystem.SelectedTarget;
        if (target == null)
            return;

        // For selected target, draw info above where it would be on screen
        // We'll place it at a fixed offset from where the player "sees" it
        Vector3 dirToTarget = target.Position - playerPosition;
        float distToTarget = dirToTarget.Length();

        // Draw a panel showing target info
        const int panelWidth = 200;
        const int panelHeight = 60;
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();

        // Position panel at top-center
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = 100;

        // Draw panel background
        DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(20, 20, 40, 200));

        // Draw panel border
        DrawRectangleLinesEx(new Rectangle(panelX, panelY, panelWidth, panelHeight), 2, new Color(0, 255, 255, 255));

        // Draw target name
        FontManager.DrawText(target.TargetName, panelX + 10, panelY + 5, 14, new Color(0, 255, 255, 255));

        // Draw distance
        string distanceText = $"Distance: {distToTarget:F1}m";
        FontManager.DrawText(distanceText, panelX + 10, panelY + 22, 11, Color.White);

        // Draw health bar
        DrawSelectedTargetHealthBar(target, panelX + 10, panelY + 38);
    }

    /// <summary>
    /// Draws the target name above the health bar.
    /// </summary>
    private void DrawTargetName(ITarget target, Rectangle targetBounds)
    {
        // Position text above the health bar
        int x = (int)(targetBounds.X + (targetBounds.Width / 2));
        int y = (int)(targetBounds.Y - 40);

        // Draw target name centered
        int textWidth = MeasureText(target.TargetName, 12);
        FontManager.DrawText(target.TargetName, x - (textWidth / 2), y, 12, new Color(0, 255, 255, 255));
    }

    /// <summary>
    /// Draws the health bar for the target above the target bounding box.
    /// </summary>
    private void DrawTargetHealthBar(ITarget target, Rectangle targetBounds)
    {
        const int barWidth = 80;
        const int barHeight = 10;

        float healthPercent = target.HealthPercent;

        // Determine color based on health
        Color healthColor = healthPercent > 0.5f
            ? Color.Green
            : healthPercent > 0.25f
                ? Color.Yellow
                : Color.Red;

        // Position bar above the target box, centered horizontally (below the name)
        int x = (int)(targetBounds.X + (targetBounds.Width / 2) - (barWidth / 2));
        int y = (int)(targetBounds.Y - barHeight - 20);

        // Draw background
        DrawRectangle(x, y, barWidth, barHeight, Color.DarkGray);

        // Draw health fill
        int fillWidth = (int)(barWidth * healthPercent);
        DrawRectangle(x, y, fillWidth, barHeight, healthColor);

        // Draw border
        DrawRectangleLinesEx(new Rectangle(x, y, barWidth, barHeight), 1, new Color(0, 255, 255, 200));
    }

    /// <summary>
    /// Draws the health bar for the selected target.
    /// </summary>
    private void DrawSelectedTargetHealthBar(ITarget target, int x, int y)
    {
        const int barWidth = 180;
        const int barHeight = 12;

        float healthPercent = target.HealthPercent;

        // Determine color based on health
        Color healthColor = healthPercent > 0.5f
            ? Color.Green
            : healthPercent > 0.25f
                ? Color.Yellow
                : Color.Red;

        // Draw background
        DrawRectangle(x, y, barWidth, barHeight, Color.DarkGray);

        // Draw health fill
        int fillWidth = (int)(barWidth * healthPercent);
        DrawRectangle(x, y, fillWidth, barHeight, healthColor);

        // Draw border
        DrawRectangleLinesEx(new Rectangle(x, y, barWidth, barHeight), 1, Color.White);

        // Draw percentage text
        string healthText = $"{healthPercent * 100:F0}%";
        FontManager.DrawText(healthText, x + barWidth + 5, y + 1, 10, healthColor);
    }
}
