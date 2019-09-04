namespace Jira2AzureDevOps.Logic.Framework
{
    public class Password
    {
        public string Value { get; }

        public Password(string password)
        {
            Value = password;
        }

        public override string ToString()
        {
            return Value.IsNullOrWhiteSpace() ? "" : "*****";
        }
    }
}