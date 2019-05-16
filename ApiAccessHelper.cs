using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
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
        public const string PD_API_KEY = "0b9f8a7f360f41c3264ab14ed5d2a760ecaf39f3";
        public const string FRONT_API_KEY = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY29wZXMiOlsic2hhcmVkOioiXSwiaWF0IjoxNTU2MzEzNjI3LCJpc3MiOiJmcm9udCIsInN1YiI6ImxlYW5zZXJ2ZXIiLCJqdGkiOiI5MDZkYTc3NjA2NWVkOTA5In0.b28IHdaeo0YXwq4dy-xEbzG54RkHnXcOwrbMpbJ5LyY";

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

        public static dynamic GetResponseFromPipedriveApi(string relativeApiUrl, string apiKey, bool urlParameters = false)
        {
            {
                // Retrieve the pipedrive deal info
                HttpWebRequest rq;
                if (!urlParameters)
                    rq = (HttpWebRequest)WebRequest.Create(String.Format("https://api.pipedrive.com/v1{0}?api_token={1}", relativeApiUrl, apiKey));
                else
                    rq = (HttpWebRequest)WebRequest.Create(String.Format("https://api.pipedrive.com/v1{0}&api_token={1}", relativeApiUrl, apiKey));

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

        public static dynamic GetResponseFromFrontApi(string relativeApiUrl, string apiKey)
        {

            //todo INTRODUCING DELAY to avoid rate limiting. Will fix by serialization(better, faster option) OR increasing rate limit
            //Thread.Sleep(3);

            var myUri = new Uri(String.Format("https://api2.frontapp.com{0}", relativeApiUrl));
            var myWebRequest = WebRequest.Create(myUri);
            var myHttpWebRequest = (HttpWebRequest)myWebRequest;
            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + apiKey);
            myHttpWebRequest.Accept = "application/json";


            //todo =============================================
            // NEED TO FIX THIS
            //todo =============================================
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
            catch (WebException e) {
                return null;
            }
            
        }

    }
}
