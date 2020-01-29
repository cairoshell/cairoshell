using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace ProshellNT2.TargetingPack
{
    public interface I_NT2Addon
    {
        string Name { get; }
        Image Icon { get; }
        void OnRun(Form mainForm);
        void OnBoot();
        void OnShutdown();
    }
}
