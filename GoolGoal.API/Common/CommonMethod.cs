using GoolGoal.API.Auth;
using GoolGoal.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace GoolGoal.API.Common
{
    public class CommonMethod
    {
        static IConfiguration _configuration = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        private readonly GoolGoalAppDbContext _db;
        public CommonMethod(GoolGoalAppDbContext dbContext)
        {
            _db = dbContext;
        }
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
        //public DataSet GetRequestData(string ReqUrl, string ReqParameter = "")
        //{
        //    DataSet ds = new DataSet();
        //    try
        //    {
        //        string url = ReqUrl;
        //        using (var client = new WebClient())
        //        {
        //            client.Headers[HttpRequestHeader.ContentType] = "application/json";
        //            client.Headers["X-RapidAPI-Key"] = _configuration["RequestHeaderKey"].ToString();
        //            client.Headers["X-RapidAPI-Host"] = _configuration["RequestHeaderHost"].ToString();
        //            string strResult = client.DownloadString(url);
        //            //{ "api":{ "results":0,"fixtures":[]} }
        //            if (!string.IsNullOrEmpty(strResult))
        //            {
        //                ds = jsonToDataSet(strResult);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    return ds;
        //}
        public DataSet GetRequestData(string ReqUrl, int linkversion, bool isInsert = false, string FixtureId = "", string ReqParameter = "")
        {
            DataSet ds = new DataSet();
            try
            {
                string URL = linkversion == 3 ? _configuration["FootballWebLinkVersion3"].ToString() : _configuration["FootballWebLinkVersion2"].ToString();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _configuration["RequestHeaderKey"].ToString());
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _configuration["RequestHeaderHost"].ToString());
                    var response = client.GetAsync(URL + ReqUrl).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        string strResult = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(strResult))
                        {
                            ds = jsonToDataSet(strResult);
                            if (isInsert)
                            {
                                if (ds != null && ds.Tables.Count > 0)
                                {
                                    if (ds.Tables["api"] != null && ds.Tables["api"].Rows.Count > 0)
                                    {
                                        if (Convert.ToInt32(ds.Tables["api"].Rows[0]["results"].ToString()) > 0)
                                        {
                                            InsertAPIResponse(URL + ReqUrl, strResult, FixtureId);
                                        }
                                        else
                                        {
                                            ds = GetAPIResponseData(URL + ReqUrl, FixtureId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("GetRequestData", ex.Message, ex.StackTrace);
            }
            return ds;
        }
        public DateTime ConvertTimestampToDate(string ts)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(ts)).ToUniversalTime();
            return dt;
        }
        public string ImageCompressed(Byte[] Imagebyte)
        {
            var profileData = "";
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            long bytess = Math.Abs(Imagebyte.Length);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytess, 1024)));
            double imageSize = Math.Round(bytess / Math.Pow(1024, place), 1);
            string imageSuffix = suf[place];
            if (imageSize > 2 && place > 1)
            {
                MemoryStream ms = new MemoryStream(Imagebyte, 0, Imagebyte.Length);
                ms.Write(Imagebyte, 0, Imagebyte.Length);
                Image image = Image.FromStream(ms, true);
                int newWidth = 410;
                int newHeight = 247;
                Image newImage = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
                MemoryStream myResult = new MemoryStream();
                newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Jpeg);
                Byte[] imagebyte = myResult.ToArray();
                profileData = Convert.ToBase64String(imagebyte);
            }
            else
            {
                profileData = Convert.ToBase64String(Imagebyte);
            }

            return profileData;
        }
        public string GetRequestDataAsString(string ReqUrl, int linkversion, string ReqParameter = "")
        {
            DataSet ds = new DataSet();
            string strResult = "";
            try
            {
                string URL = linkversion == 3 ? _configuration["FootballWebLinkVersion3"].ToString() : _configuration["FootballWebLinkVersion2"].ToString();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _configuration["RequestHeaderKey"].ToString());
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _configuration["RequestHeaderHost"].ToString());
                    var response = client.GetAsync(URL + ReqUrl).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        strResult = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("GetRequestData", ex.Message, ex.StackTrace);
            }
            return strResult;
        }

        //public async Task<string> SendMail(string userEmail, string Newpassword)
        //{
        //    string HostName = _configuration["Smtp:Server"].ToString();
        //    int Port = Convert.ToInt32(_configuration["Smtp:Port"]);
        //    string FromAddress = _configuration["Smtp:FromAddress"].ToString();
        //    string FromPassword = _configuration["Smtp:Password"].ToString();

        //    try
        //    {
        //        MailMessage mail = new MailMessage();
        //        mail.To.Add(userEmail);
        //        mail.From = new MailAddress("thakkarpooja1101@gmail.com");
        //        mail.Subject = "Updated Password";
        //        mail.Body = "Your Update password is : " + Newpassword;
        //        mail.IsBodyHtml = true;
        //        SmtpClient smtp = new SmtpClient("relay-hosting.secureserver.net", 25);
        //        smtp.EnableSsl = true;
        //        smtp.UseDefaultCredentials = false;
        //        smtp.Credentials = new NetworkCredential("thakkarpooja1101@gmail.com", "gtbcgokqlegmlumv");
        //        try
        //        {
        //            smtp.Send(mail);
        //        }
        //        catch (Exception ex)
        //        {
        //            CommonDBHelper.ErrorLog("CommonMethod - Send()", ex.Message, ex.StackTrace);
        //            throw;
        //        }

        //        return "Mail Sent Successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        CommonDBHelper.ErrorLog("CommonMethod - SendMail", ex.Message, ex.StackTrace);
        //        return ex.StackTrace;
        //    }

        //}
        //public async Task<string> SendMail(string ReceiverEmailId, string Newpassword, string FirstName, Byte[] bytes)
        //{
        //    try
        //    {
        //        var Logobasestring = Convert.ToBase64String(bytes);
        //        var Url = _configuration["AppSettings:AzureFunctionURL"].ToString();
        //        var SenderPassword = _configuration["Smtp:Password"].ToString();
        //        var SendEmailId = _configuration["Smtp:FromAddress"].ToString();
        //        var Host = _configuration["Smtp:Server"].ToString();
        //        var Port = _configuration["Smtp:Port"].ToString();

        //        var MailSubject = "Password Reset";
        //        var MailBody = "<p style='padding-left:2%;'>Hello " + FirstName + "," +
        //                        "</p><p style='padding-left: 5%;'>Your password has been updated successfully." +
        //                        "<br>Your updated password is: <b>" + Newpassword + ".</b></p>";
        //        //"<img src=\"data:image/png;base64," + Logobasestring +"\" alt='GoolGoalLogo'>";

        //        var sumUp = "<p style='padding-top: 3%;padding-left: 3%;border-left: 1px solid #d5d5ec;'>GoolGoal</p>";

        //        dynamic content = new
        //        {
        //            ReceiverEmailId = ReceiverEmailId,
        //            NewPassword = Newpassword,
        //            SenderPassword = SenderPassword,
        //            SendEmailId = SendEmailId,
        //            Host = Host,
        //            Port = Port,
        //            MailSubject = MailSubject,
        //            MailBody = MailBody,
        //            FinishLine = sumUp,
        //            LogoBase64String = Logobasestring
        //        };

        //        //CancellationToken cancellationToken;
        //        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        //        CancellationToken token = cancelTokenSource.Token;


        //        using (var client = new HttpClient())
        //        using (var request = new HttpRequestMessage(HttpMethod.Post, Url))
        //        using (var httpContent = CreateHttpContent(content))
        //        {
        //            request.Content = httpContent;
        //            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                return "Mail Sent Successfully";
        //            }
        //            return "Mail Not Sent";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string Message = ex.StackTrace;
        //        return Message;
        //    }


        //}
        public string GenerateRandomPassword()
        {
            PasswordOptions opts = null;
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }
            //string newPassword = string.Join("", chars.ToArray());
            return string.Join("", chars.ToArray());
        }
        public void InsertAPIResponse(string APIName, string APIResponse, string FixtureId)
        {
            if (!string.IsNullOrEmpty(FixtureId))
            {
                StoreAPIResponse StoreAPIResponse = _db.StoreAPIResponse.ToList().Where(x => x.FixtureId == FixtureId && x.APIName == APIName).ToList().FirstOrDefault();
                if (StoreAPIResponse == null)
                {
                    //    StoreAPIResponse.APIResponse = APIResponse;
                    //    _db.StoreAPIResponse.Update(StoreAPIResponse);
                    //    _db.SaveChanges();
                    //}
                    //else
                    //{
                    _db.StoreAPIResponse.Add(new StoreAPIResponse
                    {
                        APIName = APIName,
                        FixtureId = FixtureId,
                        APIResponse = APIResponse,
                        CreatedDateTime = DateTime.Now
                    });
                    _db.SaveChanges();
                }
            }
            else
            {
                StoreAPIResponse StoreAPIResponse = _db.StoreAPIResponse.ToList().Where(x => x.APIName == APIName).ToList().FirstOrDefault();
                if (StoreAPIResponse == null)
                {
                    _db.StoreAPIResponse.Add(new StoreAPIResponse
                    {
                        APIName = APIName,
                        FixtureId = FixtureId,
                        APIResponse = APIResponse,
                        CreatedDateTime = DateTime.Now
                    });
                    _db.SaveChanges();
                }
            }
        }
        public DataSet GetAPIResponseData(string APIName, string FixtureId)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(FixtureId))
            {
                StoreAPIResponse StoreAPIResponse = _db.StoreAPIResponse.ToList().Where(x => x.FixtureId == FixtureId && x.APIName == APIName).ToList().FirstOrDefault();
                if (StoreAPIResponse != null)
                {
                    ds = jsonToDataSet(StoreAPIResponse.APIResponse);
                }
            }
            else
            {
                StoreAPIResponse StoreAPIResponse = _db.StoreAPIResponse.ToList().Where(x => x.APIName == APIName).ToList().FirstOrDefault();
                if (StoreAPIResponse != null)
                {
                    ds = jsonToDataSet(StoreAPIResponse.APIResponse);
                }
            }
            return ds;
        }

        public async Task<string> SendMail(string ReceiverEmailId, string Newpassword, string FirstName)
        {
            var FilePath = _configuration["BlobStorageSettings:DocumentPath"] + "GoolGoalLogo.png" + _configuration["BlobStorageSettings:DocumentPathToken"];
            var SenderPassword = _configuration["Smtp:Password"].ToString();
            var SendEmailId = _configuration["Smtp:FromAddress"].ToString();
            var Host = _configuration["Smtp:Server"].ToString();
            var Port = Convert.ToInt32(_configuration["Smtp:Port"]);

            var MailSubject = "Password Reset";
            var MailBody = "<p style='padding-left:2%;'>Hello " + FirstName + "," +
                            "</p><p style='padding-left: 5%;'>Your password has been updated successfully." +
                            "<br>Your updated password is: <b>" + Newpassword + ".</b>" +
                            "<br><b>Note:- </b> We recommend you to change the password when you login first time with this new password.</p>";
            var sumUp = "<p style='padding-top: 3%;padding-left: 3%;border-left: 1px solid #d5d5ec;'>GoolGoal</p>";


            AlternateView alternateView = AlternateView.CreateAlternateViewFromString
            (
             MailBody + "<br> <div style=\"display: flex;\"><p style=\"padding-left: 2%;\">" +
             "<img src=" + FilePath + " style=\"height: 100px;width: 120px;\"></p>" +
             sumUp +
             "</div>", null, "text/html"
            );

            //Byte[] bytes = System.IO.File.ReadAllBytes(FilePath);
            //System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bytes);
            //var imageToInline = new LinkedResource(streamBitmap, MediaTypeNames.Image.Jpeg);
            //imageToInline.ContentId = "MyImage";
            //imageToInline.ContentType.Name = "GoolGoal";
            //alternateView.LinkedResources.Add(imageToInline);

            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(ReceiverEmailId);
                mail.From = new MailAddress(SendEmailId);
                mail.Subject = MailSubject;
                mail.AlternateViews.Add(alternateView);
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(Host, Port);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(SendEmailId, SenderPassword);
                try
                {
                    smtp.Send(mail);
                    return "Mail Sent Successfully";
                }
                catch (Exception ex)
                {
                    string SendMailError = ex.Message + ex.StackTrace;
                    CommonDBHelper.ErrorLog("AuthenticateController - SendMail", ex.Message, ex.StackTrace);
                    return SendMailError;
                }

            }
            catch (Exception ex)
            {
                string Error = ex.Message;
                CommonDBHelper.ErrorLog("AuthenticateController - SendMail", ex.Message, ex.StackTrace);
                return Error;
            }

        }

        public async Task<string> UploadBlobFile(IFormFile files)
        {
            string FileName = "";
            try
            {
                var BlobContainerName = "userimages";
                FileName = Path.GetRandomFileName() + Path.GetExtension(files.FileName).ToLowerInvariant();
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_configuration["BlobStorageSettings:BlobStorageConnStr"].ToString());
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobContainerName);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(FileName);
                await using (var data = files.OpenReadStream())
                {
                    await blockBlob.UploadFromStreamAsync(data);
                }
                return FileName;
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("CommonMethod - UploadBlobFile", ex.Message, ex.StackTrace);
                throw ex;
            }
        }
        public async Task DeleteBlobFile(string fileName)
        {
            try
            {
                var BlobContainerName = "userimages";
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_configuration["BlobStorageSettings:BlobStorageConnStr"].ToString());
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(BlobContainerName);
                var blob = cloudBlobContainer.GetBlobReference(fileName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("CommonMethod - DeleteBlobFile", ex.Message, ex.StackTrace);
                throw ex;
            }
        }
    }
}
