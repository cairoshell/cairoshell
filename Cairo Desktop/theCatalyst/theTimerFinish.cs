using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using NeoGeniX.Forms;
using NeoGeniX.Controls;

namespace theCatalyst
{
    public partial class theTimerFinish : DarkForm
    {
        private SoundPlayer Player;
        public theTimerFinish(int Alarm)
        {
            InitializeComponent();
            if (Alarm == 1)
            {
                Player = new SoundPlayer(Properties.Resources.Alarm3);
            } else if (Alarm == 2)
            {
                Player = new SoundPlayer(Properties.Resources.Alarm1);
            }
            else if (Alarm == 3)
            {
                Player = new SoundPlayer(Properties.Resources.Alarm2);
            }
        }

        private void theTimerFinish_Load(object sender, EventArgs e)
        {
            Player.PlayLooping();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Player.Stop();
            this.Close();
        }
    }
}
