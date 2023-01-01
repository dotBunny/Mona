using System.Diagnostics;
using Mona;
using Monitor = Mona.Monitor;

internal class Program
{
    
    public static Config Settings = null;
    
    static bool _alive = true;
    private static DateTime _lastHeartbeat;
    private static Monitor _monitor = null;


    public static void Main(string[] args)
    {
        Log.Clear();

        Log.Message("Arguments Found:");
        for (int i = 0; i < args.Length; i++)
        {
            Log.Message($"{i}: {args[i]}");
        }
        
        // Handle custom config path as an argument
        if (args.Length >= 1 && File.Exists(args[0]))
        {
            Log.Message($"Attempting to use config at {args[0]} ...");
            Settings = Config.Get(args[0]);
        }
        else
        {
            Settings = Config.Get();
        }

        // If we do not have a settings file at this point there is something wrong and we should bail
        if (Settings == null)
        {
            Log.Error("No valid settings found.");
            Environment.Exit(-1);
            return;

        }

        // Check settings
        Settings.Report();
        if (!Settings.IsValid())
        {
            Environment.Exit(-1);
            return;
        }
        
        // Setup exit logic
        Log.Message("Press CTRL+C to Exit");
        Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs e) {
            e.Cancel = true;
            _alive = false;
        };


        // Get existing running server, just in case its still there and this app failed?
        var pid = Monitor.GetPIDFromFile();
        if (pid != Monitor.BAD_PID && Monitor.IsValidPID(pid))
        {
            _monitor = new Monitor(pid);
            if (_monitor.IsValid())
            {
                Log.Message("Found valid server from PID file.");
            }
            else
            {
                _monitor = null;    
            }
        }

        // Main loop
        while (_alive)
        {
            // We dont have a server, launch it
            if (_monitor == null)
            {
                Start();
            }
            
            _monitor.Refresh();

            if (!_monitor.IsValid())
            {
                Log.Message($"Monitor has reported an issue, waiting {Settings.TimeoutSleepMilliseconds/1000} seconds to see if it resolves it self. Last good heartbeat was at {_lastHeartbeat.ToLongDateString()} on {_lastHeartbeat.ToLongTimeString()}");
                Thread.Sleep(Settings.TimeoutSleepMilliseconds);

                if (!_monitor.IsValid())
                {
                    Log.Message("Restarting!");
                    Shutdown();
                    continue;
                }
            }
            else
            {
                _lastHeartbeat = DateTime.Now;
            }
            
            Thread.Sleep(Settings.SleepMilliseconds);
        }

        Shutdown();
        Environment.Exit(0);
    }

    private static void Start()
    {
        Process startProcess = new Process();
        
        startProcess.StartInfo.WorkingDirectory = Settings.WorkingDirectory;
        startProcess.StartInfo.FileName = Settings.Application;
        startProcess.StartInfo.Arguments = Settings.Arguments;
        //startProcess.StartInfo.Environment

        startProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //startProcess.StartInfo.ErrorDialog = false;
        startProcess.StartInfo.CreateNoWindow = false;
        startProcess.StartInfo.UseShellExecute = true;
        
        startProcess.Start();
        Thread.Sleep(Settings.StartSleepMilliseconds);
        
        _monitor = new Monitor(startProcess);
        
        Log.Message($"Started with PID of {startProcess.Id.ToString()}");
        File.WriteAllText(Settings.ProcessInfoPath, startProcess.Id.ToString());
    }

    private static void Shutdown()
    {
        if (_monitor == null) return;
        
        _monitor.Kill();
        _monitor = null;
    }
}