using System;
using System.IO;
using System.Text;

namespace Licensing.Validators
{
    public class FileHelper
    {
        /// <summary>
        /// 当前根目录绝对路径
        /// </summary>
        public static string FullPath => AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="data">字符串</param>
        /// <param name="fileName">相对路径或者绝对路径</param>
        /// <param name="isFullPath">是否绝对路径 </param>
        public static void SaveFile(string data, string fileName, bool isFullPath = false)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data.ToString());
            string fullPath = $"{FullPath.TrimEnd('\\')}\\{fileName.TrimStart('\\')}";
            if (isFullPath)
            {
                fullPath = fileName;
            }
            SaveFile(buffer, fullPath);
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="data">byte数组</param>
        /// <param name="fullPath">文件绝对路径</param>
        public static void SaveFile(byte[] data, string fullPath)
        {
            CreateDirectory(fullPath);
            using (FileStream fsWrite = new FileStream($"{fullPath}", FileMode.Create, FileAccess.Write))
            {
                fsWrite.Write(data, 0, data.Length);
            }
        }
        /// <summary>
        /// 如果路径不存在创建路径
        /// </summary>
        /// <param name="fullPath">完整路径</param>
        public static void CreateDirectory(string fullPath)
        {
            string fullDir = fullPath.Substring(0, fullPath.LastIndexOf('\\') + 1);
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }
        }
    }
}
