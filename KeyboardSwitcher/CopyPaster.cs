using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{

    [Flags]
    internal enum TextReplaceMethod
    {
        Off = 0,
        Default = 1,
        TextBox = 4,
        RichTextBox = 8,
        WmCopyPaste = 32 | UseClipboard,
        SendKeys = 64 | UseClipboard,
        PressKeys = 128 | UseClipboard | UseKeyboard,
        UseClipboard = 0x1000,
        UseKeyboard = 0x2000
    }


    internal class CopyPaster
    {
        private TextReplaceMethod _replaceMethod;
        private SystemWindow _window;
        private Dictionary<string, object> _backupDict;

        public CopyPaster(SystemWindow window, TextReplaceMethod replaceMethod)
        {
            _window = window;
            _replaceMethod = replaceMethod;
        }

        public void BackupClipboard() //Копирование буфера обмена 
        {
            if (_replaceMethod.HasFlag(TextReplaceMethod.UseClipboard))
            {
                _backupDict = new Dictionary<string, object>();
                var dataObject = Clipboard.GetDataObject();
                if (dataObject != null)
                    foreach (var format in dataObject.GetFormats())
                    {
                        _backupDict.Add(format, dataObject.GetData(format));
                    }
            }
        }

        public void RestoreClipboard(bool isCopy = true) //Восстановление буфера обмена 
        {
            if (_replaceMethod.HasFlag(TextReplaceMethod.UseClipboard) && _backupDict != null)
            {
                DataObject dataObject = new DataObject();
                foreach (var kvp in _backupDict)
                {
                    dataObject.SetData(kvp.Key, kvp.Value);
                }
                Clipboard.SetDataObject(dataObject, isCopy);
            }
        }


        public bool CopyText(ref string selectedText) //Взять текст 
        {
            bool result = false;
            int clipboardId = -1;
            if (_replaceMethod.HasFlag(TextReplaceMethod.UseClipboard))
                clipboardId = GetClipboardSequenceNumber();

            switch (_replaceMethod)
            {
                case TextReplaceMethod.TextBox:
                {
                    int start = -1, next = -1;
                    SendMessage(new HandleRef(this, _window.HWnd), Messages.EM_GETSEL, out start, out next);
                    if (start != next)
                    {
                        // Возвращаемое значение длина текста в символах, не включая завершающий нулевой символ.
                        int lenAllText =
                            (int)
                                SendMessage(new HandleRef(this, _window.HWnd), Messages.WM_GETTEXTLENGTH, IntPtr.Zero,
                                    IntPtr.Zero);
                        StringBuilder sb = new StringBuilder(lenAllText + 1);
                        int lenRead =
                            (int)
                                SendMessage(new HandleRef(this, _window.HWnd), Messages.WM_GETTEXT, (IntPtr) sb.Capacity,
                                    sb);
                        if (lenRead > 0)
                        {
                            selectedText = sb.ToString().Substring(start, next - start);
                            result = true;
                        }
                    }
                }
                    break;
                case TextReplaceMethod.RichTextBox:
                {
                    int start = -1, next = -1;
                    SendMessage(new HandleRef(this, _window.HWnd), Messages.EM_GETSEL, out start, out next);
                    if (start != next)
                    {
                        int len = next - start;
                        StringBuilder sb = new StringBuilder(len + 1);
                        int lenRead =
                            (int)
                                SendMessage(new HandleRef(this, _window.HWnd), Messages.EM_GETSELTEXT, IntPtr.Zero, sb);
                        if (lenRead > 0)
                        {
                            selectedText = sb.ToString();
                            result = true;
                        }
                    }
                }
                    break;
                case TextReplaceMethod.WmCopyPaste:
                    SendMessage(new HandleRef(this, _window.HWnd), Messages.WM_COPY, IntPtr.Zero, IntPtr.Zero);
                    selectedText = Clipboard.GetText();
                    break;
                case TextReplaceMethod.SendKeys:
                    if (SendKeysToApp(Keys.ControlKey, Keys.C))
                        selectedText = Clipboard.GetText();
                    break;
                case TextReplaceMethod.PressKeys:
                    PressKeys(new Keys[] { Keys.ControlKey, Keys.C });
                    selectedText = Clipboard.GetText();
                    break;
            }

            if (_replaceMethod.HasFlag(TextReplaceMethod.UseClipboard))
            {
                int newId = GetClipboardSequenceNumber();
                result = newId == clipboardId;
            }
            return result;
        }

        public void PasteText(string text) //Вставить текст 
        {
            switch (_replaceMethod)
            {
                case TextReplaceMethod.TextBox:
                case TextReplaceMethod.RichTextBox:
                    SendMessage(new HandleRef(this, _window.HWnd), Messages.EM_REPLACESEL, new IntPtr(1), text);
                    break;
                case TextReplaceMethod.WmCopyPaste:
                    Clipboard.SetText(text);
                    SendMessage(new HandleRef(this, _window.HWnd), Messages.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
                    break;
                case TextReplaceMethod.SendKeys:
                    Clipboard.SetText(text);
                    SendKeysToApp(Keys.ControlKey, Keys.V);
                    break;
                case TextReplaceMethod.PressKeys:
                    Clipboard.SetText(text);
                    PressKeys(new Keys[] { Keys.ControlKey, Keys.V });
                    break;
            }
        }



        private bool SendKeysToApp(Keys sysKey, Keys key)
        {
            var sysKeyFlag =  (MapVirtualKey((uint) sysKey, MapTypes.MAPVK_VK_TO_VSC)<<16) | 1;
            var keyFlag = (MapVirtualKey((uint) key, MapTypes.MAPVK_VK_TO_VSC) << 16) | 1;
            var keyUpFlag = (1u << 31) | (1u << 30);

            var hProcess = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.Synchronize, 
                false, _window.ProcessId);

            if (hProcess != IntPtr.Zero)
            {
                PostMessage(new HandleRef(this, _window.HWnd), Messages.WM_SYSKEYDOWN,
                    new IntPtr((uint) sysKey), new IntPtr(sysKeyFlag));
                WaitForInputIdle(hProcess, 50);
                PostMessage(new HandleRef(this, _window.HWnd), Messages.WM_KEYDOWN,
                    new IntPtr((uint) key), new IntPtr(keyFlag));
                WaitForInputIdle(hProcess, 50);
                PostMessage(new HandleRef(this, _window.HWnd), Messages.WM_KEYUP,
                    new IntPtr((uint) key), new IntPtr(keyFlag | keyUpFlag));
                WaitForInputIdle(hProcess, 50);
                PostMessage(new HandleRef(this, _window.HWnd), Messages.WM_SYSKEYUP,
                    new IntPtr((uint) sysKey), new IntPtr(sysKeyFlag | keyUpFlag));
                WaitForInputIdle(hProcess, 50);
                CloseHandle(hProcess);

                return true;
            }
            return false;
        }

        private void PressKeys(Keys[] k)
        {
            
        }




        #region PInvoke Declarations

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(HandleRef hWnd, Messages Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(HandleRef hWnd, Messages Msg, IntPtr wParam, StringBuilder lParam);

        //If you use '[Out] StringBuilder', initialize the string builder with proper length first.

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(HandleRef hWnd, Messages Msg, out int wParam, out int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(HandleRef hWnd, Messages Msg, IntPtr wParam,
            [MarshalAs(UnmanagedType.LPStr)] string lParam);

        //Also can add 'ref' or 'out' ahead of 'String lParam', don't use CharSet.Auto since we use MarshalAs(..) in this example.

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool PostMessage(HandleRef hWnd, Messages Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, MapTypes uMapType);

        enum MapTypes : uint
        {
            MAPVK_VK_TO_VSC = 0x00,
            MAPVK_VSC_TO_VK = 0x01,
            MAPVK_VK_TO_CHAR = 0x02,
            MAPVK_VSC_TO_VK_EX = 0x03,
            MAPVK_VK_TO_VSC_EX = 0x04
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId
       );

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        public enum ProcessAccessFlags : uint
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
        static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

        // [DllImport("user32.dll", SetLastError = true)]
        //  static extern UInt32 SendInput(UInt32 numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] inputs, Int32 sizeOfInputStructure);


        private enum Messages : int
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

        #endregion

    }
}
