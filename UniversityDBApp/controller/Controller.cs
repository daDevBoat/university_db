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

    public void UpdateNumStudentsById(int instanceId, int newNumStudents)
    {

        try
        {
            _uniDb.StartTransaction();
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);

            if (course == null) throw new SelectForUpdateIsNullException();

            course.NumStudents = newNumStudents;
            _uniDb.UpdateCourseByInstanceId(course);
            _uniDb.CommitTransaction();
        }
        catch (Exception e)
        {
            _uniDb.RollBackTransaction();
            throw new DBUpdateFailedException(e);
        }
    }

    public TeachingCost? CalculateTeachingCost(int instanceId)
    {
        try
        {
            Course? course = _uniDb.FindCourseByInstanceId(instanceId);
            if (course == null) return null;
            TeachingCost? cost = _uniDb.CalculateTeachingCost(course);
            return cost;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Teacher? FindTeacher(int employementId)
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

    public void DeleteEPA(int employementId, int activityId)
    {
        try
        {
            _uniDb.DeleteEmployeePlannedActivity(employementId, activityId);
        }
        catch (Exception)
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
        catch (Exception)
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
        catch (Exception)
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
        catch (Exception)
        {
            throw;
        }
    }
}

