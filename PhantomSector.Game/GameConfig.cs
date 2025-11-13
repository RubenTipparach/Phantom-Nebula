using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PhantomSector.Game;

public class GameConfig
{
    public string StartupScene { get; set; } = "menu";

    private static readonly string ConfigPath = "game_config.yaml";

    public static GameConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var yaml = File.ReadAllText(ConfigPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                var config = deserializer.Deserialize<GameConfig>(yaml);
                Console.WriteLine($"[Config] Loaded: StartupScene='{config.StartupScene}'");
                return config;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Error loading config: {ex.Message}");
        }

        Console.WriteLine("[Config] Using default config");
        var defaultConfig = new GameConfig();
        defaultConfig.Save();
        return defaultConfig;
    }

    public void Save()
    {
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(this);
            File.WriteAllText(ConfigPath, yaml);
            Console.WriteLine($"[Config] Saved: StartupScene='{StartupScene}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Error saving config: {ex.Message}");
        }
    }
}
