using Bamboo_Card.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Bamboo_Card.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HackerNewsController : ControllerBase
    {
        private readonly ILogger<HackerNewsController> _logger;
        private readonly HackerNewsHttpClient _httpHackerNewsClient;

        public HackerNewsController(ILogger<HackerNewsController> logger, HackerNewsHttpClient httpHackerNewsClient)
        {
            _logger = logger;
            _httpHackerNewsClient = httpHackerNewsClient;
        }

        [HttpGet(Name = "GetHackerBestStories")]
        public async Task<ActionResult<IEnumerable<HackerBestStoreisReponse>>> GetHackerBestStoriesAsync()
        {
            try
            {
                var bestStories = await _httpHackerNewsClient.GetBestStoriesAsync();

                if (bestStories == null)
                {
                    _logger.LogError("Response is empty");
                    return NoContent();
                }

                var specificStories = await _httpHackerNewsClient.GetSpecificBestStoryAsync(bestStories);

                if (specificStories == null || specificStories.Count == 0)
                {
                    return NoContent();
                }

                return Ok(specificStories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Hacker News stories.");
                return StatusCode(500, "An error occurred while processing your request.");
            }

        }
    }
}
