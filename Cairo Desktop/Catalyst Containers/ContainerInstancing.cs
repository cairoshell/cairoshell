using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Catalyst_Containers
{
    class ContainerInstancing
    {
        public class Container{
            public string ID;
            public string DisplayName;
            public string CoreModID;
            public bool theCatExp;
            public bool fsExp;

}

        public static void saveCont(Container cont)
        {
            File.Copy(Constants.ContainerPath(cont.ID) + @"\config.json", Constants.ContainerPath(cont.ID) + @"\config.001",true); //Backs up the Container.
            var jsonFile = File.CreateText(Constants.ContainerPath(cont.ID) + @"\config.json");
            JsonSerializer js = new JsonSerializer();
            js.Serialize(jsonFile, cont);
            jsonFile.Close();
        }

        public static Container loadCont(string id)
        {
            var json = File.OpenText(Constants.ContainerPath(id) + @"\config.json");
            Container container = JsonConvert.DeserializeObject<Container>(json.ReadToEnd());
            json.Close();
            return container;
        }

        public static Container[] getListOfContainers()
        {
            List<Container> conts = new List<Container>();
            var dirs = Directory.GetDirectories(Constants.ContainerPath());
            foreach (string x in dirs)
            {
                if (File.Exists(Constants.ContainerPath(x) + @"\config.json"))
                {
                    conts.Add(loadCont(x));
                }
            }
            return conts.ToArray();
        }
    }
}
