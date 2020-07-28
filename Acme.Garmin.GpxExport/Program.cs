﻿// <copyright file="Program.cs" company="Acme">
// Copyright (c) Acme. All rights reserved.
// </copyright>

namespace Acme.Garmin.GpxExport
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    internal class Program
    {
        private const int ActivitiesCount = 50;
        private const int WaitTime = 5000;
        private const int MaxRetries = 5;

        private static void Main(string[] args)
        {
            var baseDirectory = new FileInfo(typeof(Program).Assembly.Location).Directory;

            if (baseDirectory == null)
            {
                return;
            }

            baseDirectory = baseDirectory.CreateSubdirectory("gpx");

            Console.Write("Enter the session id : ");


            var sessionId = Console.ReadLine();
            var exporter = new GpxExporter(sessionId, baseDirectory);

            var start = 0;

            JArray activities;
            do
            {
                Log($"Fetch next {ActivitiesCount} activities");
                activities = exporter.GetActivities(ActivitiesCount, start);
                Log($"{activities.Count} activities fetched !");
                Thread.Sleep(WaitTime);

                start = start + activities.Count;

                foreach (var activity in activities)
                {
                    var activityId = (long)activity["activityId"];
                    var activityDate = DateTime.ParseExact((string)activity["startTimeGMT"] ?? throw new ArgumentNullException(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    for (var i = 0; i < MaxRetries; i++)
                    {
                        try
                        {
                            Log($"Activity {activityId} : Downloading details (attempt {i + 1} on {MaxRetries})");
                            exporter.DownloadGpx(activityId, activityDate);
                            exporter.DownloadJson(activityId, activityDate, activity);
                            Thread.Sleep(WaitTime);
                            break;
                        }
                        catch (WebException ex)
                        {
                            Log($"Activity {activityId} : Error : {ex.Message}");
                            Thread.Sleep(WaitTime);
                        }
                    }


                    Thread.Sleep(WaitTime);
                }
            }
            while (activities.Count > 0);
        }

        private static void Log(string content)
        {
            Console.WriteLine($"{DateTime.Now} - {content}");
        }
    }
}