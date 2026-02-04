// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Commands;
using ZedCommandPalette.Components;

namespace ZedCommandPalette.Pages;

internal sealed partial class ZedProjectsListPage : DynamicListPage
{
    public ZedProjectsListPage()
    {
        Icon = Icons.ZedIcon;
        Name = "Zed Recent Projects";
        PlaceholderText = "Search recent projects...";

        EmptyContent = new CommandItem(new NoOpCommand())
        {
            Icon = Icon,
            Title = "Open Recent Projects",
            Subtitle = "No recent projects found"
        };
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        var projects = ZedRecentProjects.GetRecentProjects();

        return projects
            .Where(p => string.IsNullOrEmpty(SearchText) ||
                        p.Paths.Any(path => path.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .Select(p => new ListItem(new OpenZedProjectCommand(p)) { Icon = Icons.ZedIcon, Title = p.Name })
            .ToArray<IListItem>();
    }
}