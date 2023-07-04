# Azure DevOps CLI
A simple, no-frills CLI tool for interacting with the Azure DevOps APi

**NOTE: This tool is still a work in progress, and syntax and parameters may change without warning**

## Installation
This is a dotnet tool, so to install simply run `dotnet tool install -g JST.AzureDevOpsCLI`. If you already have the tool installed, you can update it by running `dotnet tool update -g JST.AzureDevOpsCLI`.

After installation, you can invoke the tool by running `ado` in your terminal.

## Auto-resolution
The tool will attempt to resolve the Azure DevOps organization and project. It does this by finding the first remote of the current git repository that matches the expected pattern. If you for some reason which to override this,
or use the tool outside of a git repository, both values may be supplied manually: `ado GetBuilds organization=MyOrg project=MyProject`

## Getting started
In order to use this application, you must login. This application uses PAT-based authentication, which you can generate via Azure DevOps. You may give the PAT whichever scope you wish, as long as you cover your intended use cases.

**NOTE: This tool will store the PAT base64 encoded in `SpecialFolder.ApplicationData`**

Next step is to invoke the login command, which may be done in one of two ways:

> `"mypat" | ado login`
>
> `ado login "mypat"`

The first variant may be desirable if you wish to avoid having the PAT in your command history. You can paste the PAT into a file and then pipe the contents of the file to the login command.

*NOTE: PATs are stored per organization and you may only one per organization*

*NOTE: The login command does not validate the PAT*

## Special commands
The default behavior of the tool is to attempt to map whichever command you've specified to a known API method, but there are exceptions:

`list-areas` - Lists all known areas. Areas are method groups, like Build, Release, Approval, etc. For the most part, which area a command exists under is not going to be relevant.

`list-commands (area)` - Lists all known commands. If area is specified, the list is limited to the commands of the given area

`search-command <term>` - Lists all commands that match the given term. The search is case-insensitive and only a partial match is necessary

`show-command <name> (area=<area>)` - Shows detailed information about the given command. Optionally, the `area` parameter may be supplied to limit the output

`auto-detect` - Runs the Azure DevOps organization and project auto-detection method in the current path and outputs the result

## Invoking an API call
Each API method has three things that makes them unique, name, area and HTTP method. Unless you're trying to invoke an API method with a non-unique name, you don't have to supply either area or HTTP method.

API methods have a set of parameters, which can be any combination of route, query, body, or headers. Regardless of position in the call, all parameters are defined in the same waay:

`name = value` (the whitespaces surrounding `=` are optional)

As an example, fetching revision 1 of the build definition 123 would look like this:

`ado GetDefinition definitionId=123 revision=1`

Some methods require a body to be sent with the request, which may be supplied in one of two ways:

> `"myBody" | dotnet ado ...`
>
> `ado ... body="myBody"`

## Flags
There are a number of flags that, when set, will affect the output of an API call. These flags are only valid when invoking an API call.

`-s` or `--silent` - Suppress all output

`-p` or `--pretty` - Pretty-print (format) the output

`--what-if` - Instead of invoking the command, show the full URL that would have been invoked and, if supplied, the body as well.

## Query
Finally, there is a special flag: `query`. Using the [JUST.net](https://github.com/WorkMaze/JUST.net)'s transformer, the result of the call will be transformed by the specified query, wrapped inside [#valueof](https://github.com/WorkMaze/JUST.net#-valueof).

I.e., given the query `"foo"`, the transformer will receive `"\"#valueof(foo)\""` and the output will replace the original output. The output is currently *not* pretty-printed, but this will change with a future release.

To extract the id of each build, you could use the following query: `ado GetBuilds --query="value[*].id"`

## Roadmap
This is a first release of the tool, and some features are missing or incomplete. This is a non-exhaustive list of things that need to be addressed:
* Verify that the body is valid JSON (when expected)
  * Generate DTOs for each body type, for easier validation
* Look at the content-type header to decide how to treat the response
* Improve error messages
* Improve `show-command` output
* Validate input against enum-values
* Validate input against expected type
* If a parameter is specified more than once, group the values together in a comma-separated list
* ~~Release the source code~~
* ~~Add support for header parameters~~

## Notes on changes
* You are no longer required to prefix a parameter with ':' when invoking a command
* To run a query on the result, use the `--query` (or `-q`) flag instead of the `query` parameter
