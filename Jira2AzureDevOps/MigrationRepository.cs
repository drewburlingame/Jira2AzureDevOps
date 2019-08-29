using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jira2AzureDevOps.Jira;
using Newtonsoft.Json;

namespace Jira2AzureDevOps
{
    public class MigrationRepository
    {
        private readonly LocalDirs _localDirs;

        public MigrationRepository(LocalDirs localDirs)
        {
            _localDirs = localDirs;
        }

        public IEnumerable<IssueMigration> GetAll()
        {
            return _localDirs.GetAllIssueMigrationStatusFiles().Select(LoadFromFile);
        }

        public IssueMigration GetOrCreate(IssueId issueId)
        {
            return Get(issueId) ?? new IssueMigration{IssueId = issueId};
        }

        public void Reset(IssueMigration migration)
        {
            migration.TempWorkItemId = default;
            migration.WorkItemId = default;
            migration.IssueImported = false;
            migration.Attachments.ForEach(a => a.Imported = false);
            Save(migration);
        }

        public IssueMigration Get(IssueId issueId)
        {
            var file = _localDirs.GetIssueMigrationStatusFile(issueId);
            if (!file.Exists)
            {
                return null;
            }
            return LoadFromFile(file);
        }

        public void Save(IssueMigration migration)
        {
            var file = _localDirs.GetIssueMigrationStatusFile(migration.IssueId);
            var json = JsonConvert.SerializeObject(migration, Formatting.Indented);

            // Intentionally not passing _cancellationToken.
            // I don't want to risk leaving file in a bad state.
            // small files should write quickly anyway.
            File.WriteAllText(file.FullName, json);
        }

        private static IssueMigration LoadFromFile(FileInfo file)
        {
            var json = File.ReadAllText(file.FullName);
            return JsonConvert.DeserializeObject<IssueMigration>(json);
        }
    }
}