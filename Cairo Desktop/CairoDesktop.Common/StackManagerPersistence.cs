using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;

namespace CairoDesktop.Common
{
    internal class StackManagerPersistence
    {
        private readonly string _configFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private readonly XmlSerializer _serializer;

        public StackManagerPersistence()
        {
            _serializer = System.Xml.Serialization.XmlSerializer.FromTypes(new[] { typeof(List<string>) })[0];
        }

        public void Save(IEnumerable<string> stackLocations)
        {
            List<string> locationPaths = new List<string>();

            foreach (string dir in stackLocations)
            {
                locationPaths.Add(dir);
            }

            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    "
            };

            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(_configFile, settings);
            _serializer.Serialize(writer, locationPaths);

            writer.Close();
        }

        public IEnumerable<string> Load()
        {
            List<string> result = new List<string>();
            if (ShellHelper.Exists(_configFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(_configFile);

                if (_serializer.Deserialize(reader) is List<string> locationPaths)
                {
                    foreach (string path in locationPaths)
                    {
                        result.Add(path);
                    }
                }

                reader.Close();
            }
            else
            {
                // Add some default folders on first run
                result.AddRange(new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None),
                    KnownFolders.GetPath(KnownFolder.Downloads, NativeMethods.KnownFolderFlags.None)
                });

                // Save
                Save(result);
            }

            return result;
        }
    }
}