using UniversityDBApp.controller;
using UniversityDBApp.model;
using ConsoleTables;
namespace UniversityDBApp.view;

public static class CommandLineInterpreter
{
    public static void Run()
    {
        Console.WriteLine("This is the terminal for the University DB Application");
        Console.WriteLine("Write your commands here\n");
        
        Controller commandController = new Controller();

        while (true)
        {
            Console.Write("> ");
            string[]? input = Console.ReadLine()?.Split(" ");
            string? command = input?[0];
            string[]? args = input?.Skip(1).ToArray();

            if (command == null)
            {
                Console.WriteLine("Write in a command");
                continue;
            }
            
            if (command == "exit" || command == "quit" || command == "q")
            {
                Console.WriteLine("Exiting...");
                break;
            }

            if (command == "find" && args?.Length > 2)
            {
                if (args[0] == "teaching_activity")
                {
                    Display.TeachingActivities(commandController.FindAllTeachingActivities());
                }

                if (args[0] == "course")
                {
                    if (args[1] == "id")
                    {
                        int instanceId;
                        if (!int.TryParse(args[2], out instanceId))
                        {
                            Console.WriteLine("Write an integer for the instance ID");
                            continue;
                        }
                        Course? course = commandController.FindCourseById(instanceId);

                        if (course == null) continue;
                        
                        Display.Course(course);
                    }

                    if (args[1] == "year")
                    {
                        int year;
                        if (!int.TryParse(args[2], out year))
                        {
                            Console.WriteLine("Write an integer for the year");
                            continue;
                        }
                        List<Course>? courses = commandController.FindCoursesByYear(year);

                        if (courses == null || courses.Count == 0) continue;
                        
                        Display.Courses(courses);
                    }
                }
            }
            
        }
    }
}

static class Display
{
    public static void TeachingActivities(List<TeachingActivity> activities)
    {
        foreach (var activity in activities)
        {
            Console.WriteLine(activity.ToString());
        }
    }

    public static void Course(Course course)
    {
        Console.WriteLine(course.ToString());
    }
    
    public static void Courses(List<Course> courses)
    {
        var table = new ConsoleTable("instance id", "course code", "course name", "num of students", "study year", "study period", "hp");
        foreach (var course in courses)
        {
            course.AddRow(ref table);
        }
        table.Write();
    }
}