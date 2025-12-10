using System.Net;
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
        // Throw DB connection exception here
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
            //Console.WriteLine("Connection successfully established");
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public NpgsqlConnection GetConnection() => _connection;

    public void StartTransaction()
    {
        if (_transaction != null)
        {
            //TODO Console.WriteLine("Current transaction already started");
            return;
        }

        try
        {
            _transaction = _connection.BeginTransaction();
        }
        catch (Exception ex)
        {
            throw;
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
            _transaction?.Rollback();
            throw;
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
            throw;
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
        catch (Exception ex)
        {
            throw;
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
            
            if (!reader.Read()) return null;
            
            do
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
            } while (reader.Read());
            return courses;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void UpdateCourseByInstanceId(Course course)
    {
        try
        {
            if (_transaction == null)
            {
                //TODO Console.WriteLine("Transaction not started for update");
            }
                
            /* UPDATING the course instance */
            using var updateInstanceCmd = new NpgsqlCommand(Statements.UpdateNumStudentsById, _connection, _transaction);
            updateInstanceCmd.Parameters.AddWithValue("@id", course.InstanceId);
            updateInstanceCmd.Parameters.AddWithValue("@num_students", course.NumStudents);
            updateInstanceCmd.ExecuteNonQuery();
        } 
        catch (Exception ex)
        {
            throw;
        }
    }

    public TeachingCost? CalculateTeachingCost(Course course)
    {
        try
        {
            /* finding the cost using the cost query */
            using var costCmd = new NpgsqlCommand(Statements.CostCalculationById, _connection, _transaction);
            costCmd.Parameters.AddWithValue("@id", course.InstanceId);
            costCmd.Parameters.AddWithValue("@year", course.StudyYear);
            costCmd.Parameters.AddWithValue("@period", course.StudyPeriod);
            using var reader = costCmd.ExecuteReader();

            if (!reader.Read()) return null;

            TeachingCost cost = new TeachingCost(
                course.InstanceId,
                course.CourseCode,
                course.StudyPeriod,
                reader.GetFloat(reader.GetOrdinal("planned_costs")),
                reader.GetFloat(reader.GetOrdinal("actual_costs"))
            );
            return cost;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public List<Activity>? FindActivitiesByCourse(Course course)
    {
        try
        {
            NpgsqlCommand selectActivitiesCmd;
            if (_transaction == null)
            {
                selectActivitiesCmd = new NpgsqlCommand(Statements.FindActivitiesByInstanceId, _connection);
            }
            else
            {
                selectActivitiesCmd = new NpgsqlCommand(Statements.FindActivitiesByInstanceIdForUpdate, _connection, _transaction);
            }

            selectActivitiesCmd.Parameters.AddWithValue("@id", course.InstanceId);
            using var reader = selectActivitiesCmd.ExecuteReader();
            
            if (!reader.Read()) return null;

            List<Activity> activities = new List<Activity>();
            do
            {
                Activity activity = new Activity(
                    reader.GetInt32(reader.GetOrdinal("planned_activity_id")),
                    reader.GetInt32(reader.GetOrdinal("instance_id")),
                    reader.GetString(reader.GetOrdinal("activity_name")),
                    reader.GetFloat(reader.GetOrdinal("planned_hours")),
                    reader.GetFloat(reader.GetOrdinal("factor"))
                );
                activities.Add(activity);
            } while (reader.Read());
            
            selectActivitiesCmd.Dispose();
            return activities;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public List<Activity>? FindActivitiesByTeacher(Teacher teacher)
    {
        try
        {
            NpgsqlCommand selectActivitiesCmd;
            if (_transaction == null)
            {
                selectActivitiesCmd = new NpgsqlCommand(Statements.FindActivitiesByEmployementId, _connection);
            }
            else
            {
                selectActivitiesCmd = new NpgsqlCommand(Statements.FindActivitiesByEmployementId, _connection, _transaction);
            }

            selectActivitiesCmd.Parameters.AddWithValue("@id", teacher.EmployementId);
            using var reader = selectActivitiesCmd.ExecuteReader();
            
            if (!reader.Read()) return null;

            List<Activity> activities = new List<Activity>();
            do
            {
                Activity activity = new Activity(
                    reader.GetInt32(reader.GetOrdinal("planned_activity_id")),
                    reader.GetInt32(reader.GetOrdinal("instance_id")),
                    reader.GetString(reader.GetOrdinal("activity_name")),
                    reader.GetFloat(reader.GetOrdinal("allocated_hours")),
                    reader.GetFloat(reader.GetOrdinal("factor"))
                );
                activities.Add(activity);
            } while (reader.Read());
            
            selectActivitiesCmd.Dispose();
            return activities;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public Teacher? FindTeacherByEmployementId(int employementId)
    {
        try
        {
            NpgsqlCommand selectTeacherCmd;
            if (_transaction == null)
            {
                selectTeacherCmd = new NpgsqlCommand(Statements.FindTeacherByEmployementId, _connection);
            }
            else
            {
                selectTeacherCmd = new NpgsqlCommand(Statements.FindTeacherByEmployementIdForUpdate, _connection, _transaction);
            }

            selectTeacherCmd.Parameters.AddWithValue("@id", employementId);
            using var reader = selectTeacherCmd.ExecuteReader();

            if (!reader.Read()) return null;

            Teacher teacher = new Teacher(
                reader.GetInt32(reader.GetOrdinal("employement_id")),
                reader.GetString(reader.GetOrdinal("first_name")),
                reader.GetString(reader.GetOrdinal("last_name")),
                reader.GetString(reader.GetOrdinal("job_title")),
                reader.GetInt32(reader.GetOrdinal("department_id")),
                reader.GetInt32(reader.GetOrdinal("supervisor_id"))
            );

            selectTeacherCmd.Dispose();
            return teacher;

        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public void DeleteEmployeePlannedActivity(int employementId, int plannedActivityId)
    {
        try
        {
            using var deleteActivityCmd = new NpgsqlCommand(Statements.DeleteEmployeePlannedActivityById, _connection);
            deleteActivityCmd.Parameters.AddWithValue("@employement_id", employementId);
            deleteActivityCmd.Parameters.AddWithValue("@planned_activity_id", plannedActivityId);
            deleteActivityCmd.ExecuteNonQuery();
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }

    public void CreateEmployeePlannedActivity(int employementId, int plannedActivityId, float allocatedHours)
    {
        try
        {
            using var createActivityCmd = new NpgsqlCommand(Statements.CreateEmployeePlannedActivityById, _connection);
            createActivityCmd.Parameters.AddWithValue("@employement_id", employementId);
            createActivityCmd.Parameters.AddWithValue("@planned_activity_id", plannedActivityId);
            createActivityCmd.Parameters.AddWithValue("@allocated_hours", allocatedHours);
            createActivityCmd.ExecuteNonQuery();
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }

    public void CreateActivityType(string activityName, float factor)
    {
        try
        {
            using var createActivityTypeCmd = new NpgsqlCommand(Statements.CreateActivityType, _connection);
            createActivityTypeCmd.Parameters.AddWithValue("@activity_name", activityName);
            createActivityTypeCmd.Parameters.AddWithValue("@factor", factor);
            createActivityTypeCmd.ExecuteNonQuery();
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }
    
    public void CreateActivity(string activityName, int instanceId, float plannedHours)
    {
        try
        {
            using var createActivityCmd = new NpgsqlCommand(Statements.CreateActivity, _connection);
            createActivityCmd.Parameters.AddWithValue("@activity_name", activityName);
            createActivityCmd.Parameters.AddWithValue("@instance_id", instanceId);
            createActivityCmd.Parameters.AddWithValue("@planned_hours", plannedHours);
            createActivityCmd.ExecuteNonQuery();
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }
    
   
}


/* 
CODE GRAVEYARD:
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
*/