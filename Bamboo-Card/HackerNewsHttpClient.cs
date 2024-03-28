using Bamboo_Card.Responses;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Bamboo_Card
{
    public class HackerNewsHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://hacker-news.firebaseio.com/v0/";
        private readonly ILogger<HackerNewsHttpClient> _logger;
        private readonly IMemoryCache _cache;

        public HackerNewsHttpClient(HttpClient httpClient, ILogger<HackerNewsHttpClient> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int[]> GetBestStoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}beststories.json");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<int[]>(json);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching best stories");
                throw;
            }
            
        }

        public async Task<List<HackerBestStoreisReponse>> GetSpecificBestStoryAsync(int[] storyIds)
        {
            var result = new List<HackerBestStoreisReponse>();
            foreach (int storyId in storyIds)
            {
                var storyData = await GetCachedStoryData(storyId);

                if (storyData != null)
                {
                    var totalCommentCount = await GetTotalCommentCount(storyData.kids);
                    var resultStory = CastObject(storyData);
                    resultStory.commentCount = totalCommentCount;
                    result.Add(resultStory);
                }
            }

            result = result.OrderByDescending(o => o.score).ToList();
            return result;
            
        }

        private async Task<HackerNewsStory?> GetCachedStoryData(int storyId)
        {
            var cacheKey = $"StoryData_{storyId}";
            if (!_cache.TryGetValue(cacheKey, out HackerNewsStory storyData))
            {
                storyData = await GetStoryData(storyId);
                if (storyData != null)
                {
                    _cache.Set(cacheKey, storyData, TimeSpan.FromMinutes(60));
                }
            }
            return storyData;
        }

        private async Task<HackerNewsStory?> GetStoryData(int storyId)
        {
            try
            {
                var json = await GetJsonResponse($"{_baseUrl}item/{storyId}.json");
                return JsonSerializer.Deserialize<HackerNewsStory>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching story with ID {storyId}");
                return null;
            }
        }

        private async Task<string> GetJsonResponse(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<int> GetTotalCommentCount(int[]? kidsIds)
        {
            if (kidsIds == null || kidsIds.Length == 0)
                return 0;

            var totalCommentCount = 0;
            foreach (var kidId in kidsIds)
            {
                var kidData = await GetStoryData(kidId);
                if (kidData?.type.ToLower() == "comment")
                    totalCommentCount++;
                totalCommentCount += await GetTotalCommentCount(kidData?.kids);
            }
            return totalCommentCount;
        }

        private HackerBestStoreisReponse CastObject(HackerNewsStory storyObject)
        {
            return new HackerBestStoreisReponse
            {
                title = storyObject.title,
                uri = storyObject.url,
                postedBy = storyObject.by,
                time = UnixTimeStampToDateTime(storyObject.time),
                score = storyObject.score
            };
        }

        public static DateTime UnixTimeStampToDateTime(int unixTimestamp)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(unixTimestamp);
        }

    }
}
