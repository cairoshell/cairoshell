using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProshellNT2.TargetingPack;

namespace ProshellNT2.Accessories
{
    public class MNotepad : I_NT2Addon
    {
        public string Name => "Notepad";

        public Image Icon => Properties.Resources.baselogo;

        public string Description => "Edits contents of ASCII-Based Files";

        public string Author => "Proshell Network";

        public ModVersion Version => new ModVersion(1, 0, 0, DevState.Alpha);

        public void OnBoot(NT_Interface NT)
        {
            

        }

        public void OnRun(Form mainForm, NT_Interface NT)
        {
            notepad mf = new notepad();
            mf.MdiParent = mainForm;
            mf.Show();
        }

        public void OnShutdown(NT_Interface NT)
        {

        }
    }
    public class MFileBrowser : I_NT2Addon
    {
        public string Name => "File Manager";

        public Image Icon => Properties.Resources.baselogo;

        public string Description => "Manages Files and Folder";

        public string Author => "Proshell Network";

        public ModVersion Version => new ModVersion(1, 0, 0, DevState.Alpha);

        public void OnBoot(NT_Interface NT)
        {

        }

        public void OnRun(Form mainForm, NT_Interface NT)
        {
            Form1 window = new Form1();
            window.Fileopendict = NT.FileExt;
            window.MdiParent = mainForm;
            window.Show();
        }

        public void OnShutdown(NT_Interface NT)
        {

        }
    }
    public class MCalculator : I_NT2Addon
    {
        public string Name => "Calculator";

        public Image Icon => Properties.Resources.baselogo;

        public string Description => "Performs Calculaions";

        public string Author => "Proshell Network";

        public ModVersion Version => new ModVersion(1, 0, 0, DevState.Alpha);

        public void OnBoot(NT_Interface NT)
        {

        }

        public void OnRun(Form mainForm, NT_Interface NT)
        {
            Calculator mf = new Calculator();
            mf.MdiParent = mainForm;
            mf.Show();
        }

        public void OnShutdown(NT_Interface NT)
        {

        }
    }
}
