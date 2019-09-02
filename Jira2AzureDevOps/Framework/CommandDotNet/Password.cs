namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    public class Password
    {
        public string Value { get; }

        public Password(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.IsNullOrWhiteSpace() ? "" : "*****";
        }
    }
}