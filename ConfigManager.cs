using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SlickUpdater {
    static class ConfigManager {



        public static string configFileName = "config.xml";
        //Fetchers

        public static string fetch(string element) {
            XDocument xdoc = XDocument.Load(configFileName);
            return xdoc.Element("SlickUpdater").Element(element).Value;
        }
        public static string fetch(string element, string subElement) {
            XDocument xdoc = XDocument.Load(configFileName);
            return xdoc.Element("SlickUpdater").Element(element).Element(subElement).Value;

        }
        public static string fetch(string element, string subElement, string subSubElement) {
            XDocument xdoc = XDocument.Load(configFileName);
            return xdoc.Element("SlickUpdater").Element(element).Element(subElement).Element(subSubElement).Value;
        }


        // Writers

        public static void write(string element, string value) {
            XDocument xdoc = XDocument.Load(configFileName);
            xdoc.Element("SlickUpdater").SetElementValue(element, value);
            xdoc.Save(configFileName);
        }
        public static void write(string element, string subElement, string value) {
            XDocument xdoc = XDocument.Load(configFileName);
            xdoc.Element("SlickUpdater").Element(element).SetElementValue(subElement, value);
            xdoc.Save(configFileName);
        }
        public static void write(string element, string subElement, string subSubElement, string value) {
            XDocument xdoc = XDocument.Load(configFileName);
            xdoc.Element("SlickUpdater").Element(element).Element(subElement).SetElementValue(subSubElement , value);
            xdoc.Save(configFileName);
        }
    }
}
