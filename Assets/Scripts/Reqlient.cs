using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;

// https://docs.microsoft.com/ja-jp/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class

namespace Reqlient {
    public static class HttpRequest {
        public enum RequestMethod { POST, GET }
        public enum ContentType { URLEncoded, Plain, JSON }
        // -------------------------------------------------- //
        public static string Request (string requestURL, string requestBody, RequestMethod requestMethod = RequestMethod.POST, ContentType contentType = ContentType.Plain) {
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);

            if (requestMethod == RequestMethod.POST) {
                request.Method = "POST";
            } else if (requestMethod == RequestMethod.GET) {
                request.Method = "GET";
            }

            request.UserAgent = "Reqlient/1.0.0";
            request.Timeout = 5000;

            byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);

            if (contentType == ContentType.URLEncoded) {
                request.ContentType = "application/x-www-form-urlencoded";
            } else if (contentType == ContentType.Plain) {
                request.ContentType = "application/plain";
            } else if (contentType == ContentType.JSON) {
                request.ContentType = "application/json";
            }
            request.ContentType = "application/plain";
            request.ContentLength = byteArray.Length;


            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                return responseFromServer;
            }

            response.Close();
        }
    }
}
