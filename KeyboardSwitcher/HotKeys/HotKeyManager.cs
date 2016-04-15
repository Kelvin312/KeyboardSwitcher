using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher.HotKeys
{
    public class HotKeyManager
    {
        private MouseHook _mouseHook = new MouseHook();
        private KeyboardHook _keyboardHook = new KeyboardHook();
        private List<Keys> _keyList = new List<Keys>();
        
        private bool _handled = false;

        private void RepeatKey(Keys key)
        {
            lock (SettingsLayer._hotkeyList)
            {
                foreach (var hotkey in SettingsLayer._hotkeyList)
                {
                    if (hotkey.Check)
                        hotkey.KeyChanged(_keyList, ChangedType.Repeat, ref _handled);
                }
            }
        }

        private void AppendKey(Keys key)
        {
            _handled = false;
            if (key == _keyList.LastOrDefault()) 
            {
                RepeatKey(key);
                return;
            }
            if (!_keyList.Contains(key))
            {
                _keyList.Add(key);
                lock (SettingsLayer._hotkeyList)
                {
                    foreach (var hotkey in SettingsLayer._hotkeyList)
                    {
                        hotkey.KeyChanged(_keyList, ChangedType.Down, ref _handled);
                    }
                }
            }
        }

        private void RemoveKey(Keys key)
        {
            _handled = false;
            _keyList.Remove(key);
            lock (SettingsLayer._hotkeyList)
            {
                foreach (var hotkey in SettingsLayer._hotkeyList)
                {
                    hotkey.KeyChanged(_keyList, ChangedType.Up, ref _handled);
                }
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
            _mouseHook.MouseDown += MouseHook_MouseDown;
            _mouseHook.MouseUp += MouseHook_MouseUp;
            _mouseHook.MouseWheel += MouseHook_MouseWheel;
            _keyboardHook.KeyDown += KeyboardHook_KeyDown;
            _keyboardHook.KeyUp += KeyboardHook_KeyUp;
            _mouseHook.StartHook();
            _keyboardHook.StartHook();
        }

        public void DisposeHook()
        {
            _mouseHook.MouseDown -= MouseHook_MouseDown;
            _mouseHook.MouseUp -= MouseHook_MouseUp;
            _mouseHook.MouseWheel -= MouseHook_MouseWheel;
            _keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            _keyboardHook.KeyUp -= KeyboardHook_KeyUp;
            _mouseHook.Unhook();
            _keyboardHook.Unhook();
        }
    }
}
