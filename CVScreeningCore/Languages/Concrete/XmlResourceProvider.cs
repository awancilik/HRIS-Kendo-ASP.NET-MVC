using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CVScreeningCore.Languages.Abstract;
using CVScreeningCore.Languages.Entities;

namespace CVScreeningCore.Languages.Concrete
{
    public class XmlResourceProvider : BaseResourceProvider
    {
        // File path
        private static string _filePath;

        public XmlResourceProvider()
        {
        }

        public XmlResourceProvider(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(string.Format("XML Resource file {0} was not found", filePath));
        }

        protected override IList<ResourceEntry> ReadResources()
        {
            // Parse the XML file
            var xElement = XDocument.Parse(File.ReadAllText(_filePath))
                .Element("resources");

            return xElement != null
                ? xElement
                    .Elements("resource")
                    .Select(e => new ResourceEntry
                    {
                        Name = e.Attribute("name").Value,
                        Value = e.Attribute("value").Value,
                        Culture = e.Attribute("culture").Value
                    }).ToList()
                : null;
        }

        protected override ResourceEntry ReadResource(string name, string culture)
        {
            // Parse the XML file
            var xElement = XDocument.Parse(File.ReadAllText(_filePath))
                .Element("resources");
            return xElement != null
                ? xElement
                    .Elements("resource")
                    .Where(e => e.Attribute("name").Value == name && e.Attribute("culture").Value == culture)
                    .Select(e => new ResourceEntry
                    {
                        Name = e.Attribute("name").Value,
                        Value = e.Attribute("value").Value,
                        Culture = e.Attribute("culture").Value
                    }).FirstOrDefault()
                : null;
        }
    }
}