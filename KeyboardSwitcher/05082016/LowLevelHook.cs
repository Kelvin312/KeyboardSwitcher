using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PostSwitcher
{
    internal class LowLevelHook : IDisposable
    {
        private HookType _type;
        private HookCallback _callback;
        private IntPtr _hHook;

        protected delegate bool HookCallback(IntPtr wParam, IntPtr lParam);

        protected void InitLowLevelHook(HookType type, HookCallback callback)
        {
            _type = type;
            _callback = callback;
            _hHook = IntPtr.Zero;
        }

        public bool IsHooked => _hHook != IntPtr.Zero;

        public void SetHook()
        {
            if (_hHook != IntPtr.Zero) return;
            _hHook = ApiHelper.FailIfZero(
                SetWindowsHookEx(
                _type,
                InternalCallback,
                Process.GetCurrentProcess().MainModule.BaseAddress,
                0));
        }

        public void Unhook()
        {
            if (_hHook == IntPtr.Zero) return;
            ApiHelper.FailIfZero(UnhookWindowsHookEx(_hHook));
            _hHook = IntPtr.Zero;
        }

        private IntPtr InternalCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0 && _callback(wParam, lParam))
            {
                return new IntPtr(-1);
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Unhook();
        }

        ~LowLevelHook()
        {
            Unhook();
        }

        #region PInvoke Declarations

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

        #endregion
    }
}
