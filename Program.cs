using System.Diagnostics;
using Mona;
using Monitor = Mona.Monitor;

internal class Program
{
    
    public static Config Settings = null;
    
    static bool _alive = true;
    private static Monitor _monitor = null;


    public static void Main(string[] args)
    {
        Log.Clear();

        // Handle custom config path as an argument
        if (args.Length > 1 && File.Exists(args[1]))
        {
            Settings = Config.Get(args[1]);
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

            if (!_monitor.IsValid())
            {
                Log.Message($"Monitor has reported an issue, waiting {Settings.TimeoutSleepMilliseconds} to see if it resolves it self.");
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
                Log.Message("Heartbeat");    
            }
            
            Thread.Sleep(Settings.SleepMilliseconds);
        }

        Shutdown();
        Environment.Exit(0);
    }

    private static void Start()
    {
        Process startProcess = new Process();

        startProcess.StartInfo = new ProcessStartInfo(Settings.Application, Settings.Arguments);
        startProcess.StartInfo.WorkingDirectory = Settings.WorkingDirectory;
        //startProcess.StartInfo.Environment
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