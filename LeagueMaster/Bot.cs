using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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
            PrintStatus();


            int ticks = 0;
            while (true)
            { 
                Thread.Sleep(5000);
                statusType oldStatus = status;
                GetStatus();
                AntiAfk(gameWindowDimensions);

                System.Threading.Timer afkTimer = null;

                if (!status.Equals(oldStatus) || ticks == 300)
                {
                    PrintStatus();
                    //dispose of timers
                    if (afkTimer != null) afkTimer.Dispose();

                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            //start timers
                            //do mouse stuff
                            Base.Write("Starting anti-afk mouse movement");
                            afkTimer = new System.Threading.Timer(AntiAfk, null, 500, 1000);
                            AntiAfk(gameWindowDimensions);
                            //wait
                        }
                        else
                        {
                            Cursor.Position = new Point(gameWindowDimensions.Left + 706, gameWindowDimensions.Top + 586);
                            DoRightMouseClick();
                        }
                    }
                    else
                    {
                        //client stuff
                    }
                }
                ticks = ticks + 1;
            }
        }

        static void AntiAfk(object state)
        {
            Random rand = new Random();
            switch (rand.Next(1, 4))
            {
                case 1:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 530, gameWindowDimensions.Top + 440);
                    DoRightMouseClick();
                    break;
                case 2:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 820, gameWindowDimensions.Top + 420);
                    DoRightMouseClick();
                    break;
                case 3:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 735, gameWindowDimensions.Top + 350);
                    DoRightMouseClick();
                    break;
                case 4:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 730, gameWindowDimensions.Top + 450);
                    DoRightMouseClick();
                    break;
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
                else if (Screen.testScreen("victory", gameWindowDimensions))
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
                            Base.Write("Game Over: Victory!", ConsoleColor.White);
                        }
                    }
                    else
                    {
                        Base.Write("Game in Progress");
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        static void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }
        static void DoRightMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
        }

    }
}
