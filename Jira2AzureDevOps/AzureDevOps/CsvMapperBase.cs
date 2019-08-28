using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jira2AzureDevOps.Framework;
using NLog;

namespace Jira2AzureDevOps.AzureDevOps
{
    public abstract class CsvMapperBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly FileInfo CsvMapFile;
        protected readonly string[][] Rows;
        protected readonly List<Func<string[], string>> KeyBuildersFromRow = new List<Func<string[], string>>();
        protected readonly List<Func<IssueMigration, string>> KeyBuildersFromMigration = new List<Func<IssueMigration, string>>();
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

        protected CsvMapperBase(FileInfo csvMapFile)
        {
            CsvMapFile = csvMapFile;
            Rows = CsvMapFile.ReadAllLines().Select(l => StringExtensions.Split(l, ",")).ToArray();
            if (!Rows.Any())
            {
                throw new Exception($"mapping file does not have records. {csvMapFile}");
            }
        }
        public string GetMappedValueOrThrow(IssueMigration migration)
        {
            var key = KeyBuildersFromMigration.Select(b => b(migration)).ToCsv(); 
            if(!_map.TryGetValue(key, out var adoStatus))
            {
                throw new Exception($"mapping not found for {key} in {CsvMapFile}");
            }
            return adoStatus;
        }

        public bool TryGetMappedValue(IssueMigration migration, out string value)
        {
            var key = KeyBuildersFromMigration.Select(b => b(migration)).ToCsv();
            return _map.TryGetValue(key, out value);
        }

        protected abstract void ParseHeaders(out Func<string[], string> getValueCallback);

        public void LoadMap()
        {
            var mapperName = this.GetType().Name;
            Logger.Debug($"loading {mapperName} from {CsvMapFile}");

            ParseHeaders(out var getValueCallback);

            Logger.Debug($"{mapperName} found {KeyBuildersFromRow.Count+1} columns");

            // skip header row
            for (int i = 1; i < Rows.Length; i++)
            {
                var row = Rows[i];

                if (row.Length < KeyBuildersFromMigration.Count + 1)
                {
                    throw new Exception($"row {i} is missing values.  has the mapping been completed?  values={row.ToCsv()}");
                }

                Logger.Trace("row={i} value={row}", i, row);
                var key = KeyBuildersFromRow.Select(b => b(row)).ToCsv();
                var value = getValueCallback(row);

                if (_map.ContainsKey(key))
                {
                    throw new Exception($"Duplicate key '{row.ToCsv()}' in {CsvMapFile}");
                }
                _map.Add(key, value);
            }

            Logger.Debug($"{mapperName} loaded and mapped {_map.Count} rows");
        }
    }
}