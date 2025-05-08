using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace WebService
{
    public sealed class WebService
    {
        public string ErrorMessage = "";

        private static bool ValidSecurity(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public string GetResponseResult(string url, System.Text.Encoding encoding, string method, string postData, string[] headers, string ContentType = "application/x-www-form-urlencoded", bool secValid = true)
        {
            method = method.ToUpper();
            if (secValid == false)
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidSecurity;
            }
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

            if (method == "GET")
            {
                try
                {
                    WebRequest request2 = WebRequest.Create(@url);

                    request2.Method = method;
                    WebResponse response2 = request2.GetResponse();
                    if (headers != null)
                    {
                        for (int i = 0; i < headers.GetLength(0); i++)
                        {
                            if (headers[i].Split(':').Length < 2)
                            {
                                continue;
                            }

                            if (headers[i].Split(':').Length > 1)
                            {
                                if (headers[i].Split(':')[0] == "Content-Type")
                                {
                                    request2.ContentType = headers[i].Split(':')[1];
                                    continue;
                                }
                            }
                            request2.Headers.Add(headers[i]);
                        }
                    }

                    Stream stream = response2.GetResponseStream();
                    var reader = new StreamReader(stream, encoding);
                    string content2 = reader.ReadToEnd();
                    return content2;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return "";
                }

            }
            if (method == "POST")
            {

                Stream outstream = null;
                Stream instream = null;
                StreamReader sr = null;
                HttpWebResponse response = null;

                HttpWebRequest request = null;
                byte[] data = encoding.GetBytes(postData);
                // 准备请求...
                try
                {
                    // 设置参数
                    request = WebRequest.Create(url) as HttpWebRequest;
                    var cookieContainer = new CookieContainer();
                    request.CookieContainer = cookieContainer;
                    request.AllowAutoRedirect = true;
                    request.Method = method;
                    request.Timeout = 1000000;
                    request.ContentType = ContentType;
                    if (headers != null)
                    {
                        for (int i = 0; i < headers.GetLength(0); i++)
                        {
                            if (headers[i].Split(':').Length < 2)
                            {
                                continue;
                            }

                            if (headers[i].Split(':').Length > 1)
                            {
                                if (headers[i].Split(':')[0] == "Host")
                                {
                                    request.Host = headers[i].Split(':')[1];
                                    continue;
                                }
                                else if (headers[i].Split(':')[0] == "Content-Type")
                                {
                                    request.ContentType = headers[i].Split(':')[1];
                                    continue;
                                }
                                else if (headers[i].Split(':')[0] == "Connection")
                                {
                                    request.KeepAlive = headers[i].Split(':')[1] != "close";
                                    continue;
                                }
                                else if (headers[i].Split(':')[0] == "Accept")
                                {
                                    request.Accept = headers[i].Split(':')[1];
                                    continue;
                                }

                            }
                            request.Headers.Add(headers[i]);
                        }
                    }
                    request.ContentLength = data.Length;
                    outstream = request.GetRequestStream();
                    outstream.Write(data, 0, data.Length);
                    outstream.Close();
                    //发送请求并获取相应回应数据
                    response = request.GetResponse() as HttpWebResponse;
                    //直到request.GetResponse()程序才开始向目标网页发送Post请求
                    instream = response.GetResponseStream();
                    sr = new StreamReader(instream, encoding);
                    //返回结果网页（html）代码
                    string content = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();

                    return content;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return "";
                }
            }
            ErrorMessage = "不正确的方法类型。（目前仅支持GET/POST）";
            return "";

        }//get response result
    }
}
