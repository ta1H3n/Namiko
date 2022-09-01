using System;
using System.Threading.Tasks;
using Discord;

namespace Namiko.Handlers.Services;

public class Logger
{
    public Task Console_Log(LogMessage arg)
    {
        Log(arg.ToString(), arg.Severity);
        return Task.CompletedTask;
    }
    
    public static void Log(string msg, LogSeverity logSeverity = LogSeverity.Info)
    {
        Console.WriteLine(String.Format("{0,-9}{1,-10}{2}", logSeverity, DateTime.Now.ToString("HH:mm:ss"), msg));
    }
}