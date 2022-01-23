using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FieldLevel.Models
{
    public class TypicodePost
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public static TypicodePost[] GroupAndMax(string postsJson)
        {
            var allPosts = JsonSerializer.Deserialize<TypicodePost[]>(postsJson);
            var userGroups = allPosts.GroupBy(p => (p.userId));
            return userGroups.SelectMany(grp => grp.Where(p => p.id == grp.Max(p => p.id))).ToArray();
        }
    }
}
