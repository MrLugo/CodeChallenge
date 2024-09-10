using HackerNews.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Api.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const string BestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
        private const string StoryDetailsUrl = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

        public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count)
        {
            var storyIds = await GetBestStoryIdsAsync();
            var tasks = storyIds.Take(count).Select(GetStoryDetailsAsync);
            var stories = await Task.WhenAll(tasks);
            return stories.OrderByDescending(s => s.Score);
        }

        private async Task<IEnumerable<int>> GetBestStoryIdsAsync()
        {
            return await _cache.GetOrCreateAsync("BestStoryIds", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _httpClient.GetFromJsonAsync<int[]>(BestStoriesUrl);
            });
        }

        private async Task<Story> GetStoryDetailsAsync(int storyId)
        {
            return await _cache.GetOrCreateAsync($"Story_{storyId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var response = await _httpClient.GetFromJsonAsync<HackerNewsItem>(string.Format(StoryDetailsUrl, storyId));
                return new Story
                {
                    Title = response.Title,
                    Uri = response.Url,
                    PostedBy = response.By,
                    Time = DateTimeOffset.FromUnixTimeSeconds(response.Time).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Score = response.Score,
                    CommentCount = response.Descendants
                };
            });
        }
    }
}
