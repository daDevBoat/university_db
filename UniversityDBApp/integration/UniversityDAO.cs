using UniversityDBApp.model;
namespace UniversityDBApp.integration;
using Npgsql;
using System.IO;

public class UniversityDAO : IDisposable
{
    private NpgsqlConnection _connection = null!;

    public UniversityDAO()
    {
        ConnectToDb();
    }
    
    public void ConnectToDb ()
    {
        string passwordFilePath = "C:\\Users\\Elias\\Desktop\\Data_Storage_Paradigms\\Project\\psql_password.txt";
        string password = File.ReadAllText(passwordFilePath);
        string connString = $"Host=localhost;Port=5432;Username=postgres;Password={password};Database=university_db";

        try
        {
            _connection = new NpgsqlConnection(connString);
            _connection.Open();
            Console.WriteLine("Connection successfully established");
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public NpgsqlConnection GetConnection() => _connection;

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
    
    public List<TeachingActivity> FindAllTeachingActivities()
    {
        List<TeachingActivity> activities = new List<TeachingActivity>();
        try
        {
            using var selectCommand = new NpgsqlCommand("SELECT * FROM teaching_activity", _connection);
            using var reader = selectCommand.ExecuteReader();

            while (reader.Read())
            {
                int teachingActivityId = reader.GetInt32(0);
                string teachingActivityName = reader.GetString(1);
                float factor = reader.GetFloat(2);

                activities.Add(new TeachingActivity(teachingActivityId, teachingActivityName, factor));
            }

        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return activities;
    }

    public Course FindCourse(int instanceId)
    {
        Course course = null;
        return course;
    }
}

static class Statements
{ 
    public static string FindCourseLayout = "SELECT course_layout_id, course_code, course_name, hp, study_period FROM course_layout WHERE course_layout_id = {0}";
    public static string FindCourseInstance = "SELECT instance_id, course_layout_id, num_students, study_year FROM course_instance WHERE instance_id = {0}";
}