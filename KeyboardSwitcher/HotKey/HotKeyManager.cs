using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class HotKeyManager
    {
        private MouseHook mouseHook = new MouseHook();
        private KeyboardHook keyboardHook = new KeyboardHook();
        private List<Keys> keyList = new List<Keys>();
        private List<HotKey> hotkeyList = new List<HotKey>();
        private bool _handled = false;

        private void RepeatKey(Keys key)
        {
            foreach (var hotkey in hotkeyList)
            {
                if(hotkey.Check)
                    hotkey.KeyChanged(keyList, ChangedType.Repeat, ref _handled);
            }
        }

        private void AppendKey(Keys key)
        {
            _handled = false;
            if (key == keyList.LastOrDefault()) 
            {
                RepeatKey(key);
                return;
            }
            if (!keyList.Contains(key))
            {
                keyList.Add(key);
                foreach (var hotkey in hotkeyList)
                {
                    hotkey.KeyChanged(keyList, ChangedType.Down, ref _handled);
                }
            }
        }

        private void RemoveKey(Keys key)
        {
            _handled = false;
            keyList.Remove(key);
            foreach (var hotkey in hotkeyList)
            {
                hotkey.KeyChanged(keyList, ChangedType.Up, ref _handled);
            }
        }


        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            AppendKey(e.KeyCode);
        }

        private void KeyboardHook_KeyUp(object sender, KeyEventArgs e)
        {
            RemoveKey(e.KeyCode);
        }

        private void MouseHook_MouseDown(object sender, MouseEventExtArgs e)
        {
            AppendKey(e.KeyCode);
        }

        private void MouseHook_MouseUp(object sender, MouseEventExtArgs e)
        {
            RemoveKey(e.KeyCode);
        }

        private void MouseHook_MouseWheel(object sender, MouseEventExtArgs e)
        {
            AppendKey(e.KeyCode);
            RemoveKey(e.KeyCode);
        }

        public void InitHook()
        {
            mouseHook.MouseDown += MouseHook_MouseDown;
            mouseHook.MouseUp += MouseHook_MouseUp;
            mouseHook.MouseWheel += MouseHook_MouseWheel;
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.KeyUp += KeyboardHook_KeyUp;
            mouseHook.StartHook();
            keyboardHook.StartHook();
        }

        public void DisposeHook()
        {
            mouseHook.MouseDown -= MouseHook_MouseDown;
            mouseHook.MouseUp -= MouseHook_MouseUp;
            mouseHook.MouseWheel -= MouseHook_MouseWheel;
            keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            keyboardHook.KeyUp -= KeyboardHook_KeyUp;
            mouseHook.Unhook();
            keyboardHook.Unhook();
        }
    }
}
