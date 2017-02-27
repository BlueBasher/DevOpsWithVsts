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

private async static Task ProcessBuild(string personalAccessToken, int buildId, string tenantUrl, string column, bool columnDone, TraceWriter log)
{
    log.Info("ProcessBuild " + buildId + " " + tenantUrl + " " + column + " " + columnDone);
    var tenantUri = new Uri(tenantUrl);
    var uri = new Uri(tenantUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped));
    var credentials = new VssBasicCredential("", personalAccessToken);
    using (var workItemTrackingHttpClient = new WorkItemTrackingHttpClient(uri, credentials))
    {
        using (var buildClient = new Microsoft.TeamFoundation.Build.WebApi.BuildHttpClient(uri, credentials))
        {
            var build = await buildClient.GetBuildAsync(buildId);
            if (build.Result == Microsoft.TeamFoundation.Build.WebApi.BuildResult.Succeeded)
            {
                var workItemsRefs = await buildClient.GetBuildWorkItemsRefsAsync(build.Project.Id, build.Id);
                var workItems = await workItemTrackingHttpClient.GetWorkItemsAsync(workItemsRefs.Select(r => int.Parse(r.Id)));
                await UpdateWorkItems(workItemTrackingHttpClient, workItems, column, columnDone, log);
            }
            else
            {
                log.Info("Build " + buildId + " not succeeded");
            }
        }
    }

    log.Info($"Build Completed: " + buildId);
}

private async static Task ProcessRelease(string personalAccessToken, string releaseUrl, string tenantUrl, string column, bool columnDone, TraceWriter log)
{
    log.Info("Start ProcessRelease " + releaseUrl + " " + tenantUrl + " " + column + " " + columnDone);
    using (var httpClient = new HttpClient())
    {
        httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("" + ":" + personalAccessToken)));
        var response = await httpClient.GetAsync(releaseUrl);
        var content = await response.Content.ReadAsStringAsync();
        dynamic releaseData = JsonConvert.DeserializeObject(content);
        var buildId = int.Parse(releaseData.artifacts[0].definitionReference.version.id.ToString());
        await ProcessBuild(personalAccessToken, buildId, tenantUrl, column, columnDone, log);
    }
    log.Info($"Release completed: " + releaseUrl);
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
