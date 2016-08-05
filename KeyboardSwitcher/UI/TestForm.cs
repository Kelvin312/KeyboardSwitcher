using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PostSwitcher;

namespace KeyboardSwitcher.UI
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private MixerControl mc;
        private MouseHook mo;
        private SystemWindow wnd;

        private void TestForm_Load(object sender, EventArgs e)
        {
            mc = new MixerControl();
            // mc.Init();
            checkBox1.Checked = mc.Mute;
            trackBar1.Value = (int)mc.MasterVolume;

            mo = new MouseHook();
            mo.SetHook();
            mo.MouseDown += Mo_MouseDown;
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
            string text="";
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
            string text = "";
            bool res = cp.CopyTextSendKeys(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextSendKeys(text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            CopyPaster cp = new CopyPaster(wnd);
            string text = "";
            bool res = cp.CopyTextPressKeys(ref text);
            textBox2.Text = res.ToString() + "\r\n" + text;
            text = "{хаХа 159 vBn}";
            cp.PasteTextPressKeys(text);
        }
    }
}
