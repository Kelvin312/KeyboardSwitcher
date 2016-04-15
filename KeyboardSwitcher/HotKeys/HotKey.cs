using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher.HotKeys
{
    public class HotKey
    {
        public  List<Keys> KeyList { get; }
        public HotKeyType Type { get; }
        public bool IsOtherNotPress { get; }
        public bool IsEnableHandled { get; }
        public HotKeyEventType EventType { get; }

        private int _index;
        private bool _isOtherPress;
        private bool _isRun;

        public bool Check { get; private set; }
       

        public HotKey(List<Keys> keyses, HotKeyEventType eventType, bool isEnableHandled = true,
            HotKeyType type = HotKeyType.Press, bool isOtherNotPress = false)
        {
            KeyList = keyses;
            EventType = eventType;
            Type = type;
            IsOtherNotPress = isOtherNotPress;
            IsEnableHandled = isEnableHandled && (type == HotKeyType.Press || type == HotKeyType.Hold);

            _isOtherPress = false;
            _index = 0;
            Check = false;
        }

        public void KeyChanged(List<Keys> keyses, ChangedType ct, ref bool handled)
        {
            switch (ct)
            {
                case ChangedType.Down:
                    _isRun = false;
                    if (IsEqually(keyses))
                    {
                        switch (Type)
                        {
                            case HotKeyType.Press:
                            case HotKeyType.Hold:
                                EventLayer._hotKeyEvent(EventType,true,ref _isRun);
                                break;

                        }
                        Check = true;
                    }
                    else
                    {
                        if (Check && Type == HotKeyType.Hold) EventLayer._hotKeyEvent(EventType, false, ref _isRun);
                        Check = false;
                    }
                    break;
                case ChangedType.Repeat:
                    if (Check && Type == HotKeyType.Hold) EventLayer._hotKeyEvent(EventType, true,ref _isRun);
                    if (Check && Type == HotKeyType.ReleaseNotHold) Check = false;
                    break;
                case ChangedType.Up:
                    _isRun = false;
                    if (IsOtherNotPress) --_index;
                    if (Check && Type == HotKeyType.Hold)
                    {
                        EventLayer._hotKeyEvent(EventType, false, ref _isRun);
                        Check = false;
                    }
                    if (keyses.Count == 0)
                    {
                        if(Check && (Type == HotKeyType.Release || Type == HotKeyType.ReleaseNotHold))
                            EventLayer._hotKeyEvent(EventType, true, ref _isRun);
                        _isOtherPress = false;
                        _index = 0;
                        Check = false;
                    }
                    break;
            }
            if (_isRun && IsEnableHandled) handled = true;
        }


        private bool IsEqually(List<Keys> keyses)
        {
            if (IsOtherNotPress)
            {
                if (!_isOtherPress &&
                    keyses.Count == _index + 1 &&
                    _index < KeyList.Count &&
                    keyses[_index] == KeyList[_index])
                {
                    _index++;
                    return _index == KeyList.Count;
                }
                _isOtherPress = true;
                return false;
            }
            else
            {
                _index = 0;
                if (keyses.Count == KeyList.Count)
                {
                    foreach (var key in keyses)
                    {
                        if (key != KeyList[_index]) return false;
                        _index++;
                    }
                }
                return true;
            }
        }
    }



    public enum ChangedType
    {
        Down = 0,
        Up = 1,
        Repeat = 3,
    }


    public enum HotKeyType
    {
        Press,
        Hold,
        Release,
        ReleaseNotHold
    }

   
}
