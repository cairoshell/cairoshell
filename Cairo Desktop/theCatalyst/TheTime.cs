using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DarkUI.Forms;

namespace theCatalyst
{
    public partial class TheTime : DarkForm
    {
        public TheTime()
        {
            InitializeComponent();
        }
        decimal timersecs = 1;
        decimal timeron = 0;
        int selection = 1;
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value > 0)
            {
                numericUpDown3.Maximum = 0;
            } else
            {
                numericUpDown3.Maximum = 1;
                if (numericUpDown3.Value == 0) {
                    numericUpDown3.Value = 1;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                timeron = 0;
                button2.Enabled = false;
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                button1.Text = "Start";
            }
            else
            {
                switch (comboBox1.SelectedItem.ToString()){
                    case "theGroove":
                        selection = 1;
                        break;
                    case "Jungle":
                        selection = 2;
                        break;
                }
                timersecs = (numericUpDown1.Value * 60 * 60) + (numericUpDown2.Value * 60) + numericUpDown3.Value;
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                timer1.Start();
                button1.Text = "Stop";
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                button2.Text = "Resume";
            }
            else
            {
                timer1.Start();
                button2.Text = "Pause";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double secs = ((double)timersecs - (double)timeron);
            TimeSpan t = TimeSpan.FromSeconds(secs);

            label4.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                            t.Hours,
                            t.Minutes,
                            t.Seconds,
                            t.Milliseconds);
            
            timeron = timeron + (decimal)(.1);
            decimal d = (timeron / timersecs) * 100;
            progressBar1.Value = (int)Math.Round(d);
            if(timeron == timersecs)
            {
                timer1.Stop();
                timeron = 0;
                button2.Enabled = false;
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                button1.Text = "Start";
                theTimerFinish finish = new theTimerFinish(selection);
                finish.MdiParent = this.MdiParent;
                finish.Show();
            }
        }

        private void TheTime_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 1;
        }
    }
}
