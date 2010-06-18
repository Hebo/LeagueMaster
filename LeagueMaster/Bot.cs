using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace LeagueMaster
{
    public class Bot
    {
        #region Member Definitions
        static IntPtr clientWindowHandle;
        static RECT clientWindowDimensions;
        static IntPtr gameWindowHandle;
        static RECT gameWindowDimensions;
        
        public enum WindowStatusType { Client, Game };
        public enum GameStatusType { InProgress, Ended };
        public enum ClientStatusType { Unqueued, Queue, ScoreScreen };

        public struct statusType
        {
           public WindowStatusType WindowStatus;
           public GameStatusType GameStatus;
           public ClientStatusType ClientStatus;
        }
        public static statusType status;

        #endregion

        public void BotManager()
        {
            Base.Write("Bot Initialized");
            GetStatus();

            while (true)
            { 
                Thread.Sleep(1000);
                statusType oldStatus = status;
                GetStatus();

                if (!status.Equals(oldStatus))
                {
                    PrintStatus();
                    //dispose of timers



                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            //start timers
                            //do mouse stuff
                            //wait
                        }
                        else
                        {
                            //click end game
                        }
                    }
                    else
                    {
                        //client stuff
                    }
                }

            }
            

            
        }

        public static bool GetStatus()
        {

            if (Base.IsProcessOpen(Base.gameName))
            {
                gameWindowHandle = GetWindowHandle(Base.gameName);
                Win32.GetWindowRect(gameWindowHandle, out gameWindowDimensions);
                //Console.Write("Game: Handle " + gameWindowHandle + " @ " + gameWindowDimensions.Left + "*" + gameWindowDimensions.Top + "\n");
                status.WindowStatus = WindowStatusType.Game;
            }
            else
            {
                clientWindowHandle = GetWindowHandle(Base.clientName);
                Win32.GetWindowRect(gameWindowHandle, out clientWindowDimensions);
                //Console.Write("Client: Handle " + clientWindowHandle + " @ " + clientWindowDimensions.Left + "*" + clientWindowDimensions.Top + "\n");
                status.WindowStatus = WindowStatusType.Client;
            }

            if (status.WindowStatus == WindowStatusType.Game)
            {
                //start ocring to figure out wtf
                if (Screen.testScreen("defeat", gameWindowDimensions))
                {
                    status.GameStatus = GameStatusType.Ended;
                }
                else
                {
                    status.GameStatus = GameStatusType.InProgress;
                }

            }
            else
            {
                if (true)
                {
                    status.ClientStatus = ClientStatusType.Queue;
                }
                else if (false)
	            {
                    status.ClientStatus = ClientStatusType.ScoreScreen;
            	}
                else
                {
                    status.ClientStatus = ClientStatusType.ScoreScreen;
                }

            }

            //save status

            return true;
        }

        public static void PrintStatus()
        {
            if (GetStatus())
            {
                if (status.WindowStatus == WindowStatusType.Game)
                {
                    if (status.GameStatus == GameStatusType.Ended)
                    {
                        if (Screen.testScreen("defeat", gameWindowDimensions))
                        {
                            Base.Write("Game Over: Defeat!", ConsoleColor.White);
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
                    if (status.ClientStatus == ClientStatusType.Queue)
                    {
                        Base.Write("Client: In match queue");
                    }
                    else if (status.ClientStatus == ClientStatusType.Unqueued)
                    {
                        Base.Write("Client: Not In match queue");
                    }
                    else
                    {
                        Base.Write("Client: Score Screen");
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
