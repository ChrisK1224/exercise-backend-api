using FieldLevel.Models;
using Microsoft.AspNetCore.Mvc;
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
        public FieldLevelController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        [Route("LatestPosts")]
        public async Task<ActionResult<TypicodePost[]>> GetLatestPosts()
        {
            try
            {
                var client = _httpClientFactory.CreateClient(Typicode.Client);
                HttpResponseMessage result = await client.GetAsync(Typicode.Calls.Posts);
                if (result.IsSuccessStatusCode)
                {
                    var jsonResult = result.Content.ReadAsStringAsync().Result;
                    //Group the results by userId and pull only the latest Id per user
                    var filteredPosts = TypicodePost.GroupAndMax(jsonResult);
                    //var allPosts = JsonSerializer.Deserialize<TypicodePost[]>(jsonResult);
                    return Ok(filteredPosts);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
