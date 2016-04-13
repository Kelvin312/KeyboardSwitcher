using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyboardSwitcher
{
    internal class LowLevelHook
    {
        private readonly HookType _type;
        private HookCallback _callback;
        private IntPtr _hHook;

        protected delegate bool HookCallback(IntPtr wParam, IntPtr lParam);

        protected LowLevelHook(HookType type)
        {
            _type = type;
            _hHook = IntPtr.Zero;
        }

        protected HookCallback Callback
        {
            set { _callback = value; }
        }

        public bool IsHooked
        {
            get { return _hHook != IntPtr.Zero; }
        }

        public void StartHook()
        {
            if (_hHook == IntPtr.Zero && _callback != null)
            {
                HookProcedure hookProcedure = InternalCallback;

                _hHook = SetWindowsHookEx(
                    _type,
                    hookProcedure,
                    Process.GetCurrentProcess().MainModule.BaseAddress,
                    0);

                if (_hHook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public void Unhook()
        {
            if (_hHook != IntPtr.Zero)
            {
                if (UnhookWindowsHookEx(_hHook) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                _hHook = IntPtr.Zero;
            }
        }

        private IntPtr InternalCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0 && _callback(wParam, lParam))
            {
                return new IntPtr(-1);
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        ~LowLevelHook()
        {
            Unhook();
        }

        private delegate IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            HookType type,
            HookProcedure lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
          CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CallNextHookEx(
          IntPtr hHook,
          int nCode,
          IntPtr wParam,
          IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
           CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(IntPtr hHook);


        protected enum Messages : int
        {
            WM_MOUSEMOVE = 0x200,

            WM_LBUTTONDOWN = 0x201,
            WM_RBUTTONDOWN = 0x204,
            WM_MBUTTONDOWN = 0x207,

            WM_LBUTTONUP = 0x202,
            WM_RBUTTONUP = 0x205,
            WM_MBUTTONUP = 0x208,

            WM_LBUTTONDBLCLK = 0x203,
            WM_RBUTTONDBLCLK = 0x206,
            WM_MBUTTONDBLCLK = 0x209,

            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x20E,

            WM_XBUTTONDOWN = 0x20B,
            WM_XBUTTONUP = 0x20C,
            WM_XBUTTONDBLCLK = 0x20D,

            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105
        }

        protected enum HookType : int
        {
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }
    }
}
