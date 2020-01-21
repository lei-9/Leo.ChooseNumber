using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Leo.ChooseNumber.Core
{
    public class FileUtils
    {
        private static object _fileLock = new object();

        public void CreateOrAppendTxt(string relativePath, string content)
        {
            lock (_fileLock)
            {
                var filePath = $"{AppDomain.CurrentDomain.BaseDirectory}{relativePath}";
                //路径文件夹不存在则创建
                var dirPath = filePath.Substring(0, filePath.LastIndexOf("\\", StringComparison.Ordinal));
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                if (File.Exists(filePath))
                {   //存在
                    using var sw = File.AppendText(filePath);
                    sw.WriteLine(content);
                }
                else
                {
                    //不存在则创建后写入
                    using var sw = File.CreateText(filePath);
                    sw.WriteLine(content);
                }
            }
        }
    }
}
