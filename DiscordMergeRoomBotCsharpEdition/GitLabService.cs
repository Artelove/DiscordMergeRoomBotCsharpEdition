using Newtonsoft.Json.Linq;
using System.Web;

namespace DiscordMergeRoomBotCsharpEdition
{

    public class GitLabService
    {
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;

        public GitLabService(HttpClient httpClient, string accessToken)
        {
            _httpClient = httpClient;
            _accessToken = accessToken;
        }

        public async Task<User> GetUserAsync(string id)
        {
            var requestUrl = $"https://gitlab.com/api/v4/users/{id}?access_token={_accessToken}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);

            return new User
            {
                Id = json["id"].Value<int>(),
                Name = json["name"].Value<string>(),
                AvatarUrl = json["avatar_url"].Value<string>(),
                WebUrl = json["web_url"].Value<string>(),
            };
        }

        public async Task<string> GetRawFileFromBranchByNameAsync(string projectId, string filePath, string branchName)
        {
            var requestUrl = $"https://gitlab.com/api/v4/projects/{projectId}/repository/files/{HttpUtility.UrlEncode(filePath)}/raw/?ref={HttpUtility.UrlEncode(branchName)}&access_token={_accessToken}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
