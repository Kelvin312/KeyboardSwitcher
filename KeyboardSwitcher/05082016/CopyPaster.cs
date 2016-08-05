using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PostSwitcher
{

    //[Flags]
    //public enum TextReplaceMethod
    //{
    //    Off = 0,
    //    Default = 1,
    //    TextBox = 4,
    //    RichTextBox = 8,
    //    WmCopyPaste = 32 | UseClipboard,
    //    SendKeys = 64 | UseClipboard,
    //    PressKeys = 128 | UseClipboard | UseKeyboard,
    //    UseClipboard = 0x1000,
    //    UseKeyboard = 0x2000
    //}


    internal class CopyPaster
    {
        private SystemWindow _window;
        private Dictionary<string, object> _backupDict;

        public CopyPaster(SystemWindow window)
        {
            _window = window;
        }

        public void BackupClipboard() //Копирование буфера обмена 
        {
            _backupDict = new Dictionary<string, object>();
            var dataObject = Clipboard.GetDataObject();
            if (dataObject != null)
                foreach (var format in dataObject.GetFormats())
                {
                    _backupDict.Add(format, dataObject.GetData(format));
                }
        }

        public void RestoreClipboard() //Восстановление буфера обмена 
        {
            if (_backupDict != null)
            {
                DataObject dataObject = new DataObject();
                foreach (var kvp in _backupDict)
                {
                    dataObject.SetData(kvp.Key, kvp.Value);
                }
                Clipboard.SetDataObject(dataObject, true);
                _backupDict.Clear();
                _backupDict = null;
            }
        }

        public bool CopyTextFromTextBox(ref string selectedText)
        {
            int start = -1, next = -1;
            SendMessage(_window.HWnd, Msg.EM_GETSEL, out start, out next);
            if (start != next)
            {
                // Возвращаемое значение длина текста в символах, не включая завершающий нулевой символ.
                int lenAllText = (int) SendMessage(_window.HWnd, Msg.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
                StringBuilder sb = new StringBuilder(lenAllText + 1);
                int lenRead = (int) SendMessage(_window.HWnd, Msg.WM_GETTEXT, (IntPtr) sb.Capacity, sb);
                if (lenRead > 0)
                {
                    selectedText = sb.ToString().Substring(start, next - start);
                    return true;
                }
            }
            return false;
        }

        public bool CopyTextFromRichTextBox(ref string selectedText)
        {
            int start = -1, next = -1;
            SendMessage(_window.HWnd, Msg.EM_GETSEL, out start, out next);
            if (start != next)
            {
                int len = next - start;
                StringBuilder sb = new StringBuilder(len + 1);
                int lenRead = (int) SendMessage(_window.HWnd, Msg.EM_GETSELTEXT, IntPtr.Zero, sb);
                if (lenRead > 0)
                {
                    selectedText = sb.ToString();
                    return true;
                }
            }
            return false;
        }

        public bool CopyTextWmCopyPaste(ref string selectedText)
        {
            int oldId = GetClipboardSequenceNumber();
            SendMessage(_window.HWnd, Msg.WM_COPY, IntPtr.Zero, IntPtr.Zero);
            int newId = GetClipboardSequenceNumber();
            if (oldId == newId) return false;
            selectedText = Clipboard.GetText();
            return true;
        }

        public bool CopyTextSendKeys(ref string selectedText)
        {
            int oldId = GetClipboardSequenceNumber();
            if (!SendKeys(new Keys[] {Keys.ControlKey, Keys.C})) return false;
            int newId = GetClipboardSequenceNumber();
            if (oldId == newId) return false;
            selectedText = Clipboard.GetText();
            return true;
        }

        public bool CopyTextPressKeys(ref string selectedText)
        {
            int oldId = GetClipboardSequenceNumber();
            PressKeys(new Keys[] {Keys.ControlKey, Keys.C});
            int newId = GetClipboardSequenceNumber();
            if (oldId == newId) return false;
            selectedText = Clipboard.GetText();
            return true;
        }

        public void PasteTextFromTextBox(string text)
        {
            SendMessage(_window.HWnd, Msg.EM_REPLACESEL, new IntPtr(1), text);
        }

        public void PasteTextFromRichTextBox(string text)
        {
            SendMessage(_window.HWnd, Msg.EM_REPLACESEL, new IntPtr(1), text);
        }

        public void PasteTextWmCopyPaste(string text)
        {
            Clipboard.SetText(text);
            SendMessage(_window.HWnd, Msg.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
        }

        public void PasteTextSendKeys(string text)
        {
            Clipboard.SetText(text);
            SendKeys(new Keys[] { Keys.ControlKey, Keys.V });
        }

        public void PasteTextPressKeys(string text)
        {
            Clipboard.SetText(text);
            PressKeys(new Keys[] {Keys.ControlKey, Keys.V});
        }

        private bool SendKeys(Keys[] keys)
        {
            var hProcess = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.Synchronize, false, _window.ProcessId);
            if (hProcess == IntPtr.Zero) return false;

            for (var i = 0; i < keys.Length - 1; i++)
            {
                SendKeyToApp(hProcess, keys[i], true, true);
            }
            SendKeyToApp(hProcess, keys[keys.Length - 1], true, false);
            SendKeyToApp(hProcess, keys[keys.Length - 1], false, false);
            for (var i = keys.Length - 2; i >= 0; i--)
            {
                SendKeyToApp(hProcess, keys[i], false, true);
            }

            CloseHandle(hProcess);
            return true;
        }

        private void SendKeyToApp(IntPtr hProcess, Keys key, bool isDown, bool isSys)
        {
            var keyFlag = (MapVirtualKey((uint) key, MapTypes.MAPVK_VK_TO_VSC) << 16) | 1;
            var keyUpFlag = (1u << 31) | (1u << 30);
            if (isDown)
            {
                PostMessage(_window.HWnd, isSys ? Msg.WM_SYSKEYDOWN : Msg.WM_KEYDOWN,
                    new IntPtr((uint) key), new IntPtr(keyFlag));
            }
            else
            {
                PostMessage(_window.HWnd, isSys ? Msg.WM_SYSKEYUP : Msg.WM_KEYUP,
                    new IntPtr((uint) key), new IntPtr(keyFlag | keyUpFlag));
            }
            WaitForInputIdle(hProcess, 50);
        }

        private void PressKeys(Keys[] keys, bool isScan = true, int sleep = 40) //Нажимает последовательность клавиш 
        {
            var inputs = new INPUT[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                inputs[i] = MakeKeyInput(keys[i], true, isScan);
            }
            SendInput((uint) keys.Length, inputs, Marshal.SizeOf(typeof (INPUT)));
            Thread.Sleep(sleep); //25 ms мало
            for (var i = keys.Length - 1; i >= 0; i--)
            {
                inputs[i] = MakeKeyInput(keys[i], false, isScan);
            }
            SendInput((uint) keys.Length, inputs, Marshal.SizeOf(typeof (INPUT)));
        }

        private bool IsExtendedKey(Keys vkCode)
        {
            return
                vkCode == Keys.Menu ||
                vkCode == Keys.LMenu ||
                vkCode == Keys.RMenu ||
                vkCode == Keys.ControlKey ||
                vkCode == Keys.LControlKey ||
                vkCode == Keys.RControlKey ||
                vkCode == Keys.Insert ||
                vkCode == Keys.Delete ||
                vkCode == Keys.Home ||
                vkCode == Keys.End ||
                vkCode == Keys.PageUp ||
                vkCode == Keys.PageDown ||
                vkCode == Keys.Right ||
                vkCode == Keys.Up ||
                vkCode == Keys.Left ||
                vkCode == Keys.Down ||
                vkCode == Keys.NumLock ||
                vkCode == Keys.Cancel ||
                vkCode == Keys.Snapshot ||
                vkCode == Keys.Divide;
        }

        private INPUT MakeKeyInput(Keys vkCode, bool isDown, bool isScan)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBOARD_INPUT
                {
                    wVk = isScan ? (ushort) 0 : (ushort) vkCode,
                    wSc = isScan ? (ushort) MapVirtualKey((uint) vkCode, MapTypes.MAPVK_VK_TO_VSC) : (ushort) 0,
                    Flags = (isScan
                        ? KeyEventFlag.ScanCode
                        : (IsExtendedKey(vkCode) ? KeyEventFlag.ExtendedKey : KeyEventFlag.None)
                        ) | (isDown ? KeyEventFlag.None : KeyEventFlag.KeyUp),
                    Time = 0,
                    dwExtraInfo = 0
                }
            };
        }

        #region PInvoke Declarations

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, Msg msg, IntPtr wParam, StringBuilder lParam);

        //If you use '[Out] StringBuilder', initialize the string builder with proper length first.

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Msg msg, out int wParam, out int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Msg msg, IntPtr wParam,
            [MarshalAs(UnmanagedType.LPStr)] string lParam);

        //Also can add 'ref' or 'out' ahead of 'String lParam', don't use CharSet.Auto since we use MarshalAs(..) in this example.

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, MapTypes uMapType);

        private enum MapTypes : uint
        {
            MAPVK_VK_TO_VSC = 0x00,
            MAPVK_VSC_TO_VK = 0x01,
            MAPVK_VK_TO_CHAR = 0x02,
            MAPVK_VSC_TO_VK_EX = 0x03,
            MAPVK_VK_TO_VSC_EX = 0x04
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("user32.dll")]
        private static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

        private enum Msg : int
        {
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,

            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            EM_GETSEL = 0x00B0,
            EM_GETSELTEXT = 0x043E, //Только RichTextBox
            EM_REPLACESEL = 0x00C2,

            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] inputs, int sizeOfInputStructure);

        private const uint INPUT_KEYBOARD = 0x01;

        [Flags]
        private enum KeyEventFlag : uint
        {
            None = 0,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            ScanCode = 0x0008,
            Unicode = 0x0004
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct INPUT
        {
            [FieldOffset(0)] public uint type;
            [FieldOffset(4)] public KEYBOARD_INPUT ki;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct KEYBOARD_INPUT
        {
            public ushort wVk;
            public ushort wSc;
            public KeyEventFlag Flags;
            public uint Time;
            public uint dwExtraInfo;
            public uint Padding1;
            public uint Padding2;
        }


        #endregion
    }
}
