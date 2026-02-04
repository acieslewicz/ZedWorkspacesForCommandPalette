// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ZedCommandPalette;

internal sealed partial class ZedCommandPalettePage : ListPage
{
    public ZedCommandPalettePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\zed-logo.svg");
        Title = "Zed for Command Palette";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new NoOpCommand()) { Title = "TODO: Implement your extension here" }
        ];
    }
}
