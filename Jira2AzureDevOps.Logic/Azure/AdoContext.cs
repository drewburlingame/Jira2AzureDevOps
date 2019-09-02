using System.Threading;

namespace Jira2AzureDevOps.Logic.Azure
{
    public class AdoContext
    {
        public IAdoApiSettings ApiSettings { get; }
        public AdoApi Api { get; }

        public AdoContext(IAdoApiSettings apiSettings, CancellationToken cancellationToken)
        {
            ApiSettings = apiSettings;
            Api = new AdoApi(apiSettings, cancellationToken);
        }

        public bool TryConnect()
        {
            return Api.TryConnectToTfs();
        }
    }
}