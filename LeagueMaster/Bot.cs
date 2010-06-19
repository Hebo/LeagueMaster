using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;


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
        public static statusType status = new statusType();

        #endregion

        public void BotManager()
        {
            Base.Write("Bot Initialized", ConsoleColor.White);

            System.Threading.Timer afkTimer = null;
            System.Threading.Timer surrenderTimer = null;

            int ticks = 0;
            while (true)
            { 
                Thread.Sleep(5000);
                statusType oldStatus = status;
                GetStatus();

                //Ticks in the event one of our clicks does not register
                if (ticks == 0 || ticks == 20 || !oldStatus.Equals(status))
                {
                    GetStatus(true);
                    //dispose of timers
                    if (afkTimer != null) afkTimer.Dispose();
                    if (surrenderTimer != null) surrenderTimer.Dispose();

                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            Base.Write("(Re)Starting Anti-AFK & Auto-Surrender in 30 seconds", ConsoleColor.White);
                            Bot.BringWindowToTop(Base.gameWindowName, false);
                            afkTimer = new System.Threading.Timer(AntiAfk, null, 30000, 10000);
                            surrenderTimer = new System.Threading.Timer(AttemptSurrender, null, 100000, 35000);
                        }
                        else
                        { //in victory or defeat game screens
                            Base.Write("Leaving game screen...");

                            Cursor.Position = RelativePoint(gameWindowDimensions, 0.504285714, 0.631111111);
                            new InputSimulator().Mouse.LeftButtonClick();
                            Bot.BringWindowToTop(Base.gameWindowName, false);
                            Thread.Sleep(1000);
                            FuzzyClick();
                        }
                    }
                    else
                    {
                        if (status.ClientStatus == ClientStatusType.ScoreScreen)
                        {
                            Base.Write("Clicking \"Play Again\" in a moment");
                            Cursor.Position = RelativePoint(clientWindowDimensions, 0.90625, 0.93125);
                            BringWindowToTop(Base.clientWindowName, false);
                            Thread.Sleep(3000);
                            FuzzyClick(); 
                        }
                        else if (status.ClientStatus == ClientStatusType.Unqueued)
                        {
                            Base.Write("Clicking \"Play Again\" in a moment");
                            Cursor.Position = RelativePoint(clientWindowDimensions, 0.90625, 0.93125);
                            BringWindowToTop(Base.clientWindowName, true);
                            Thread.Sleep(3000);
                            FuzzyClick(); 
                        }
                        else
                        {
                            //queued, do nothing but wait
                            Base.Write("Waiting on queue...");
                        }
                    }
                   ticks = 1;
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
                    Cursor.Position = RelativePoint(gameWindowDimensions, 0.368055556, 0.488888889);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 2:
                    Cursor.Position = RelativePoint(gameWindowDimensions, 0.569444444, 0.466666667);;
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 3:
                    Cursor.Position = RelativePoint(gameWindowDimensions, 0.510416667, 0.388888889);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 4:
                    Cursor.Position = RelativePoint(gameWindowDimensions, 0.506944444, 0.5);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
            }
        }

        static void AttemptSurrender(object state)
        {
            Base.Write("Attempting Surrender", ConsoleColor.White);
            new InputSimulator().Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            Thread.Sleep(500);
            new InputSimulator().Keyboard.TextEntry("/surrender");
            Thread.Sleep(500);
            new InputSimulator().Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
        }

        // if we enter a fail state, detect it so we can recover 
        static void DetectFailState(object state)
        {
            if (Screen.testScreen("itemshop", gameWindowDimensions))
            {
                Base.Write("Fail State: Item Shop", ConsoleColor.Yellow);
                //recover
                new InputSimulator().Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
            }
        }

        public static bool GetStatus( bool print = false )
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
                Win32.GetWindowRect(clientWindowHandle, out clientWindowDimensions);
                //Console.Write("Client: Handle " + clientWindowHandle + " @ " + clientWindowDimensions.Left + "*" + clientWindowDimensions.Top + "\n");
                status.WindowStatus = WindowStatusType.Client;
            }

            if (status.WindowStatus == WindowStatusType.Game)
            {
                //start ocring to figure out wtf
                if (Screen.testScreen("defeat", gameWindowDimensions))
                {
                    status.GameStatus = GameStatusType.Ended;
                    if (print)
                    {
                        Base.Write("Game Over: Defeat!", ConsoleColor.Yellow);
                    }

                }
                else if (Screen.testScreen("victory", gameWindowDimensions))
                {
                    status.GameStatus = GameStatusType.Ended;
                    if (print)
                    {
                       Base.Write("Game Over: Victory!", ConsoleColor.Yellow);
                    }
                    
                }
                else
                {
                    status.GameStatus = GameStatusType.InProgress;
                    if (print)
                    {
                        Base.Write("Game in progress", ConsoleColor.Yellow);
                    }
                }

            }
            else
            {
                if (Screen.testScreen("score", clientWindowDimensions))
                {
                    status.ClientStatus = ClientStatusType.ScoreScreen;  
                    if (print)
                    {
                        Base.Write("In game client: Score Screen", ConsoleColor.White);
                    }
                }
                else if (Screen.testScreen("levelup", clientWindowDimensions))
	            {
                    if (print)
                    {
                        Base.Write("Level up!", ConsoleColor.Yellow);
                        Base.Write("Clearing popup", ConsoleColor.White);
                    }

                    Cursor.Position = RelativePoint(clientWindowDimensions, 0.5, 0.6475);
                    Thread.Sleep(1000);
                    new InputSimulator().Mouse.LeftButtonClick();

                    status.ClientStatus = ClientStatusType.Unqueued;
            	}
                else if (false) //todo
	            {
                    status.ClientStatus = ClientStatusType.Unqueued;
            	}
                else
                {
                    status.ClientStatus = ClientStatusType.Queue;
                    if (print)
                    {
                        Base.Write("In game client: Queued", ConsoleColor.White);
                    }
                }

            }

            //save status

            return true;
        }

        static IntPtr GetWindowHandle( string name )
        {
            Process[] processes = Process.GetProcessesByName(name);
            IntPtr pFoundWindow = processes[0].MainWindowHandle;
            return pFoundWindow;
        }

        static public Point RelativePoint(RECT dimensions, double xPct, double yPct)
        {
            //Base.Write("Width:" + dimensions.Width + "*" + xPct);
            int x = (int)((double)dimensions.Width * xPct);
            int y = (int)((double)dimensions.Height * yPct);

            var abolutePoint = new Point(dimensions.Left + x, dimensions.Top + y);

            //Base.Write("Calculated:" + abolutePoint.ToString());
            return abolutePoint;
        }

        //click multiple times in an area incase we miss
        static public void FuzzyClick( bool rightClick = false )
        {
            

                Point[] Coords = {
                                     new Point(0, 0),
                                     new Point(12, 0),
                                     new Point(0, 12),
                                     new Point(0, -12),
                                     new Point(-12, 0),
                                 };

                foreach (var pos in Coords)
                {
                    Cursor.Position = new Point(Cursor.Position.X + pos.X, Cursor.Position.Y + pos.Y);
                    Thread.Sleep(100);
                    if (rightClick == false)
                    {
                    new InputSimulator().Mouse.LeftButtonClick();
                    }
                    else
	{
                new InputSimulator().Mouse.RightButtonClick();
	}
                    Cursor.Position = new Point(Cursor.Position.X - pos.X, Cursor.Position.Y - pos.Y);
                }
            

            
        }


        #region P/Invoke

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        // You can also call FindWindow(default(string), lpWindowName) or FindWindow((string)null, lpWindowName)


        public static bool BringWindowToTop(string windowName, bool wait)
        {
            int hWnd = FindWindow(windowName, wait);
            if (hWnd != 0)
            {
                return SetForegroundWindow((IntPtr)hWnd);
            }
            return false;
        }

        // THE FOLLOWING METHOD REFERENCES THE FindWindowAPI
        public static int FindWindow(string windowName, bool wait)
        {
            int hWnd = (int) FindWindow(null, windowName);
            while (wait && hWnd == 0)
            {
                System.Threading.Thread.Sleep(500);
                hWnd = (int) FindWindow(null, windowName);
            }

            return hWnd;
        }




        #endregion
    }
}
