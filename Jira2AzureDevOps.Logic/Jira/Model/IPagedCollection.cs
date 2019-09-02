namespace Jira2AzureDevOps.Logic.Jira.Model
{
    public interface IPagedCollection
    {
        long StartAt { get; set; }
        long MaxResults { get; set; }
        long Total { get; set; }
    }
}