using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardSwitcher
{
    public static class EventLayer
    {
        public delegate void HotKeyEventHandler(HotKeyEventType eventType, bool isDown, ref bool isRun);
        
        public static HotKeyEventHandler _hotKeyEvent;

    }
    public enum HotKeyEventType
    {
        a, b, c
    }
}
