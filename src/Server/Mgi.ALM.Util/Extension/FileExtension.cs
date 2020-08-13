using System.IO;
using System.Text;

namespace Mgi.ALM.Util.Extension
{
    /// <summary>
    /// 对文件求MD5码，保证文件的一致性
    /// </summary>
    public static class FileCode
    {
        /// <summary>
        /// 文件的MD5码
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Md5(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
