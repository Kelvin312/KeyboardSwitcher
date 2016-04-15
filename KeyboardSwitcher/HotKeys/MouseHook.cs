using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher.HotKeys
{
    internal class MouseHook : LowLevelHook
    {
        public event EventHandler<MouseEventExtArgs> MouseDown;
        public event EventHandler<MouseEventExtArgs> MouseUp;
        public event EventHandler<MouseEventExtArgs> MouseWheel;
        public event EventHandler<MouseEventExtArgs> MouseMove;

        private POINT _oldPoint;

        public MouseHook() : base(HookType.WH_MOUSE_LL)
        {
            base.Callback = MouseCallback;
        }

        public new void StartHook()
        {
            if (!IsHooked)
            {
                _oldPoint = new POINT(-1, -1);
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
            Keys keyCode = Keys.None;
            short mouseDelta = 0;
            int clickCount = 0;

            bool isMouseButtonDown = false;
            bool isMouseButtonUp = false;

            switch ((Messages) wParam)
            {
                case Messages.WM_LBUTTONDOWN:
                case Messages.WM_LBUTTONUP:
                case Messages.WM_LBUTTONDBLCLK:
                    button = MouseButtons.Left;
                    keyCode = Keys.LButton;
                    break;
                case Messages.WM_RBUTTONDOWN:
                case Messages.WM_RBUTTONUP:
                case Messages.WM_RBUTTONDBLCLK:
                    button = MouseButtons.Right;
                    keyCode = Keys.RButton;
                    break;
                case Messages.WM_MBUTTONDOWN: 
                case Messages.WM_MBUTTONUP:
                case Messages.WM_MBUTTONDBLCLK:
                    button = MouseButtons.Middle;
                    keyCode = Keys.MButton;
                    break;
                case Messages.WM_XBUTTONDOWN:
                case Messages.WM_XBUTTONUP:
                case Messages.WM_XBUTTONDBLCLK:
                    if (mouseHookStruct.MouseData == 1)
                    {
                        button = MouseButtons.XButton1;
                        keyCode = Keys.XButton1;
                    }
                    else
                    {
                        button = MouseButtons.XButton2;
                        keyCode = Keys.XButton2;
                    }
                    break;
            }


            switch ((Messages) wParam)
            {
                case Messages.WM_MOUSEWHEEL:
                    mouseDelta = mouseHookStruct.MouseData;
                    if (mouseDelta > 0) keyCode = (Keys)KeysEx.WheelUp;
                    if (mouseDelta < 0) keyCode = (Keys)KeysEx.WheelDown;
                    break;
                case Messages.WM_MOUSEHWHEEL:
                    //mouseDelta = mouseHookStruct.MouseData;
                    break;
                case Messages.WM_LBUTTONDOWN:
                case Messages.WM_RBUTTONDOWN:
                case Messages.WM_MBUTTONDOWN:
                case Messages.WM_XBUTTONDOWN:
                    clickCount = 1;
                    isMouseButtonDown = true;
                    break;
                case Messages.WM_LBUTTONUP:
                case Messages.WM_RBUTTONUP:
                case Messages.WM_MBUTTONUP:
                case Messages.WM_XBUTTONUP:
                    clickCount = 1;
                    isMouseButtonUp = true;
                    break;
                case Messages.WM_LBUTTONDBLCLK:
                case Messages.WM_RBUTTONDBLCLK:
                case Messages.WM_MBUTTONDBLCLK:
                case Messages.WM_XBUTTONDBLCLK:
                    clickCount = 2;
                    break;
            }

            var e = new MouseEventExtArgs(
                button,
                keyCode,
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
            if (_oldPoint != mouseHookStruct.Point)
            {
                _oldPoint = mouseHookStruct.Point;
                MouseMove?.Invoke(null, e);
            }

            return e.Handled;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MouseLLHookStruct
        {
            [FieldOffset(0x00)] public POINT Point;
            [FieldOffset(0x0A)] public Int16 MouseData;
            [FieldOffset(0x10)] public Int32 Timestamp;
        }
    }

    public enum KeysEx
    {
        WheelUp = Keys.Shift | 1,
        WheelDown = Keys.Shift | 2
    }

    public class MouseEventExtArgs : MouseEventArgs
    {
        public MouseEventExtArgs(MouseButtons buttons, Keys keyCode, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            KeyCode = keyCode;
            Handled = false;
        }

        internal MouseEventExtArgs(MouseEventArgs e) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
        }

        public Keys KeyCode { get; }

        /// <summary>
        /// Получает или задает значение, определяющее, было ли обработано событие.
        /// </summary>
        public bool Handled { get; set; }
    }
}



