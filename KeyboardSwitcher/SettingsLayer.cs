using System;
using System.Collections.Generic;
using KeyboardSwitcher.HotKeys;

namespace KeyboardSwitcher
{
    internal static class SettingsLayer
    {
        public static List<HotKey> _hotkeyList = new List<HotKey>();

        public static class XWheelConfig
        {
            public static bool Enable { get; set; }
        }

        public static class XMoveConfig
        {
            public static bool Enable { get; set; }
            public static bool IsActivate { get; set; }
            public static bool IsTop { get; set; }
        }
    }
}
