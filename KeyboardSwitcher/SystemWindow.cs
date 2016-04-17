using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class SystemWindow
    {
        private IntPtr _hwnd;
        public IntPtr HWnd { get { return _hwnd; } }
        public SystemWindow(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }
        public SystemWindow(IntPtr hwnd, int pid, int tid)
        {
            _hwnd = hwnd;
            _processId = pid;
            _threadId = tid;
        }

        public static SystemWindow ForegroundWindow()
        {
            return new SystemWindow(GetForegroundWindow());
        }

        public static SystemWindow WindowFromPoint(Point point)
        {
            IntPtr hwnd = WindowFromPoint((POINT) point);
            if (hwnd.ToInt64() == 0)
            {
                return null;
            }
            return new SystemWindow(hwnd);
        }

        public SystemWindow FocusedWindow() //Получение активного контрола 
        {
            //@kurumpa http://stackoverflow.com/a/28409126
            IntPtr hwnd = HWnd;
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(info);
            var success = GetGUIThreadInfo(ThreadId, ref info);

            // target = hwndCaret || hwndFocus || (AttachThreadInput + GetFocus) || hwndActive
            var currentThreadId = GetCurrentThreadId();
            if (currentThreadId != ThreadId) AttachThreadInput(ThreadId, currentThreadId, true);
            var focusedHandle = GetFocus();
            if (currentThreadId != ThreadId) AttachThreadInput(ThreadId, currentThreadId, false);

            if (success)
            {
                if (info.hwndCaret != IntPtr.Zero)
                {
                    hwnd = info.hwndCaret;
                }
                else if (info.hwndFocus != IntPtr.Zero)
                {
                    hwnd = info.hwndFocus;
                }
                else if (focusedHandle != IntPtr.Zero)
                {
                    hwnd = focusedHandle;
                }
                else if (info.hwndActive != IntPtr.Zero)
                {
                    hwnd = info.hwndActive;
                }
            }
            else
            {
                hwnd = focusedHandle;
            }

            return new SystemWindow(hwnd, ProcessId, ThreadId);
        }


        public SystemWindow RootWindow() //Получение корневого окна 
        {
            return new SystemWindow(GetAncestor(HWnd, GetAncestorFlags.GetRoot));
        }


        private string _processName;

        public string ProcessName
        {
            get
            {
                if (string.IsNullOrEmpty(_processName))
                {
                    using (var p = Process.GetProcessById(ProcessId))
                    {
                        _processName = p.MainModule.ModuleName;
                    }
                }
                return _processName;
            }
        }

        private string _className;

        public string ClassName
        {
            get
            {
                if (string.IsNullOrEmpty(_className))
                {
                    int length = 64;
                    while (true)
                    {
                        StringBuilder sb = new StringBuilder(length);
                        ApiHelper.FailIfZero(GetClassName(_hwnd, sb, sb.Capacity));
                        if (sb.Length != length - 1)
                        {
                            _className = sb.ToString();
                            break;
                        }
                        length *= 2;
                    }
                }
                return _className;
            }
        }

        public string Title
        {
            get
            {
                StringBuilder sb = new StringBuilder(GetWindowTextLength(_hwnd) + 1);
                GetWindowText(_hwnd, sb, sb.Capacity);
                return sb.ToString();
            }

            set
            {
                SetWindowText(_hwnd, value);
            }
        }

        private int _processId;
        private int _threadId;
        public int ProcessId
        {
            get
            {
                if(_processId < 5) _threadId = GetWindowThreadProcessId(HWnd, out _processId);
                return _processId;
            }
        }

        public int ThreadId
        {
            get
            {
                if (_processId < 5) _threadId = GetWindowThreadProcessId(HWnd, out _processId);
                return _threadId;
            }
        }

        private CultureInfo _keyboardLayout;
        public CultureInfo KeyboardLayout
        {
            get
            {
                if (_keyboardLayout == null)
                {
                    _keyboardLayout = new CultureInfo((short) GetKeyboardLayout(ThreadId));
                }
                return _keyboardLayout;
            }
            set
            {
                PostMessage(new HandleRef(this, HWnd), WM_INPUTLANGCHANGEREQUEST,
                    new IntPtr(INPUTLANGCHANGE_SYSCHARSET),
                    LoadKeyboardLayout($"{value.LCID:X8}", KLF_ACTIVATE));
                _keyboardLayout = null;
            }
        }


        public Point OffsetFromPoint(Point point)
        {
            RECT rect;
            GetWindowRect(_hwnd, out rect);
            return new Point(point.X - rect.Left, point.Y - rect.Top);
        }

        public void MoveWindowToPoint(Point point, bool isActivate = false, bool isTop = false)
        {
            SWP swp = SWP.AsynchronousWindowPosition | SWP.IgnoreResize;
            if (!isActivate) swp |= SWP.DoNotActivate;
            if (!isTop) swp |= SWP.IgnoreZOrder;
            SetWindowPos(HWnd, HWNDInsertAfter.Top, point.X, point.Y, 0, 0, swp);
        }

        public void SendMouseWheel(MouseEventArgs e)
        {
            var wParam = new IntPtr((e.Delta << 16) /*| mkKeyState*/);
            var lParam = new IntPtr((e.X & 0xFFFF) | (e.Y << 16));
            PostMessage(new HandleRef(this, HWnd), WM_MOUSEWHEEL, wParam, lParam);
        }

        /// <summary>
        /// Send a message to this window that it should close. This is equivalent
        /// to clicking the "X" in the upper right corner or pressing Alt+F4.
        /// </summary>
        public void SendClose()
        {
            SendMessage(new HandleRef(this, HWnd), WM_CLOSE, new IntPtr(0), new IntPtr(0));
        }

        /// <summary>
        /// Whether this window is currently visible. A window is visible if its 
        /// and all ancestor's visibility flags are true.
        /// </summary>
        public bool Visible
        {
            get
            {
                return IsWindowVisible(_hwnd);
            }
        }

        /// <summary>
        /// The ID of a control within a dialog. This is used in
        /// WM_COMMAND messages to distinguish which control sent the command.
        /// </summary>
        public int DialogID
        {
            get
            {
                return GetWindowLong32(_hwnd, (int)GWL.GWL_ID);
            }
        }

        /// <summary>
        /// Highlights the window with a red border.
        /// </summary>
        public void Highlight(Color color)
        {
            RECT rect;
            GetWindowRect(_hwnd, out rect);
            using (WindowDeviceContext windowDC = GetDeviceContext(false))
            {
                using (Graphics g = windowDC.CreateGraphics())
                {
                    g.DrawRectangle(new Pen(color, 4), 0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }
            }
        }

        /// <summary>
        /// Forces the window to invalidate its client area and immediately redraw itself and any child controls. 
        /// </summary>
        public void Refresh()
        {
            // By using parent, we get better results in refreshing old drawing window area.
            IntPtr hwndToRefresh = GetParent(_hwnd);
            if (hwndToRefresh == IntPtr.Zero || !IsChild(hwndToRefresh, _hwnd)) hwndToRefresh = _hwnd;
 
            InvalidateRect(hwndToRefresh, IntPtr.Zero, true);
            RedrawWindow(hwndToRefresh, IntPtr.Zero, IntPtr.Zero, RDW.RDW_FRAME | RDW.RDW_INVALIDATE | RDW.RDW_UPDATENOW | RDW.RDW_ALLCHILDREN | RDW.RDW_ERASENOW);
        }

        /// <summary>
        /// Gets a device context for this window.
        /// </summary>
        /// <param name="clientAreaOnly">Whether to get the context for
        /// the client area or for the full window.</param>
        public WindowDeviceContext GetDeviceContext(bool clientAreaOnly)
        {
            if (clientAreaOnly)
            {
                return new WindowDeviceContext(this, GetDC(_hwnd));
            }
            else
            {
                return new WindowDeviceContext(this, GetWindowDC(_hwnd));
            }
        }

        #region PInvoke Declarations


        [StructLayout(LayoutKind.Sequential)]
         struct GUITHREADINFO
        {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [DllImport("user32.dll")]
         static extern bool GetGUIThreadInfo(int idThread, ref GUITHREADINFO lpgui);

        [DllImport("kernel32.dll")]
         static extern int GetCurrentThreadId();

        [DllImport("user32.dll")]
         static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
         static extern IntPtr GetFocus();

        [DllImport("user32.dll", ExactSpelling = true)]
         static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

         enum GetAncestorFlags:uint
        {
            GetParent = 1,
            GetRoot = 2,
            GetRootOwner = 3
        }

        [DllImport("user32.dll", SetLastError = true)]
         static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);

         static class HWNDInsertAfter
        {
            public static IntPtr
            NoTopMost = new IntPtr(-2),
            TopMost = new IntPtr(-1),
            Top = new IntPtr(0),
            Bottom = new IntPtr(1);
        }

        [Flags]
         enum SWP : uint
        {
            AsynchronousWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            DoNotActivate = 0x0010,
            DoNotCopyBits = 0x0100,
            IgnoreMove = 0x0002,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotRedraw = 0x0008,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            IgnoreResize = 0x0001,
            IgnoreZOrder = 0x0004,
            ShowWindow = 0x0040,
        }

         const int WM_MOUSEWHEEL = 0x020A;
         const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

        const int INPUTLANGCHANGE_SYSCHARSET = 0x0001;
        const int KLF_ACTIVATE = 0x0001;


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(HandleRef hWnd, int Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll")]
         static extern IntPtr GetKeyboardLayout(int idThread);

        [DllImport("user32.dll")]
         static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        private enum GWL : int
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [DllImport("user32.dll")]
        static extern bool GetWindowPlacement(IntPtr hWnd,
           ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        static extern bool SetWindowPlacement(IntPtr hWnd,
           [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect,
           int nBottomRect);

        [DllImport("user32.dll")]
        static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        [DllImport("user32.dll")]
        static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062
        }

        enum GetWindowRegnReturnValues : int
        {
            ERROR = 0,
            NULLREGION = 1,
            SIMPLEREGION = 2,
            COMPLEXREGION = 3
        }

        static readonly uint EM_GETPASSWORDCHAR = 0xD2, EM_SETPASSWORDCHAR = 0xCC;
        static readonly uint BM_GETCHECK = 0xF0, BM_SETCHECK = 0xF1;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
         static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
         static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, [Out] StringBuilder lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        private const int WM_CLOSE = 16;

        private enum GetWindow_Cmd
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        private enum RDW : uint
        {
            RDW_INVALIDATE = 0x0001,
            RDW_INTERNALPAINT = 0x0002,
            RDW_ERASE = 0x0004,

            RDW_VALIDATE = 0x0008,
            RDW_NOINTERNALPAINT = 0x0010,
            RDW_NOERASE = 0x0020,

            RDW_NOCHILDREN = 0x0040,
            RDW_ALLCHILDREN = 0x0080,

            RDW_UPDATENOW = 0x0100,
            RDW_ERASENOW = 0x0200,

            RDW_FRAME = 0x0400,
            RDW_NOFRAME = 0x0800,
        }

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RDW flags);

        #endregion
    }

    /// <summary>
    /// A device context of a window that allows you to draw onto that window.
    /// </summary>
    public class WindowDeviceContext : IDisposable
    {
        IntPtr hDC;
        SystemWindow sw;

        internal WindowDeviceContext(SystemWindow sw, IntPtr hDC)
        {
            this.sw = sw;
            this.hDC = hDC;
        }

        /// <summary>
        /// The device context handle.
        /// </summary>
        public IntPtr HDC { get { return hDC; } }

        /// <summary>
        /// Creates a Graphics object for this device context.
        /// </summary>
        public Graphics CreateGraphics()
        {
            return Graphics.FromHdc(hDC);
        }

        /// <summary>
        /// Frees this device context.
        /// </summary>
        public void Dispose()
        {
            if (hDC != IntPtr.Zero)
            {
                ReleaseDC(sw.HWnd, hDC);
                hDC = IntPtr.Zero;
            }
        }

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }

}
