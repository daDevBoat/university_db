using UniversityDBApp.model;
using ConsoleTables;
namespace UniversityDBApp.view;

static class Display
{
    public static void Course(Course course)
    {
        Console.WriteLine(course.ToString());
    }
    
    public static void Courses(List<Course> courses)
    {
        var table = new ConsoleTable("instance id", "course code", "course name", "num of students", "study year", "study period", "hp");
        foreach (var course in courses)
        {
            course.AddRow(table);
        }
        table.Write();
    }
    
    public static void TeachingCost(TeachingCost cost)
    {
        Console.WriteLine(cost.ToString());
    }

    public static void Teacher(Teacher teacher)
    {
        Console.WriteLine(teacher.ToString());
    }
    
    public static void Activities(List<Activity> activities)
    {
        var table = new ConsoleTable("planned activity id", "activity name", "planned hours (no factor)", "factor");
        foreach (var activity in activities)
        {
            activity.AddRow(table);
        }
        table.Write();
        
    }
}