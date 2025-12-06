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

    public List<TeachingActivity> FindAllTeachingActivities()
    {
        List<TeachingActivity> activities = new List<TeachingActivity>();
        try
        {
            activities = _uniDb.FindAllTeachingActivities();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        return activities;
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
}

