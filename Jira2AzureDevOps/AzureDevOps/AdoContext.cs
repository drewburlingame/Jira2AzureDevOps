using NLog;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class AdoContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AdoApiSettings ApiSettings { get; }
        public AdoApi Api { get; }

        public AdoContext(AdoApiSettings apiSettings)
        {
            ApiSettings = apiSettings;
            Api = new AdoApi(apiSettings);
        }

        public bool TryConnect()
        {
            return Api.TryConnectToTfs();
        }
    }
}