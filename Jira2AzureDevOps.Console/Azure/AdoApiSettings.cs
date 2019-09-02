using CommandDotNet;
using Jira2AzureDevOps.Console.Framework;
using Jira2AzureDevOps.Logic.Azure;
using Jira2AzureDevOps.Logic.Framework;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Console.Azure
{
    public class AdoApiSettings : ISelfValidatingArgumentModel, IAdoApiSettings
    {
        [Option(Description = "Azure DevOps API Token")]
        public Password AdoToken { get; set; }

        [Option(Description = "Azure DevOps URL")]
        public string AdoUrl { get; set; }

        [Option(Description = "The project to import into")]
        public string AdoProject { get; set; }

        //https://docs.microsoft.com/en-us/azure/devops/boards/work-items/work-item-fields?view=azure-devops
        [Option(Description = "Code of the field used to store original Id. {ProcessName}.{FieldName}")]
        public string JiraIdField { get; set; } = "JiraId";

        public IEnumerable<string> GetValidationErrors()
        {
            if (AdoToken == null || AdoToken.Value.IsNullOrWhiteSpace()) yield return "ado-token is required";
            if (AdoUrl.IsNullOrWhiteSpace()) yield return "ado-url is required";
            if (AdoProject.IsNullOrWhiteSpace()) yield return "ado-project is required";
            if (JiraIdField.IsNullOrWhiteSpace()) yield return "jira-id-field is required";
        }
    }
}