using UniversityDBApp.model;
namespace UniversityDBApp.integration;
using Npgsql;
using System.IO;

public class UniversityDAO : IDisposable
{
    private NpgsqlConnection _connection = null!;
    private NpgsqlTransaction? _transaction;

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

    public void StartTransaction()
    {
        if (_transaction != null)
        {
            Console.WriteLine("Current transaction already started");
            return;
        }

        try
        {
            _transaction = _connection.BeginTransaction();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    
    public void CommitTransaction()
    {
        if (_transaction == null)
        {
            Console.WriteLine("No transaction is started");
            return;
        }

        try
        {
            _transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void RollBackTransaction()
    {
        if (_transaction == null)
        {
            Console.WriteLine("No transaction is started");
            return;
        }

        try
        {
            _transaction.Rollback();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _connection?.Dispose();
    }
    
    public List<TeachingActivity> FindAllTeachingActivities()
    {
        List<TeachingActivity> activities = new List<TeachingActivity>();
        try
        {
            NpgsqlCommand selectCommand;
            if (_transaction == null)
            {
                selectCommand = new NpgsqlCommand("SELECT * FROM teaching_activity", _connection);
            }
            else
            {
                selectCommand = new NpgsqlCommand("SELECT * FROM teaching_activity", _connection, _transaction);
            }
            
            using var reader = selectCommand.ExecuteReader();
            
            while (reader.Read())
            {
                int teachingActivityId = reader.GetInt32(0);
                string teachingActivityName = reader.GetString(1);
                float factor = reader.GetFloat(2);

                activities.Add(new TeachingActivity(teachingActivityId, teachingActivityName, factor));
            }
            selectCommand.Dispose();
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
            NpgsqlCommand selectInstanceCmd;
            /* Deciding if it is in need of not commiting (method used as part of update) */
            if (_transaction == null)
            {
                selectInstanceCmd = new NpgsqlCommand(Statements.FindCourseInstanceById, _connection);
            }
            else
            {
                selectInstanceCmd = new NpgsqlCommand(Statements.FindCourseInstanceByIdForUpdate, _connection, _transaction);
            }
            
            /* SELECTING the course instance */
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
            selectInstanceCmd.Dispose();
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
            return courses;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public bool UpdateCourseByInstanceId(Course course)
    {
        /* The try-catch is handled in the controller */
        /* Deciding if it is in need of not commiting (method used as part of update) */
        if (_transaction == null)
        {
            Console.WriteLine("Transaction not started for update");
            return false;
        }
            
        /* UPDATING the course instance */
        using var updateInstanceCmd = new NpgsqlCommand(Statements.UpdateNumStudentsById, _connection, _transaction);
        updateInstanceCmd.Parameters.AddWithValue("@id", course.InstanceId);
        updateInstanceCmd.Parameters.AddWithValue("@num_students", course.NumStudents);
        updateInstanceCmd.ExecuteNonQuery();
        return true;
    }

   
}

static class Statements
{ 
    public const string FindCourseInstanceById = """
                                                 SELECT * 
                                                 FROM course_instance i 
                                                 JOIN course_layout l ON l.course_layout_id = i.course_layout_id 
                                                 WHERE instance_id = @id
                                                 """;
    public const string FindCourseInstanceByIdForUpdate = """
                                                 SELECT * 
                                                 FROM course_instance i 
                                                 JOIN course_layout l ON l.course_layout_id = i.course_layout_id 
                                                 WHERE instance_id = @id FOR UPDATE
                                                 """;
    public const string FindCourseInstanceByYear = """
                                                   SELECT * 
                                                   FROM course_instance i 
                                                   JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                   WHERE study_year = @year
                                                   """;
    public const string UpdateNumStudentsById = """
                                                UPDATE course_instance
                                                SET
                                                    num_students = @num_students
                                                WHERE instance_id = @id
                                                """;


}