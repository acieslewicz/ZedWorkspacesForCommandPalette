// Modifications Copyright (c) acieslewicz
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Helpers;
using ZedCommandPalette.Pages;

namespace ZedCommandPalette;

public partial class ZedCommandPaletteCommandsProvider : CommandProvider
{
    private readonly CommandItem _zedPageItem;

    public ZedCommandPaletteCommandsProvider()
    {
        DisplayName = "Zed for Command Palette";
        Icon = Icons.ZedIcon;
        Settings = SettingsManager.Instance.Settings;

        _zedPageItem = new CommandItem(new ZedProjectsListPage()) { Title = "Open Recent Zed" };
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [_zedPageItem];
    }
}