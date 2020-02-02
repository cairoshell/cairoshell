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
        public static void openEditor(Form mf)
        {
            theEditor editor = new theEditor();
            editor.skinningManager1.ParentForm = null;
            editor.MdiParent = mf;
            editor.Show();
        }
        public static void openCalc(Form mf)
        {
            theCalc editor = new theCalc();
            editor.skinningManager1.ParentForm = null;
            editor.MdiParent = mf;
            editor.Show();
        }
        public static void openTime(Form mf)
        {
            TheTime editor = new TheTime();
            editor.skinningManager1.ParentForm = null;
            editor.MdiParent = mf;
            editor.Show();
        }
        public static void openAbout(Form mf)
        {
            AboutBox1 about = new AboutBox1();
            about.skinningManager1.ParentForm = null;
            about.MdiParent = mf;
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
