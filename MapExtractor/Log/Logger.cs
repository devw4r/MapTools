// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Diagnostics;

namespace AlphaCoreExtractor.Log
{
    public static class Logger
    {
        private static Stopwatch ProgressRate = new Stopwatch();

        public static void Info(string message)
        {
            Console.WriteLine($"{"[INFORMATION]"} {message}");
        }

        public static void Notice(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{"[NOTICE]"} {message}");
            Console.ResetColor();
        }

        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{"[SUCCESS]"} {message}");
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{"[ERROR]"} {message}");
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{"[WARNING]"} {message}");
            Console.ResetColor();
        }

        public static void Progress(Int64 processed, Int64 total, int rateMs = 0)
        {
            Int64 progress = processed * 100 / total;

            if (rateMs > 0 && !ProgressRate.IsRunning)
                ProgressRate.Restart();

            if ((rateMs > 0 && ProgressRate.ElapsedMilliseconds < rateMs) && progress != 100)
                return;
            else if(rateMs > 0 && progress != 100)
                ProgressRate.Restart();

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{"[PROGRESS]"} {progress}%");
            Console.ResetColor();

            if (progress == 100)
            {
                Console.Write(Environment.NewLine);
                ProgressRate.Stop();
            }
        }
    }
}
