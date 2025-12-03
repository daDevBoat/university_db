using UniversityDBApp.integration;
using Npgsql;
namespace UniversityDBApp.controller;

public class Controller
{
    private UniversityDb UniDb = new UniversityDb();
    public Controller()
    {
        
    }
}


class UniversityDb
{
    private UniversityDAO dao;
    public UniversityDb()
    {
        dao = new UniversityDAO();
    }
}