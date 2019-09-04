
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

see the list of commands [here](commands.md).  Defaults for command options can be overridden with the `Options.config` file as described in [Setting credentials](#Setting-credentials) below.

#### Export from Jira

Commands located under command: `export`

Export issues by project or id. 
Exported data is cached to the working directory _(see more below)_ which can be archived. 
If in a month it's discovered other fields need to be imported or additional issues need to be imported, the code or mapping files can be modified and the process can be run again.  
You don't have to "get it right" the first time. This iterative import makes it easier to get data in the system quickly and update later if/as needed.

#### Import to Azure DevOps

Commands located under command: `import`

Import issues by project or by id. 
An export does not have to be completed first. When an Import queries directly from Jira, an export occurs as a by-product of the caching.

##### Mapping files

Issue Type and Status mapping files are CSV files where the left columns form the key and the right column the target value.

###### Issue Type mappings
the key columns can include the project and issue type.  
If only one column exists, then only that column is used to generate the key.  
For example, if only the issue type column was specified, then issues would be mapped based on type alone, regardless of project.  
This allows mappings to be specified across projects.

use `report stub-issue-types-mapping [options] > issue-type-mapping.csv` to generate the csv file and then add the mapped value in the `Work Item Type` column.

###### Status mappings
the key columns can include the project, issue type, status category and status.
If only the status category column exists, then all issues in a status category will be mapped the given status.
If only the issue type and status category column exists, then all issues of a type and status category will be mapped the given status.

use `report stub-status-mapping [options] > status-mapping.csv` to generate the csv file and then add the mapped value in the `Work Item Status` column.

## Configuration options

#### Working directory

Working directory can be set with option `-W` or `--workspace` and defaults to the directory the commands are run in..

#### Setting credentials

Credentials are provided as options for commands that use them (`--jira-username`, `--jira-token`, `--jira-url`, `--ado-token` ,`--ado-url`).

Defaults for all options can be provided via an `Options.config` file.  See `Options.template.config` for the format.  You can specify credentials options here to avoid showing them elsewhere.  Tokens will be output as "*****" by help and logging.

> Note: Options.config will only work for command options and not for command operands (aka "arguments" in help documentation).  See [Using response files] as an alternative for argument configurations.

#### Using response files

Response files allow grouping arguments in a file and using them by passing the file as an argument.  For example, `export issues-by-id` takes a list of issue ids.  If you created a file `issues_ids.rsp` containing a list of issue ids, you could execute the command using that file like this 

`Jira2AzureDevOps.exe export issues-by-id @issueids.rsp`

The console app recognizes the `@` and then expands the rsp file, putting the args in where the rsp file was.  Any file path can be used after the `@`.  i.e.  @c:\my-rsp-files\test-issues.rsp

To see how this works, you can use the [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet) Parse directive to see how the rsp file is converted.  Simply include `[parse]` as the first argument to the exe (before any commands)

`Jira2AzureDevOps.exe [parse] export issues-by-id @issueids.rsp`
	
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
* Azure DevOps does not know how to interpret Markdown so Jira issues using markdown are not well formatted
* Export and import can be specified Jira issues or projects. If projects is specified, all issues within that project are queried. That worked fine for my project.  If you need more specificity, you'll need to create additional commands and method in the IJiraApi.  A good candidate would be an operation that takes a JQL query to filter items.
* Azure DevOps import will fail when descriptions and comments contain special characters. It looks like Jira and Azure DevOps except a different encoding.

I will accept pull requests if you'd like to contribute back to the project.

## Acknowledgements

#### Solidify's Jira to Azure DevOps migrator
I started the migration with [Solidify's Jira to Azure DevOps migrator](https://github.com/solidify/jira-azuredevops-migrator).  
It did most of what I needed but there were a few things missing:
* Comments weren't migrated
* Data exported from Jira was modified before being persisted to disk. I wanted raw data for historical reference and to play with the mappings without the need to download from Jira again.
* I needed to report on existing data to understand what fields, status & types were actually used vs theoretically possible. We had a lot of obsolete issue types and statuses in our system and creating the mappings were cumbersome.
  * I initially added these to the tool but found the process of creating and troubleshooting less convenient without CommandDotNet.
* Custom logging framework forced use of MS telemetrics and didn't support structured logging or log file management, like rolling logs.
  * I used NLog, with settings to match their defaults.
* Failure tracking. I wanted failed exports and imports recorded to a file so I could retry just the failures. During import, I was able to fix errors and re-run just the failures until the fail-file was empty.

What wasn't ported:
* Changelogs (History)
* Issue links
* Creating projects and issue types if they didn't exist

This [blog post](https://solidify.se/jira-to-vsts-migration-work-items/) gives context to understand what challenges are involved in the migration and how to tool addresses them.
This [blog post](https://solidify.se/jira-azure-devops-migration/) is instructions for using the tool.

This was immensely helpful. Thank you.

#### AzureDevOpsPlayground
I also found [AzureDevOpsPlayground](https://github.com/alkampfergit/AzureDevOpsPlayground) to be helpful while gaining understanding of how to use Azure DevOps apis. 
The Azure DevOps REST APIs aren't complete enough for importing work item so we had to rely on the soap-based SDK and this library helped me understand that better. 