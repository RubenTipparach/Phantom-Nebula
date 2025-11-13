using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vector3 = System.Numerics.Vector3;
using XnaMathHelper = Microsoft.Xna.Framework.MathHelper;
using XnaQuaternion = Microsoft.Xna.Framework.Quaternion;
using PhantomSector.Game.Core;

namespace PhantomSector.Game.Screens;

public class PhysicsDemoScreen : GameScreen
{
    private BasicEffect _effect;

    // Physics
    private Simulation _simulation;
    private BufferPool _bufferPool;
    private SimpleThreadDispatcher _threadDispatcher;

    // Debug
    private double _debugTimer = 0;
    private const double DebugInterval = 1.0; // Print debug info every second

    // Rendering
    private VertexPositionNormal[] _cubeVertices;
    private short[] _cubeIndices;
    private VertexPositionNormal[] _sphereVertices;
    private short[] _sphereIndices;

    // Camera
    private OrbitCamera _camera;

    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;

    public PhysicsDemoScreen() : base("Physics Demo")
    {
        TransitionOnTime = 0.5f;
        TransitionOffTime = 0.5f;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Initialize Bepu Physics
        _bufferPool = new BufferPool();
        _threadDispatcher = new SimpleThreadDispatcher(System.Environment.ProcessorCount);

        _simulation = Simulation.Create(_bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));

        // Create ground plane
        var groundShape = new Box(50, 1, 50);
        var groundShapeIndex = _simulation.Shapes.Add(groundShape);
        _simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), groundShapeIndex));

        // Create pyramid of boxes
        CreateBoxPyramid();

        // Create rendering geometry using GeometryGenerator
        GeometryGenerator.CreateCube(out _cubeVertices, out _cubeIndices);
        GeometryGenerator.CreateSphere(out _sphereVertices, out _sphereIndices, 16, 16);

        // Initialize camera
        _camera = new OrbitCamera(ScreenManager.GraphicsDevice);
        _camera.SetOrbitAngles(XnaMathHelper.PiOver4, 0.5f);

        // Initialize BasicEffect
        _effect = new BasicEffect(ScreenManager.GraphicsDevice)
        {
            VertexColorEnabled = false,
            LightingEnabled = true,
            PreferPerPixelLighting = true,
            AmbientLightColor = new Microsoft.Xna.Framework.Vector3(0.3f, 0.3f, 0.3f),
            DirectionalLight0 =
            {
                Enabled = true,
                DiffuseColor = new Microsoft.Xna.Framework.Vector3(0.8f, 0.8f, 0.8f),
                Direction = Microsoft.Xna.Framework.Vector3.Normalize(new Microsoft.Xna.Framework.Vector3(-1, -1, -1)),
                SpecularColor = new Microsoft.Xna.Framework.Vector3(0.5f, 0.5f, 0.5f)
            }
        };

        System.Console.WriteLine("=== Physics Demo Screen Loaded ===");
        System.Console.WriteLine("Controls: Right-click + drag to rotate camera, Mouse wheel to zoom, Left-click to shoot sphere, ESC to exit");
    }

    private void CreateBoxPyramid()
    {
        var boxShape = new Box(1, 1, 1);
        var boxShapeIndex = _simulation.Shapes.Add(boxShape);
        var boxInertia = boxShape.ComputeInertia(1);

        const int pyramidLevels = 6;
        for (int level = 0; level < pyramidLevels; level++)
        {
            int boxesPerSide = pyramidLevels - level;
            float yPos = level * 1.05f + 0.5f;

            for (int x = 0; x < boxesPerSide; x++)
            {
                for (int z = 0; z < boxesPerSide; z++)
                {
                    float xPos = (x - boxesPerSide / 2.0f) * 1.05f;
                    float zPos = (z - boxesPerSide / 2.0f) * 1.05f;

                    _simulation.Bodies.Add(BodyDescription.CreateDynamic(
                        new Vector3(xPos, yPos, zPos),
                        boxInertia,
                        boxShapeIndex,
                        0.01f));
                }
            }
        }
    }


    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (IsActive && !otherScreenHasFocus)
        {
            _camera.HandleInput(gameTime);
            _camera.Update(gameTime);
            UpdatePhysics(gameTime);
        }
    }

    public override void HandleInput(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // ESC to open pause menu
        if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            ScreenManager.ShowPauseMenu();
        }

        // Shoot sphere on left click
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            ShootSphere();
        }

        _previousKeyboardState = keyboardState;
        _previousMouseState = mouseState;
    }

    private void UpdatePhysics(GameTime gameTime)
    {
        // Update physics simulation
        _simulation.Timestep(1 / 60f, _threadDispatcher);

        // Debug output
        _debugTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_debugTimer >= DebugInterval)
        {
            _debugTimer = 0;
            int totalBodies = _simulation.Bodies.ActiveSet.Count + _simulation.Bodies.Sets.Length;
            System.Console.WriteLine($"[DEBUG] Active Bodies: {_simulation.Bodies.ActiveSet.Count}, Camera Distance: {_camera.Distance:F2}, FPS: {1.0 / gameTime.ElapsedGameTime.TotalSeconds:F0}");
        }
    }

    private void ShootSphere()
    {
        var sphereShape = new Sphere(0.5f);
        var sphereShapeIndex = _simulation.Shapes.Add(sphereShape);
        var sphereInertia = sphereShape.ComputeInertia(5);

        // Shoot from camera position in the direction camera is looking
        var startPos = _camera.Position;
        var shootDirection = _camera.Forward;
        float force = 50f;

        var bodyDescription = BodyDescription.CreateDynamic(
            new Vector3(startPos.X, startPos.Y, startPos.Z),
            new BodyVelocity(new Vector3(shootDirection.X * force, shootDirection.Y * force, shootDirection.Z * force)),
            sphereInertia,
            sphereShapeIndex,
            0.01f);

        var handle = _simulation.Bodies.Add(bodyDescription);
        System.Console.WriteLine($"[SHOOT] Sphere fired! Handle: {handle.Value}, Position: ({startPos.X:F2}, {startPos.Y:F2}, {startPos.Z:F2}), Direction: ({shootDirection.X:F2}, {shootDirection.Y:F2}, {shootDirection.Z:F2})");
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var graphicsDevice = ScreenManager.GraphicsDevice;

        // Clear the screen
        graphicsDevice.Clear(Color.CornflowerBlue);

        // Set up proper 3D render states
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;

        // Set up camera
        _effect.View = _camera.View;
        _effect.Projection = _camera.Projection;

        // Draw ground
        DrawBox(new Vector3(0, -0.5f, 0), System.Numerics.Quaternion.Identity, new Vector3(50, 1, 50), Color.DarkGreen);

        // Draw all dynamic bodies (active and sleeping)
        for (int setIndex = 0; setIndex < _simulation.Bodies.Sets.Length; setIndex++)
        {
            ref var set = ref _simulation.Bodies.Sets[setIndex];
            if (!set.Allocated)
                continue;

            for (int bodyIndex = 0; bodyIndex < set.Count; bodyIndex++)
            {
                var bodyHandle = set.IndexToHandle[bodyIndex];
                var bodyReference = _simulation.Bodies[bodyHandle];
                var shapeIndex = bodyReference.Collidable.Shape;

                if (shapeIndex.Type == Box.Id)
                {
                    var box = _simulation.Shapes.GetShape<Box>(shapeIndex.Index);
                    DrawBox(bodyReference.Pose.Position, bodyReference.Pose.Orientation,
                        new Vector3(box.Width, box.Height, box.Length), Color.LightBlue);
                }
                else if (shapeIndex.Type == Sphere.Id)
                {
                    var sphere = _simulation.Shapes.GetShape<Sphere>(shapeIndex.Index);
                    DrawSphere(bodyReference.Pose.Position, sphere.Radius, Color.Red);
                }
            }
        }
    }

    private void DrawBox(Vector3 position, System.Numerics.Quaternion orientation, Vector3 size, Color color)
    {
        var graphicsDevice = ScreenManager.GraphicsDevice;

        // Convert Bepu types to XNA types
        var xnaPos = new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z);
        var xnaQuat = new XnaQuaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);

        var world = Microsoft.Xna.Framework.Matrix.CreateScale(size.X, size.Y, size.Z) *
                   Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(xnaQuat) *
                   Microsoft.Xna.Framework.Matrix.CreateTranslation(xnaPos);

        _effect.World = world;
        _effect.DiffuseColor = color.ToVector3();

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _cubeVertices,
                0,
                _cubeVertices.Length,
                _cubeIndices,
                0,
                _cubeIndices.Length / 3
            );
        }
    }

    private void DrawSphere(Vector3 position, float radius, Color color)
    {
        var graphicsDevice = ScreenManager.GraphicsDevice;

        var xnaPos = new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z);
        var world = Microsoft.Xna.Framework.Matrix.CreateScale(radius * 2) *
                   Microsoft.Xna.Framework.Matrix.CreateTranslation(xnaPos);

        _effect.World = world;
        _effect.DiffuseColor = color.ToVector3();

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _sphereVertices,
                0,
                _sphereVertices.Length,
                _sphereIndices,
                0,
                _sphereIndices.Length / 3
            );
        }
    }

    public override void UnloadContent()
    {
        _simulation?.Dispose();
        _bufferPool?.Clear();
        _threadDispatcher?.Dispose();
        _effect?.Dispose();

        System.Console.WriteLine("=== Physics Demo Screen Unloaded ===");

        base.UnloadContent();
    }
}

// Required Bepu callbacks
struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(Simulation simulation) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial.FrictionCoefficient = 0.5f;
        pairMaterial.MaximumRecoveryVelocity = 2f;
        pairMaterial.SpringSettings = new SpringSettings(30f, 1f);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }

    public void Dispose() { }
}

struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    public Vector3 Gravity;

    public PoseIntegratorCallbacks(Vector3 gravity)
    {
        Gravity = gravity;
    }

    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => false;

    public void Initialize(Simulation simulation) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrepareForIntegration(float dt) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        velocity.Linear.X += Gravity.X * dt;
        velocity.Linear.Y += Gravity.Y * dt;
        velocity.Linear.Z += Gravity.Z * dt;
    }
}

class SimpleThreadDispatcher : IThreadDispatcher
{
    private readonly int _threadCount;
    private System.Threading.Tasks.Task[] _workers;
    private readonly BufferPool[] _threadPools;

    public int ThreadCount => _threadCount;

    public SimpleThreadDispatcher(int threadCount)
    {
        _threadCount = threadCount;
        _threadPools = new BufferPool[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            _threadPools[i] = new BufferPool();
        }
    }

    public void DispatchWorkers(System.Action<int> workerBody, int maximumWorkerCount)
    {
        int workerCount = System.Math.Min(maximumWorkerCount, _threadCount);
        _workers = new System.Threading.Tasks.Task[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            int workerIndex = i;
            _workers[i] = System.Threading.Tasks.Task.Run(() => workerBody(workerIndex));
        }

        System.Threading.Tasks.Task.WaitAll(_workers);
    }

    public BufferPool GetThreadMemoryPool(int workerIndex)
    {
        return _threadPools[workerIndex];
    }

    public void Dispose()
    {
        for (int i = 0; i < _threadPools.Length; i++)
        {
            _threadPools[i]?.Clear();
        }
    }
}
