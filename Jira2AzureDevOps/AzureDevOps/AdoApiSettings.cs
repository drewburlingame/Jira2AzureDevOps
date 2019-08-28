using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Framework.CommandDotNet;
using System.Collections.Generic;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class AdoApiSettings : ISelfValidatingArgumentModel
    {
        [Password]
        [Option(Description = "Azure DevOps API Token")]
        public string AdoToken { get; set; }

        [Option(Description = "Azure DevOps URL")]
        public string AdoUrl { get; set; }

        [Option(Description = "The project to import into")]
        public string AdoProject { get; set; }

        //https://docs.microsoft.com/en-us/azure/devops/boards/work-items/work-item-fields?view=azure-devops
        [Option(Description = "Code of the field used to store original Id. {ProcessName}.{FieldName}")]
        public string JiraIdField { get; set; } = "JiraId";

        public IEnumerable<string> GetValidationErrors()
        {
            if (AdoToken.IsNullOrEmpty()) yield return "ado-token is required";
            if (AdoUrl.IsNullOrEmpty()) yield return "ado-url is required";
            if (AdoProject.IsNullOrEmpty()) yield return "ado-project is required";
            if (JiraIdField.IsNullOrEmpty()) yield return "jira-id-field is required";
        }
    }
}