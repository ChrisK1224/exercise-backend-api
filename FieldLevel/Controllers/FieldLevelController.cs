using FieldLevel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static FieldLevel.Misc.ClientVariables;

namespace FieldLevel.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FieldLevelController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly string TypicodeCacheKey = "typicode";
        public FieldLevelController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<FieldLevelController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
        }
        [HttpGet]
        [Route("LatestPosts")]
        public async Task<ActionResult<TypicodePost[]>> LatestPosts()
        {
            try
            {
                TypicodePost[] cachedPosts;
                // If found in cache, return cached data directly
                if (_memoryCache.TryGetValue(TypicodeCacheKey, out cachedPosts))
                {
                    return Ok(cachedPosts);
                }
                var client = _httpClientFactory.CreateClient(Typicode.Client);
                HttpResponseMessage result = await client.GetAsync(Typicode.Calls.Posts);
                if (result.IsSuccessStatusCode)
                {
                    var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                    var jsonResult = result.Content.ReadAsStringAsync().Result;
                    //Group the results by userId and pull only the latest Id per user
                    cachedPosts = TypicodePost.GroupAndMax(jsonResult);
                    //Set latest posts in the cache to use for any requests in the next minute
                    _memoryCache.Set(TypicodeCacheKey, cachedPosts, cacheOptions);
                    return Ok(cachedPosts);
                }
                else
                {
                    _logger.LogError("Error in typeicode call from FieldLevelController.LatestPosts");
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }
    }
}
