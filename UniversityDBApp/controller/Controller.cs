using UniversityDBApp.integration;
using UniversityDBApp.model;
using Npgsql;
namespace UniversityDBApp.controller;

public class Controller
{
    private UniversityDAO _uniDb = new UniversityDAO();
    public Controller()
    {
        
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
}

