using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProshellNT2
{
    public class NT_CM : CatalystContainer.CoreModContracts.ICoreModule
    {
        public string DisplayName
        {
            get
            {
                return "Proshell NT 2";
            }
        }

        public string UniqueID
        {
            get
            {
                return "NT2.2020";
            }
        }

        public void Boot(string id, Form player)
        {
            throw new NotImplementedException();
        }
    }
}
