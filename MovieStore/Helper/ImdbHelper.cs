using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MovieStore.Helper
{
    public class ImdbHelper
    {
        public static async Task<ImdbInfo> GetImdbInfo(string imdbId)
        {
            string response = await ImdbClient.GetStringAsync(String.Format(UrlFormat, imdbId));
            int start = response.IndexOf('(');
            int end = response.LastIndexOf(')');
            string json = response.Substring(start + 1, end - start - 1);

            return JsonConvert.DeserializeObject<ImdbInfo>(json);
        }

        static ImdbHelper()
        {
            ImdbClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            ImdbClient.BaseAddress = new Uri("http://p.media-imdb.com/static-content/documents/v1/title/");
        }

        private const string UrlFormat = "{0}/ratings%3Fjsonp=imdb.rating.run:imdb.api.title.ratings/data.json";

        private static HttpClient ImdbClient;
    }

    #region ImdbInfo

    public class ImdbInfo
    {
        [JsonProperty("@meta")]
        public Meta Meta { get; set; }

        [JsonProperty("resource")]
        public Resource Resource { get; set; }
    }

    public class Meta
    {
        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("serviceTimeMs")]
        public double ServiceTimeMs { get; set; }
    }

    public class Resource
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleType")]
        public string TitleType { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("bottomRank")]
        public int BottomRank { get; set; }

        [JsonProperty("canRate")]
        public bool CanRate { get; set; }

        [JsonProperty("otherRanks")]
        public OtherRank[] OtherRanks { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("ratingCount")]
        public int RatingCount { get; set; }

        [JsonProperty("ratingsHistograms")]
        public Dictionary<string, RatingsHistogram> RatingsHistograms { get; set; }

        [JsonProperty("topRank")]
        public int TopRank { get; set; }
    }

    public partial class OtherRank
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rankType")]
        public string RankType { get; set; }
    }

    public class RatingsHistogram
    {
        [JsonProperty("aggregateRating")]
        public double AggregateRating { get; set; }

        [JsonProperty("demographic")]
        public string Demographic { get; set; }

        [JsonProperty("histogram")]
        public Dictionary<string, int> Histogram { get; set; }

        [JsonProperty("totalRatings")]
        public int TotalRatings { get; set; }
    }

    #endregion
}
