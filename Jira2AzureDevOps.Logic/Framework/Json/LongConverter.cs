namespace Jira2AzureDevOps.Logic.Framework.Json
{
    public class LongConverter : TypedJsonConverter<long>
    {
        protected override long Parse(string value) => long.Parse(value);
    }
}