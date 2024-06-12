using System;
using System.Collections.Generic;
using System.Xml;

namespace Licensing.Validators
{
    public class License
    {
        private XmlDocument _fileContents;

        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();


        public DateTime ExpirationDate { get; set; }

        public XmlDocument FileContents
        {
            get
            {
                return _fileContents;
            }
            set
            {
                _fileContents = value;
                if (_fileContents == null)
                {
                    Signature = null;
                    return;
                }

                XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(_fileContents.NameTable);
                xmlNamespaceManager.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");
                Signature = (XmlElement)_fileContents.SelectSingleNode("//sig:Signature", xmlNamespaceManager);
            }
        }

        public LicenseType LicenseType { get; set; }

        public string Name { get; set; }

        public XmlElement Signature { get; private set; }

        public Guid UserId { get; set; }
    }
}
