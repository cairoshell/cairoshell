using System.Collections.Generic;

namespace CairoDesktop.Localization
{
    public class DisplayString
    {
        public DisplayString()
        {

        }

        private static string getString(string stringName)
        {
            Dictionary<string, string> lang;
            bool isDefault = false;

            lang = Language.en_US;
            isDefault = true;

            if (lang.ContainsKey(stringName))
                return lang[stringName];
            else
            {
                // default is en_US - return string from there if not found in set language
                if (!isDefault)
                {
                    lang = Language.en_US;
                    if (lang.ContainsKey(stringName))
                        return lang[stringName];
                }
            }

            return stringName;
        }

        public static string sProgramsMenu
        {
            get
            {
                return getString("sProgramsMenu");
            }
        }

        public static string sPlacesMenu
        {
            get
            {
                return getString("sPlacesMenu");
            }
        }
    }
}
