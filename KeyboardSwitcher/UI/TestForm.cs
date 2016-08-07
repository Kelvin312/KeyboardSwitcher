using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using KeyboardSwitcher.KeyBinding;
using PostSwitcher;

namespace KeyboardSwitcher.UI
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        //protected override bool ShowWithoutActivation
        //{
        //    get { return true; }
        //}

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var baseParams = base.CreateParams;
        //        baseParams.ExStyle |= 0x08000000 | 0x00000080;
        //        return baseParams;
        //    }
        //}

        //protected override void DefWndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case 0x21: //WM_MOUSEACTIVATE
        //            m.Result = (IntPtr)0x0003; //MA_NOACTIVATE
        //            return;
        //    }
        //    base.DefWndProc(ref m);
        //}

        private MixerControl mc;
       // private MouseHook mo;
        private KeyboardHook ke;
        private SystemWindow wnd;
        private BindingManager man;
        private BindingItem bi;

        private void TestForm_Load(object sender, EventArgs e)
        {
            mc = new MixerControl();
            // mc.Init();
            checkBox1.Checked = mc.Mute;
            trackBar1.Value = (int) mc.MasterVolume;

           // mo = new MouseHook();
          //  mo.SetHook();
           // mo.MouseDown += Mo_MouseDown;

            ke = new KeyboardHook();
            ke.SetHook();
            ke.KeyDown += Ke_KeyDown;

            man = new BindingManager();
            man.InitHook();

             bi = new BindingItem((new []{Keys.ControlKey, Keys.K}).ToList(),BindingType.Press, 1,0);

            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            KeyBindingForm form = new KeyBindingForm(man);
            form.CreateDialog(bi);
            if (form.ShowDialog() == DialogResult.OK)
            {
                bi = form.ResultItem;
            }
            form.Close();
        }


        private void Ke_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LWin)
            {
                button4_Click(null, null);
            }
        }

        private void Mo_MouseDown(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                wnd = SystemWindow.WindowFromPoint(e.Location);
                textBox1.Text = wnd.ProcessName + " " + wnd.ClassName;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mc.Mute = checkBox1.Checked;
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            mc.MasterVolume = (float) trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            string text = "";
            bool res = cp.CopyTextFromTextBox(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextFromTextBox(text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            string text = "";
            bool res = cp.CopyTextFromRichTextBox(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextFromRichTextBox(text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            string text = "";
            bool res = cp.CopyTextWmCopyPaste(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextWmCopyPaste(text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            cp.ReplaceLastWord();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            // cp.test_1();
            string text = "";
            bool res = cp.CopyTextPressKeys(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextPressKeys(text);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBOARD_INPUT
        {
            public ushort wVk;
            public ushort wSc;
            public uint Flags;
            public uint Time;
            public IntPtr dwExtraInfo;
            public uint Padding1;
            public uint Padding2;
        }


        private void button6_Click(object sender, EventArgs e)
        {




            KEYBOARD_INPUT ex = new KEYBOARD_INPUT();
            textBox2.Text = string.Format("{0} wVk\r\n{1} wSc\r\n{2} Flags\r\n{3} Time\r\n{4} dwExtraInfo\r\n{5} Padding1\r\n{6} Padding2",
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "wVk"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "wSc"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "Flags"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "Time"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "dwExtraInfo"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "Padding1"),
                (int) Marshal.OffsetOf(typeof (KEYBOARD_INPUT), "Padding2"));

        }

        private void button7_Click(object sender, EventArgs e)
        {
            wnd.Highlight(Color.Red);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            wnd.Refresh();
        }

       
    }
}
