using System;
using System.IO;
using System.Net;
using System.Text;

namespace sLYNCy_WPF
{
    public class WebRequests
    {
        public HttpWebRequest request;
        private HttpWebResponse response;
        public string url = "";
        public string postData = "";
        public MainWindow UI;

        public WebRequests()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }
        public void InitialiseRequest()
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();
        }


        public HttpWebResponse MakeGETRequest()
        {
            try
            {
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                return response;
            }
            catch (WebException webex)
            {
                response = webex.Response as HttpWebResponse;
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public HttpWebResponse MakePOSTRequest()
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                response = (HttpWebResponse)request.GetResponse();
                return response;
            }
            catch (WebException webex)
            {
                response = (HttpWebResponse)webex.Response;
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetResponseString(HttpWebResponse response)
        {
            if (response != null)
            {
                try
                {
                    Stream test = response.GetResponseStream();
                    string responseString = "";
                    if (test.CanRead)
                    {
                        responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        return responseString;
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
            return "";
        }

        public static WebHeaderCollection GetResponseHeaders(HttpWebResponse response)
        {
            if (response != null)
            {
                try
                {
                    WebHeaderCollection headers = response.Headers;
                    return headers;
                }
                catch (Exception ept)
                {
                    return null;
                }
            }
            return null;
        }

        public static HttpWebResponse MakeWebRequest(WebRequests r)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)r.request.GetResponse();
                return response;
            }
            catch (WebException webException)
            {
                HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
                return errorResponse;
            }
        }
    }
}
