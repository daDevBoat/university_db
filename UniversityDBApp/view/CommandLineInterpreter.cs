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

            if (command == "find" && args?.Length > 1)
            {

                if (args[0] == "course" && args?.Length > 2)
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

                    if (args[1] == "year" && args?.Length > 2)
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

                if (args[0] == "cost" && args?.Length > 1)
                {
                    //Console.WriteLine("Hei");
                    int instanceId;
                    if (!int.TryParse(args[1], out instanceId))
                    {
                        Console.WriteLine("Write an integer for the instance ID");
                        continue;
                    }

                    TeachingCost? cost = commandController.CalculateTeachingCost(instanceId);
                    if (cost == null)
                    {
                        Console.WriteLine($"No courses found to calculate cost with id: {instanceId}");
                        continue;
                    }
                    Display.TeachingCost(cost);
                    continue;
                }

                if (args[0] == "teacher" && args?.Length > 1)
                {
                    int employementID;
                    if (!int.TryParse(args[1], out employementID))
                    {
                        Console.WriteLine("Write an integer for the employement ID");
                        continue;
                    }
                    Teacher? teacher = commandController.FindTeacher(employementID);
                    if (teacher == null)
                    {
                        Console.WriteLine("No teacher found");
                        continue;
                    }
                    Display.Teacher(teacher);
                    continue;
                }
                
                if (args[0] == "activities" && args?.Length > 2)
                {
                    int id;
                    if (!int.TryParse(args[2], out id))
                    {
                        Console.WriteLine("Write an integer for the ID");
                        continue;
                    }

                    List<Activity>? activities = null;
                    if (args[1] == "teacher") activities = commandController.FindActivitiesByEmployementId(id);
                    else if (args[1] == "course") activities = commandController.FindActivitiesByInstanceId(id);
                    
                    if (activities ==  null) 
                    {
                        Console.WriteLine("No activities found");
                        continue;
                    }
                    Display.Activities(activities);
                    continue;
                    
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

