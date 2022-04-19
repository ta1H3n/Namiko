using Discord;
using System;

namespace Namiko
{
    public static class Logger
    {
        public static void Log(string msg, LogSeverity logSeverity = LogSeverity.Info)
        {
            Console.WriteLine(String.Format("{0,-9}{1,-10}{2}", logSeverity, DateTime.Now.ToString("HH:mm:ss"), msg));
        }
    }
}
