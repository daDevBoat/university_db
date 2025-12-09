using UniversityDBApp.integration;
using UniversityDBApp.model;
using Npgsql;
namespace UniversityDBApp.controller;

public class Controller
{
    private UniversityDAO _uniDb = new UniversityDAO();

    public void Dispose()
    {
        _uniDb.Dispose();
    }

    public Course? FindCourseById(int instanceId)
    {
        try
        {
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            return course;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    public List<Course>? FindCoursesByYear(int year)
    {
        try
        {
            List<Course>? courses = _uniDb.FindCoursesByYear(year);
            return courses;
        } catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    public bool UpdateNumStudentsById(int instanceId, int newNumStudents)
    {
        bool completed = false;
        try
        {
            _uniDb.StartTransaction();
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            
            if (course == null) return completed;
            
            course.NumStudents = newNumStudents;
            completed = _uniDb.UpdateCourseByInstanceId(course);
            _uniDb.CommitTransaction();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            _uniDb.RollBackTransaction();
            Console.WriteLine("Update was rolled back");
        }
        return completed;
    }

    public TeachingCost? CalculateTeachingCost(int instanceId)
    {
        try
        {
            _uniDb.StartTransaction();
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            if (course == null) return null;
            TeachingCost? cost = _uniDb.CalculateTeachingCost(course);
            _uniDb.CommitTransaction();
            return cost;
        }
        catch (NpgsqlException ex)
        {
            _uniDb.RollBackTransaction();
            throw;
        }
    }

    public Teacher? FindTeacher(int employementId)
    {
        try
        {
            Teacher? teacher = _uniDb.FindTeacherByEmployementId(employementId);
            return teacher;
        } catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    public List<Activity>? FindActivitiesByInstanceId(int instanceId)
    {
        try
        {
            _uniDb.StartTransaction();
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            if (course == null) return null;
            List<Activity>? activities = _uniDb.FindActivitiesByCourse(course);
            _uniDb.CommitTransaction();
            return activities;
        }
        catch (NpgsqlException ex)
        {
            return null;
            throw;
        }
    }
    
    public List<Activity>? FindActivitiesByEmployementId(int employementId)
    {
        try
        {
            _uniDb.StartTransaction();
            Teacher? teacher = _uniDb.FindTeacherByEmployementId(employementId);
            if (teacher == null) return null;
            List<Activity>? activities = _uniDb.FindActivitiesByTeacher(teacher);
            _uniDb.CommitTransaction();
            return activities;
        }
        catch (NpgsqlException ex)
        {
            return null;
            throw;
        }
    }

    public void DeleteEPA(int employementId, int activityId)
    {
        try
        {
            _uniDb.DeleteEmployeePlannedActivity(employementId, activityId);
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }
    public void CreateEPA(int employementId, int activityId, float allocatedHours)
    {
        try
        {
            _uniDb.CreateEmployeePlannedActivity(employementId, activityId, allocatedHours);
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
            _uniDb.CreateActivityType(activityName, factor);
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
            _uniDb.CreateActivity(activityName, instanceId, plannedHours);
        }
        catch (NpgsqlException ex)
        {
            throw;
        }
    }
}

