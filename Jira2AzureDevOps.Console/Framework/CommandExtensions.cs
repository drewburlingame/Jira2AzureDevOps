using System.Collections.Generic;
using System.Linq;
using CommandDotNet;
using Jira2AzureDevOps.Logic.Framework;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class CommandExtensions
    {
        public static string GetPath(this Command command, string separator = " ") =>
            command.GetParentCommands(true)
                .Reverse().Skip(1).Select(c => c.Name)
                .ToCsv(separator);

        public static IEnumerable<Command> GetAllDescendentsCommands(this Command command, bool includeCurrent = false, bool orderByPath = false)
        {
            if (includeCurrent)
            {
                yield return command;
            }

            var commands = orderByPath 
                ? (IEnumerable<Command>)command.Subcommands.OrderBy(c => c.Name) 
                : command.Subcommands;
            foreach (var child in commands)
            {
                yield return child;
                foreach (var grandChild in child.GetAllDescendentsCommands())
                {
                    yield return grandChild;
                }
            }
        }
    }
}