using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class MouseHook : LowLevelHook
    {
        public event EventHandler<MouseEventExtArgs> MouseDown;
        public event EventHandler<MouseEventExtArgs> MouseUp;
        public event EventHandler<MouseEventExtArgs> MouseWheel;
        public event EventHandler<MouseEventExtArgs> MouseMove;

        private int _oldX;
        private int _oldY;

        public MouseHook() : base(HookType.WH_MOUSE_LL)
        {
            base.Callback = MouseCallback;
        }

        public new void StartHook()
        {
            if (!IsHooked)
            {
                _oldX = -1;
                _oldY = 1;
                base.StartHook();
            }
        }
        public void TryUnhook()
        {
            if (MouseDown == null &&
                MouseMove == null &&
                MouseUp == null &&
                MouseWheel == null)
            {
                base.Unhook();
            }
        }

        private bool MouseCallback(IntPtr wParam, IntPtr lParam)
        {

            MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)
                Marshal.PtrToStructure(lParam, typeof (MouseLLHookStruct));

            MouseButtons button = MouseButtons.None;
            short mouseDelta = 0;
            int clickCount = 0;

            bool isMouseButtonDown = false;
            bool isMouseButtonUp = false;

            switch ((Messages) wParam)
            {
                case Messages.WM_LBUTTONDOWN:
                    isMouseButtonDown = true;
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case Messages.WM_LBUTTONUP:
                    isMouseButtonUp = true;
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case Messages.WM_LBUTTONDBLCLK:
                    isMouseButtonDown = true;
                    button = MouseButtons.Left;
                    clickCount = 2;
                    break;
                case Messages.WM_RBUTTONDOWN:
                    isMouseButtonDown = true;
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case Messages.WM_RBUTTONUP:
                    isMouseButtonUp = true;
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case Messages.WM_RBUTTONDBLCLK:
                    isMouseButtonDown = true;
                    button = MouseButtons.Right;
                    clickCount = 2;
                    break;
                case Messages.WM_MBUTTONDOWN:
                    isMouseButtonDown = true;
                    button = MouseButtons.Middle;
                    clickCount = 1;
                    break;
                case Messages.WM_MBUTTONUP:
                    isMouseButtonUp = true;
                    button = MouseButtons.Middle;
                    clickCount = 1;
                    break;
                case Messages.WM_MBUTTONDBLCLK:
                    isMouseButtonDown = true;
                    button = MouseButtons.Middle;
                    clickCount = 2;
                    break;
                case Messages.WM_MOUSEWHEEL:
                    mouseDelta = mouseHookStruct.MouseData;
                    break;
                case Messages.WM_MOUSEHWHEEL:
                    mouseDelta = mouseHookStruct.MouseData;
                    break;
                case Messages.WM_XBUTTONDOWN:
                    button = mouseHookStruct.MouseData == 1
                        ? MouseButtons.XButton1
                        : MouseButtons.XButton2;
                    isMouseButtonDown = true;
                    clickCount = 1;
                    break;
                case Messages.WM_XBUTTONUP:
                    button = mouseHookStruct.MouseData == 1
                        ? MouseButtons.XButton1
                        : MouseButtons.XButton2;
                    isMouseButtonUp = true;
                    clickCount = 1;
                    break;
                case Messages.WM_XBUTTONDBLCLK:
                    isMouseButtonDown = true;
                    button = mouseHookStruct.MouseData == 1
                        ? MouseButtons.XButton1
                        : MouseButtons.XButton2;
                    clickCount = 2;
                    break;
            }

            var e = new MouseEventExtArgs(
                button,
                clickCount,
                mouseHookStruct.Point.X,
                mouseHookStruct.Point.Y,
                mouseDelta);

            if (MouseDown != null && isMouseButtonDown)
            {
                MouseDown.Invoke(null, e);
            }

            if (MouseUp != null && isMouseButtonUp)
            {
                MouseUp.Invoke(null, e);
            }

            if (MouseWheel != null && mouseDelta != 0)
            {
                MouseWheel.Invoke(null, e);
            }

            //If someone listens to move and there was a change in coordinates raise move event
            if ((MouseMove != null) && (_oldX != mouseHookStruct.Point.X || _oldY != mouseHookStruct.Point.Y))
            {
                _oldX = mouseHookStruct.Point.X;
                _oldY = mouseHookStruct.Point.Y;

                MouseMove.Invoke(null, e);
            }

            return e.Handled;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MouseLLHookStruct
        {
            [FieldOffset(0x00)] public Point Point;
            [FieldOffset(0x0A)] public Int16 MouseData;
            [FieldOffset(0x10)] public Int32 Timestamp;
        }
    }


    public class MouseEventExtArgs : MouseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the MouseEventArgs class. 
        /// </summary>
        public MouseEventExtArgs(MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
        }

        internal MouseEventExtArgs(MouseEventArgs e) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
        }

        /// <summary>
        /// Получает или задает значение, определяющее, было ли обработано событие.
        /// </summary>
        public bool Handled { get; set; }
    }
}



