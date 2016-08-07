using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PostSwitcher
{
    internal class MouseHook : LowLevelHook
    {
        public event EventHandler<MouseEventExtArgs> MouseDown;
        public event EventHandler<MouseEventExtArgs> MouseUp;
        public event EventHandler<MouseEventExtArgs> MouseWheel;
        public event EventHandler<MouseEventExtArgs> MouseMove;

        private POINT _oldPoint;

        public MouseHook():base(HookType.WH_MOUSE_LL) 
        {
        }

        public new void SetHook()
        {
            if (IsHooked) return;
            _oldPoint = new POINT(-1, -1);
            base.SetHook();
        }

        protected override bool HookCallback(IntPtr wParam, IntPtr lParam)
        {
            var mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof (MouseLLHookStruct));

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

            if (MouseMove != null && !_oldPoint.Equals(mouseHookStruct.Point))
            {
                MouseMove.Invoke(null, e);
            }

            _oldPoint = mouseHookStruct.Point;
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

        public Keys KeyCode { get; }

        /// <summary>
        /// Получает или задает значение, определяющее, было ли обработано событие.
        /// </summary>
        public bool Handled { get; set; }
    }
}



