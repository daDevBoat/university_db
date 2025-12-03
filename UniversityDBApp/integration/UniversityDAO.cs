namespace UniversityDBApp.integration;
using Npgsql;

public class UniversityDAO : IDisposable
{
    private NpgsqlConnection _connection;

    public UniversityDAO()
    {
        ConnectToDb();
    }
    
    public void ConnectToDb ()
    {
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=PASS;Database=university_db";

        try
        {
            _connection = new NpgsqlConnection(connString);
            _connection.Open();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public NpgsqlConnection GetConnection() => connection;

    public void Dispose()
    {
        connection?.Close();
        connection?.Dispose();
    }
    
}