using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace LeagueMaster
{
    class Base
    {
        public const string clientName = "LolClient";
        public const string gameName = "League of Legends";
        
        static Thread oThread;

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
                Write("Fatal Error: League of Lols is not open", ConsoleColor.Red);
                
                ConsoleKeyInfo ck;
                ck = Console.ReadKey(true);
                return;
            }
            
            //start bot interraction process
            var myBot = new Bot();
            oThread = new Thread(new ThreadStart(myBot.BotManager));
            oThread.Start();
            Thread.Sleep(1000);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Press Q to quit.");
            Console.ForegroundColor = ConsoleColor.Gray;

            while (Console.ReadKey(true).Key != ConsoleKey.Q );

            Close();
        }
      
        static void Close()
        {
            oThread.Abort();
            Console.Beep();
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
