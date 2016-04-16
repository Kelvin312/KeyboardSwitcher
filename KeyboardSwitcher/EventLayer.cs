using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeyboardSwitcher.HotKeys;

namespace KeyboardSwitcher
{
    public static class EventLayer
    {
        private static HotKeyManager hotKeyManager = new HotKeyManager();
        private static MyModules myModules = new MyModules();


        public static void RunProgramm()
        {
            
        }

        public static void HotKeyEvent(HotKeyEventType eventType, bool isDown, ref bool isRun)
        {
            
        }

    }
    public enum HotKeyEventType
    {
        a, b, c
    }
}
