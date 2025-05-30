using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 读取嵌入资源的内容
                using (Stream stream = assembly.GetManifestResourceStream(filePath))
                {
                    if (stream == null)
                    {
                        Console.WriteLine($"未找到资源: {filePath}");
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
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
