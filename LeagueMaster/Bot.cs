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
        public static statusType status;

        #endregion

        public void BotManager()
        {
            Base.Write("Bot Initialized");
            GetStatus();

            System.Threading.Timer afkTimer = null;
            int ticks = 0;
            while (true)
            { 
                Thread.Sleep(5000);
                statusType oldStatus = status;
                GetStatus();
                
                Base.Write(ticks);

                //Ticks in the event one of our clicks does not register
                if (ticks == 0 || ticks == 20 || !oldStatus.Equals(status))
                {
                    GetStatus(true);
                    //dispose of timers
                    if (afkTimer != null) afkTimer.Dispose();

                    if (status.WindowStatus == WindowStatusType.Game)
                    {
                        if (status.GameStatus == GameStatusType.InProgress)
                        {
                            Base.Write("Starting/Resuming Anti-afk mouse movement in 5 seconds");
                            ActivateApplication(Base.gameName);
                            afkTimer = new System.Threading.Timer(AntiAfk, null, 5000, 1000);
                        }
                        else
                        { //in victory or defeat game screens
                            Base.Write("Leaving game screen...");
                            Cursor.Position = new Point(gameWindowDimensions.Left + 706, gameWindowDimensions.Top + 586);
                            ActivateApplication(Base.gameName);
                            Thread.Sleep(1000);
                            new InputSimulator().Mouse.LeftButtonClick();
                        }
                    }
                    else
                    { //in score or main client screens
                        Base.Write("Clicking \"Play Again\" in five seconds");
                        Cursor.Position = new Point(clientWindowDimensions.Left + 1155, clientWindowDimensions.Top + 743);
                        ActivateApplication(Base.clientName);
                        Thread.Sleep(5000);
                        new InputSimulator().Mouse.LeftButtonClick();
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
                    Cursor.Position = new Point(gameWindowDimensions.Left + 530, gameWindowDimensions.Top + 440);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 2:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 820, gameWindowDimensions.Top + 420);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 3:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 735, gameWindowDimensions.Top + 350);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
                case 4:
                    Cursor.Position = new Point(gameWindowDimensions.Left + 730, gameWindowDimensions.Top + 450);
                    new InputSimulator().Mouse.RightButtonClick();
                    break;
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
                if (true)
                {
                    status.ClientStatus = ClientStatusType.Queue;
                    if (print)
                    {
                        Base.Write("In game client, location unknown", ConsoleColor.Yellow);
                    }
                }
                else if (false) //todo
	            {
                    status.ClientStatus = ClientStatusType.ScoreScreen;
            	}
                else
                {
                    status.ClientStatus = ClientStatusType.Unqueued;
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


        // Sets the window to be foreground
        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        // Activate or minimize a window
        [DllImportAttribute("User32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        private void ActivateApplication(string briefAppName)
        {
            Process[] procList = Process.GetProcessesByName(briefAppName);

            if (procList.Length > 0)
            {
                ShowWindow(procList[0].MainWindowHandle, SW_RESTORE);
                SetForegroundWindow(procList[0].MainWindowHandle);
            }
        }
    }
}
