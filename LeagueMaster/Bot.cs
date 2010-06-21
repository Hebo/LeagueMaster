﻿using System;
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
        public enum ClientStatusType { Unqueued, Queue, ScoreScreen, LevelUp };

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

            bool actionNeeded = false;

            int ticks = 0;
            while (true)
            {
                Thread.Sleep(5000);
                statusType oldStatus = status;
                actionNeeded = GetStatus();
            
                //Ticks in the event one of our clicks does not register
                if ( ticks == 0 || ticks == 30 || !oldStatus.Equals(status) || actionNeeded)
                {
                    if (ticks == 30) actionNeeded = GetStatus(false);
                    else actionNeeded = GetStatus(true);
      
                    //dispose of timers
                    if (afkTimer != null) afkTimer.Dispose();
                    if (surrenderTimer != null) surrenderTimer.Dispose();

                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            Base.Write("(Re)Starting Anti-AFK & Auto-Surrender in 10 seconds", ConsoleColor.White);

                            afkTimer = new System.Threading.Timer(AntiAfk, null, 15000, 10000);
                            surrenderTimer = new System.Threading.Timer(AttemptSurrender, null, 35000, 35000);
                        }
                        else
                        { //in victory or defeat game screens
                            Base.Write("Leaving game screen...");
                            Bot.BringWindowToTop(Base.gameWindowName, false);
#if DEFCON
                            Cursor.Position = RelativePoint(gameWindowDimensions, 710, 583);
#endif
#if MINI
                            Cursor.Position = RelativePoint(gameWindowDimensions, 502, 416);
#endif
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();
                        }
                    }
                    else
                    {
                        if (status.ClientStatus == ClientStatusType.LevelUp)
                        {
                            Base.Write("Clearing popup", ConsoleColor.White);
#if DEFCON
                    Cursor.Position = RelativePoint(clientWindowDimensions, 640, 519);
#endif
#if MINI
                    Cursor.Position = RelativePoint(clientWindowDimensions, 514, 439);
#endif
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();

                        }
                        if (status.ClientStatus == ClientStatusType.ScoreScreen)
                        {
                            Base.Write("Clicking \"Play Again\" in a moment");
                            BringWindowToTop(Base.clientWindowName, true);

#if DEFCON
                            Cursor.Position = RelativePoint(clientWindowDimensions, 1160, 740);
#endif
#if MINI
                            Cursor.Position = RelativePoint(clientWindowDimensions, 925, 595);
#endif

                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();

                        }
                        else if (status.ClientStatus == ClientStatusType.Unqueued)
                        {
                            Base.Write("Clicking \"Play Again\" in a moment");
                            BringWindowToTop(Base.clientWindowName, true);

#if DEFCON
                            Cursor.Position = RelativePoint(clientWindowDimensions, 1160, 740);
#endif
#if MINI
                            Cursor.Position = RelativePoint(clientWindowDimensions, 1160, 740);
#endif
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();
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
#if DEFCON
                            Cursor.Position = RelativePoint(gameWindowDimensions, 575, 483);
#endif
#if MINI
            Cursor.Position = RelativePoint(gameWindowDimensions, 458, 332);
#endif
                new InputSimulator().Mouse.RightButtonClick();
        }

        static void AttemptSurrender(object state)
        {
            Base.Write("Attempting Surrender");
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
            bool actionNeeded = false;
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
                if (Screen.testScreen("score", clientWindowDimensions))
                {
                    status.ClientStatus = ClientStatusType.ScoreScreen;  
                    if (print)
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

        static public Point RelativePoint(RECT dimensions, int x, int y)
        {
            var abolutePoint = new Point(dimensions.Left + x, dimensions.Top + y);
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
