using System;
using System.Configuration;
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

        public bool Abort { get; set; }
        private bool _surrender;

        public enum WindowStatusType { Client, Game };
        public enum GameStatusType { InProgress, Ended };
        public enum ClientStatusType { Unqueued, Queue, ScoreScreen, LevelUp };

        public struct statusType
        {
           public WindowStatusType WindowStatus;
           public GameStatusType GameStatus;
           public ClientStatusType ClientStatus;
        }
        public static statusType status = new statusType();

        static public Position positions;
        #endregion

        public void BotManager()
        {
            _surrender = Convert.ToBoolean( ConfigurationManager.AppSettings["enable_surrender"] );
            if (_surrender)
                Base.Write("Automatic Surrender: Enabled", ConsoleColor.White);
            else
                Base.Write("Automatic Surrender: Disabled", ConsoleColor.White);

            try
            {
               positions = new Position();
               Base.Write("Bot initialized with resolution " + positions.resolution, ConsoleColor.White);
            }
            catch (Exception e)
            {
                Base.Write(e);
                System.Environment.Exit(1);
            }

            System.Threading.Timer afkTimer = null;
            System.Threading.Timer surrenderTimer = null;

            bool actionNeeded = false;

            while (true)
            {
                Thread.Sleep(5000);
                statusType oldStatus = status;
                actionNeeded = GetStatus();
            
                if ( !oldStatus.Equals(status) || actionNeeded)
                {
                    actionNeeded = GetStatus(true);
      
                    //dispose of timers
                    if (afkTimer != null) afkTimer.Dispose();
                    if (surrenderTimer != null) surrenderTimer.Dispose();

                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            if (_surrender)
                            {
                                Base.Write("Starting Anti-AFK and Auto-Surrender", ConsoleColor.White);
                                afkTimer = new System.Threading.Timer(AntiAfk, null, 15000, 10000);
                                surrenderTimer = new System.Threading.Timer(AttemptSurrender, null, 60000, 60000);
                            }
                            else
                            {
                                Base.Write("Starting Anti-AFK", ConsoleColor.White);
                                afkTimer = new System.Threading.Timer(AntiAfk, null, 15000, 10000);
                            }
                        }
                        else
                        { //in victory or defeat game screens
                            Base.Write("Leaving game screen...");

                            Cursor.Position = RelativePoint(gameWindowDimensions, positions.Get("end_game_button"));
                            BringWindowToTop(Base.gameWindowName, false);
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();
                        }
                    }
                    else
                    {
                        if (status.ClientStatus == ClientStatusType.LevelUp)
                        {
                            Base.Write("Clearing popup", ConsoleColor.White);
                            BringWindowToTop(Base.clientWindowName, true);
                            Cursor.Position = RelativePoint(clientWindowDimensions, positions.Get("level_up_button"));
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();

                        }
                        if (status.ClientStatus == ClientStatusType.ScoreScreen)
                        {
                            if (Abort)
                            {
                                Console.Beep();
                                Base.Write("Game Over. Aborting...");
                                Base.Close();
                            }
                            Base.Write("Clicking \"Play Again\" in a moment");
                            BringWindowToTop(Base.clientWindowName, true);
                            Cursor.Position = RelativePoint(clientWindowDimensions, positions.Get("play_again_button"));
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();

                        }
                        else if (status.ClientStatus == ClientStatusType.Unqueued)
                        {
                            if (Abort)
                            {
                                Console.Beep();
                                Base.Write("Game Over. Aborting...");
                                Base.Close();
                            }
                            Base.Write("Clicking \"Play Again\" in a moment");
                            BringWindowToTop(Base.clientWindowName, true);
                            Cursor.Position = RelativePoint(clientWindowDimensions, positions.Get("play_again_button"));
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();
                        }
                        else
                        {
                            if (Abort)
                            {
                                Console.Beep();
                                Base.Write("Game Over. Aborting...");
                                Base.Close();
                            }
                            //queued, do nothing but wait
                            Base.Write("Waiting on queue...");
                        }
                    }
               }
            }   
        }

        static void AntiAfk(object state)
        {
              Cursor.Position = RelativePoint(gameWindowDimensions, positions.Get("anti_afk"));
              BringWindowToTop(Base.gameWindowName, false);
              Thread.Sleep(500);
                
              new InputSimulator().Mouse.RightButtonClick();
        }

        static void AttemptSurrender(object state)
        {
#if DEBUG
            Base.Write("Attempting Surrender");
#endif
            Cursor.Position = RelativePoint(gameWindowDimensions, positions.Get("end_game_button"));
            BringWindowToTop(Base.gameWindowName, false);
            Thread.Sleep(500);
            new InputSimulator().Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            Thread.Sleep(1000);
            new InputSimulator().Keyboard.TextEntry("/surrender");
            Thread.Sleep(1000);
            new InputSimulator().Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
        }

        // if we enter a fail state, detect it so we can recover (UNIMPLEMENTED)
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
            bool actionNeeded = false;
            if (Base.IsProcessOpen(Base.gameName))
            {
                gameWindowHandle = GetWindowHandle(Base.gameName);
                Win32.GetWindowRect(gameWindowHandle, out gameWindowDimensions);
                status.WindowStatus = WindowStatusType.Game;
            }
            else
            {
                clientWindowHandle = GetWindowHandle(Base.clientName);
                Win32.GetWindowRect(clientWindowHandle, out clientWindowDimensions);
                status.WindowStatus = WindowStatusType.Client;
            }

            if (status.WindowStatus == WindowStatusType.Game)
            {
                //start imaging to figure out wtf
                if (Screen.testScreen("defeat", gameWindowDimensions))
                {
                    status.GameStatus = GameStatusType.Ended;
                    if (print)
                    {
                        Base.Write("Game Over: Defeat!", ConsoleColor.Yellow);
                    }
                    actionNeeded = true;

                }
                else if (Screen.testScreen("victory", gameWindowDimensions))
                {
                    status.GameStatus = GameStatusType.Ended;
                    if (print)
                    {
                       Base.Write("Game Over: Victory!", ConsoleColor.Yellow);
                    }
                    actionNeeded = true;
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
                if (Screen.testScreen("score", clientWindowDimensions) || Screen.testScreen("score_disconnect", clientWindowDimensions))
                {
                    status.ClientStatus = ClientStatusType.ScoreScreen;
                    if (print && Screen.testScreen("score_disconnect", clientWindowDimensions))
                    {
                        Base.Write("In game client: Score Screen (PVP.net chat disconnected)", ConsoleColor.White);
                    }
                    else if (print)
                    {
                        Base.Write("In game client: Score Screen", ConsoleColor.White);
                    }
                    actionNeeded = true;
                }
                else if (Screen.testScreen("levelup", clientWindowDimensions))
	            {
                    status.ClientStatus = ClientStatusType.LevelUp;
                    if (print)
                    {
                        Base.Write("Level up!", ConsoleColor.Yellow);
                    }
                    actionNeeded = true;
            	}
                else if (false) //todo
	            {
                    status.ClientStatus = ClientStatusType.Unqueued;
                    actionNeeded = true;
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
            return actionNeeded;
        }

        static IntPtr GetWindowHandle( string name )
        {
            Process[] processes = Process.GetProcessesByName(name);
            IntPtr pFoundWindow = processes[0].MainWindowHandle;
            return pFoundWindow;
        }
        
        static public Point RelativePoint(RECT dimensions, Position.positionType pos)
        {
            var abolutePoint = new Point(dimensions.Left + pos.x, dimensions.Top + pos.y);
            return abolutePoint;
        }

        //click multiple times in an area incase we miss (not used)
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
