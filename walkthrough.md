This document will walk you through a migration from Jira to Azure DevOps.

## Prep

* Get the exe
  * Download a [release](https://github.com/drewburlingame/Jira2AzureDevOps/releases)
  * Fork and clone and build https://github.com/drewburlingame/Jira2AzureDevOps
* Configure `Options.config`
  * In the exe directory, copy `Options.template.config` to `Options.config`
  * Configure the Jira username, token and url
  * Configure the Azure DevOps token, url and target project
  * Configure the workspace unless you're running the commands from the working directory in the shell

## Export

Run `export metadata` to export the list of projects and other metadata for issues.  This is not require for an import.  You can use it to grok the system and for archiving.

Run `export issues-by-project --fail-file failed-exports.rsp` to download issues for all projects. 

To filter by project, specify the projects as arguments `export issues-by-project ProjA ProjB --fail-file failed-exports.rsp`

When the run is completed, check failed-exports.rsp for failures and use the logs to troubleshoot.

To rerun the failed exports, use `export issues-by-id @failed-exports.rsp`.  To target specific issues, you can extract the ids into a separate file or supply the ids directly to the command as `export issues-by-id ProjA-1 ProjA-2 ...`

If `export issues-by-project` is stopped before completion, you have a couple of options for resuming.

1. run `export issues-by-project` again.  The export will query Jira for the list of issue ids but won't query Jira for issues that are already downloaded... unless the `--force` option is specified.
2. run `export issues-by-project --issue-list-source Both ...`. The export will query Jira for the list, but only starting from the last downloaded issue for each project.
3. run `export issues-by-project --resume-after ProjA-1`. The export will query Jira for the list starting from the specified issue

If new issues are added after an export, use `export issues-by-project --issue-list-source Both` to export the new issues.

If issues are updated, use `--force` to export those issues.

> note: if you expect edits after the update, consider adding an `export issues-edited-since` that uses a JQL to get the list of issues added/edited after the specified date.

## Create mapping files

Run `report stub-status-mapping -ptcsH > status-mappings.csv` to generate a stub of the status mapping file that contains columns with the project key, issue type, status category and status.

If you don't want to import issues of a certain type, status or status category, remove those rows from the mapping and they will be skipped.

Remove any columns that are not significant in the mapping.  For example, by removing the project, issue type and status columns and all issues are mapped to the new status based soley by status category.  This is useful if you have a lot of statuses and don't want to recreate all of those in Azure.

Run `report stub-issue-types-mapping ptH > issue-types-mappings.csv` to generate a stub of the issue types mapping file contiaining project key and isue type.

## Import

Run `import issues-by-project --fail-file failed-imports.rsp -t issue-types-mapping.csv -s status-mappings.csv` to import issues for all projects.

As with exports, you can use the failed-imports.rsp file to retry issues that failed to import.

Use `--force` to delete existing items, otherwise they will be skipped.  Use this when you need to change the way mappings work or adding/modifying other field mappings.

## Archive

Archive the workspace directory for historical reference.  You can revisit the archive if you need to import other issues or modify alreayd imported issues.

> Note, if you can generate the mappings.csv files before export, you can skip the export step and run import immediately.  The export will occur as a side-effect of caching Jira responses. 
