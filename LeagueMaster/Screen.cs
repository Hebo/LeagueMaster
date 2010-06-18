using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;


namespace LeagueMaster
{
    class Screen
    {
        public static bool testScreen(string screenName, RECT dimensions)
        {
            var scr = Screens[screenName];

            foreach (PatternType pix in scr)
            {
                //add offset in window to window's location on screen
                Color sample = Win32.GetPixelColor(dimensions.Left + pix.relativeX, dimensions.Top + pix.relativeY);

                //Console.WriteLine(sample.ToString() + "@" + (dimensions.Left + pix.relativeX) + "x" + (dimensions.Top + pix.relativeY) + " v " + pix.pixelColor.ToString());
                if (sample != pix.pixelColor)
                {
                    return false;
                }
            }
            return true;
        }

        #region screens
        private static readonly Dictionary<string, PatternType[]> Screens
            = new Dictionary<string, PatternType[]> 
        {
            //middle of E on defeat screen
            {"defeat", new PatternType[] { new PatternType(631, 251, Color.FromArgb(170, 3, 3)) }},
            {"victory", new PatternType[] { new PatternType(692, 310, Color.FromArgb(255, 244, 106)) }}, //T in victory
        };



        class PatternType
        {
            public int relativeX;
            public int relativeY;
            public Color pixelColor;

            public PatternType(int x, int y, Color col)
            {
                relativeX = x;
                relativeY = y;
                pixelColor = col;
            }
        }

        #endregion
    }

        #region Win32 Functions
        sealed class Win32
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static public extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            static extern IntPtr GetDC(IntPtr hwnd);

            [DllImport("user32.dll")]
            static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

            static public System.Drawing.Color GetPixelColor(int x, int y)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                uint pixel = GetPixel(hdc, x, y);
                ReleaseDC(IntPtr.Zero, hdc);
                Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                             (int)(pixel & 0x0000FF00) >> 8,
                             (int)(pixel & 0x00FF0000) >> 16);
                return color;
            }
        }
        #endregion

        #region RECT Definition
        // add a reference to system.drawing.dll
        // Note: backing fields were added because structs don't automatically supply them.
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            int _left;
            int _top;
            int _right;
            int _bottom;

            public RECT(global::System.Drawing.Rectangle rectangle)
                : this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom)
            {
            }
            public RECT(int left, int top, int right, int bottom)
            {
                _left = left;
                _top = top;
                _right = right;
                _bottom = bottom;
            }

            public int X
            {
                get { return Left; }
                set { Left = value; }
            }
            public int Y
            {
                get { return Top; }
                set { Top = value; }
            }
            public int Left
            {
                get { return _left; }
                set { _left = value; }
            }
            public int Top
            {
                get { return _top; }
                set { _top = value; }
            }
            public int Right
            {
                get { return _right; }
                set { _right = value; }
            }
            public int Bottom
            {
                get { return _bottom; }
                set { _bottom = value; }
            }
            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value - Top; }
            }
            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }
            public global::System.Drawing.Point Location
            {
                get { return new global::System.Drawing.Point(Left, Top); }
                set
                {
                    Left = value.X;
                    Top = value.Y;
                }
            }
            public global::System.Drawing.Size Size
            {
                get { return new global::System.Drawing.Size(Width, Height); }
                set
                {
                    Right = value.Width + Left;
                    Bottom = value.Height + Top;
                }
            }

            public global::System.Drawing.Rectangle ToRectangle()
            {
                return new global::System.Drawing.Rectangle(this.Left, this.Top, this.Width, this.Height);
            }
            public static global::System.Drawing.Rectangle ToRectangle(RECT Rectangle)
            {
                return Rectangle.ToRectangle();
            }
            public static RECT FromRectangle(global::System.Drawing.Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }

            public static implicit operator global::System.Drawing.Rectangle(RECT Rectangle)
            {
                return Rectangle.ToRectangle();
            }
            public static implicit operator RECT(global::System.Drawing.Rectangle Rectangle)
            {
                return new RECT(Rectangle);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + Left + "; " + "Top: " + Top + "; Right: " + Right + "; Bottom: " + Bottom + "}";
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == Left && Rectangle.Top == Top && Rectangle.Right == Right && Rectangle.Bottom == Bottom;
            }
            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((global::System.Drawing.Rectangle)Object));
                }

                return false;
            }

            public override int GetHashCode()
            {
                return Left.GetHashCode() ^ Right.GetHashCode() ^ Top.GetHashCode() ^ Bottom.GetHashCode();
            }
        }
        #endregion
}
