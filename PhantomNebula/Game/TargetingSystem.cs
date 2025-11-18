using System;
using System.Numerics;
using Raylib_cs;
using PhantomNebula.Core;

namespace PhantomNebula.Game;

/// <summary>
/// Manages target selection and hover detection for game entities.
/// Tracks which entity is currently targeted and provides targeting information.
/// </summary>
public class TargetingSystem
{
    /// <summary>
    /// The currently selected target, or null if none selected.
    /// </summary>
    public ITarget? SelectedTarget { get; private set; }

    /// <summary>
    /// The entity currently being hovered over, or null if none.
    /// </summary>
    public ITarget? HoveredTarget { get; private set; }

    /// <summary>
    /// The bounding box of the currently hovered target in screen space.
    /// Only valid if HoveredTarget is not null.
    /// </summary>
    public Rectangle HoveredTargetScreenBounds { get; private set; }

    /// <summary>
    /// The bounding box of the target entity in screen space (always visible, even when not hovering).
    /// </summary>
    public Rectangle TargetScreenBounds { get; private set; }

    /// <summary>
    /// Distance to the hovered target in world units.
    /// </summary>
    public float HoveredTargetDistance { get; private set; }

    /// <summary>
    /// Distance to the target entity in world units (always updated).
    /// </summary>
    public float TargetDistance { get; private set; }

    /// <summary>
    /// Distance to the selected target in world units.
    /// </summary>
    public float SelectedTargetDistance { get; private set; }

    /// <summary>
    /// Clears the currently selected target.
    /// </summary>
    public void ClearSelection()
    {
        SelectedTarget = null;
    }

    /// <summary>
    /// Selects a new target.
    /// </summary>
    public void SelectTarget(ITarget target)
    {
        SelectedTarget = target;
    }

    /// <summary>
    /// Updates hover detection and target information.
    /// Call this once per frame with the current camera and viewport info.
    /// </summary>
    public void Update(
        Vector3 playerPosition,
        ITarget? targetEntity,
        Camera3D camera,
        int screenWidth,
        int screenHeight,
        Vector2 mouseScreenPos)
    {
        // Clear previous hover
        HoveredTarget = null;
        HoveredTargetScreenBounds = default;
        HoveredTargetDistance = 0;

        // Clear target bounds
        TargetScreenBounds = default;
        TargetDistance = 0;

        if (targetEntity == null)
            return;

        // Check if target is alive
        if (targetEntity.IsDead)
            return;

        // Convert target position to screen space
        Vector2 targetScreenPos = Raylib.GetWorldToScreen(targetEntity.Position, camera);

        // Define bounding box size (this will be the hit area for mouse hover)
        const float boxWidth = 80f;
        const float boxHeight = 100f;

        // Create screen-space bounding box centered on the target
        Rectangle screenBounds = new(
            targetScreenPos.X - boxWidth / 2,
            targetScreenPos.Y - boxHeight / 2,
            boxWidth,
            boxHeight
        );

        // Always update target screen bounds and distance
        TargetScreenBounds = screenBounds;
        Vector3 directionToTarget = targetEntity.Position - playerPosition;
        TargetDistance = directionToTarget.Length();

        // Check if mouse is over the bounding box
        if (Raylib.CheckCollisionPointRec(mouseScreenPos, screenBounds))
        {
            HoveredTarget = targetEntity;
            HoveredTargetScreenBounds = screenBounds;

            // Calculate distance
            HoveredTargetDistance = directionToTarget.Length();
        }

        // Update selected target distance
        if (SelectedTarget != null)
        {
            Vector3 directionToSelected = SelectedTarget.Position - playerPosition;
            SelectedTargetDistance = directionToSelected.Length();
        }
    }

    /// <summary>
    /// Handles right-click targeting input.
    /// Returns true if a target was selected this frame.
    /// </summary>
    public bool HandleTargetingInput()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Right) && HoveredTarget != null)
        {
            SelectTarget(HoveredTarget);
            return true;
        }

        return false;
    }
}
