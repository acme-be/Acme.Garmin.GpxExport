// <copyright file="Urls.cs" company="Acme">
// Copyright (c) Acme. All rights reserved.
// </copyright>

namespace Acme.Garmin.GpxExport.Configuration
{
    using System;
    using System.Linq;

    public static class Urls
    {
        private static readonly Random dice = new Random();

        /// <summary>
        /// Get the url to the activities.
        /// </summary>
        /// <param name="limit">The amount of items in a query.</param>
        /// <param name="start">The start element.</param>
        /// <returns></returns>
        public static string Activities(int limit, int start)
        {
            return $"https://connect.garmin.com/modern/proxy/activitylist-service/activities/search/activities?limit={limit}&start={start}&_={dice.Next(0, int.MaxValue)}";
        }

        /// <summary>
        /// Get the url of a gpx.
        /// </summary>
        /// <param name="activityId">The activity associated with the gpx.</param>
        /// <returns></returns>
        public static string Gpx(long activityId)
        {
            return $"https://connect.garmin.com/modern/proxy/download-service/export/gpx/activity/{activityId}";
        }
    }
}