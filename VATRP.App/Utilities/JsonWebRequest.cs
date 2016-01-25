using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class JsonWebRequest
    {

        // To provide a generic web request that will attempt to load the information into the 
        // specified (T) class/type
        public static T MakeJsonWebRequestAuthenticated<T>(string webRequestUrl, string userName, string password)
        {
                        WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.Credentials = new NetworkCredential(userName, password);
                request.PreAuthenticate = true;
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = request.GetResponse();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                try
                {
                    // deserialize json to ResourceInfo List
                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to parse json response. Details: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get json information. Details: " + ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }            
            }
        }

        public static T MakeJsonWebRequest<T>(string webRequestUrl)
        {
            WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = request.GetResponse();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                try
                {
                    // deserialize json to ResourceInfo List
                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to parse json response. Details: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get json information. Details: " + ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }            
            }
        }
    }
}
