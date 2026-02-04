// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Diagnostics;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Components;

namespace ZedCommandPalette.Commands;

internal sealed partial class OpenZedProjectCommand : InvokableCommand
{
    private readonly ZedProject _project;

    public OpenZedProjectCommand(ZedProject project)
    {
        _project = project;
        Name = project.Name;
    }

    public override ICommandResult Invoke()
    {
        if (_project.Paths.Count < 1)
            return CommandResult.Confirm(new ConfirmationArgs
                { Title = "error", Description = "Project does not have any associated paths." });
        if (_project.RemoteConnectionId is not null)
            return CommandResult.Confirm(new ConfirmationArgs
                { Title = "error", Description = "Cannot currently open remote projects in Zed." });

        var args = string.Join(' ', _project.Paths.Select(p => $"\"{p}\""));

        Process.Start(new ProcessStartInfo
        {
            FileName = "zed",
            Arguments = args,
            UseShellExecute = true,
            CreateNoWindow = true
        });

        return CommandResult.Dismiss();
    }
}