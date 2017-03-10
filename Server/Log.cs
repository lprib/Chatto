using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace Server
{
    public static class Log
    {
        public static void LogTrace(string log) => LogColor(log, Blue);

        public static void LogInfo(string log) => LogColor(log, Green);

        public static void LogWarn(string log) => LogColor(log, Red);

        private static void LogColor(string str, ConsoleColor color)
        {
            var oringinalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = oringinalColor;
        }
    }
}
