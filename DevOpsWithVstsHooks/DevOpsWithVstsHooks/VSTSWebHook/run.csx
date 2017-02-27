#r "Newtonsoft.Json"

using System;
using System.Configuration;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Start run");
    var personalAccessToken = ConfigurationManager.AppSettings["personalAccessToken"];
    var jsonContent = await req.Content.ReadAsStringAsync();
    log.Info($"Content: " + jsonContent);
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    // Azure Alert
    if (data.status != null && data.context != null)
    {
        var alertStatus = data.status.ToString();
        var alertName = data.context.name.ToString();
        var alertTimeStamp = data.context.timestamp.ToString();
        await ProcessAzureAlert(personalAccessToken, alertStatus, alertName, alertTimeStamp);
        return req.CreateResponse(HttpStatusCode.OK);
    }

    return req.CreateResponse(HttpStatusCode.BadRequest);
}

private static async Task ProcessAzureAlert(string personalAccessToken, string alertStatus, string alertName, string alertTimeStamp)
{
    if (alertStatus == "Activated")
    {
        var uri = new Uri($"https://{ConfigurationManager.AppSettings["vstsDomain"]}.visualstudio.com");
        var projectName = ConfigurationManager.AppSettings["vstsLsiProject"];
        var credentials = new VssBasicCredential("", personalAccessToken);
        using (var workItemTrackingHttpClient = new WorkItemTrackingHttpClient(uri, credentials))
        {
            var lsi = new JsonPatchDocument();
            lsi.AddRange(new List<JsonPatchOperation>
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = $"The alert {alertName} is {alertStatus}"
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/DevOps.IncidentSeverity",
                    Value = "1 - High"
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/DevOps.ServiceCategory",
                    Value = "FrontEnd"
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/DevOps.IncidentDetected",
                    Value = alertTimeStamp
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/DevOps.IncidentStarted",
                    Value = alertTimeStamp
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/DevOps.DetectionMethod",
                    Value = "Automated"
                }
            });

            await workItemTrackingHttpClient.CreateWorkItemAsync(lsi, projectName, "Live Site Incident");
        }
    }
    await Task.FromResult(0);
}
