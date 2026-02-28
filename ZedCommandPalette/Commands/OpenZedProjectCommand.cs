// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Diagnostics;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Components;
using ZedCommandPalette.Helpers;

namespace ZedCommandPalette.Commands;

internal sealed partial class OpenZedProjectCommand : InvokableCommand
{
    private readonly ZedProject _project;

    public OpenZedProjectCommand(ZedProject project)
    {
        _project = project;
        Name = "Open";
    }

    public override ICommandResult Invoke()
    {
        var args = string.Join(' ', _project.Paths.Select(p => $"\"{p}\""));

        switch (_project.RemoteConnection)
        {
            case null:
                break;
            case RemoteConnection.Ssh ssh:
                {
                    if (ssh.Host is null)
                        return CommandResult.Confirm(new ConfirmationArgs
                        { Title = "Error", Description = "SSH Project is missing Host." });
                    var user = ssh.User is not null ? $"{ssh.User}@" : "";
                    var port = ssh.Port is not null ? $":{ssh.Port}" : "";
                    var target = $"ssh://{user}{ssh.Host}{port}";
                    args = string.Join(' ', _project.Paths.Select(p => $"\"{target}{p}\""));
                    break;
                }
            case RemoteConnection.Wsl wsl:
                {
                    if (wsl.Distro is null)
                        return CommandResult.Confirm(new ConfirmationArgs
                        {
                            Title = "Error",
                            Description = "WSL Project is missing Distro."
                        });

                    var target = wsl.User is not null ? $"{wsl.User}@{wsl.Distro}" : wsl.Distro;
                    args = $"--wsl {target} {args}";
                    break;
                }
            case var unknownConnection:
                return CommandResult.Confirm(new ConfirmationArgs
                {
                    Title = "Error",
                    Description = $"Cannot currently open {unknownConnection.Kind} projects."
                });
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = SettingsManager.Instance.ZedPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        return CommandResult.Dismiss();
    }
}