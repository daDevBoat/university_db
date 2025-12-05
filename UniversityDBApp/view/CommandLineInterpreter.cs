using UniversityDBApp.controller;
using UniversityDBApp.model;

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

            if (command == "find" && args?.Length == 2)
            {
                Console.WriteLine(args[0],  args[1]);
                if (args[0] == "teaching_activity")
                {
                    Display.TeachingActivities(commandController.FindAllTeachingActivities());
                }

                if (args[0] == "course")
                {
                    if (args[1] == "id")
                    {
                        int instanceId;
                        if (!int.TryParse(args[1], out instanceId))
                        {
                            Console.WriteLine("Write an integer for the instance ID");
                            continue;
                        }
                        
                        Course? course = commandController.FindCourse(instanceId);

                        if (course == null)
                        {
                            Console.WriteLine("No course with that instance id exists");
                            continue;
                        }

                        Display.Course(course);
                    }

                    if (args[1] == "year")
                    {
                        
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
}