using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Catalyst_Containers
{
    public static class Init
    {
        public static void Start()
        {
            MainForm mf = new MainForm();
            mf.Show();
        }
    }
}
