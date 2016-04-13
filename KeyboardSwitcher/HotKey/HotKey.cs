using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class HotKey
    {
        private readonly List<Keys> _keyList;
        private readonly HotKeyType _type;
        private readonly bool _isOtherNotPress;
        private bool _isEnableHandled;
        private readonly HotKeyEventHandler _hotKeyEvent;
        private int _index;
        private bool _isOtherPress;

        public bool Check { get; set; } //Если сработал 

        public HotKey(List<Keys> keyses, HotKeyEventHandler hotKeyEvent, bool isEnableHandled = true,
            HotKeyType type = HotKeyType.Press, bool isOtherNotPress = false)
        {
            _keyList = keyses;
            _hotKeyEvent = hotKeyEvent;
            _type = type;
            _isOtherNotPress = isOtherNotPress;
            _isEnableHandled = isEnableHandled && (type == HotKeyType.Press || type == HotKeyType.Hold);

            _isOtherPress = false;
            _index = 0;
            Check = false;
        }

        public void KeyChanged(List<Keys> keyses, ChangedType ct, ref bool handled)
        {
            switch (ct)
            {
                case ChangedType.Down:
                    if (IsEqually(keyses))
                    {
                        switch (_type)
                        {
                            case HotKeyType.Press:
                            case HotKeyType.Hold:
                                _hotKeyEvent(true);
                                break;

                        }
                        Check = true;
                    }
                    else
                    {
                        if (Check && _type == HotKeyType.Hold) _hotKeyEvent(false);
                        Check = false;
                    }
                    break;
                case ChangedType.Repeat:
                    if (Check && _type == HotKeyType.Hold) _hotKeyEvent(true);
                    if (Check && _type == HotKeyType.ReleaseNotHold) Check = false;
                    break;
                case ChangedType.Up:
                    if (_isOtherNotPress) --_index;
                    if (Check && _type == HotKeyType.Hold)
                    {
                        _hotKeyEvent(false);
                        Check = false;
                    }
                    if (keyses.Count == 0)
                    {
                        if(Check && (_type == HotKeyType.Release || _type == HotKeyType.ReleaseNotHold)) _hotKeyEvent(true);
                        _isOtherPress = false;
                        _index = 0;
                        Check = false;
                    }
                    break;
            }
            if (Check && _isEnableHandled) handled = true;
        }


        private bool IsEqually(List<Keys> keyses)
        {
            if (_isOtherNotPress)
            {
                if (!_isOtherPress &&
                    keyses.Count == _index + 1 &&
                    _index < _keyList.Count &&
                    keyses[_index] == _keyList[_index])
                {
                    _index++;
                    return _index == _keyList.Count;
                }
                _isOtherPress = true;
                return false;
            }
            else
            {
                _index = 0;
                if (keyses.Count == _keyList.Count)
                {
                    foreach (var key in keyses)
                    {
                        if (key != _keyList[_index]) return false;
                        _index++;
                    }
                }
                return true;
            }
        }
    }

    internal delegate void HotKeyEventHandler(bool isDown);

    internal enum ChangedType
    {
        Down = 0,
        Up = 1,
        Repeat = 3,
    }

   
    internal enum HotKeyType
    {
        Press,
        Hold,
        Release,
        ReleaseNotHold
    }
}
