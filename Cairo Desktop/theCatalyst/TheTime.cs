using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NeoGeniX.Forms;

namespace theCatalyst
{
    public partial class TheTime : Form
    {
        Dictionary<string, TimeZoneInfo> tzd = new Dictionary<string, TimeZoneInfo>();
        ReadOnlyCollection<TimeZoneInfo> tz;
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
        string countDownString;
        DateTime endTime;
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
                    case "Default":
                        selection = 1;
                        break;
                    case "theGroove":
                        selection = 2;
                        break;
                    case "Jungle":
                        selection = 3;
                        break;
                }
                timersecs = (numericUpDown1.Value * 60 * 60) + (numericUpDown2.Value * 60) + numericUpDown3.Value;
                endTime = DateTime.Now.AddSeconds((double)timersecs);
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
            TimeSpan leftTime = endTime.Subtract(DateTime.Now);
            if (leftTime.TotalSeconds < 0)
            {
                countDownString = "00:00:00:00";
                label4.Text = countDownString;
                timer1.Stop();
                button2.Enabled = false;
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                button1.Text = "Start";
                theTimerFinish finish = new theTimerFinish(selection);
                finish.MdiParent = this.MdiParent;
                finish.Show();
            }
            else
            {
                countDownString = leftTime.Hours.ToString("00") + ":" +
                  leftTime.Minutes.ToString("00") + ":" +
                  leftTime.Seconds.ToString("00") + ":" +
                   (leftTime.Milliseconds / 10).ToString("00");
                label4.Text = countDownString;
            }
        }

        private void TheTime_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;

            #region World Clock Code Init
            
            tz = TimeZoneInfo.GetSystemTimeZones();

            foreach (TimeZoneInfo timezone in tz)
            {
                comboBox2.Items.Add(timezone.DisplayName);
                tzd.Add(timezone.DisplayName, timezone);
            }
            TimeZoneInfo ctz = TimeZoneInfo.Local;
            comboBox2.SelectedItem = ctz.DisplayName;
            #endregion
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //get current time
            TimeZoneInfo stz = tzd[comboBox2.Text];
            DateTime tzdt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, stz);
            int hh = tzdt.Hour;
            int mm = tzdt.Minute;
            int ss = tzdt.Second;
            //time
            bool ispm = false;
            string time = "";

            //12-hour handling
            if (hh > 12)
            {
                hh = hh - 12;
                ispm = true;
            }
            //padding leading zero
            if (hh < 10)
            {
                time += "0" + hh;
            }
            else
            {
                time += hh;
            }
            time += ":";

            if (mm < 10)
            {
                time += "0" + mm;
            }
            else
            {
                time += mm;
            }
            time += ":";

            if (ss < 10)
            {
                time += "0" + ss;
            }
            else
            {
                time += ss;
            }

            time += " ";
            if (ispm)
            {
                time += "PM";
            }
            else
            {
                time += "AM";
            }
            label6.Text = time;
        }
        private DateTime _start;
        private void button3_Click(object sender, EventArgs e)
        {
            _start = DateTime.Now;
            timer3.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            TimeSpan duration = DateTime.Now - _start;
            label5.Text = String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer3.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start("timedate.cpl");
        }
    }
}
