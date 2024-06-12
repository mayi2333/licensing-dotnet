using System;
using System.Globalization;
using System.Xml;

namespace Licensing.Validators
{
    public static class LicenseParser
    {

        public static License LoadLicenseContent(string licenseContent)
        {
            if (string.IsNullOrEmpty(licenseContent))
            {
                throw new ArgumentNullException("licenseContent", "Must be a valid file path");
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(licenseContent);
            return Parse(xmlDocument);
        }

        public static License LoadLicenseFile(string licensePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(licensePath);
            return Parse(xmlDocument);
        }

        public static XmlDocument OpenFile(string licenseFilename)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(licenseFilename);
            return xmlDocument;
        }

        public static License Parse(XmlDocument doc)
        {
            License license = new License();
            license.FileContents = doc;
            if (license.Signature == null)
            {
                throw new InvalidFormatException("Could not find the crypto-signature for the license");
            }

            XmlNode xmlNode = doc.SelectSingleNode("/license/@id");
            if (xmlNode == null)
            {
                throw new InvalidFormatException("Could not find id attribute in the license");
            }

            license.UserId = new Guid(xmlNode.Value);
            XmlNode xmlNode2 = doc.SelectSingleNode("/license/@expiration");
            if (xmlNode2 == null)
            {
                throw new InvalidFormatException("Could not find the expiration date in the license");
            }

            license.ExpirationDate = DateTime.ParseExact(xmlNode2.Value, "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            XmlNode xmlNode3 = doc.SelectSingleNode("/license/@type");
            if (xmlNode3 == null)
            {
                throw new InvalidFormatException("Could not determine the license type");
            }

            license.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), xmlNode3.Value);
            XmlNode xmlNode4 = doc.SelectSingleNode("/license/name/text()");
            if (xmlNode4 == null)
            {
                throw new InvalidFormatException("Could not find licensee's name in license");
            }

            license.Name = xmlNode4.Value;
            XmlNode xmlNode5 = doc.SelectSingleNode("/license");
            if (xmlNode5?.Attributes == null)
            {
                return license;
            }

            foreach (XmlAttribute attribute in xmlNode5.Attributes)
            {
                if (!(attribute.Name == "type") && !(attribute.Name == "expiration") && !(attribute.Name == "id"))
                {
                    Console.WriteLine("License has attribute: [" + attribute.Name + ", " + attribute.Value + "]");
                }
            }

            return license;
        }
    }
}
