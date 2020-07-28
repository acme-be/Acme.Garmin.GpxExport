// <copyright file="GpxExporter.cs" company="Acme">
// Copyright (c) Acme. All rights reserved.
// </copyright>

namespace Acme.Garmin.GpxExport
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;

    using Acme.Garmin.GpxExport.Configuration;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that manage export of gpx.
    /// </summary>
    public class GpxExporter
    {
        private readonly DirectoryInfo baseDirectory;
        private readonly string sessionId;

        /// <summary>
        /// Initialize a new instance of the Gpx Exporter.
        /// </summary>
        /// <param name="sessionId">The session id used for requests.</param>
        /// <param name="baseDirectory">The base directory to save GPX.</param>
        public GpxExporter(string sessionId, DirectoryInfo baseDirectory)
        {
            this.sessionId = sessionId;
            this.baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Download the gpx in the base directory, if the file does not exists.
        /// </summary>
        /// <param name="activityId">The id of the activity.</param>
        /// <param name="activityDate">The date for the activity.</param>
        public void DownloadGpx(long activityId, DateTime activityDate)
        {
            var output = new FileInfo(Path.Combine(this.baseDirectory.FullName, $"{activityDate:yyyy-MM-dd-HH-mm}-{activityId}.gpx"));

            if (output.Exists)
            {
                return;
            }

            var url = Urls.Gpx(activityId);
            var data = this.GetData(url);

            File.WriteAllText(output.FullName, data);

            output.CreationTimeUtc = output.LastWriteTimeUtc = activityDate;
        }

        public void DownloadJson(in long activityId, in DateTime activityDate, JToken activity)
        {
            var output = new FileInfo(Path.Combine(this.baseDirectory.FullName, $"{activityDate:yyyy-MM-dd-HH-mm}-{activityId}.json"));

            if (output.Exists)
            {
                return;
            }

            File.WriteAllText(output.FullName, activity.ToString());

            output.CreationTimeUtc = output.LastWriteTimeUtc = activityDate;
        }

        public JArray GetActivities(int limit, int start)
        {
            var url = Urls.Activities(limit, start);
            var activities = this.GetJson(url);
            return activities;
        }

        private void ConfigureRequest(HttpWebRequest request)
        {
            request.CookieContainer = new CookieContainer();

            var sessionCookie = new Cookie("SESSIONID", this.sessionId, "/", "connect.garmin.com");
            sessionCookie.HttpOnly = true;
            sessionCookie.Secure = true;
            request.CookieContainer.Add(sessionCookie);
        }

        private string GetData(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";

            this.ConfigureRequest(request);

            using var response = request.GetResponse();
            using var responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                return null;
            }

            using var responseReader = new StreamReader(responseStream);

            return responseReader.ReadToEnd();
        }

        private JArray GetJson(string url)
        {
            var json = this.GetData(url);

            return JArray.Parse(json);
        }
    }
}