using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PostSwitcher
{
    public class BindingManager
    {
        private MouseHook _mouseHook = new MouseHook();
        private KeyboardHook _keyboardHook = new KeyboardHook();
        private List<Keys> _keyList = new List<Keys>();
        private SortedSet<Keys> _handledList = new SortedSet<Keys>();
        private bool _isExclusive = true;

        public bool IsHandled { get; set; }
        public bool IsAnyNotPressed => _keyList.Count == 0;
        public List<BindingItem> BindingList { get; }
        public List<Keys> LastPressKeys { get; }

        public BindingManager()
        {
            BindingList = new List<BindingItem>();
            LastPressKeys = new List<Keys>();
        }

        public void RemoveBindingItem(int index)
        {
            lock (BindingList)
            {
                var item = BindingList[index];
                if (item.Check && item.KeyType.HasFlag(BindingType.Hold))
                {
                    item.HotKeyEvent(false, item);
                }
                BindingList.RemoveAt(index);
            }
        }

        public void AddBindingItem(BindingItem item)
        {
            BindingList.Add(item);
        }

        public void ActivateHotKey(bool isDown, BindingItem e)
        {
            IsHandled |= e.KeyType.HasFlag(BindingType.IsEnableHandled);
        }

        private bool AppendKey(Keys key)
        {
            if (key == _keyList.LastOrDefault()) //Удерживается последняя нажатая кнопка
            {
                lock (BindingList)
                {
                    foreach (var hotkey in BindingList)
                    {
                        if (hotkey.Check) hotkey.KeyRepeat();
                    }
                }
                return _handledList.Contains(key);
            }
            else if (!_keyList.Contains(key)) //Нажата новая кнопка
            {
                IsHandled = false;
                _keyList.Add(key);
                int count = _keyList.Count;
                if (count == 1 && key == Keys.LButton) return false;
                lock (BindingList)
                {
                    foreach (var hotkey in BindingList)
                    {
                        if (hotkey.Check)
                        {
                            hotkey.KeyNotEqually(true, count);
                        }
                        else if (hotkey.KeyList.Count == count)
                        {
                            int i;
                            for (i = 0; i < count && hotkey.KeyList[i] == _keyList[i]; i++) ;
                            if (i == count) hotkey.KeyDown(_isExclusive);
                        }
                    }
                }

                lock (LastPressKeys)
                {
                    if (LastPressKeys.Count == _keyList.Count - 1)
                    {
                        LastPressKeys.Add(key);
                    }
                    else
                    {
                        LastPressKeys.Clear();
                        LastPressKeys.AddRange(_keyList);
                    }
                }

                if (!IsHandled) return false;
                _handledList.Add(key);
                return true;
            }
            else
            {
                throw new Exception("Ошибка HKM, нажата нажатая кнопка");
                //return;
            }
        }

        private bool RemoveKey(Keys key)
        {
            var index = _keyList.LastIndexOf(key);
            _isExclusive = _keyList.Count == 1;
            if (index < 0 || index >= _keyList.Count)
            {
                throw new Exception("Ошибка HKM, отпущена не нажатая кнопка");
                //return; 
            }
            _keyList.RemoveAt(index);
            lock (BindingList)
            {
                foreach (var hotkey in BindingList)
                {
                    if (hotkey.Check) hotkey.KeyNotEqually(false, _keyList.Count);
                }
            }

            if (!_handledList.Contains(key)) return false;
            _handledList.Remove(key);
            return true;
        }


        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = AppendKey(e.KeyCode);
        }

        private void KeyboardHook_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = RemoveKey(e.KeyCode);
        }

        private void MouseHook_MouseDown(object sender, MouseEventExtArgs e)
        {
            e.Handled = AppendKey(e.KeyCode);
        }

        private void MouseHook_MouseUp(object sender, MouseEventExtArgs e)
        {
            e.Handled = RemoveKey(e.KeyCode);
        }

        private void MouseHook_MouseWheel(object sender, MouseEventExtArgs e)
        {
            if (_keyList.Count > 0)
            {
                e.Handled = AppendKey(e.KeyCode);
                RemoveKey(e.KeyCode);
            }
            else
            {
                //MyModules._XWheelModule.MouseWheel(e);
            }
        }

        private void _mouseHook_MouseMove(object sender, MouseEventExtArgs e)
        {
            //MyModules._XMoveModule.MouseMove(e.Location);
        }

        public void InitHook()
        {
            _isExclusive = true;
            _keyList.Clear();
            _handledList.Clear();
            LastPressKeys.Clear();

            _mouseHook.MouseDown += MouseHook_MouseDown;
            _mouseHook.MouseUp += MouseHook_MouseUp;
            _mouseHook.MouseWheel += MouseHook_MouseWheel;
            _keyboardHook.KeyDown += KeyboardHook_KeyDown;
            _keyboardHook.KeyUp += KeyboardHook_KeyUp;

            _mouseHook.MouseMove += _mouseHook_MouseMove;
            _mouseHook.SetHook();
            _keyboardHook.SetHook();
        }

        public void DisposeHook()
        {
            _mouseHook.MouseDown -= MouseHook_MouseDown;
            _mouseHook.MouseUp -= MouseHook_MouseUp;
            _mouseHook.MouseWheel -= MouseHook_MouseWheel;
            _keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            _keyboardHook.KeyUp -= KeyboardHook_KeyUp;

            _mouseHook.MouseMove -= _mouseHook_MouseMove;
            _mouseHook.Unhook();
            _keyboardHook.Unhook();
        }
    }
}
