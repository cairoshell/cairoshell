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
        string Description { get; }
        ModVersion Version { get; }
        string Author { get; }
        Image Icon { get; }
        void OnRun(Form mainForm, NT_Interface NT);
        void OnBoot(NT_Interface NT);
        void OnShutdown(NT_Interface NT);
    }
    public class ModVersion{
        
        public ModVersion(int Maj, int Min, int B, DevState Dev)
        {
            Major = Maj;
            Minor = Min;
            Build = B;
            ds = Dev;
        }
        public int Major = 0;
        public int Minor = 0;
        public int Build = 0;
        public DevState ds = DevState.Alpha;
        public override string ToString()
        {
            string dss = "";
            switch (ds)
            {
                case DevState.Alpha:
                    dss = "Alpha ";
                    break;
                case DevState.Beta:
                    dss = "Beta ";
                    break;
                case DevState.RC:
                    dss = "Release Canidate ";
                    break;
                case DevState.Release:
                    dss = "";
                    break;
                default:
                    dss = "";
                    break;
            }
            return dss + Major.ToString() + "." + Minor.ToString() + " Build " + Build.ToString();
        }
    }
    public enum DevState
    {
        Release,
        Beta,
        Alpha,
        RC
    }
    public interface IFileOpen
    {
        void useFile(string file, Form mainForm);

    }

    public class NT_Interface
    {
        public Dictionary<string, IFileOpen> FileExt = new Dictionary<string, IFileOpen>();
    }
}
