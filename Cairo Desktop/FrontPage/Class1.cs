using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontPage
{
    public class Init
    {
        public static void StartDashboard()
        {
            Dashboard db = new Dashboard();
            db.ShowDialog();
        }
    }
}
