using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class HotKey
    {
        public List<Keys> KeyList { get; }
        public SpecialKeyType SpecialKey { get; }
        public HotKeyType Type { get; }
        public bool IsOtherNotPress { get; }
        public CheckState Check { get; set; } //Если сработал 

        public delegate void HotKeyEventHandler(bool isDown);
        public event HotKeyEventHandler HotKeyEvent;

        private int _index;
        private bool _isOtherPress;

        public HotKey(List<Keys> keyses, SpecialKeyType special = SpecialKeyType.None, 
            HotKeyType type = HotKeyType.Press, bool isOtherNotPress = false)
        {
            KeyList = keyses;
            SpecialKey = special;
            Type = type;
            IsOtherNotPress = isOtherNotPress;

            _isOtherPress = false;
            _index = 0;
            Check = CheckState.Uncheck;
        }

        public void AllKeyRelease()
        {
            if (Check == CheckState.Activate)
            {
                if (Type == HotKeyType.Release || Type == HotKeyType.ReleaseNotHold) HotKeyEvent?.Invoke(true);
                if (Type == HotKeyType.Hold) HotKeyEvent?.Invoke(false);
            }
            _isOtherPress = false;
            _index = 0;
            Check = CheckState.Uncheck;
        }

        public void KeyChanged(List<Keys> keyses, bool isAdd)
        {
            bool isEqually = false;
            if (IsOtherNotPress)
            {
                if (!_isOtherPress &&
                    keyses.Count == _index + 1 &&
                    _index < KeyList.Count &&
                    keyses[_index] == KeyList[_index])
                {
                    _index++;
                    isEqually = _index == KeyList.Count;
                }
                else
                {
                    _isOtherPress = true;
                }
            }
            else
            {
                _index = 0;
                if (keyses.Count == KeyList.Count)
                {
                    foreach (var key in keyses)
                    {
                        if (key != KeyList[_index])
                        {
                            break;
                        }
                        _index++;
                    }
                }
                isEqually = _index == KeyList.Count;
            }


            if ((Check == CheckState.Activate || Check == CheckState.Down) && !isEqually) //Если другая кнопка
            {
                if (Check == CheckState.Activate && Type == HotKeyType.Hold) HotKeyEvent?.Invoke(false);
                if ((Type != HotKeyType.Release && Type != HotKeyType.ReleaseNotHold) || 
                    Check == CheckState.Down || isAdd)
                    Check = CheckState.Uncheck;
            }

            if (isEqually && Check == CheckState.Uncheck && isAdd) //Если правильная кнопка
            {
                if (SpecialKey == SpecialKeyType.None)
                {
                    Check = CheckState.Activate;
                    if (Type == HotKeyType.Press || Type == HotKeyType.Hold)
                        HotKeyEvent?.Invoke(true);
                }
                else
                {
                    Check = CheckState.Down;
                }
            }
        }

        public void KeyRepeatOrSpecial(SpecialKeyType sKey)
        {
            if (Check == CheckState.Activate && Type == HotKeyType.ReleaseNotHold)
                Check = CheckState.Uncheck; //Если не повторять

            if (sKey == SpecialKeyType.None) //Repeat
            {
                if (Check == CheckState.Activate && Type == HotKeyType.Hold)
                    HotKeyEvent?.Invoke(true);
            }
            else
            {
                if (SpecialKey == sKey) //Если всё ок
                {
                    if (Check == CheckState.Down) Check = CheckState.Activate;
                    if (Check == CheckState.Activate && (Type == HotKeyType.Press || Type == HotKeyType.Hold))
                        HotKeyEvent?.Invoke(true);
                }
                else
                {
                    if (Check == CheckState.Activate)
                    {
                        if(Type == HotKeyType.Hold) HotKeyEvent?.Invoke(false);
                        if (Type != HotKeyType.Press) Check = CheckState.Uncheck;
                    }
                    _isOtherPress = IsOtherNotPress;
                }
            }
        }
    }

    internal enum CheckState
    {
        Uncheck = 0,
        Down = 1,
        Activate = 2,
        Error = 3
    }

    internal enum SpecialKeyType
    {
        None = 0,
        WheelUp = 120,
        WheelDown = -120
    }

    internal enum HotKeyType
    {
        Press,
        Hold,
        Release,
        ReleaseNotHold
    }
}
