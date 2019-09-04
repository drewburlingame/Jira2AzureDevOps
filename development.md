## Developing with this solution

I've used this as an opportunity to prove out some of the benefits of the [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet) v3 preview which provides tools to ease troubleshooting.  Documentation for these tools is pending so I'll summarize here.

### Architecture

##### Design Theory

A console app is an interface to business logic, much the same as a web api is.  In this way, commands are analagous to endpoints.

This app is designed the similar to a web api. Let's compare the layers...

|Layer|Web API|Console|
|---|---|---|
|Host|Program|Program|
|API|Endpoints|Commands|
|Business Logic|Business Logic|Business Logic|

The same business logic could be hosted in both types of app and the console can provides types of interaction, feedback and long lived processes that require additional components in a web app.

##### Implementation

* `Jira2AzureDevOps` contains just the program and configuration logic
* `Jira2AzureDevOps.Console` contains all of the commands and middleware logic
* `Jira2AzureDevOps.Logic` contains aall of the migration busines logic

The working directory serves as the local data store for a migration.  A migration file is stored for each migration to track it's import status.

Export & Import use the same IJiraApi stack. Export is a by-produce of the `CachedJiraApi` using `LocalDirJiraApi`. Due to this design, Import also performs an Export for issues that aren't already exported.  This ensures all issues in the cache and available for archive.

C# classes were created for Json deserialization.  I used [QuickType](https://app.quicktype.io/#l=cs&r=json2csharp) to generate the classes from json data with some tweaks afterward.  The classes do not contain a comprehensive list of every field.  They contain what I needed for the project and extra where I didn't remove the unused properties.  Enhance these objects if you need to import more fields.  I found this approach useful for interrogating the data.
 
#### RSP files
described in the [README.md](README.md)

#### Using appSettings for default options
described in the [README.md](README.md)

enabled by [SetDefaultsFromConfigMiddleware](Jira2AzureDevOps.Console/Framework/SetDefaultsFromConfigMiddleware.cs)

#### Debug Directive

Debugging console apps can be a hassle. Changing the arguments in the project properties is a lot of extra steps.

[CommandDotNet](https://github.com/bilal-fazlani/commanddotnet) includes a useful feature for debugging called a [Debug Directive](https://github.com/dotnet/command-line-api/wiki/Features-overview#debugging) initially created by the System.CommandLine project.
By specifying `[debug]` as the first argument, you'll be prompted to attach a debugger to the current process.

> $ jira2ado [debug] export issues-by-id
> 
> Attach your debugger to process 18640 (dotnet).

This makes it easy to debug with different arguments every time.

#### Logging

I used NLog for structured logging and to take advantage of that ecosystem of appenders without lock-in.  Some options include:
* [MS Application Insights](https://github.com/microsoft/ApplicationInsights-dotnet-logging)
* [AWS](https://github.com/aws/aws-logging-dotnet)
* [DataDog](https://docs.datadoghq.com/logs/log_collection/csharp/?tab=nlog)
* [NLogs official integration list](https://nlog-project.org/config/)

Console logging is enabled by default at INFO level and above.  Commands who's output should not contain console logs (eg. report commands) can disable console logging with the `DisableConsoleLoggingAttribute`

enabled by [DisableConsoleLoggingMiddleware](Jira2AzureDevOps.Console/Framework/DisableConsoleLoggingMiddleware.cs)

Also by default, when a command is run, Repro Headers (arguments and other system information) are included at the start of the run.  These can be disabled for a command with the `NoReproHeadersAttribute`.  
Tip: Repro headers are not logged to console when `DisableConsoleLoggingAttribute` but will still output to the log file.

enabled by [ReproHeadersMiddleware](Jira2AzureDevOps.Console/Framework/ReproHeadersMiddleware.cs)

#### IArgumentModel validation

[Argument models](https://bilal-fazlani.github.io/commanddotnet/argument-models/) allows defining arguments in classes for reuse. 
Implement `ISelfValidatingArgumentModel` to provide validation errors for the model.

enabled by [SelfValidatingArgumentsMiddleware](Jira2AzureDevOps.Console/Framework/SelfValidatingArgumentsMiddleware.cs)

#### Console Enumerator

the [ConsoleEnumerator](Jira2AzureDevOps.Console/Framework/ConsoleEnumerator.cs) provides the `EnumerateOperation` extension method to provide a consistent experience for
* tracking count of processed and errored and elapsed time
* estimated time remaining and estimated time complete
* writing to failure files
* responding to Ctrl+C cancellations by stopping the enumeration