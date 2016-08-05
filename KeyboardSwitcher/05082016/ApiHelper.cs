using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PostSwitcher
{
    internal class ApiHelper
    {
        public static int FailIfZero(int returnValue)
        {
            if (returnValue == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return returnValue;
        }

        public static IntPtr FailIfZero(IntPtr returnValue)
        {
            if (returnValue == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return returnValue;
        }
    }

    
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X { get; }
        public int Y { get; }

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(POINT p)
        {
            return new Point(p.X, p.Y);
        }

        public static implicit operator POINT(Point p)
        {
            return new POINT(p.X, p.Y);
        }

        public bool Equals(POINT p)
        {
            return X == p.X && Y == p.Y;
        }
    }


    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left { get;}
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Height => Bottom - Top;
        public int Width => Right - Left;
        public Size Size => new Size(Width, Height);
        public Point Location => new Point(Left, Top);

        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        public static RECT FromRectangle(Rectangle rectangle)
        {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public override int GetHashCode()
        {
            return Left ^ ((Top << 13) | (Top >> 0x13))
                   ^ ((Width << 0x1a) | (Width >> 6))
                   ^ ((Height << 7) | (Height >> 0x19));
        }

        public static implicit operator Rectangle(RECT rect)
        {
            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static implicit operator RECT(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}
