using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace PostSwitcher
{

    [Flags]
    public enum BindingType : int
    {
        [Description("По нажатию")]
        Press = 1,
        [Description("Удерживать")]
        Hold = 2,
        [Description("По отпусканию всех клавиш")]
        Release = 4,
        [Description("По отпусканию, без удержания")]
        ReleaseNotHold = 8,
        IsOtherNotPress = 256,
        IsEnableHandled = 512,
        Mask = 127
    }

    public class BindingItem
    {
        public List<Keys> KeyList { get; }
        public BindingType KeyType { get; }
        public int ModuleId { get; }
        public int Param { get; }

        public delegate void HotKeyDelegate(bool isDown, BindingItem e);
        public HotKeyDelegate HotKeyEvent;
        public bool Check { get; private set; }

       
        public BindingItem(List<Keys> keys, BindingType type, int moduleId, int param)
        {
            if (type.HasFlag(BindingType.Release | BindingType.ReleaseNotHold))
            {
                type &= ~BindingType.IsEnableHandled;
            }
            KeyList = keys;
            KeyType = type;
            ModuleId = moduleId;
            Param = param;
            Check = false;
        }

        public void KeyDown(bool isExclusive)
        {
            if (KeyType.HasFlag(BindingType.IsOtherNotPress) && !isExclusive) return;
            if (KeyType.HasFlag(BindingType.Press | BindingType.Hold)) HotKeyEvent(true, this);
            Check = !KeyType.HasFlag(BindingType.Press);
        }

        public void KeyNotEqually(bool isDown, int count)
        {
            if (KeyType.HasFlag(BindingType.Hold))
            {
                HotKeyEvent(false, this);
                Check = false;
            }
            else if (count == 0) //Срабатываем при отпускании
            {
                HotKeyEvent(true, this);
                Check = false;
            }
            else //Флаг на отпускание, а мы нажали
                Check = !isDown;
        }

        public void KeyRepeat()
        {
            if (KeyType.HasFlag(BindingType.Hold)) HotKeyEvent(true, this);
            if (KeyType.HasFlag(BindingType.ReleaseNotHold)) Check = false;
        }
    }
}
