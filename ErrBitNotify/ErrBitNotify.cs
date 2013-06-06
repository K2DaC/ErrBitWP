using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Xml;
using System.Text;

namespace ErrBitNotify
{
    public class ErrBitNotify
    {
        private const String VERSION = "0.1.0";
        private const String NAME = "Windows Phone ErrBit Notifier";
        private const String URL = "http://errzure.azurewebsites.net/";
        private const String DIR = "ErrBitNotify";

        private static String mApiKey = "";
        private static String mAppVersion = "0";
        private static String mApiEndpoint;
        private static Exception mException;
        private List<ParsedException> mListOfCodeLines;

        public static void Register(String apiKey, String endpoint, Application app)
        {
            mApiEndpoint = "http://" + endpoint + "/notifier_api/v2/notices";
            mApiKey = apiKey;
            mAppVersion = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
            app.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(app_UnhandledException);
            SendAllExceptionsToServer();
        }

        static void app_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            mException = e.ExceptionObject;
            WriteXMLToFile();
            SendAllExceptionsToServer();
        }

        private static void SendAllExceptionsToServer()
        {
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string searchPattern = DIR + "\\*";
                    string[] fileNames = myIsolatedStorage.GetFileNames(searchPattern);
                    if (fileNames.Length > 0)
                    {
                        try
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(mApiEndpoint);
                            httpWebRequest.Method = "POST";
                            httpWebRequest.ContentType = "application/xml; charset=utf-8";
                            httpWebRequest.BeginGetRequestStream(result =>
                            {
                                PostWebRequest(result, httpWebRequest, fileNames[0]);
                            }, null);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            { }
        }

        private static void PostWebRequest(IAsyncResult result,
                              HttpWebRequest request,
                              string filename)
        {
            Stream postStream = request.EndGetRequestStream(result);
            String post;
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream isoFileStream = myIsolatedStorage.OpenFile(DIR + "//" + filename, FileMode.Open);
                using (StreamReader reader = new StreamReader(isoFileStream))
                {
                    post = reader.ReadToEnd();
                }
            }
            byte[] postBytes = Encoding.UTF8.GetBytes(post);
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();

            request.BeginGetResponse(res =>
            {
                GetResponseCallback(res, request, filename);
            }, null);

        }

        private static void GetResponseCallback(IAsyncResult asynchronousResult, HttpWebRequest request,
                              string filename)
        {
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            HttpStatusCode rcode = response.StatusCode;
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                myIsolatedStorage.DeleteFile(DIR + "\\" + filename);
            }
            SendAllExceptionsToServer(); //Keep sending files until all are submitted!
        }

        public static void WriteXMLToFile()
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists(DIR))
                    myIsolatedStorage.CreateDirectory(DIR);

                String time = "" + DateTime.Now.Ticks;
                String filename = mAppVersion + "-" + time + ".xml";
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(DIR + "\\" + filename, FileMode.Create, myIsolatedStorage))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    using (XmlWriter writer = XmlWriter.Create(isoStream, settings))
                    {
                        List<ParsedException> list = SplitException();
                        writer.WriteStartElement("notice", "");
                        writer.WriteAttributeString("version", "2.0");
                        writer.WriteStartElement("api-key", "");
                        writer.WriteString(mApiKey);
                        writer.WriteEndElement();
                        writer.WriteStartElement("notifier", "");
                        writer.WriteStartElement("name", "");
                        writer.WriteString(NAME);
                        writer.WriteEndElement();
                        writer.WriteStartElement("version", "");
                        writer.WriteString(VERSION);
                        writer.WriteEndElement();
                        writer.WriteStartElement("url", "");
                        writer.WriteString(URL);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteStartElement("error", "");
                        writer.WriteStartElement("class");
                        writer.WriteString(mException.GetType().FullName);
                        writer.WriteEndElement();
                        writer.WriteStartElement("message");
                        writer.WriteString("[" + mAppVersion + "] " + mException.Message);
                        writer.WriteEndElement();
                        writer.WriteStartElement("backtrace");
                        for (int i = 0; i < list.Count; i++)
                        {
                            writer.WriteStartElement("line");
                            writer.WriteAttributeString("method", list[i].Method);
                            writer.WriteAttributeString("number", "");
                            writer.WriteAttributeString("file", "");
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteStartElement("request", "");
                        writer.WriteStartElement("url", "");
                        writer.WriteEndElement();
                        writer.WriteStartElement("component", "");
                        writer.WriteEndElement();
                        writer.WriteStartElement("action", "");
                        writer.WriteEndElement();
                        writer.WriteStartElement("cgi-data", "");
                        writer.WriteStartElement("var", "");
                        writer.WriteAttributeString("key", "Manufacturer");
                        writer.WriteString(DeviceStatus.DeviceManufacturer);
                        writer.WriteEndElement();
                        writer.WriteStartElement("var", "");
                        writer.WriteAttributeString("key", "Device");
                        writer.WriteString(DeviceStatus.DeviceName);
                        writer.WriteEndElement();
                        writer.WriteStartElement("var", "");
                        writer.WriteAttributeString("key", "Brand");
                        writer.WriteString(DeviceStatus.DeviceManufacturer);
                        writer.WriteEndElement();
                        writer.WriteStartElement("var", "");
                        writer.WriteAttributeString("key", "OS Version");
                        writer.WriteString(System.Environment.OSVersion.Version.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("var", "");
                        writer.WriteAttributeString("key", "App Version");
                        writer.WriteString(mAppVersion);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteStartElement("server-environment", "");
                        writer.WriteStartElement("environment-name", "");
                        writer.WriteString("production");
                        writer.WriteEndElement();
                        writer.WriteStartElement("app-version", "");
                        writer.WriteString(mAppVersion);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Flush();
                    }
                }
            }
        }


        private static List<ParsedException> SplitException()
        {
            List<ParsedException> list = new List<ParsedException>();
            string[] lines = mException.StackTrace.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                list.Add(new ParsedException(lines[i]));
            }
            return list;
        }

        private class ParsedException
        {
            public ParsedException(String method)
            {
                Method = method.Trim();
            }

            public String Method { get; set; }
            public String Class { get; set; }
            public String Line { get; set; }
        }
    }
}
