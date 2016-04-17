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
        private bool _isExclusive = true;

        private void AppendKey(Keys key)
        {
            if (key == _keyList.LastOrDefault())
            {
                lock (SettingsLayer._hotkeyList)
                {
                    foreach (var hotkey in SettingsLayer._hotkeyList)
                    {
                        if (hotkey.Check) hotkey.KeyRepeat();
                    }
                }
            }
            else if (!_keyList.Contains(key))
            {
                _handled = false;
                _keyList.Add(key);
                int count = _keyList.Count;
                if (count != 1 || key != Keys.LButton)
                {
                    lock (SettingsLayer._hotkeyList)
                    {
                        foreach (var hotkey in SettingsLayer._hotkeyList)
                        {
                            if (hotkey.Check)
                            {
                                hotkey.KeyNotEqually(true, count);
                            }
                            else if (hotkey.KeyList.Count == count)
                            {
                                int i;
                                for (i = 0; i < count && hotkey.KeyList[i] == _keyList[i]; i++) ;
                                if (i == count) hotkey.KeyDown(_isExclusive, ref _handled);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Ошибка HKM, нажата нажатая кнопка");
                //return;
            }
        }

        private void RemoveKey(Keys key)
        {
            int count = _keyList.Count - 1;
            var index = _keyList.LastIndexOf(key);
            _isExclusive = 0 == count;
            _handled = index == count;
            if (index < 0 || index > count)
            {
                throw new Exception("Ошибка HKM, отпущена не нажатая кнопка");
                //return; 
            }
            _keyList.RemoveAt(index);

            lock (SettingsLayer._hotkeyList)
            {
                foreach (var hotkey in SettingsLayer._hotkeyList)
                {
                    if (hotkey.Check) hotkey.KeyNotEqually(false, count);
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
            if (_keyList.Count > 0)
            {
                AppendKey(e.KeyCode);
                RemoveKey(e.KeyCode);
            }
            else
            {
                MyModules.XWheel(e);
            }
        }

        public void InitHook()
        {
            _mouseHook.MouseDown += MouseHook_MouseDown;
            _mouseHook.MouseUp += MouseHook_MouseUp;
            _mouseHook.MouseWheel += MouseHook_MouseWheel;
            _keyboardHook.KeyDown += KeyboardHook_KeyDown;
            _keyboardHook.KeyUp += KeyboardHook_KeyUp;

            _mouseHook.MouseMove += MyModules.MouseMove;
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

            _mouseHook.MouseMove -= MyModules.MouseMove;
            _mouseHook.Unhook();
            _keyboardHook.Unhook();
        }
    }
}
