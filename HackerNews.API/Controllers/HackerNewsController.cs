using HackerNews.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HackerNewsController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;

        public HackerNewsController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        [HttpGet("best/{count}")]
        public async Task<IActionResult> GetBestStories(int count)
        {
            if (count <= 0 || count > 500)
            {
                return BadRequest("Count must be between 1 and 500.");
            }

            var bestStories = await _hackerNewsService.GetBestStoriesAsync(count);
            return Ok(bestStories);
        }
    }
}

