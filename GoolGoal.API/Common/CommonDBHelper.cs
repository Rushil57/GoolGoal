using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GoolGoal.API.Common
{
    public class CommonDBHelper
    {

        static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        public static string dbConnection = conf["ConnectionStrings:ConnStr"].ToString();

        public static void ErrorLog(string ControllerName, string Error, string StackTrace)
        {
            try
            {
                SqlConnection con = new SqlConnection(dbConnection);
                var stList = StackTrace.ToString().Split('\\');
                var sterror = "";
                //for (int i = 0; i < stList.Length; i++)
                //{
                //    sterror += stList[i];
                //}
                string query = "insert into [ErrorLogs] (ControllerName,Error,StackTrace,Timest) values('" + ControllerName + "','" + Error.Replace("'", "''") + "','" + stList[0] + "',getdate())";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
