using Raylib_cs;
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
    /// Renders the targeting UI for hovered and selected targets.
    /// Only draws UI if the target is in front of the camera.
    /// </summary>
    public void Draw(TargetingSystem targetingSystem, Vector3 playerPosition, Camera3D camera)
    {
        // Get the current target (hovered or selected)
        ITarget? currentTarget = targetingSystem.HoveredTarget ?? targetingSystem.SelectedTarget;

        // Check if target is behind the camera
        if (currentTarget != null)
        {
            Vector3 dirToTarget = currentTarget.Position - camera.Position;
            // If dot product of direction and camera forward is negative, target is behind
            Vector3 cameraForward = Vector3.Normalize(camera.Target - camera.Position);
            if (Vector3.Dot(dirToTarget, cameraForward) < 0)
            {
                // Target is behind camera, don't draw UI
                return;
            }
        }

        // Determine if we should show highlight (hovered or selected)
        bool isHighlighted = targetingSystem.HoveredTarget != null || targetingSystem.SelectedTarget != null;

        // Always draw target bounding box if target exists (dark blue, or bright cyan if highlighted)
        if (targetingSystem.TargetScreenBounds.Width > 0)
        {
            if (isHighlighted)
            {
                // Draw bright cyan highlight when hovered or selected
                DrawHighlightedTargetBox(targetingSystem);
            }
            else
            {
                // Draw dark blue box when not highlighted
                DrawTargetBox(targetingSystem);
            }
        }

        // Draw health bar above target box if hovered or selected
        if (targetingSystem.TargetScreenBounds.Width > 0 && isHighlighted)
        {
            ITarget? target = targetingSystem.HoveredTarget ?? targetingSystem.SelectedTarget;
            if (target != null)
            {
                DrawTargetHealthBar(target, targetingSystem.TargetScreenBounds);
            }
        }

        // Draw target name above health bar if hovered or selected
        if (targetingSystem.TargetScreenBounds.Width > 0 && isHighlighted)
        {
            ITarget? target = targetingSystem.HoveredTarget ?? targetingSystem.SelectedTarget;
            if (target != null)
            {
                DrawTargetName(target, targetingSystem.TargetScreenBounds);
            }
        }
    }

    /// <summary>
    /// Draws the target bounding box in dark blue (when not highlighted).
    /// </summary>
    private void DrawTargetBox(TargetingSystem targetingSystem)
    {
        Rectangle bounds = targetingSystem.TargetScreenBounds;
        Color boxColor = new(25, 50, 100, 200); // Dark blue

        // Draw corner brackets
        DrawTargetCorners(bounds, boxColor);

        // Draw distance text at the bottom of the box
        string distanceText = $"{targetingSystem.TargetDistance:F1}m";
        int textWidth = MeasureText(distanceText, 12);
        int textX = (int)(bounds.X + bounds.Width / 2 - textWidth / 2);
        int textY = (int)(bounds.Y + bounds.Height + 5);

        DrawText(distanceText, textX, textY, 12, boxColor);
    }

    /// <summary>
    /// Draws the target bounding box in bright cyan (when highlighted/hovered/selected).
    /// </summary>
    private void DrawHighlightedTargetBox(TargetingSystem targetingSystem)
    {
        Rectangle bounds = targetingSystem.TargetScreenBounds;
        Color boxColor = new(0, 255, 255, 255); // Bright cyan

        // Draw corner brackets
        DrawTargetCorners(bounds, boxColor);

        // Draw distance text at the bottom of the box
        string distanceText = $"{targetingSystem.TargetDistance:F1}m";
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
        ITarget target = targetingSystem.SelectedTarget;

        // Convert target position to screen space
        Vector2 targetScreenPos = GetWorldToScreen(target.Position, new Camera3D());
        // Note: We need camera for proper conversion, but we'll calculate it differently below

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
