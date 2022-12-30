using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata.Ecma335;

namespace Mona;

public class Monitor
{
    public const int BAD_PID = -1;
    
    private Process _process;
    public Monitor(int pid)
    {
        _process = Process.GetProcessById(pid);
    }

    public Monitor(Process process)
    {
        _process = process;
    }

    public bool IsValid()
    {
        if (_process == null) return false;

        if (Program.Settings.CheckResponding && !_process.Responding)
        {
            return false;
        }

        if (Program.Settings.CheckHasExited && _process.HasExited)
        {
            return false;
        }

        return true;
    }

    public void Kill()
    {
        _process.Kill(true);
    }
    
    public static int GetPIDFromFile()
    {
        Log.Message($"Attempt to get PID from {Program.Settings.ProcessInfoPath} ...");
        int pid = BAD_PID;

        // Look for ProcessInfoPath
        if (File.Exists(Program.Settings.ProcessInfoPath) && int.TryParse(File.ReadAllText(Program.Settings.ProcessInfoPath).Trim(), out pid))
        {
            Log.Message($"PID found: {pid}");
        }
        else
        {
            Log.Message("PID not found.");
        }
        return pid;
    }

    public static bool IsValidPID(int pid)
    {
        try
        {
            Process.GetProcessById(pid);
        }
        catch
        {
            return false;
        }

        return true;
    }
}