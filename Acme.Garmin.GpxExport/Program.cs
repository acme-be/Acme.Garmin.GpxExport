// <copyright file="Program.cs" company="Acme">
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
        private const int ActivitiesCount = 500;
        private const int MaxRetries = 5;
        private const int RetryWaitTime = 5000;
        private const int WaitTime = 5000;

        private static void Log(string content)
        {
            Console.WriteLine($"{DateTime.Now} - {content}");
        }

        private static void Main()
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
                var nextIndex = start;
                activities = RetryIfError($"Fetch next {ActivitiesCount} activities", () => exporter.GetActivities(ActivitiesCount, nextIndex));

                Thread.Sleep(WaitTime);

                start = start + activities.Count;

                foreach (var activity in activities)
                {
                    var activityId = (long)activity["activityId"];
                    var activityDate = DateTime.ParseExact((string)activity["startTimeGMT"] ?? throw new ArgumentNullException(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    var shouldSleep = RetryIfError($"Activity {activityId} : Downloading details", () =>
                    {
                        var requestExecuted = exporter.DownloadGpx(activityId, activityDate);
                        requestExecuted = requestExecuted | exporter.DownloadJson(activityId, activityDate, activity);
                        return requestExecuted;
                    });

                    if (shouldSleep)
                    {
                        Thread.Sleep(WaitTime);
                    }

                    break;
                }
            } while (activities.Count > 0);
        }

        private static T RetryIfError<T>(string callReference, Func<T> call)
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                try
                {
                    Log($"{callReference} - (attempt {i + 1} on {MaxRetries})");
                    var value = call();
                    Log($"{callReference} - Success");
                    return value;
                }
                catch (WebException ex)
                {
                    Log($"{callReference} - Error : {ex.Message}");
                    Thread.Sleep(RetryWaitTime);
                }
            }

            throw new ApplicationException($"The max number of retries has been reached for {callReference}");
        }
    }
}