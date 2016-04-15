using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher.HotKeys
{
    internal class KeyboardHook : LowLevelHook
    {
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        public KeyboardHook() : base(HookType.WH_KEYBOARD_LL)
        {
            base.Callback = KeyboardCallback;
        }

        public void TryUnhook()
        {
            if (KeyDown == null &&
                KeyUp == null)
            {
                base.Unhook();
            }
        }

        private bool KeyboardCallback(IntPtr wParam, IntPtr lParam)
        {
            KeyboardHookStruct keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            var e = new KeyEventArgs((Keys)keyboardHookStruct.VirtualKeyCode);

            switch ((Messages) wParam)
            {
                case Messages.WM_KEYDOWN:
                case Messages.WM_SYSKEYDOWN:
                    KeyDown?.Invoke(null, e);
                    break;
                case Messages.WM_KEYUP:
                case Messages.WM_SYSKEYUP:
                    KeyUp?.Invoke(null, e);
                    break;
            }

            return e.Handled;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct KeyboardHookStruct
        {
            public int VirtualKeyCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }
    }
}
