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

    public TeachingCost? CalculateTeachingCost(Course course)
    {
        try
        {
            if (_transaction == null)
            {
                Console.WriteLine("Transaction not started for update");
                return null;
            }
            
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
        catch (NpgsqlException ex)
        {
            throw;
        }
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

    public const string CostCalculationById = """
                                              WITH salaries AS (
                                                  SELECT DISTINCT ON (s.employement_id)
                                                  s.employement_id, s.year, s.period, s.salary
                                                  FROM salary_history s 
                                                  WHERE 
                                                      CASE 
                                                          WHEN s.year = @year THEN s.period <= @period ::period_enum
                                                          WHEN s.year < @year THEN TRUE
                                                          ELSE FALSE
                                                      END
                                                  ORDER BY s.employement_id ASC, s.year DESC, s.period DESC
                                              ), 
                                              average_salary AS (
                                                  SELECT AVG(salaries.salary) AS salary FROM salaries
                                              ),
                                              planned_costs AS (
                                                  SELECT 
                                                      f.lecture_cost + f.seminar_cost + f.lab_cost + f.tutorial_cost + f.other_cost + f.exam_cost + f.admin_cost AS total_cost
                                                  FROM (
                                                      SELECT 
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Lecture' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS lecture_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Seminar' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS seminar_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Lab' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS lab_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Tutorial' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS tutorial_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Other' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS other_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Exam' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS exam_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Admin' THEN ROUND((pa.planned_hours * ta.factor * avg.salary)::numeric, 2) END, 0)) AS admin_cost
                                                  FROM course_instance i
                                                  JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                  JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                  JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                  JOIN average_salary avg ON TRUE
                                                  WHERE i.study_year = @year AND i.instance_id = @id) AS f
                                              )
                                              SELECT fo.course_code, fo.instance_id, fo.study_period, ROUND(pc.total_cost) AS planned_costs, ROUND(fo.total_cost) AS actual_costs
                                              FROM (
                                                  SELECT 
                                                      fi.course_code, fi.instance_id, fi.study_period,
                                                      fi.lecture_cost + fi.seminar_cost + fi.lab_cost + fi.tutorial_cost + fi.other_cost + fi.exam_cost + fi.admin_cost AS total_cost
                                                  FROM (
                                                      SELECT 
                                                          l.course_code, i.instance_id, l.study_period,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Lecture' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS lecture_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Seminar' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS seminar_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Lab' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS lab_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Tutorial' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS tutorial_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Other' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS other_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Exam' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS exam_cost,
                                                          SUM(COALESCE(CASE WHEN ta.activity_name = 'Admin' THEN ROUND((epa.allocated_hours * ta.factor * s.salary)::numeric, 2) END, 0)) AS admin_cost
                                                      FROM course_instance i 
                                                      JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                      JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                      JOIN employee_planned_activity epa ON epa.planned_activity_id = pa.planned_activity_id
                                                      JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                      JOIN employee e ON e.employement_id = epa.employement_id
                                                      JOIN job_title j ON j.job_title_id = e.job_title_id
                                                      JOIN person p ON p.person_id = e.person_id
                                                      JOIN salaries s ON s.employement_id = e.employement_id
                                                      WHERE i.study_year = @year AND i.instance_id = @id
                                                      GROUP BY l.course_code, i.instance_id, l.study_period) AS fi) AS fo
                                              JOIN planned_costs pc ON TRUE 
                                              """;
}