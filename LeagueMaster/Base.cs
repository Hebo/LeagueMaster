using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace LeagueMaster
{
    class Base
    {
        public const string clientName = "LolClient";
        public const string gameName = "League of Legends";
        public const string clientWindowName = "PVP.net Client";
        public const string gameWindowName = "League of Legends (TM) Client";
        
        static Thread oThread;
        static bool closing = false;

        static void Main(string[] args)
        {
            Console.Title = "League Master v1.5";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------------------");
            Console.WriteLine("League M4ster: IT'S OVER 9000");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("");
            ResetConsoleColor();

            //confirm lol is running
            if (!IsProcessOpen(clientName) && !IsProcessOpen(gameName))
            {
                Write("Error: League of Legends is closed", ConsoleColor.Red);
                
                ConsoleKeyInfo ck;
                ck = Console.ReadKey(true);
                return;
            }

            if (!IsProcessOpen(gameName))
            {
                Bot.BringWindowToTop(Base.clientWindowName, true);
            }
            
            Write("Ensure League of Legends is in the queue or a game");

            //start bot interraction process
            var myBot = new Bot();
            oThread = new Thread(new ThreadStart(myBot.BotManager));
            oThread.Start();
            Thread.Sleep(1000);

            Write("Press A to abort after the current game ends", ConsoleColor.Yellow);
            Write("Press Q to quit immediately", ConsoleColor.Yellow);

            ConsoleKey key = Console.ReadKey(true).Key;
            while (key != ConsoleKey.Q && !closing)
            {
                if (Console.KeyAvailable)
                {
                    key = Console.ReadKey(true).Key;
                }
                if (key == ConsoleKey.A && !myBot.Abort)
                {
                    myBot.Abort = true;
                    Write("Aborting after the current game ends", ConsoleColor.White);
                }
                Thread.Sleep(1000);
            }

            Close();
        }
      
        public static void Close()
        {
            closing = true;
            oThread.Abort();
            Environment.Exit(0);
        }

        public static void Write(object msg, ConsoleColor c = ConsoleColor.Gray)
        {
            // Create a writer and open the file:
            StreamWriter log;

            if (!File.Exists("LeagueMaster.log"))
            {
                log = new StreamWriter("LeagueMaster.log");
            }
            else
            {
                log = File.AppendText("LeagueMaster.log");
            }

            // Write to the file:
            log.Write("[" + DateTime.Now + "] ");
            log.WriteLine(msg);
            
            log.Close(); // Close the stream:

            WriteTimeStamp();
            Console.ForegroundColor = c;
            dynamic dynMsg = msg;
            Console.WriteLine(dynMsg);
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
