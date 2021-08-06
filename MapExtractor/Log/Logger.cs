﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Log
{
    public static class Logger
    {
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

        public static void Progress(int progress)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{"[PROGRESS]"} {progress}%");
            Console.ResetColor();

            if(progress == 100)
                Console.Write(Environment.NewLine);
        }
    }
}
