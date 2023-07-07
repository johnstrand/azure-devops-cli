using Ado;
using Ado.Arguments;
using Ado.Commands;

try
{
    var _args = ArgReader.Parse(args);
    if (_args.TryGetFlag("v", "verbose", out _))
    {
        Log.EnableVerbose();
    }

    if (_args.TryMatchCommand("list-areas"))
    {
        ListAreas.Execute(_args);
    }
    else if (_args.TryMatchCommand("list-commands"))
    {
        ListCommands.Execute(_args);
    }
    else if (_args.TryMatchCommand("search-command"))
    {
        SearchCommand.Execute(_args);
    }
    else if (_args.TryMatchCommand("show-command"))
    {
        ShowCommand.Execute(_args);
    }
    else if (_args.TryMatchCommand("auto-detect"))
    {
        _args.EnsureAllRead();
        AutoDetect.Execute();
    }
    else if (_args.TryMatchCommand("login"))
    {
        EnsureOrganizationAndProjectIsSet(_args);
        Login.Execute(_args);
    }
    else if (_args.TryMatchCommand("help") || !_args.HasCommands())
    {
        Help.Execute(_args);
    }
    else
    {
        EnsureOrganizationAndProjectIsSet(_args);
        await Invoke.Execute(_args);
    }

    return 0;
}
catch (Exception ex)
{
#if DEBUG
    Console.WriteLine(ex.ToString());
#else
    Console.WriteLine($"Error: {ex.Message}");
#endif
    return 1;
}

static void EnsureOrganizationAndProjectIsSet(ArgReader _args)
{
    if (_args.HasParameter(WellKnownParameters.Organization) && _args.HasParameter(WellKnownParameters.Project))
    {
        return;
    }

    if (!Git.FindAzureDevopsInfo(out var org, out var project, out var repo))
    {
        throw new("Failed to resolve Azure DevOps remote information, and organization and/or project was not specified in parameters");
    }

    if (!_args.HasParameter(WellKnownParameters.Organization))
    {
        _args.SetParameter(WellKnownParameters.Organization, org);
    }

    if (!_args.HasParameter(WellKnownParameters.Project))
    {
        _args.SetParameter(WellKnownParameters.Project, project);
    }
}