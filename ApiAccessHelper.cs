using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading;

namespace FrontPipedriveIntegrationProject
{
    public class ApiAccessHelper
    {
        public static string PD_API_KEY;
        public static string FRONT_API_KEY;

        public static dynamic PostPipedriveJson(string relativeApiUrl, object data, string method = "POST")
        {
            // Retrieve the pipedrive deal info
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(String.Format("https://api.pipedrive.com/v1{0}?api_token={1}", relativeApiUrl, PD_API_KEY));
            rq.Method = method;
            rq.AllowWriteStreamBuffering = true;
            rq.SendChunked = false;
            rq.ContentType = "application/json";
            using (var request = rq.GetRequestStream())
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                string dataString = serializer.Serialize(data);

                //  BUGBUG: Pipedrive does not handle utf-8, must use ASCII or get "invalid or malformed json" error
                using (var sw = new StreamWriter(request, Encoding.ASCII))
                {
                    sw.Write(dataString);
                    sw.Flush();
                    request.Close();

                    using (var response = rq.GetResponse())
                    {
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            var content = sr.ReadToEnd();

                            dynamic obj = serializer.DeserializeObject(content);

                            if (obj["success"])
                            {
                                return obj["data"];
                            }
                            else
                            {
                                throw new InvalidOperationException("API returned error");
                            }
                        }
                    }
                }
            }
        }

        public static dynamic GetResponseFromPipedriveApi(string relativeApiUrl, bool urlParameters = false)
        {
            {
                // Retrieve the pipedrive deal info
                HttpWebRequest rq;
                if (!urlParameters)
                    rq = (HttpWebRequest)WebRequest.Create(String.Format("https://api.pipedrive.com/v1{0}?api_token={1}", relativeApiUrl, PD_API_KEY));
                else
                    rq = (HttpWebRequest)WebRequest.Create(String.Format("https://api.pipedrive.com/v1{0}&api_token={1}", relativeApiUrl, PD_API_KEY));

                using (var response = rq.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var content = sr.ReadToEnd();

                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        dynamic obj = serializer.DeserializeObject(content);

                        if (obj["success"])
                        {
                            return obj["data"];
                        }
                        else
                        {
                            throw new InvalidOperationException("API returned error");
                        }
                    }
                }
            }
        }

        public static dynamic GetResponseFromFrontApi(string relativeApiUrl)
        {
            var myUri = new Uri(String.Format("https://api2.frontapp.com{0}", relativeApiUrl));
            var myWebRequest = WebRequest.Create(myUri);
            var myHttpWebRequest = (HttpWebRequest)myWebRequest;
            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + FRONT_API_KEY);
            myHttpWebRequest.Accept = "application/json";
            
            try
            {
                var myWebResponse = myWebRequest.GetResponse();

                var responseStream = myWebResponse.GetResponseStream();
                if (responseStream == null) return null;

                var myStreamReader = new StreamReader(responseStream, Encoding.Default);
                var jsonString = myStreamReader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                if (jsonString.Length > serializer.MaxJsonLength) serializer.MaxJsonLength = jsonString.Length;
                var json = serializer.Deserialize<object>(jsonString);

                responseStream.Close();
                myWebResponse.Close();
                return json;
            }
            catch (WebException) {
                ;
                //decimal resetTimestamp = Convert.ToDecimal(e.Response.Headers.Get("X-RateLimit-Reset"));
                //decimal unixTimestamp = (decimal)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                //if(unixTimestamp < resetTimestamp)
                //Thread.Sleep((int)(resetTimestamp - unixTimestamp) *1000);
                //Console.WriteLine("Sleeping for " + (resetTimestamp - unixTimestamp));
                Console.WriteLine("Sleeping for 4 seconds");
                Thread.Sleep(4000);
                return GetResponseFromFrontApi(relativeApiUrl);
            }            
        }
    }
}