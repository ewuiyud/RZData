using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Tools
{
    public static class FileTool
    {
        public static string ReadFileContent(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                return content;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("错误: 关键文件丢失，请从新安装!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"错误: 发生了一个未知错误: {e.Message}");
            }
            return null;
        }
    }
}
