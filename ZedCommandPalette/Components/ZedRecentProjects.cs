// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace ZedCommandPalette.Components;

internal class ZedProject(long workspaceId, List<string> paths, long? remoteConnectionId)
{
    public long WorkspaceId { get; } = workspaceId;
    public long? RemoteConnectionId { get; } = remoteConnectionId;
    public List<string> Paths { get; } = paths;

    public string Name => Paths.Count switch
    {
        0 => "Unnamed Workspace",
        _ => Path.GetFileName(Paths[0])
    };
}

internal class ZedRecentProjects
{
    private static string GetZedDbPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Zed", "db", "0-stable", "db.sqlite");
    }

    internal static List<ZedProject> GetRecentProjects()
    {
        var dbPath = GetZedDbPath();
        if (!File.Exists(dbPath)) return [];

        using var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
                                  SELECT workspace_id, paths, paths_order, remote_connection_id
                                  FROM workspaces
                                  WHERE paths IS NOT NULL
                                  ORDER BY timestamp DESC
                              """;

        using var reader = command.ExecuteReader();

        var projects = new List<ZedProject>();

        while (reader.Read())
            projects.Add(new ZedProject(
                reader.GetInt64(0),
                ParsePaths(
                    reader.IsDBNull(1) ? null : reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)
                ),
                reader.IsDBNull(3) ? null : reader.GetInt64(3)
            ));

        return projects;
    }

    // TODO: Add a method that can get the Remote Connection from the database start with just Wsl
    // Do an enum by connection type for Wsl and just get the nullable distro and user

    private static List<string> ParsePaths(string? rawPaths, string? rawOrder)
    {
        if (string.IsNullOrEmpty(rawPaths)) return [];

        var paths = rawPaths.Split('\n').ToList();

        if (string.IsNullOrEmpty(rawOrder)) return paths;

        var order = rawOrder.Split(',').Select(s => (int.TryParse(s, out var value), value)).Where(pair => pair.Item1)
            .Select(pair => pair.value).ToList();

        return order.Count != paths.Count
            ? paths
            : order.Zip(paths).OrderBy(pair => pair.First).Select(pair => pair.Second).ToList();
    }
}