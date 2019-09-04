
# Purpose

* Migrate issues from Jira into Azure DevOps as work items
* Cache Jira data to a local directory for archiving and replay
* Provide idempotent replay of both import and export
* Tools to simplify mapping status & issue types
* Tools for quick troubleshooting and resolution of errors (see [development](development.md))

## Prerequisites

* Jira account w/ API token
* Azure DevOps account w/ API token
* Existing Azure DevOps project configured with target work item types and states.

## Features

see the list of commands [here](commands.md)

#### Export from Jira

Exported data is cached to the working directory _(see more below)_ which can be archived. 
If in a month it's discovered other fields need to be imported or additional issues need to be imported, the code or mapping files can be modified and the process can be run again.  
You don't have to "get it right" the first time. This iterative import makes it easier to get data in the system quickly and update later if/as needed.

#### Import to Azure DevOps

Import Work Items. An export does not have to be completed first. When an Import queries directly from Jira, an export occurs as a by-product of the caching.

TODO: 
* describe mapping files
* walkthrough.md to walkthrough a migration

## Configuration options

### Working directory

Working directory can be set with option `-W` or `--workspace` and defaults to the directory the commands are run in..

### Setting credentials

Credentials are provided as options for commands that use them (`--jira-username`, `--jira-token`, `--jira-url`, `--ado-token` ,`--ado-url`).

Defaults for all options can be provided via an `Options.config` file.  See `Options.template.config` for the format.  You can specify credentials options here to avoid showing them elsewhere.  Tokens will be output as "*****" by help and logging.

> Note: Options.config will only work for command options and not for command operands (aka "arguments" in help documentation).  See [Using response files] as an alternative for argument configurations.

### Using response files

Response files allow grouping arguments in a file and using them by passing the file as an argument.  For example, `jira export issues-by-id` takes a list of issue ids.  If you created a file `issues_ids.rsp` containing a list of issue ids, you could execute the command using that file like this 

`Jira2AzureDevOps.exe jira export issues-by-id @issueids.rsp`

The console app recognizes the `@` and then expands the rsp file, putting the args in where the rsp file was.  Any file path can be used after the `@`.  i.e.  @c:\my-rsp-files\test-issues.rsp

To see how this works, you can use the [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet) Parse directive to see how the rsp file is converted.  Simply include `[parse]` as the first argument to the exe (before any commands)

`Jira2AzureDevOps.exe [parse] jira export issues-by-id @issueids.rsp`
	
## List of WorkItem fields

https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/fields/list?view=azure-devops-rest-5.1

summarized at [workItemFieldList.json](workItemFieldList.json) and [workItemFieldList.csv](workItemFieldList.csv)

## Working Directory

The working directory will generate the following folders:

* jira-cache
  * attachments
  * issues
  * meta
* logs

archive jira-cache when completed.

## Caveats & remaining work

* Almost no test automation exists. There was little time to complete this while I had access to both Jira and AzureDevops for the migration project.
* Comments and Changelog are fetched with the Issue instead of separately. This can lead to cases where either are truncated because Jira returns only a single page for each.  The count in the responses should be checked and remaining pages downloaded if you need to import them.  The app will contain logs starting with `Pages are missing` if this occurs.
* Comments are appended to the end of the description of each issues.
* Changelogs (History) are not imported
* Links to other issues are not imported and linked items are not imported solely by virtue of being a linked item.
* Azure Devops does not know how to interpret Markdown so Jira issues using markdown are not well formatted
* Export and import can be specified Jira issues or projects. If projects is specified, all issues within that project are queried. That worked fine for my project.  If you need more specificity, you'll need to create additional commands and method in the IJiraApi.  A good candidate would be an operation that takes a JQL query to filter items.

I will accept pull requests if you'd like to contribute back to the system.




