using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace UserRegistrationApi.Services
{
    public class CamundaService
    {
        private readonly HttpClient _client;
        private const string CamundaBaseUrl = "http://localhost:8080/engine-rest";

        public CamundaService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri(CamundaBaseUrl);
        }

        public async Task StartProcessAsync(int registrationId, string applicantName)
        {
            var payload = new
            {
                variables = new
                {
                    registrationId = new { value = registrationId, type = "Integer" },
                    applicantName = new { value = applicantName, type = "String" }
                }
            };

            await _client.PostAsJsonAsync("/process-definition/key/UserRegistrationProcess/start", payload);
        }

        public async Task CompleteTaskAsync(string taskId, bool approved)
        {
            var payload = new
            {
                variables = new
                {
                    approved = new { value = approved, type = "Boolean" }
                }
            };

            await _client.PostAsJsonAsync($"/task/{taskId}/complete", payload);
        }

        public async Task<string> GetTasksAsync()
        {
            var response = await _client.GetStringAsync("/task?candidateGroup=admin");
            return response;
        }
    }
}
