using Jira2AzureDevOps.Logic.Framework;
using Jira2AzureDevOps.Logic.Jira.Json;
using Newtonsoft.Json;
using System;

namespace Jira2AzureDevOps.Logic.Jira
{
    [JsonConverter(typeof(IssueIdConverter))]
    public class IssueId : IComparable<IssueId>
    {
        public string Project { get; }
        public int Id { get; }

        public IssueId(string key)
        {
            var parts = key.Split("-");
            Project = parts[0];
            Id = int.Parse(parts[1]);
        }

        public override string ToString()
        {
            return $"{Project}-{Id}";
        }

        public int CompareTo(IssueId other)
        {
            return CompareIssueIds(this, other);
        }

        public static bool operator >(IssueId x, IssueId y)
        {
            return CompareIssueIds(x, y) > 0;
        }

        public static bool operator <(IssueId x, IssueId y)
        {
            return CompareIssueIds(x, y) < 0;
        }

        public static bool operator >=(IssueId x, IssueId y)
        {
            return CompareIssueIds(x, y) >= 0;
        }

        public static bool operator <=(IssueId x, IssueId y)
        {
            return CompareIssueIds(x, y) <= 0;
        }

        public static bool operator ==(IssueId x, IssueId y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(IssueId x, IssueId y)
        {
            return !Equals(x, y);
        }


        protected bool Equals(IssueId other)
        {
            return string.Equals(Project, other.Project) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((IssueId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Project != null ? Project.GetHashCode() : 0) * 397) ^ Id;
            }
        }

        private static int CompareIssueIds(IssueId x, IssueId y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(null, y))
            {
                return 1;
            }

            if (ReferenceEquals(null, x))
            {
                return -1;
            }

            var projectComparison = string.Compare(x.Project, y.Project, StringComparison.Ordinal);
            return projectComparison != 0
                ? projectComparison
                : x.Id.CompareTo(y.Id);
        }
    }
}