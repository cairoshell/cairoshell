using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProshellNT2.TargetingPack;

namespace CombineContainer
{
    public class Class1 : I_NT2Addon
    {
        public string Name => "Combine for NT 2";

        public Image Icon => Properties.Resources.baselogo;

        public void OnBoot()
        {

        }

        public void OnRun(Form mainForm)
        {
            mainform mf = new mainform();
            mf.MdiParent = mainForm;
            mf.Show();
        }

        public void OnShutdown()
        {

        }
    }
}
