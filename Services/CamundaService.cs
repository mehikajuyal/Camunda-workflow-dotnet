using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace UserRegistrationApi.Services
{
    public class CamundaService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public CamundaService(IConfiguration config, HttpClient httpClient)
        {
            _baseUrl = config["Camunda:BaseUrl"] ?? throw new ArgumentNullException("Camunda:BaseUrl");
            _httpClient = httpClient;
        }

        // Start process with registrationId and name
        public async Task StartProcessAsync(int registrationId, string name)
        {
            var url = $"{_baseUrl}/process-definition/key/registration-approval/start";

            var payload = new
            {
                variables = new
                {
                    registrationId = new { value = registrationId, type = "Integer" },
                    name = new { value = name, type = "String" }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        // Get all tasks
        public async Task<List<dynamic>> GetTasksAsync()
        {
            var url = $"{_baseUrl}/task?processDefinitionKey=registration-approval";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
        }

        // NEW: Get variables for a specific task
        public async Task<Dictionary<string, dynamic>> GetTaskVariablesAsync(string taskId)
        {
            var url = $"{_baseUrl}/task/{taskId}/variables";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json) ?? new();
        }

        // Complete task with approval flag
        public async Task CompleteTaskAsync(string taskId, bool approved)
        {
            var url = $"{_baseUrl}/task/{taskId}/complete";

            var payload = new
            {
                variables = new
                {
                    approved = new { value = approved, type = "Boolean" }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
