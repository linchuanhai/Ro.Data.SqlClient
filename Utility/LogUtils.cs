using System;
using System.Text;
using System.IO;
using System.Web;

namespace Ro.Data.SqlClient
{   
    public class LogHelper
    {
        public static string GetCurrentApplicationPhyPath()
        {
            string root = string.Empty;

            if (!string.IsNullOrEmpty(HttpRuntime.AppDomainAppId))
            {
                root = HttpRuntime.AppDomainAppPath.ToLower();
            }
            else
            {
                root = AppDomain.CurrentDomain.BaseDirectory.ToLower();
            }
            root = root.ToLower().Replace("\\bin", string.Empty);
            root = root.ToLower().Replace("\\debug", string.Empty);
            root = root.ToLower().Replace("\\release", string.Empty);
            root = root.EndsWith("\\") ? root : root + "\\";
            return root;
        }

        public static void Write(Exception ex, params string[] message)
        {
            string logDir = GetCurrentApplicationPhyPath() + "\\App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + DateTime.Now.Hour.ToString() + "\\";
            string errorFile = "Error_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                File.AppendAllText(logDir + errorFile,
                     @"Time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine
+"Message:" + ex.Message
+ Environment.NewLine
+"Source :" + ex.Source
+ Environment.NewLine
+"Trace :" + ex.StackTrace
+ Environment.NewLine
+ "---------------------------------------------------"
+ Environment.NewLine);

                if (message.Length > 0)
                    Write(message[0]);
            }
            catch { }
        }


        public static void Write(string message)
        {
            string logDir = GetCurrentApplicationPhyPath() + "\\App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + DateTime.Now.Hour + "\\";
            string errorFile = "Error_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                File.AppendAllText(logDir + errorFile,
                     @"Time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+Environment.NewLine
+"Message:" + message + Environment.NewLine
+ "---------------------------------------------------"
+ Environment.NewLine);


            }
            catch { }
        }

        public static void Write(string format, params object[] args)
        {
            var content = string.Format(format, args);
            Write(content);
        }


        public static void WriteLog(string message)
        {
            string logDir = GetCurrentApplicationPhyPath() + "\\App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + DateTime.Now.Hour + "\\";
            string logFile = "Log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string path = string.Empty;
                string referrer = string.Empty;
                string userAgent = string.Empty;
                string ip = string.Empty;
                string postData = string.Empty;
                if (HttpContext.Current != null)
                {
                    var request = HttpContext.Current.Request;
                    path = request.RawUrl;
                    if (request.UrlReferrer != null)
                    {
                        referrer = request.UrlReferrer.ToString();
                    }

                    userAgent = HttpContext.Current.Request.UserAgent;
                    ip = request.UserHostAddress;

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < System.Web.HttpContext.Current.Request.Form.Count; i++)
                    {
                        if (i > 0)
                            sb.Append("&");
                        sb.Append(request.Form.GetKey(i));
                        sb.Append("=");
                        sb.Append(request.Form[i].ToString());

                    }
                    postData = sb.ToString();
                }

                File.AppendAllText(logDir + logFile,
                     string.Format(@"Time:{0}"+ Environment.NewLine
+"Message:{1}"
+Environment.NewLine
+"Path:{2}"
+Environment.NewLine
+"Referrer:{3}"
+ Environment.NewLine
+"UserAgent:{4}"
+Environment.NewLine
+"IP:{5}"
+ Environment.NewLine
+"PostData:{6}"
+Environment.NewLine
+"---------------------------------------------------"
+ Environment.NewLine,
DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message, path, referrer, userAgent, ip, postData));

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static void WriteLog(string format, params object[] args)
        {
            var content = string.Format(format, args);
            WriteLog(content);
        }
    }
}