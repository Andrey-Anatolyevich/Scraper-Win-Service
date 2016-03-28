using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace ParserCore
{
    internal class WebHelper
    {
        public bool GetHtmlFromUrl(string url, out string result)
        {
            result = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ProtocolVersion = HttpVersion.Version11;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*;q=0.8";
                request.Headers.Set("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                request.Headers.Set("Accept-Encoding", "gzip, deflate, sdch");

                request.Method = "GET";
                request.Timeout = 10000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return false;

                    Stream receiveStream = response.GetResponseStream();
                    GZipStream zipStream = new GZipStream(receiveStream, CompressionMode.Decompress);

                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                        readStream = new StreamReader(zipStream);
                    else
                        readStream = new StreamReader(zipStream, Encoding.GetEncoding(response.CharacterSet));

                    result = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}