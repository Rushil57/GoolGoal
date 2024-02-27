using GoolGoal.API.Auth;
using GoolGoal.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GoolGoal.API.Controllers
{
    //[ApiController]
    public class APIBaseController : ControllerBase
    {
        //IConfiguration _configuration;
        //public APIBaseController(
        //IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        public DataSet jsonToDataSet(string jsonString)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
                xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);
                DataSet ds = new DataSet();
                ds.ReadXml(new XmlNodeReader(xd));
                return ds;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
        public DataSet GetRequestData(string ReqUrl, string ReqParameter = "")
        {
            DataSet ds = new DataSet();
            try
            {
                string url = ReqUrl;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers["X-RapidAPI-Key"] = conf["RequestHeaderKey"].ToString();
                    client.Headers["X-RapidAPI-Host"] = conf["RequestHeaderHost"].ToString();
                    string strResult = client.DownloadString(url);
                    //{ "api":{ "results":0,"fixtures":[]} }
                    //if (!string.IsNullOrEmpty(strResult))
                    //{
                    //    ds = jsonToDataSet(strResult);
                    //}
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("APIBaseController - GetRequestData", ex.Message, ex.StackTrace);
            }
            return ds;
        }
        //public string getCall()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("http://localhost:55587/");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        //GET Method
        //        HttpResponseMessage response =  client.GetAsync("api/Department/1");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var Result = response.Content;
        //        }
        //    }
        //}
        public string ConvertTimestampToDate(string ts)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(ts)).ToLocalTime();
            return dt.ToString("HH:mm");
        }
    }
}
