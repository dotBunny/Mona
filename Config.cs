using System.Text.Json.Serialization;

namespace Mona;
using System.Text.Json;

public class Config
{
    public string Application  { get; set; }
    public string Arguments { get; set; }
    public string BasePath { get; set; }
    public string ProcessInfoPath { get; set; }
    public int SleepMilliseconds { get; set; }
    
    string _filePath;
    string _fileContent;

    public static Config Get(string jsonPath = null)
    {
        // Establish some settings for the serializer
        var jsonSettings = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
        
        try
        {
            Config foundConfig = null;
            if (jsonPath == null)
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mona.json");
                if (!File.Exists(filePath)) return null;

                var content = File.ReadAllText(filePath);
                
                foundConfig = JsonSerializer.Deserialize<Config>(content, jsonSettings)!;

                
                foundConfig._filePath = filePath;
                foundConfig._fileContent = content;
            }
            else
            {
                if (!File.Exists(jsonPath)) return Get();
                
                var content = File.ReadAllText(jsonPath);
                
                foundConfig = JsonSerializer.Deserialize<Config>(content, jsonSettings)!;
                
                foundConfig._filePath = jsonPath;
                foundConfig._fileContent = content;
            }
            
            CleanUpConfig(foundConfig);
            return foundConfig;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Error(e.StackTrace);
            
            return Get();
        }
    }

    public static void CleanUpConfig(Config config)
    {
        if (config.BasePath == null)
        {
            config.BasePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        if (string.IsNullOrEmpty(config.ProcessInfoPath))
        {
            config.ProcessInfoPath = Path.Combine(config.BasePath, "mona.pid");
        }

        if (config.SleepMilliseconds <= 0)
        {
            config.SleepMilliseconds = 1000 * 10;
        }
    }

    public void Report()
    {
        Log.Variable("Config._filePath", _filePath);
        Log.Variable("Config.BasePath", BasePath);
        Log.Variable("Config.Application", Application);
        Log.Variable("Config.Arguments", Arguments);
        Log.Variable("Config.ProcessInfoPath", ProcessInfoPath);
        Log.Variable("Config.SleepMilliseconds", SleepMilliseconds.ToString());
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Application))
        {
            Log.Error("No application found.");
            return false;
        }
        
        return true;
    }
}