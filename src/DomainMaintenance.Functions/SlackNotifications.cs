using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DomainMaintenance.Functions;

public static class SlackNotifications
{
    [FunctionName("Notifications")]
    public static async Task RunAsync([QueueTrigger("notifications")] string message, ILogger log)
    {
        var endpoint = Environment.GetEnvironmentVariable("SLACK_ENDPOINT");
        if (string.IsNullOrEmpty(endpoint))
        {
            // No endpoint configured, so just log the message
            log.LogWarning("The environment variable 'SLACK_ENDPOINT' is not set");
            return;
        }

        // Convert the message to JSON
        var slackMessage = new SlackMessage
        {
            Text = message
        };

        var json = JsonConvert.SerializeObject(slackMessage);


        // send a slack message  
        using var client = HttpClientFactory.Create();
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(endpoint);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public class SlackMessage
    {
        [JsonProperty("text")] public string Text { get; set; }
    }
}