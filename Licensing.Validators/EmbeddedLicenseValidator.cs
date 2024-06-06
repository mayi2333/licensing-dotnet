using Fortelinea.Licensing.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Yort.Ntp;

namespace Licensing.Validators
{
    public class EmbeddedLicenseValidator : LicenseValidator
    {
        private readonly NtpClient _ntpClient = new NtpClient("time-a-b.nist.gov");
        /// <summary>
        ///     Creates a new instance of <seealso cref="EmbeddedLicenseValidator" />.
        /// </summary>
        /// <param name="publicKey">public key</param>
        public EmbeddedLicenseValidator(string publicKey) : base(publicKey) { }

        /// <param name="license"></param>
        /// <inheritdoc />
        protected override Task<License> HandleUpdateableLicenseAsync(License license) { return Task.FromResult(license); }

        /// <summary>
        /// 验证授权信息
        /// </summary>
        /// <param name="xmlLicensePath"></param>
        /// <returns></returns>
        /// <exception cref="CryptoSignatureVerificationFailedException"></exception>
        /// <exception cref="LicenseExpiredException"></exception>
        public override async Task AssertValidLicenseAsync(string xmlLicensePath)
        {
            Guid userId = GetUserId();
            //if (!File.Exists(xmlLicensePath))
            //{
            //    var showDialog = new LicensingValidatorForm(xmlLicensePath, userId).ShowDialog();
            //    if (showDialog == System.Windows.Forms.DialogResult.Cancel)
            //    {
            //        System.Environment.Exit(System.Environment.ExitCode);
            //    }
            //    else if (!File.Exists(xmlLicensePath))
            //    {
            //        System.Environment.Exit(System.Environment.ExitCode);
            //    }
            //}
            License license = LicenseParser.LoadLicenseFile(xmlLicensePath);

            await ValidateLicenseAsync(license, userId);
        }

        public async Task ValidateLicenseAsync(License license, Guid userId)
        {
            License updatedLicense = await HandleUpdateableLicenseAsync(license);
            ValidateLicenseType(updatedLicense);
            ValidateLicenseUserId(updatedLicense, userId);
            //new XmlDocument().LoadXml(xmlLicenseContents);
            if (!IsValidLicenseCryptoSignature(updatedLicense))
            {
                throw new CryptoSignatureVerificationFailedException("许可证签名验证失败");
            }

            if (await IsLicenseExpiredAsync(updatedLicense))
            {
                throw new LicenseExpiredException($"许可证过期 {updatedLicense.ExpirationDate}");
            }
        }

        private void ValidateLicenseUserId(License license, Guid userId)
        {
            if (license.UserId != userId)
            {
                throw new LicenseExpiredException($"用户Id验证失败");
            }
        }

        public Guid GetUserId()
        {
            MD5 md5 = MD5.Create();
            StringBuilder computerInfo = new StringBuilder();
            computerInfo.Append(GetDiskID());
            computerInfo.Append(GetComputerName());
            computerInfo.Append(GetUserName());
            computerInfo.Append(GetSystemType());
            computerInfo.Append(GetCpuID());
            computerInfo.Append(GetTotalPhysicalMemory());
            byte[] data = Encoding.UTF8.GetBytes(computerInfo.ToString());
            byte[] md5data = md5.ComputeHash(data);
            return new Guid(md5data);
        }
        #region 获取计算机信息
        /// <summary>
        /// 获取硬盘ID
        /// </summary>
        /// <returns>硬盘ID</returns>
        public static string GetDiskID()
        {
            try
            {
                string HDid = "";
                using (ManagementClass mc = new ManagementClass("Win32_DiskDrive"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            HDid = (string)mo.Properties["Model"].Value;
                        }
                        return HDid;
                    }
                }
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 获取计算机名
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            try
            {
                return System.Environment.MachineName;

            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        /// <summary>
        /// 操做系统的登陆用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            try
            {
                string un = "";

                un = Environment.UserName;
                return un;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        /// <summary>
        /// 获取PC类型
        /// </summary>
        /// <returns>PC类型</returns>
        public static string GetSystemType()
        {
            try
            {
                string st = "";
                using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {

                            st = mo["SystemType"].ToString();

                        }
                        return st;
                    }
                }
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        /// <summary>
        /// 获取CPU序列号代码
        /// </summary>
        /// <returns>CPU序列号代码</returns>
        public static string GetCpuID()
        {
            try
            {
                string cpuInfo = "";//cpu序列号
                using (ManagementClass mc = new ManagementClass("Win32_Processor"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        }
                        return cpuInfo;
                    }
                }
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        /// <summary>
        /// 获取物理内存
        /// </summary>
        /// <returns>物理内存</returns>
        public static string GetTotalPhysicalMemory()
        {
            try
            {
                string st = "";
                using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            st = mo["TotalPhysicalMemory"].ToString();
                        }
                        return st;
                    }
                }
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        #endregion
        /// <inheritdoc />
        protected override void ValidateLicenseType(License license)
        {
            switch (license.LicenseType)
            {
                case LicenseType.None: return;
                case LicenseType.Floating: throw new NotImplementedException();
                case LicenseType.Personal: throw new NotImplementedException();
                case LicenseType.Standard: throw new NotImplementedException();
                case LicenseType.Subscription: throw new NotImplementedException();
                case LicenseType.Trial: throw new NotImplementedException();
            }
        }
        private async Task<DateTime> GetCurrentDateTimeAsync()
        {
            //var now = NTPTimeHelper.GetNetworkTime();
            //if (now == DateTime.MinValue)
            //{
            //    throw new NetworkUnavailableException("获取当前时间异常");
            //}
            //return now;
            //if (!NetworkInterface.GetIsNetworkAvailable())
            //{
            //    if (RequireNetworkTimeCheck)
            //    {
            //        throw new NetworkUnavailableException();
            //    }

            //    return DateTime.UtcNow;
            //}

            //try
            //{
            //    return (await _ntpClient.RequestTimeAsync()).NtpTime;
            //}
            //catch (Exception ex)
            //{
            //    if (RequireNetworkTimeCheck)
            //    {
            //        throw ex;
            //    }

            //    return DateTime.UtcNow;
            //}

            return (await _ntpClient.RequestTimeAsync()).NtpTime;
        }
        private async Task<bool> IsLicenseExpiredAsync(License license)
        {
            return await GetCurrentDateTimeAsync() > license.ExpirationDate;
        }
    }
}
