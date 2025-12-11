using UniversityDBApp.integration;
using UniversityDBApp.model;
using Npgsql;
using System.Data;
namespace UniversityDBApp.controller;

public class Controller
{
    private UniversityDAO _uniDb { get;}

    public Controller()
    {
        try
        {
            _uniDb = new UniversityDAO();
        }
        catch (Exception)
        {
            throw;
        }
    }

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
        catch (Exception)
        {
            throw;
        }
    }

    public List<Course>? FindCoursesByYear(int year)
    {
        try
        {
            List<Course>? courses = _uniDb.FindCoursesByYear(year);
            return courses;
        } 
        catch (Exception)
        {
            throw;
        }        
    }

    public int? UpdateNumStudentsById(int instanceId, int newNumStudents)
    {

        try
        {
            _uniDb.StartTransaction(IsolationLevel.Serializable);
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);

            if (course == null) throw new SelectForUpdateIsNullException();

            course.NumStudents = newNumStudents;
            int? rowsAffected = _uniDb.UpdateCourseByInstanceId(course);
            _uniDb.CommitTransaction();
            return rowsAffected;
        }
        catch (Exception e)
        {
            _uniDb.RollBackTransaction();
            throw new DBUpdateFailedException(e.Message);
        }
    }

    public TeachingCost? CalculateTeachingCost(int instanceId)
    {
        try
        {
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            if (course == null) return null;
            TeachingCost? cost = _uniDb.FindTeachingCost(course);
            return cost;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Teacher? FindTeacherById(int employementId)
    {
        try
        {
            Teacher? teacher = _uniDb.FindTeacherByEmployementId(employementId);
            return teacher;
        } 
        catch (Exception)
        {
            throw;
        }
    }

    public List<Activity>? FindActivitiesByInstanceId(int instanceId)
    {
        try
        {
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            if (course == null) return null;
            List<Activity>? activities = _uniDb.FindActivitiesByCourse(course);
            return activities;
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    public List<Activity>? FindActivitiesByEmployementId(int employementId)
    {
        try
        {
            Teacher? teacher = _uniDb.FindTeacherByEmployementId(employementId);
            if (teacher == null) return null;
            List<Activity>? activities = _uniDb.FindActivitiesByTeacher(teacher);
            return activities;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public int? DeleteEPA(int employementId, int activityId)
    {
        try
        {
            int? rowsAffected = _uniDb.DeleteEmployeePlannedActivity(employementId, activityId);
            return rowsAffected;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public int? CreateEPA(int employementId, int activityId, float allocatedHours)
    {
        try
        {
            int? rowsAffected = _uniDb.CreateEmployeePlannedActivity(employementId, activityId, allocatedHours);
            return rowsAffected;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public int? CreateActivityType(string activityName, float factor)
    {
        try
        {
            int? rowsAffected = _uniDb.CreateActivityType(activityName, factor);
            return rowsAffected;
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    public int? CreateActivity(string activityName, int instanceId, float plannedHours)
    {
        try
        {
            int? rowsAffected = _uniDb.CreateActivity(activityName, instanceId, plannedHours);
            return rowsAffected;
        }
        catch (Exception)
        {
            throw;
        }
    }
}

