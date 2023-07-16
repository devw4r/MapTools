﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Diagnostics;

namespace AlphaCoreExtractor.Log
{
    public static class Logger
    {
        private static readonly object _writeLock = new object();
        private static Stopwatch ProgressRate = new Stopwatch();

        public static void Info(string message)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{"[INFORMATION]"} {message}");
                Console.ResetColor();
            }
        }

        public static void Notice(string message)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{"[NOTICE]"} {message}");
                Console.ResetColor();
            }
        }

        public static void Success(string message)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{"[SUCCESS]"} {message}");
                Console.ResetColor();
            }
        }

        public static void Error(string message)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{"[ERROR]"} {message}");
                Console.ResetColor();
            }
        }

        public static void Warning(string message)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{"[WARNING]"} {message}");
                Console.ResetColor();
            }
        }

        public static void Progress(string label, Int64 processed, Int64 total, int updateRate = 0)
        {
            lock (_writeLock)
            {
                Int64 progress = processed * 100 / total;

                if (updateRate > 0 && !ProgressRate.IsRunning)
                    ProgressRate.Restart();

                if ((updateRate > 0 && ProgressRate.ElapsedMilliseconds < updateRate) && progress != 100)
                    return;
                else if (updateRate > 0 && progress != 100)
                    ProgressRate.Restart();

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[PROGRESS] {label} - {progress}%");
                Console.ResetColor();

                if (progress == 100)
                {
                    Console.Write(Environment.NewLine);
                    ProgressRate.Stop();
                }
            }
        }
    }
}
