/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Util;
using Newtonsoft.Json;
using NodaTime;
using static QuantConnect.StringExtensions;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Mentions of the given company's ticker in the WallStreetBets daily discussion thread
    /// </summary>
    public class QuiverWallStreetBets : BaseData
    {
        private static readonly TimeSpan _period = TimeSpan.FromDays(1);
        
        /// <summary>
        /// Data source ID
        /// </summary>
        public static int DataSourceId { get; } = 2009;

        /// <summary>
        /// Date of the daily discussion thread
        /// </summary>
        [JsonProperty(PropertyName = "Date")]
        [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of mentions on the given date
        /// </summary>
        [JsonProperty(PropertyName = "Mentions")]
        public int Mentions { get; set; }

        /// <summary>
        /// This ticker's rank on the given date (as determined by total number of mentions)
        /// </summary>
        [JsonProperty(PropertyName = "Rank")]
        public int Rank { get; set; }

        /// <summary>
        /// Average sentiment of all comments containing the given ticker on this date. Sentiment is calculated using VADER sentiment analysis.
        /// The value can range between -1 and +1. Negative values imply negative sentiment, whereas positive values imply positive sentiment.
        /// </summary>
        [JsonProperty(PropertyName = "Sentiment")]
        public decimal Sentiment { get; set; }

        /// <summary>
        /// The time the data point ends at and becomes available to the algorithm
        /// </summary>
        public override DateTime EndTime => Time + _period;

        /// <summary>
        /// Required for successful Json.NET deserialization
        /// </summary>
        public QuiverWallStreetBets()
        {
        }

        /// <summary>
        /// Creates a new instance of QuiverWallStreetBets from a CSV line
        /// </summary>
        /// <param name="csvLine">CSV line</param>
        public QuiverWallStreetBets(string csvLine)
        {
            // Date[0], Mentions[1], Rank[2], Sentiment[3]
            var csv = csvLine.Split(',');
            Date = Parse.DateTimeExact(csv[0], "yyyyMMdd");
            Mentions = Parse.Int(csv[1]);
            Rank = Parse.Int(csv[2]);
            Sentiment = Parse.Decimal(csv[3]);

            Time = Date;
        }

        /// <summary>
        /// Return the Subscription Data Source gained from the URL
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Subscription Data Source.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            var source = Path.Combine(
                Globals.DataFolder,
                "alternative",
                "quiver",
                "wallstreetbets",
                $"{config.Symbol.Value.ToLowerInvariant()}.csv"
            );
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv);
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Content of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>
        /// Quiver WallStreetBets object
        /// </returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            return new QuiverWallStreetBets(line)
            {
                Symbol = config.Symbol
            };
        }

        /// <summary>
        /// Formats a string with the Quiver WallStreetBets information.
        /// </summary>
        public override string ToString()
        {
            return Invariant($"{Symbol}({Date}) :: ") +
                Invariant($"WallStreetBets Mentions: {Mentions} ") +
                Invariant($"WallStreetBets Rank: {Rank} ") +
                Invariant($"WallStreetBets Sentiment: {Sentiment}");
        }

        /// <summary>
        /// Indicates if there is support for mapping
        /// </summary>
        /// <returns>True indicates mapping should be used</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="DateTimeZone"/> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return TimeZones.Utc;
        }
    }
}
