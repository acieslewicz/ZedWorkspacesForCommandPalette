// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Commands;
using ZedCommandPalette.Components;

namespace ZedCommandPalette.Pages;

internal sealed partial class ZedProjectsListPage : DynamicListPage, INotifyItemsChanged
{
    private List<(ZedProject Project, ListItem Item)> _projectItems = [];

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

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add
        {
            ItemsChanged += value;
            LoadProjects();
        }
        remove => ItemsChanged -= value;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (oldSearch == newSearch) return;
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        return _projectItems
            .Where(entry => string.IsNullOrEmpty(SearchText) ||
                            entry.Project.Paths.Any(path =>
                                path.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .Select(entry => entry.Item)
            .ToArray<IListItem>();
    }

    private void LoadProjects()
    {
        _projectItems = ZedRecentProjects.GetRecentProjects()
            .Select(p => (p, new ListItem(new OpenZedProjectCommand(p))
            {
                Icon = Icons.ZedIcon,
                Title = p.Name,
                Subtitle = p.Paths.FirstOrDefault() ?? "",
                Tags = p.RemoteConnection is not null ? [new Tag { Text = p.RemoteConnection.Kind }] : [],
                MoreCommands = [new CommandContextItem(new RefreshCommand(LoadProjects))]
            })).ToList();

        RaiseItemsChanged();
    }
}