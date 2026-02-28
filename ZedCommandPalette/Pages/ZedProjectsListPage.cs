// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ZedCommandPalette.Commands;
using ZedCommandPalette.Components;

namespace ZedCommandPalette.Pages;

internal sealed partial class ZedProjectListItem : ListItem
{
    public ZedProject Project { get; }

    public ZedProjectListItem(ZedProject project) : base(new OpenZedProjectCommand(project))
    {
        Project = project;
        Icon = Icons.ZedIcon;
        Title = project.Name;
        Subtitle = project.Paths.FirstOrDefault() ?? "";
        Tags = project.RemoteConnection is not null ? [new Tag { Text = project.RemoteConnection.Kind }] : [];
    }
}

internal sealed partial class ZedProjectsListPage : DynamicListPage, INotifyItemsChanged
{
    private List<ZedProjectListItem> _projectItems = [];

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
                            entry.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .ToArray<IListItem>();
    }

    private void LoadProjects()
    {
        _projectItems = ZedRecentProjects.GetRecentProjects()
            .Select(p => new ZedProjectListItem(p)
            {
                MoreCommands = [new CommandContextItem(new RefreshCommand(LoadProjects))]
            }).ToList();

        RaiseItemsChanged();
    }
}