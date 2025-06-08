using ChatApp.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Services;

public class SqliteDatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public SqliteDatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SQLite") ?? "Data Source=chat.db";
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Role TEXT NOT NULL,
                    User TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    Timestamp TEXT NOT NULL
                )";
        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveMessageAsync(Message message)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
                INSERT INTO Messages (Role, User, Content, Timestamp)
                VALUES ($role, $user, $content, $timestamp)";
        command.Parameters.AddWithValue("$role", message.Role);
        command.Parameters.AddWithValue("$user", message.User);
        command.Parameters.AddWithValue("$content", message.Content);
        command.Parameters.AddWithValue("$timestamp", message.Timestamp.ToString("o"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Message>> GetRecentMessagesAsync(int limit)
    {
        var messages = new List<Message>();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
                SELECT Id, Role, User, Content, Timestamp
                FROM Messages
                ORDER BY Timestamp DESC
                LIMIT $limit";
        command.Parameters.AddWithValue("$limit", limit);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new Message
            {
                Id = reader.GetInt32(0),
                Role = reader.GetString(1),
                User = reader.GetString(2),
                Content = reader.GetString(3),
                Timestamp = DateTime.Parse(reader.GetString(4))
            });
        }

        messages.Reverse(); // Order by Timestamp ASC
        return messages;
    }
}
