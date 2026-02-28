// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ZedCommandPalette.Commands;

internal sealed partial class RefreshCommand : InvokableCommand
{
    private readonly Action _onRefresh;

    public RefreshCommand(Action onRefresh)
    {
        _onRefresh = onRefresh;
        Name = "Refresh";
        Icon = Icons.RefreshIcon;
    }

    public override ICommandResult Invoke()
    {
        _onRefresh();
        return CommandResult.KeepOpen();
    }
}