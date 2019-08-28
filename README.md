
# Purpose

* Export all issues and attachments from Jira
  * Filter by project(s) or issue(s)
  * downloaded to a local directory
    * for use as a cache and for --jira-offline mode
    * can be archived for historical reference
* Export meta-data that would be useful for preparing the mappings into Azure DevOps
* Import selected issues to Azure DevOps
  * converting
    * Issue Types
    * States
* Extensible design to enable any mapping strategy
  * including use of external data
* Dev & debug helpers
  * RSP files
  * Debug Directive (see Debugging section below)
  * Supplied arguments logged with each command
  * NLog for structured logging and to take advantage of that ecosystem of appenders without lock-in.  Some options include:
    * https://github.com/microsoft/ApplicationInsights-dotnet-logging 
    * https://github.com/aws/aws-logging-dotnet
    * https://docs.datadoghq.com/logs/log_collection/csharp/?tab=nlog
    * https://github.com/Appdynamics/AppDynamics.DEXTER/blob/master/NLog.config
    * https://nlog-project.org/config/

## Features

* Exports from Jira
  * Issues
  * Metadata
  * Attachments
* Cached to local directory
  * can be archived for historical purposes
  * export can be rerun and cached items will not be downloaded again unless `--force` option is specified.
    * this makes it less costly to recover from errors.
  * can be used for offline mode after all data is exported
  * allows re-importing for failures
* Imports Issues to Azure DevOps
  * tracks imported items and allows resume after 
  * adds label = "imported-from-jira" 

## Prerequisites

* Jira account w/ API token
* Azure DevOps account w/ API token
* Existing Azure DevOps project configured with target work item types and states.


### Commands

* __jira__
  * __export__
    * __issues-by-id__ export the given issue(s) by id
    * __issues-by-project__ export all issues for the given project(s)
    * __metadata__ export organization level metadata 
      * Projects
      * Issue Fields
      * Issue Linke Types
      * Issue Priorities
      * Issue Resolutions
      * Issue Statuses
      * Issue Types
      * Labels
  * __summarize__
    * __issue-types__ prints a list of issue types grouped by project
    * __status__ prints all statuses grouped by project, issue type and then category
	
## List of WorkItem fields

https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/fields/list?view=azure-devops-rest-5.1

summarized at [workItemFieldList.json](workItemFieldList.json)


## Caveates

This was a low-budget implementation and so many normal steps to harden the app have not been implemented.
The goal was to build something that could be iterated on by other developers.  Pull requests are encouraged.

* No automated testing.  It has all been manually tested.  Tests will come if time allows.
* Errors during export will crash the program.  Export is idempotent so it can be restarted.
  * I'd like to add files to skip issues with errors an option to load that file for `export issues-by-id`
            

### local dev

bash alias

> function jira2ado() { ~/src/drew/Jira2AzureDevOps/Jira2AzureDevOps/bin/Debug/Jira2AzureDevOps.exe $@; }

cmd alias via `aliases.bat`

> jira2ado=%USERPROFILE%\src\drew\Jira2AzureDevOps\Jira2AzureDevOps\bin\Debug\Jira2AzureDevOps.exe $*

Working dir 

* c:/tmp/jira2ado - run jira2ado commands from here
* c:/tmp/jira2ado/jira-settings.rsp
  * contains jira auth tokens and jira url
  * after all jira items are downloaded, includes the --offline flag so I don't query Jira again
  * commented out lines of other arguments that I may want to test.  see below.

From the working dir, the program is executed as

``` cmd
jira2ado jira export @jira-settings.rsp issues-by-project
```

jira-settings.rsp contains

``` cmd
--jira-username **** --jira-token **** --jira-url https://company-slug.atlassian.net
#--jira-offline
```

ado-settings.rsp contains

``` cmd
--ado-token **** --ado-url https://dev.azure.com/company-slug --jira-offline
```

`--jira-offline` if all your issues are already downloaded.  Use other issues to stream directly from Jira (caching still occurs)

#### Debugging

_CommandDotNet_ includes a useful feature for debugging called a [Debug Directive](https://github.com/dotnet/command-line-api/wiki/Features-overview#debugging) initially created by the System.CommandLine project.
By specifying `[debug]` as the first argument, you'll be prompted to attach a debugger to the current process.

> $ jira2ado [debug] jira export issues-by-id
> Attach your debugger to process 18640 (dotnet).
