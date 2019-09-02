using Jira2AzureDevOps.Logic.Framework;

namespace Jira2AzureDevOps.Logic.Azure
{
    public interface IAdoApiSettings
    {
        Password AdoToken { get; set; }
        string AdoUrl { get; set; }
        string AdoProject { get; set; }
        string JiraIdField { get; set; }
    }
}