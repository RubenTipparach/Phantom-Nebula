using System;
using System.IO;
using System.Numerics;
using Raylib_cs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PhantomNebula.Core;

/// <summary>
/// 3D Vector config struct
/// </summary>
public class VectorConfig
{
    public float X { get; set; } = 0.0f;
    public float Y { get; set; } = 0.0f;
    public float Z { get; set; } = 0.0f;

    public Vector3 ToVector3() => new Vector3(X, Y, Z);
}

/// <summary>
/// Object transform config (position, scale, rotation)
/// </summary>
public class TransformConfig
{
    public VectorConfig Position { get; set; } = new();
    public float Scale { get; set; } = 1.0f;
    public VectorConfig Rotation { get; set; } = new();
}

/// <summary>
/// Debug action keybindings
/// </summary>
public class DebugActionsConfig
{
    public bool Enabled { get; set; } = false;
    public string SpawnExplosionOnShip { get; set; } = "X";
}

/// <summary>
/// Debug settings
/// </summary>
public class DebugConfig
{
    public bool DebugMode { get; set; } = false;
    public bool DebugPhysics { get; set; } = false;
    public DebugActionsConfig DebugActions { get; set; } = new();
}

/// <summary>
/// Lighting settings
/// </summary>
public class LightingConfig
{
    public bool EnableLightDirectionControls { get; set; } = false;
    public VectorConfig LightDirection { get; set; } = new() { X = 0, Y = 0.26f, Z = 0.96f };
}

/// <summary>
/// Model configuration (contains model path, albedo, and emissive textures)
/// </summary>
public class ModelEntityConfig
{
    public string Model { get; set; } = "";
    public string Albedo { get; set; } = "";
    public string Emissive { get; set; } = "";
}

/// <summary>
/// Models configuration for all entities
/// </summary>
public class ModelsConfig
{
    public ModelEntityConfig Ship { get; set; } = new() { Model = "Resources/Models/shippy1.obj", Albedo = "Resources/Models/shippy.png", Emissive = "Resources/Models/shippy_em.png" };
    public ModelEntityConfig Satellite { get; set; } = new() { Model = "Resources/Models/satelite.obj", Albedo = "Resources/Models/shippy.png", Emissive = "Resources/Models/shippy_em.png" };
    public ModelEntityConfig Planet { get; set; } = new() { Albedo = "Resources/planet.png" };
}

/// <summary>
/// Objects configuration
/// </summary>
public class ObjectsConfig
{
    public TransformConfig Planet { get; set; } = new() { Position = new() { X = -100 }, Scale = 4 };
    public TransformConfig Ship { get; set; } = new() { Position = new() { X = 5 }, Scale = 0.2f };
    public TransformConfig Satellite { get; set; } = new() { Position = new() { X = 20, Y = 0, Z = 20 }, Scale = 0.5f };
}

/// <summary>
/// Game configuration loaded from YAML
/// </summary>
public class GameConfig
{
    public DebugConfig Debug { get; set; } = new();
    public ModelsConfig Models { get; set; } = new();
    public LightingConfig Lighting { get; set; } = new();
    public ObjectsConfig Objects { get; set; } = new();

    // Backwards compatibility properties
    public bool DebugMode { get => Debug.DebugMode; set => Debug.DebugMode = value; }
    public bool DebugPhysics { get => Debug.DebugPhysics; set => Debug.DebugPhysics = value; }
    public bool EnableLightDirectionControls { get => Lighting.EnableLightDirectionControls; set => Lighting.EnableLightDirectionControls = value; }

    public float LightDirectionX { get => Lighting.LightDirection.X; set => Lighting.LightDirection.X = value; }
    public float LightDirectionY { get => Lighting.LightDirection.Y; set => Lighting.LightDirection.Y = value; }
    public float LightDirectionZ { get => Lighting.LightDirection.Z; set => Lighting.LightDirection.Z = value; }

    public float PlanetPositionX { get => Objects.Planet.Position.X; set => Objects.Planet.Position.X = value; }
    public float PlanetPositionY { get => Objects.Planet.Position.Y; set => Objects.Planet.Position.Y = value; }
    public float PlanetPositionZ { get => Objects.Planet.Position.Z; set => Objects.Planet.Position.Z = value; }
    public float PlanetScale { get => Objects.Planet.Scale; set => Objects.Planet.Scale = value; }
    public float PlanetRotationX { get => Objects.Planet.Rotation.X; set => Objects.Planet.Rotation.X = value; }
    public float PlanetRotationY { get => Objects.Planet.Rotation.Y; set => Objects.Planet.Rotation.Y = value; }
    public float PlanetRotationZ { get => Objects.Planet.Rotation.Z; set => Objects.Planet.Rotation.Z = value; }

    public float ShipPositionX { get => Objects.Ship.Position.X; set => Objects.Ship.Position.X = value; }
    public float ShipPositionY { get => Objects.Ship.Position.Y; set => Objects.Ship.Position.Y = value; }
    public float ShipPositionZ { get => Objects.Ship.Position.Z; set => Objects.Ship.Position.Z = value; }
    public float ShipScale { get => Objects.Ship.Scale; set => Objects.Ship.Scale = value; }
    public float ShipRotationX { get => Objects.Ship.Rotation.X; set => Objects.Ship.Rotation.X = value; }
    public float ShipRotationY { get => Objects.Ship.Rotation.Y; set => Objects.Ship.Rotation.Y = value; }
    public float ShipRotationZ { get => Objects.Ship.Rotation.Z; set => Objects.Ship.Rotation.Z = value; }

    // Satellite settings (new)
    public float SatellitePositionX { get => Objects.Satellite.Position.X; set => Objects.Satellite.Position.X = value; }
    public float SatellitePositionY { get => Objects.Satellite.Position.Y; set => Objects.Satellite.Position.Y = value; }
    public float SatellitePositionZ { get => Objects.Satellite.Position.Z; set => Objects.Satellite.Position.Z = value; }
    public float SatelliteScale { get => Objects.Satellite.Scale; set => Objects.Satellite.Scale = value; }
    public float SatelliteRotationX { get => Objects.Satellite.Rotation.X; set => Objects.Satellite.Rotation.X = value; }
    public float SatelliteRotationY { get => Objects.Satellite.Rotation.Y; set => Objects.Satellite.Rotation.Y = value; }
    public float SatelliteRotationZ { get => Objects.Satellite.Rotation.Z; set => Objects.Satellite.Rotation.Z = value; }

    // Singleton instance
    private static GameConfig? _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Load configuration from YAML file
    /// </summary>
    public static GameConfig Load(string configPath = "game_config.yaml")
    {
        try
        {
            if (File.Exists(configPath))
            {
                var yaml = File.ReadAllText(configPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var config = deserializer.Deserialize<GameConfig>(yaml);
                Console.WriteLine($"[GameConfig] Loaded configuration from {configPath}");
                return config ?? new GameConfig();
            }
            else
            {
                Console.WriteLine($"[GameConfig] Config file not found at {configPath}, using defaults");
                var defaultConfig = new GameConfig();
                defaultConfig.Save(configPath);
                return defaultConfig;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameConfig] Error loading config: {ex.Message}");
            return new GameConfig();
        }
    }

    /// <summary>
    /// Save configuration to YAML file
    /// </summary>
    public void Save(string configPath = "game_config.yaml")
    {
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(this);
            File.WriteAllText(configPath, yaml);
            Console.WriteLine($"[GameConfig] Saved configuration to {configPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameConfig] Error saving config: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert string key name to Raylib KeyboardKey
    /// </summary>
    public static KeyboardKey GetKeyFromString(string keyName)
    {
        return keyName.ToUpper() switch
        {
            "X" => KeyboardKey.X,
            "Z" => KeyboardKey.Z,
            "C" => KeyboardKey.C,
            "V" => KeyboardKey.V,
            "B" => KeyboardKey.B,
            "N" => KeyboardKey.N,
            "M" => KeyboardKey.M,
            "F1" => KeyboardKey.F1,
            "F2" => KeyboardKey.F2,
            "F3" => KeyboardKey.F3,
            "F4" => KeyboardKey.F4,
            "F5" => KeyboardKey.F5,
            "SPACE" => KeyboardKey.Space,
            _ => KeyboardKey.X // Default fallback
        };
    }
}
