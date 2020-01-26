using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrontPage
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //get current time
            int hh = DateTime.Now.Hour;
            int mm = DateTime.Now.Minute;
            int ss = DateTime.Now.Second;
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
            label2.Text = DateTime.Today.ToLongDateString();
            label1.Text = time;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
