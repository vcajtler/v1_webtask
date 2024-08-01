using System.Data;
using Dapper;
using Npgsql;

namespace V1_WebTask.Utilities;

public class DatabaseUtility
{
    private readonly NpgsqlConnection _connection;

    public DatabaseUtility(DatabaseData databaseData)
    {
        var connectionString =
            $"Host={databaseData.Host};Port={databaseData.Port};Database={databaseData.DatabaseName};Username={databaseData.Username};Password={databaseData.Password}";

            _connection = new NpgsqlConnection(connectionString);

    }

    public async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        try
        {
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            return _connection;
        }
        catch (Exception ex)
        {
            throw new Exception("Connection to database has not been established successfully.\n"
                                + ex.Message);
        }
    }

    public async Task<object?> ExecuteQueryAsync(string query, object? parameters = null)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<object>(query, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("ExecuteScalarAsync method was not successful.\n" 
                                + ex.Message);
        }
    }
}