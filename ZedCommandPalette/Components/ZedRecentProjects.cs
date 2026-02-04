// Copyright (c) acieslewicz
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using ZedCommandPalette.Helpers;

namespace ZedCommandPalette.Components;

internal abstract record RemoteConnection(string Kind)
{
    internal record Wsl(string? Distro, string? User) : RemoteConnection("wsl");

    internal record Unknown(string Kind) : RemoteConnection(Kind);
}

internal class ZedProject(long workspaceId, List<string> paths, RemoteConnection? remoteConnection)
{
    public long WorkspaceId { get; } = workspaceId;
    public RemoteConnection? RemoteConnection { get; } = remoteConnection;
    public List<string> Paths { get; } = paths;

    public string Name => Paths.Count switch
    {
        0 => "Unnamed Workspace",
        _ => Path.GetFileName(Paths[0])
    };
}

internal class ZedRecentProjects
{
    internal static List<ZedProject> GetRecentProjects()
    {
        var dbPath = SettingsManager.Instance.DbPath;
        if (!File.Exists(dbPath)) return [];

        using var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
                                  SELECT w.workspace_id, w.paths, w.paths_order, rc.kind, rc.distro, rc.user
                                  FROM workspaces w
                                  LEFT JOIN remote_connections rc ON w.remote_connection_id = rc.id
                                  WHERE paths IS NOT NULL
                                  ORDER BY timestamp DESC
                              """;

        using var reader = command.ExecuteReader();

        var projects = new List<ZedProject>();

        while (reader.Read())
        {
            var paths = ParsePaths(
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2)
            );
            var remoteConnection = ParseRemoteConnection(
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5)
            );
            projects.Add(new ZedProject(
                reader.GetInt64(0),
                paths,
                remoteConnection
            ));
        }

        return projects;
    }

    private static RemoteConnection? ParseRemoteConnection(string? kind, string? distro, string? user)
    {
        if (kind is null) return null;

        return kind switch
        {
            "wsl" => new RemoteConnection.Wsl(distro, user),
            _ => new RemoteConnection.Unknown(kind)
        };
    }

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