using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace theCatalyst
{
    public class Init
    {
        public static void openEditor()
        {
            theEditor editor = new theEditor();
            editor.Show();
        }
        public static void openCalc()
        {
            theCalc editor = new theCalc();
            editor.Show();
        }
        public static void openTime()
        {
            TheTime editor = new TheTime();
            editor.Show();
        }
        public static void openAbout()
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }
    }
    public class Accounts
    {
        public static void Reset()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
        }
    }
}
