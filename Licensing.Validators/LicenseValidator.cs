using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml;

namespace Licensing.Validators
{
    public abstract class LicenseValidator
    {
        private readonly CspParameters _cspParameters;

        private readonly int _keySize;

        private readonly string _publicKey;

        public bool RequireNetworkTimeCheck { get; set; } = true;


        protected LicenseValidator(string publicKey, int keySize = 4096, CspParameters cspParameters = null)
        {
            _publicKey = publicKey;
            _keySize = keySize;
            _cspParameters = cspParameters;
        }

        public virtual async Task AssertValidLicenseAsync(string xmlLicenseContents)
        {
            License license = LicenseParser.LoadLicenseContent(xmlLicenseContents);
            License updatedLicense = await HandleUpdateableLicenseAsync(license);
            ValidateLicenseType(updatedLicense);
            new XmlDocument().LoadXml(xmlLicenseContents);
            if (!IsValidLicenseCryptoSignature(updatedLicense))
            {
                throw new CryptoSignatureVerificationFailedException();
            }

            if (await IsLicenseExpiredAsync(updatedLicense))
            {
                throw new LicenseExpiredException($"License expired {updatedLicense.ExpirationDate}");
            }
        }

        protected abstract Task<DateTime> GetCurrentDateTimeAsync();
        
        protected abstract Task<License> HandleUpdateableLicenseAsync(License license);

        private async Task<bool> IsLicenseExpiredAsync(License license)
        {
            return await GetCurrentDateTimeAsync() > license.ExpirationDate;
        }

        protected bool IsValidLicenseCryptoSignature(License clientLicense)
        {
            RSACryptoServiceProvider rSACryptoServiceProvider = ((_cspParameters == null) ? new RSACryptoServiceProvider(_keySize) : new RSACryptoServiceProvider(_keySize, _cspParameters));
            RSAKeyExtensions.FromXmlString(rSACryptoServiceProvider, _publicKey);
            SignedXml signedXml = new SignedXml(clientLicense.FileContents);
            signedXml.LoadXml(clientLicense.Signature);
            return signedXml.CheckSignature(rSACryptoServiceProvider);
        }

        protected abstract void ValidateLicenseType(License license);
    }
}
