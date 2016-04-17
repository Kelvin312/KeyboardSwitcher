using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KeyboardSwitcher.HotKeys;

namespace KeyboardSwitcher
{
    internal class MyModules
    {
        public static void XWheel(MouseEventExtArgs e)
        {
            if(!SettingsLayer.XWheelConfig.Enable) return;
            var window = SystemWindow.WindowFromPoint(e.Location);
            window.SendMouseWheel(e);
            e.Handled = true;
        }

        private static bool _isMove = false;
        private static SystemWindow _windowMove;
        private static Point _offsetMove;

        public static void XMove(bool isDown, ref bool isRun)
        {
            if (!SettingsLayer.XMoveConfig.Enable) return;
            if (!_isMove && isDown)
            {
                var cursorPos = Cursor.Position;
                _windowMove = SystemWindow.WindowFromPoint(cursorPos).RootWindow();
                _offsetMove = _windowMove.OffsetFromPoint(cursorPos);
                Cursor.Current = Cursors.Hand;
            }
            _isMove = isDown;
            if (!_isMove)
            {
                Cursor.Current = Cursors.Default;
            }
            isRun = true;
        }

        public static void MouseMove(object sender, MouseEventExtArgs e)
        {
            if (_isMove)
            {
                _windowMove.MoveWindowToPoint(new Point(e.X - _offsetMove.X,e.Y-_offsetMove.Y), 
                    SettingsLayer.XMoveConfig.IsActivate,SettingsLayer.XMoveConfig.IsTop);
            }
        }

        public void TextSwitch(bool isDown, ref bool isRun)
        {
            
        }

    }
}
