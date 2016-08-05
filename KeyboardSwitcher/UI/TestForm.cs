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

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mc.Mute = checkBox1.Checked;
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            mc = new MixerControl();
           // mc.Init();
            checkBox1.Checked = mc.Mute;
            trackBar1.Value = (int)mc.MasterVolume;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            mc.MasterVolume = (float) trackBar1.Value;
        }
    }
}
