using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher.HotKeys
{
    public class HotKey
    {
        public List<Keys> KeyList { get; }
        public HotKeyType KeyType { get; }
        public int ModuleId { get; }

        public bool Check { get; private set; }

        private bool HotKeyEvent(bool isDown)
        {
            bool isRun = false;
            EventLayer.HotKeyEvent(ModuleId, isDown, ref isRun);
            return isRun;
        }

        public HotKey(List<Keys> keyses, HotKeyType type, int moduleId)
        {
            if (type.HasFlag(HotKeyType.Press | HotKeyType.Hold))
            {
                type &= ~HotKeyType.IsEnableHandled;
            }
            KeyList = keyses;
            KeyType = type;
            ModuleId = moduleId;
            Check = false;
        }

        public void KeyDown(bool isExclusive, ref bool handled)
        {
            if (!KeyType.HasFlag(HotKeyType.IsOtherNotPress) || isExclusive)
            {
                handled = (KeyType.HasFlag(HotKeyType.Press | HotKeyType.Hold) &&
                           HotKeyEvent(true) &&
                           KeyType.HasFlag(HotKeyType.IsEnableHandled)) || handled;

                Check = !KeyType.HasFlag(HotKeyType.Press);
            }
        }

        public void KeyNotEqually(bool isDown, int count)
        {
            if (KeyType.HasFlag(HotKeyType.Hold))
            {
                HotKeyEvent(false);
                Check = false;
            }
            else if (count == 0)
            {
                HotKeyEvent(true);
                Check = false;
            }
            else
                Check = !isDown;
        }

        public void KeyRepeat()
        {
            if (KeyType.HasFlag(HotKeyType.Hold)) HotKeyEvent(true);
            if (KeyType.HasFlag(HotKeyType.ReleaseNotHold)) Check = false;
        }
    }


    [Flags]
    public enum HotKeyType : int
    {
        Press = 1,
        Hold = 2,
        Release = 4,
        ReleaseNotHold = 8,
        IsOtherNotPress = 256,
        IsEnableHandled = 512,
        Mask = 127
    }

}
