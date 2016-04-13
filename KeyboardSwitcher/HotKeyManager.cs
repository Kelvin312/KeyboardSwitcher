using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyboardSwitcher
{
    internal class HotKeyManager
    {
        private MouseHook mouseHook = new MouseHook();
        private KeyboardHook keyboardHook = new KeyboardHook();
        private List<Keys> keyList = new List<Keys>();


        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void KeyboardHook_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void MouseHook_MouseDown(object sender, MouseEventExtArgs e)
        {
            throw new NotImplementedException();
        }

        private void MouseHook_MouseUp(object sender, MouseEventExtArgs e)
        {
            throw new NotImplementedException();
        }

        private void MouseHook_MouseWheel(object sender, MouseEventExtArgs e)
        {
            throw new NotImplementedException();
        }












        public HotKeyManager()
        {
            
        }

        public void InitHook()
        {
            mouseHook.MouseDown += MouseHook_MouseDown;
            mouseHook.MouseUp += MouseHook_MouseUp;
            mouseHook.MouseWheel += MouseHook_MouseWheel;
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.KeyUp += KeyboardHook_KeyUp;
            mouseHook.StartHook();
            keyboardHook.StartHook();
        }

        public void DisposeHook()
        {
            mouseHook.MouseDown -= MouseHook_MouseDown;
            mouseHook.MouseUp -= MouseHook_MouseUp;
            mouseHook.MouseWheel -= MouseHook_MouseWheel;
            keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            keyboardHook.KeyUp -= KeyboardHook_KeyUp;
            mouseHook.Unhook();
            keyboardHook.Unhook();
        }
    }
}
