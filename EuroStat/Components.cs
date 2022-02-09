using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.ComponentModel;

namespace EuroStat {
    public static class Components {
        public static string GetDisplayName(Type t) {
            return Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute)) != null ? ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).DisplayName : t.Name;
        }
        public static string DisplayName(this System.Type Value) {
            try {
                return GetDisplayName((System.Type)Value);
            } catch { return string.Empty; }
        }

        public delegate void DataSetDownloadProgress(int ProgressPercentage, long BytesReceived, long TotalBytesToReceive);
        public delegate void DataSetDownloaded(DataSet Result, bool Cancelled, Exception Error);
        public delegate void DataSetFinal(DataSet Result);
        public static DataSet GetDataSet(string URI, IProgress<decimal> PR) {
            if (PR != null) PR.Report(0);
            DataSet ds = new DataSet();
            System.Net.ServicePointManager.Expect100Continue = true;
            try {
                System.Net.HttpWebRequest wRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URI);
                wRequest.KeepAlive = true;
                using (System.Net.HttpWebResponse wResponse = (System.Net.HttpWebResponse)wRequest.GetResponse())
                    if (wResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                        Encoding encoding = string.IsNullOrWhiteSpace(wResponse.CharacterSet) ? Encoding.Default : Encoding.GetEncoding(wResponse.CharacterSet);
                        using (Stream rStream = wResponse.GetResponseStream())
                            if (wResponse.ContentLength > 0)
                                using (Stream mStream = new MemoryStream()) {
                                    long respLenght = wResponse.ContentLength; long Lenght = 0; byte[] buffer = new byte[1024];
                                    while (true) {
                                        int bytesRead = rStream.Read(buffer, 0, buffer.Length);
                                        Lenght += bytesRead;
                                        if (bytesRead == 0)
                                            break;
                                        else
                                            mStream.Write(buffer, 0, bytesRead);
                                        if (PR != null) PR.Report((decimal)Lenght / (decimal)respLenght * 100.0m);
                                    }
                                    using (var sReader = new StreamReader(mStream, encoding))
                                    using (StringReader stringReader = new StringReader(ModifXML(sReader.ReadToEnd())))
                                        ds.ReadXml(stringReader);
                                }
                            else
                                using (var sReader = new StreamReader(rStream, encoding))
                                using (StringReader stringReader = new StringReader(ModifXML(sReader.ReadToEnd())))
                                    ds.ReadXml(stringReader);
                    }
            } catch (Exception gds) {
            } finally { GC.Collect(); GC.Collect(); GC.Collect(); }
            if (PR != null) PR.Report(100);
            return ds;
        }
        public static string ModifXML(string XML) {
            return Regex.Replace(XML, @"(</?)(\w+:)", "$1").Replace("<s:Source><Ref", "<s:SourceRef").Replace("</s:Source>", "").Replace("<s:Target><Ref", "<s:TargetRef").Replace("</s:Target>", "").Replace("<Source><Ref", "<SourceRef").Replace("</Source>", "").Replace("<Target><Ref", "<TargetRef").Replace("</Target>", "");
        }
        public static void BeginLoadDataSet(string URI, DataSetDownloadProgress DDP, DataSetDownloaded DSD, DataSetFinal DSF) {
            System.Net.WebClient wClient = new System.Net.WebClient();
            Uri uri = new Uri(URI);
            wClient.DownloadProgressChanged += delegate (object o, System.Net.DownloadProgressChangedEventArgs dpcea) {
                if (DDP != null) DDP.Invoke(dpcea.ProgressPercentage, dpcea.BytesReceived, dpcea.TotalBytesToReceive);
            };
            wClient.DownloadStringCompleted += delegate (object o, System.Net.DownloadStringCompletedEventArgs dscea) {
                if (dscea.Cancelled) {
                    if (DSD != null) DSD.Invoke(null, dscea.Cancelled, dscea.Error);
                    return;
                }
                DataSet ds = new DataSet();
                try {
                    using (System.IO.StringReader stringReader = new System.IO.StringReader(Components.ModifXML(dscea.Result)))
                        ds.ReadXml(stringReader);
                    if (DSF != null) DSF.Invoke(ds);

                } finally { if (DSD != null) DSD.Invoke(ds, dscea.Cancelled, dscea.Error); }
            };
            wClient.DownloadStringAsync(uri);
        }

        public static async Task<DataSet> GetDataSetAsync(string URI, IProgress<decimal> PR) {
            DataSet ds = new DataSet();
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            using (StringReader stringReader = new StringReader(ModifXML(await httpClient.GetStringAsync(URI))))
                ds.ReadXml(stringReader);
            return ds;
        }
    }
}
