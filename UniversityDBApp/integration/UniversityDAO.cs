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

    public Course? FindCourseByInstanceId(int instanceId)
    {
        try
        {
            /* SELECTING the course instance */
            using var selectInstanceCmd = new NpgsqlCommand(Statements.FindCourseInstanceById, _connection);
            selectInstanceCmd.Parameters.AddWithValue("@id", instanceId);
            using var reader = selectInstanceCmd.ExecuteReader();

            if (!reader.Read()) return null;
            
            /* Creating the course object */
            Course course = new Course(
                instanceId,
                reader.GetString(reader.GetOrdinal("course_code")),
                reader.GetString(reader.GetOrdinal("course_name")),
                reader.GetInt32(reader.GetOrdinal("num_students")),
                reader.GetInt32(reader.GetOrdinal("study_year")),
                reader.GetString(reader.GetOrdinal("study_period")),
                reader.GetFloat(reader.GetOrdinal("hp"))
            );
            reader.Close();
            return course;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    
    public List<Course>? FindCoursesByYear(int year)
    {
        try
        {
            /* SELECTING the course instances */
            using var selectInstanceCmd = new NpgsqlCommand(Statements.FindCourseInstanceByYear, _connection);
            selectInstanceCmd.Parameters.AddWithValue("@year", year);
            using var reader = selectInstanceCmd.ExecuteReader();
            List<Course> courses = new List<Course>();
            while (reader.Read())
            {
                /* Creating the course object */
                Course course = new Course(
                    reader.GetInt32(reader.GetOrdinal("instance_id")),
                    reader.GetString(reader.GetOrdinal("course_code")),
                    reader.GetString(reader.GetOrdinal("course_name")),
                    reader.GetInt32(reader.GetOrdinal("num_students")),
                    reader.GetInt32(reader.GetOrdinal("study_year")),
                    reader.GetString(reader.GetOrdinal("study_period")),
                    reader.GetFloat(reader.GetOrdinal("hp"))
                );
                courses.Add(course);
            }
            
            reader.Close();
            return courses;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    
    //public 

   
}

static class Statements
{ 
    //public const string FindCourseLayoutById = "SELECT * FROM course_layout WHERE course_layout_id = @id";
    public const string FindCourseInstanceById = """
                                                 SELECT * 
                                                 FROM course_instance i 
                                                 JOIN course_layout l ON l.course_layout_id = i.course_layout_id 
                                                 WHERE instance_id = @id
                                                 """;
    public const string FindCourseInstanceByYear = """
                                                   SELECT * 
                                                   FROM course_instance i 
                                                   JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                   WHERE study_year = @year
                                                   """;
    
}