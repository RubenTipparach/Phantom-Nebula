using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PhantomNebula.Core;

/// <summary>
/// Game configuration loaded from YAML
/// </summary>
public class GameConfig
{
    // Debug settings
    public bool DebugMode { get; set; } = false;
    public bool EnableLightDirectionControls { get; set; } = false;

    // Light settings
    public float LightDirectionX { get; set; } = 0.0f;
    public float LightDirectionY { get; set; } = 0.26f;
    public float LightDirectionZ { get; set; } = 0.96f;

    // Planet settings
    public float PlanetPositionX { get; set; } = -100.0f;
    public float PlanetPositionY { get; set; } = 0.0f;
    public float PlanetPositionZ { get; set; } = 0.0f;
    public float PlanetScale { get; set; } = 4.0f;
    public float PlanetRotationX { get; set; } = 0.0f;
    public float PlanetRotationY { get; set; } = 0.0f;
    public float PlanetRotationZ { get; set; } = 0.0f;

    // Ship settings
    public float ShipPositionX { get; set; } = 5.0f;
    public float ShipPositionY { get; set; } = 0.0f;
    public float ShipPositionZ { get; set; } = 0.0f;
    public float ShipScale { get; set; } = 0.2f;
    public float ShipRotationX { get; set; } = 0.0f;
    public float ShipRotationY { get; set; } = 0.0f;
    public float ShipRotationZ { get; set; } = 0.0f;

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
}
