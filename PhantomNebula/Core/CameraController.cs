using System;
using System.Numerics;
using Raylib_cs;

namespace PhantomNebula.Core;

/// <summary>
/// Mouse-based orbit camera controller
/// Orbits around a target transform with mouse and scroll wheel controls
/// </summary>
public class CameraController
{
    private ITransform targetTransform;
    private Camera3D camera;

    // Orbit parameters
    private float orbitDistance = 15.0f;
    private float minDistance = 2.0f;
    private float maxDistance = 100.0f;
    private float orbitSpeed = 0.5f;
    private float zoomSpeed = 0.5f;

    // Camera clipping planes
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 10000.0f;

    // Angle parameters
    private float yaw = 0.0f;
    private float pitch = 45.0f * ((float)Math.PI / 180.0f);

    // Mouse tracking
    private Vector2 lastMousePos = Vector2.Zero;
    private bool isDragging = false;
    private bool dragStartedOnUI = false;
    private bool mouseButtonWasDown = false;

    public CameraController(ITransform targetTransform)
    {
        this.targetTransform = targetTransform;
        this.camera = new Camera3D
        {
            Position = Vector3.Zero,
            Target = targetTransform.Position,
            Up = Vector3.UnitY,
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };

        // Apply default near/far plane values
        SetProjectionMatrix(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }

    /// <summary>
    /// Update camera based on input and target position
    /// </summary>
    public void Update(float deltaTime, bool mouseOverUI = false)
    {
        HandleMouseInput(mouseOverUI);
        UpdateCameraPosition();
    }

    private void HandleMouseInput(bool mouseOverUI)
    {
        Vector2 currentMousePos = Raylib.GetMousePosition();
        bool mouseButtonDown = Raylib.IsMouseButtonDown(MouseButton.Left);

        // Detect button press (transition from up to down)
        if (mouseButtonDown && !mouseButtonWasDown)
        {
            // Button just pressed - record if it started on UI
            dragStartedOnUI = mouseOverUI;
        }

        // Left mouse button dragging for rotation
        if (mouseButtonDown)
        {
            // Don't allow any camera interaction if drag started on UI
            if (dragStartedOnUI)
            {
                mouseButtonWasDown = true;
                return;
            }

            if (!isDragging)
            {
                // Start dragging (we know it didn't start on UI from check above)
                lastMousePos = currentMousePos;
                isDragging = true;
            }

            // Stop dragging if mouse enters UI area while dragging
            if (isDragging && mouseOverUI)
            {
                isDragging = false;
            }
            // Continue updating camera if dragging and not over UI
            else if (isDragging)
            {
                Vector2 delta = currentMousePos - lastMousePos;

                // Update angles based on mouse movement
                yaw -= delta.X * orbitSpeed * 0.01f;
                pitch += delta.Y * orbitSpeed * 0.01f;

                // Clamp pitch to avoid flipping
                pitch = float.Clamp(pitch, -MathF.PI * 0.49f, MathF.PI * 0.49f);

                lastMousePos = currentMousePos;
            }
        }
        else
        {
            // Button released - reset all drag states
            isDragging = false;
            dragStartedOnUI = false;
        }

        mouseButtonWasDown = mouseButtonDown;

        // Scroll wheel for zoom
        float scrollDelta = Raylib.GetMouseWheelMove();
        if (Math.Abs(scrollDelta) > 0.001f)
        {
            orbitDistance -= scrollDelta * zoomSpeed;
            orbitDistance = float.Clamp(orbitDistance, minDistance, maxDistance);
        }

        // Right mouse button to pan
        if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            // Can be extended for panning functionality
        }
    }

    private void UpdateCameraPosition()
    {
        // Calculate orbit position
        Vector3 orbitPos = new Vector3(
            (float)(Math.Sin(yaw) * Math.Cos(pitch) * orbitDistance),
            (float)(Math.Sin(pitch) * orbitDistance),
            (float)(Math.Cos(yaw) * Math.Cos(pitch) * orbitDistance)
        );

        // Set camera position relative to target - simple orbit centered on target
        camera.Position = targetTransform.Position + orbitPos;
        camera.Target = targetTransform.Position;
        camera.Up = Vector3.UnitY;
    }

    /// <summary>
    /// Get the camera for rendering
    /// </summary>
    public Camera3D GetCamera()
    {
        // Note: Raylib doesn't directly support near/far in Camera3D struct
        // You need to manually set projection matrix if needed
        // This is done in BeginMode3D by Raylib using default values
        // To customize, call Rlgl.SetMatrixProjection before rendering
        return camera;
    }

    /// <summary>
    /// Apply custom projection matrix with near/far planes
    /// Call this before BeginMode3D to override projection
    /// </summary>
    public void SetProjectionMatrix(int screenWidth, int screenHeight)
    {
        float aspect = (float)screenWidth / screenHeight;
        float fovY = camera.FovY * ((float)Math.PI / 180.0f);

        // Create perspective projection matrix
        Matrix4x4 projection = Raylib_cs.Raymath.MatrixPerspective(fovY, aspect, NearPlane, FarPlane);
        Rlgl.SetMatrixProjection(projection);
    }

    /// <summary>
    /// Set the target transform to orbit around
    /// </summary>
    public void SetTarget(ITransform target)
    {
        targetTransform = target;
    }

    /// <summary>
    /// Reset camera to default position and angles
    /// </summary>
    public void Reset()
    {
        yaw = 0.0f;
        pitch = 45.0f * ((float)Math.PI / 180.0f);
        orbitDistance = 15.0f;
    }

    /// <summary>
    /// Get current orbit distance
    /// </summary>
    public float OrbitDistance
    {
        get => orbitDistance;
        set => orbitDistance = float.Clamp(value, minDistance, maxDistance);
    }

    /// <summary>
    /// Get current yaw angle
    /// </summary>
    public float Yaw
    {
        get => yaw;
        set => yaw = value;
    }

    /// <summary>
    /// Get current pitch angle
    /// </summary>
    public float Pitch
    {
        get => pitch;
        set => pitch = float.Clamp(value, -MathF.PI * 0.49f, MathF.PI * 0.49f);
    }
}
