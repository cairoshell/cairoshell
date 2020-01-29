using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Windows.Forms;

namespace Catalyst_Containers
{
    public class ContainerInstancing
    {
        public class Container
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public string CoreModID { get; set; }
            public bool theCatExp { get; set; }
            public bool fsExp { get; set; }

        }

        public static void saveCont(Container cont)
        {
            Directory.CreateDirectory(Constants.ContainerPath(cont.ID));
            if (File.Exists(Constants.ContainerPath(cont.ID) + @"\config.json"))
                File.Copy(Constants.ContainerPath(cont.ID) + @"\config.json", Constants.ContainerPath(cont.ID) + @"\config.001", true); //Backs up the Container.
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

        public static void deleteCont(Container cont)
        {
            Directory.Delete(Constants.ContainerPath(cont.ID), true);
        }
        public static Container[] getListOfContainers()
        {
            List<Container> conts = new List<Container>();
            var dirs = Directory.GetDirectories(Constants.ContainerPath());
            foreach (string xo in dirs)
            {
                string x = Path.GetFileName(xo);
                if (File.Exists(Constants.ContainerPath(x) + @"\config.json"))
                {
                    conts.Add(loadCont(x));
                }
            }
            return conts.ToArray();
        }
    }

        public class RandomGenerator
        {
            // Generate a random number between two numbers    
            public int RandomNumber(int min, int max)
            {
                Random random = new Random();
                return random.Next(min, max);
            }

            // Generate a random string with a given size    
            public string RandomString(int size, bool lowerCase)
            {
                StringBuilder builder = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }
                if (lowerCase)
                    return builder.ToString().ToLower();
                return builder.ToString();
            }

            // Generate a random password    
            public string RandomPassword()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(RandomString(4, true));
                builder.Append(RandomNumber(1000, 9999));
                builder.Append(RandomString(2, false));
                return builder.ToString();
            }
        }
}
