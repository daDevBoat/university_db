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

            if (command == null) continue;
            
            if (command == "exit" || command == "quit" || command == "q")
            {
                Console.WriteLine("Exiting...");
                commandController.Dispose();
                return;
            }

            if (command == "find" && args?.Length > 2)
            {
                if (args[0] == "teaching_activity")
                {
                    Display.TeachingActivities(commandController.FindAllTeachingActivities());
                    continue;
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

                        if (course == null)
                        {
                            Console.WriteLine($"No course found with instance ID: {instanceId}");
                            continue;
                        }
                        
                        Display.Course(course);
                        continue;
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

                        if (courses == null || courses.Count == 0)
                        {
                            Console.WriteLine($"No courses found for the year: {year}");
                            continue;
                        }
                        
                        Display.Courses(courses);
                        continue;
                    }
                }
            }

            if (command == "update" && args?.Length > 2)
            {
                if (args[0] == "course")
                {
                    int instanceId;
                    if (!int.TryParse(args[1], out instanceId))
                    {
                        Console.WriteLine("Write an integer for the instance ID");
                        continue;
                    }

                    if (args[2] == "num_students")
                    {
                        int numStudents;
                        if (!int.TryParse(args[3], out numStudents))
                        {
                            Console.WriteLine("Write an integer for the number of students");
                            continue;
                        }

                        if (!commandController.UpdateNumStudentsById(instanceId, numStudents))
                        {
                            Console.WriteLine("Updated failed");
                        }
                        continue;
                    }
                }
            }
            
            Console.WriteLine("Write a valid command. To see a list of valid commands write: help");
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
            course.AddRow(table);
        }
        table.Write();
    }
}