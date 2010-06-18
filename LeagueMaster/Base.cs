using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LeagueMaster
{
    class Base
    {
        public const string clientName = "LolClient";
        public const string gameName = "League of Legends";

        static void Main(string[] args)
        {
            Console.Title = "League Master";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------------------");
            Console.WriteLine("League M4ster: IT'S OVER 9000");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("");
            ResetConsoleColor();

            //confirm lol is running
            if ( !IsProcessOpen( clientName ) )
            {
                WriteTimeStamp();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fatal Error: League of Legends is not open");
                ResetConsoleColor();
                return;
            }
            

            Bot.GetStatus();
            Bot.PrintStatus();

            //do next move (begin launch)

            //start bot interraction process

            //wait  on close key


        }

        public static void Write(ConsoleColor c, string msg, params object[] args)
        {
            WriteTimeStamp();
            Console.ForegroundColor = c;
            Console.WriteLine(msg, args);
            ResetConsoleColor();
        }

        
        
        public static void WriteTimeStamp()
        {
            string theTime = DateTime.Now.ToString("MM/dd hh:mm:ss tt");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[" + theTime + "] ");
            ResetConsoleColor();
        }

        public static bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        public static void ResetConsoleColor()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            return;
        }
    }
}
