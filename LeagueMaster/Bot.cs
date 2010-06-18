using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LeagueMaster
{
    class Bot
    {
        #region Member Definitions
        static IntPtr clientWindowHandle;
        static RECT clientWindowDimensions;
        static IntPtr gameWindowHandle;
        static RECT gameWindowDimensions;

        enum WindowStatusType { Client, Game };
        static WindowStatusType WindowStatus;

        enum GameStatusType { InProgress, Ended };
        static GameStatusType GameStatus;

        enum ClientStatusType { Queue, ScoreScreen };
        static ClientStatusType ClientStatus;
        #endregion

        public static bool GetStatus()
        {
            Console.Write("Retrieving status...   ");

            if (Base.IsProcessOpen(Base.gameName))
            {
                gameWindowHandle = GetWindowHandle(Base.gameName);
                Win32.GetWindowRect(gameWindowHandle, out gameWindowDimensions);
                Console.Write("Game: Handle " + gameWindowHandle + " @ " + gameWindowDimensions.Left + "*" + gameWindowDimensions.Top + "\n");
                WindowStatus = WindowStatusType.Game;
            }
            else
            {
                clientWindowHandle = GetWindowHandle(Base.clientName);
                Win32.GetWindowRect(gameWindowHandle, out clientWindowDimensions);
                Console.Write("Client: Handle " + clientWindowHandle + " @ " + clientWindowDimensions.Left + "*" + clientWindowDimensions.Top + "\n");
                WindowStatus = WindowStatusType.Client;
            }

            if (WindowStatus == WindowStatusType.Game)
            {
                //start ocring to figure out wtf
                if (Screen.testScreen("defeat", gameWindowDimensions))
                {
                    Console.WriteLine("defeet!!!!");
                    GameStatus = GameStatusType.Ended;
                }
                else
                {
                    Console.WriteLine("asdfasdf!!!!");
                    GameStatus = GameStatusType.InProgress;
                }

            }

            //save status

            return true;
        }

        public static void PrintStatus()
        {
            if (GetStatus())
            {
                if (WindowStatus == WindowStatusType.Game)
                {
                    if (GameStatus == GameStatusType.Ended)
                    {
                        if (Screen.testScreen("defeat", gameWindowDimensions))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Base.WriteTimeStamp();
                            Console.WriteLine("Game Over: Defeat!");
                            Base.ResetConsoleColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Base.WriteTimeStamp();
                            Console.WriteLine("Game Over: Victory!");
                            Base.ResetConsoleColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Base.WriteTimeStamp();
                        Console.WriteLine("Game in Progress");
                        Base.ResetConsoleColor();
                    }
                }
                else
                {
                    if (ClientStatus == ClientStatusType.Queue)
                    {
                        Base.Write(ConsoleColor.Yellow, "Client: In match queue");
                    }
                    else
                    {
                        Base.Write(ConsoleColor.Yellow, "Client: Not In match queue");
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Unable to read status");
                Base.ResetConsoleColor();
            }
        }

        static IntPtr GetWindowHandle( string name )
        {
            Process[] processes = Process.GetProcessesByName(name);
            IntPtr pFoundWindow = processes[0].MainWindowHandle;
            return pFoundWindow;
        }


    }
}
