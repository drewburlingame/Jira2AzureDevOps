using Jira2AzureDevOps.Logic.Framework.Json;

namespace Jira2AzureDevOps.Logic.Jira.Json
{
    public class IssueIdConverter : TypedJsonConverter<IssueId>
    {
        protected override IssueId Parse(string value) => new IssueId(value);
    }
}