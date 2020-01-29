using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalystContainer.CoreModContracts
{
    public interface ICoreModule
    {
        string DisplayName { get; }
        
        string UniqueID { get; }
        void Boot(string id, Form player, Exposure exp);
    }

    public class Exposure
    {

    }

    public class Container
    {
        public string DisplayName;
        public string CoreMod;
    }
}
