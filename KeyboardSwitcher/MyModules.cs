using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using KeyboardSwitcher.HotKeys;

namespace KeyboardSwitcher
{
    public class SettingsLayer
    {
        public static List<HotKey> _hotkeyList = new List<HotKey>();
        public static Dictionary<string,Dictionary<string, FilterConfig>> _filterList = new Dictionary<string, Dictionary<string, FilterConfig>>();

        public static FilterConfig GetWindowConfig(string processName, string className)
        {
            var filter = new FilterConfig();
            filter.Update(_filterList[""][""]);
            filter.Update(_filterList[""][className]);
            filter.Update(_filterList[processName][""]);
            filter.Update(_filterList[processName][className]);
            return filter;
        }

        public static CopyPasterConfig _copyPaster = new CopyPasterConfig();
    }

    public class FilterConfig
    {
        public void Update(FilterConfig current)
        {
            if (current.XMoveStatus != 
                        XMoveMethod.Default)
                        XMoveStatus = 
                current.XMoveStatus;

            if (current.TextReplaceStatus != 
                        TextReplaceMethod.Default)
                        TextReplaceStatus = 
                current.TextReplaceStatus;

            if (current.TextSwitchStatus != 
                        TextSwitchMethod.Default)
                        TextSwitchStatus = 
                current.TextSwitchStatus;

            if (current.ActiveLayoutStatus != 
                        ActiveLayoutMethod.Default)
                        ActiveLayoutStatus = 
                current.ActiveLayoutStatus;
        }


        public TextReplaceMethod TextReplaceStatus { get; set; }

        public enum XMoveMethod
        {
            Off = 0,
            Default = 1,
            Enable = 2
        }

        public XMoveMethod XMoveStatus { get; set; }

        public enum TextSwitchMethod
        {
            Off = 0,
            Default = 1,
            Enable = 2,
            Devenv = 3
        }
        public TextSwitchMethod TextSwitchStatus { get; set; }

        public enum ActiveLayoutMethod
        {
            Off = 0,
            Default = 1,
            RuRu = 0x0419,
            EnUs = 0x0409
        }

        public ActiveLayoutMethod ActiveLayoutStatus { get; set; }
    }

    public class XWheelModule
    {
        public const int Id = 0;
        public bool Enable { get; set; }

        public void MouseWheel(MouseEventExtArgs e)
        {
            if (!Enable) return;
            var window = SystemWindow.WindowFromPoint(e.Location);
            window.SendMouseWheel(e);
            e.Handled = true;
        }
    }

    public class XMoveModule
    {
        public const int Id = 1;
        public bool Enable { get; set; }
        public bool IsActivate { get; set; }
        public bool IsTop { get; set; }

        private bool _isMove = false;
        private SystemWindow _windowMove;
        private Point _offsetMove;

        public void EntryPoint(bool isDown, ref bool isRun)
        {
            
            if (!_isMove && isDown)
            {
                if (!Enable) return;
                var cursorPos = Cursor.Position;
                _windowMove = SystemWindow.WindowFromPoint(cursorPos).RootWindow();
                var filter = SettingsLayer.GetWindowConfig(_windowMove.ProcessName, "");
                if(filter.XMoveStatus == FilterConfig.XMoveMethod.Off) return;
                _offsetMove = _windowMove.OffsetFromPoint(cursorPos);
                Cursor.Current = Cursors.SizeAll;
                isRun = true;
            }
            if (_isMove && !isDown)
            {
                Cursor.Current = Cursors.Default;
            }
            _isMove = isDown;
        }

        public void MouseMove(Point e)
        {
            if (_isMove)
            {
                _windowMove.MoveWindowToPoint(new Point(e.X - _offsetMove.X, e.Y - _offsetMove.Y), IsActivate, IsTop);
            }
        }
    }

    public class CopyPasterConfig
    {
        public bool NotUseClipboardForTextBoxAndRichTextBox { get; set; }
    }

    public class TextSwitchModule
    {
        public const int Id = 2;
        public bool Enable { get; set; }

        public enum OptionsSwitch
        {
            OnlyText,
            IfTextToLayout,
            TextAndLayout
        }

        public OptionsSwitch OptionsSwitchStatus { get; set; }

        public void EntryPoint(bool isDown, ref bool isRun)
        {
            if (!Enable) return;
            var fWindow = SystemWindow.ForegroundWindow().FocusedWindow();
            if (!(fWindow.KeyboardLayout.Equals(new CultureInfo("ru-RU")) ||
                  fWindow.KeyboardLayout.Equals(new CultureInfo("en-US")))) return;
            var filter = SettingsLayer.GetWindowConfig(fWindow.ProcessName, fWindow.ClassName);
            if (filter.TextSwitchStatus == FilterConfig.TextSwitchMethod.Off ||
                filter.TextReplaceStatus == TextReplaceMethod.Off) return;
            if (SettingsLayer._copyPaster.NotUseClipboardForTextBoxAndRichTextBox)
            {
                foreach (
                    var word in fWindow.ClassName.Split(new char[] {' ', '.'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word.Equals("RichEdit20W", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filter.TextReplaceStatus = TextReplaceMethod.RichTextBox;
                        break;
                    }
                    if (word.Equals("Edit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filter.TextReplaceStatus = TextReplaceMethod.TextBox;
                        break;
                    }
                    if (word.Equals("TEdit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filter.TextReplaceStatus = TextReplaceMethod.TextBox;
                        break;
                    }
                }
            }

            isRun = true;

            CopyPaster copyPaster = new CopyPaster(fWindow, filter.TextReplaceStatus);
            string text = "";
            copyPaster.BackupClipboard();
            if (copyPaster.CopyText(ref text))
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    text = TextConverter(text, fWindow.KeyboardLayout);
                    copyPaster.PasteText(text);
                }
                copyPaster.RestoreClipboard();
            }
        }

        private string TextConverter(string text, CultureInfo layout)
        {
            const string RusKey =
                "Ё!\"№;%:?*()_+ЙЦУКЕНГШЩЗХЪ/ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,ё1234567890-=йцукенгшщзхъ\\фывапролджэячсмитьбю. ";
            const string EngKey =
                "~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./ ";

            string str = "";
            if (layout.Equals(new CultureInfo("ru-RU")))
            {
                foreach (char c in text)
                {
                    try
                    {
                        str += EngKey.Substring(RusKey.IndexOf(c), 1);
                    }
                    catch
                    {
                        str += c;
                    }
                }
            }
            else if (layout.Equals(new CultureInfo("en-US")))
            {
                foreach (char c in text)
                {
                    try
                    {
                        str += RusKey.Substring(EngKey.IndexOf(c), 1);
                    }
                    catch
                    {
                        str += c;
                    }
                }
            }
            else
                str = text;
            return str;
        }
    }


    public class MyModules
    {
        private static HotKeyManager _hotKeyManager = new HotKeyManager();
        public static XWheelModule _XWheelModule = new XWheelModule();
        public static XMoveModule _XMoveModule = new XMoveModule();
        public static TextSwitchModule _TextSwitchModule = new TextSwitchModule();

        public static void RunProgramm(BackgroundWorker worker, DoWorkEventArgs e)
        {
            _hotKeyManager.InitHook();
        }

        public static void ExitProgramm()
        {
            _hotKeyManager.DisposeHook();
        }

        public static void HotKeyEvent(int moduleId, bool isDown, ref bool isRun)
        {
            switch (moduleId)
            {
                case XMoveModule.Id: 
                    _XMoveModule.EntryPoint(isDown,ref isRun);
                    break;
                case TextSwitchModule.Id:
                    _TextSwitchModule.EntryPoint(isDown, ref isRun);
                    break;
            }
        }
    }
}
