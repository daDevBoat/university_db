using Npgsql;
using UniversityDBApp.model;
using UniversityDBApp.controller;
using UniversityDBApp.integration;

namespace UniversityDBApp.view;

public static class CommandHandler
{
    private static Controller _cmdController { get; }

    static CommandHandler()
    {
        try
        {
            _cmdController = new Controller();
            Console.WriteLine("Connection to database was sucessfully etstablished.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("FATAL ERROR: " + ex.Message);
            Environment.Exit(1);
        }
    }
    
    /* The public methods are the L1 handlers used by CommandLineInterpreter */
    public static void Help(string? helpType)
    {
        if (helpType == null)
        {
            Console.WriteLine("Write help <command> to get help for that command. Available commands:");
            Console.WriteLine("[exit, find, update, allocate, deallocate, create]\n");
            return;
        }

        switch (helpType)
        {
            case "exit":
                Console.WriteLine("Type exit to exit CLI.");
                break;
            case "find":
                Console.WriteLine("Available find commands");
                Console.WriteLine("find course id <courseId>");
                Console.WriteLine("find courses year <year>");
                Console.WriteLine("find cost course <courseId>");
                Console.WriteLine("find teacher id <teacherId>");
                Console.WriteLine("find activities teacher|course <teacherId|courseId>");
                break;
            case "update":
                Console.WriteLine("Available update commands");
                Console.WriteLine("update course <courseId> num_students <int>");
                break;
            case "allocate":
                Console.WriteLine("Available allocate commands");
                Console.WriteLine("allocate teacher <teacherId> activity <activityId> hours <int>");
                break;
            case "deallocate":
                Console.WriteLine("Available deallocate commands");
                Console.WriteLine("deallocate teacher <teacherId> activity <activityId>");
                break;
            case "create":
                Console.WriteLine("Available create commands");
                Console.WriteLine("create activity_type name <string> factor <float>");
                Console.WriteLine("create activity name <string> course <courseId> hours <int>");
                break;
            default:
                Console.WriteLine("Write a valid help command. To see a list of valid commands write: help");
                break;
        }
    }
    
    public static void Exit()
    {
        try
        {
            _cmdController.Dispose();
            Console.WriteLine("Exiting...");
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public static void Find(string[] args)
    {
        string findType = args[0];
        switch (findType)
        {
            case "course" when args[1] == "id":
                FindCourseByIdHandler(args[2]);
                break;
            case "courses" when args[1] == "year":
                FindCoursesByYearHandler(args[2]);
                break;
            case "cost" when args[1] == "course": 
                FindTeachingCostByIdHandler(args[2]);
                break;
            case "teacher" when args[1] == "id":
                FindTeacherByIdHandler(args[2]);
                break;
            case "activities" when args[1] == "teacher":
                FindActivityByIdHandler(args[2], "teacherId");
                break;
            case "activities" when args[1] == "course":
                FindActivityByIdHandler(args[2], "courseId");
                break;
            default:
                Console.WriteLine("Write a valid find command. To see a list of valid commands write: help");
                break;
        }
    }
    
    public static void Update(string[] args)
    {
        // Switch case was used as more update functionality would be a future possibility
        string updateType = args[0];
        switch (updateType)
        {
            case "course" when args[2] == "num_students":
                UpdateCourseNumStudentsHandler(args[1], args[3]);
                break;
            default:
                Console.WriteLine("Write a valid update command. To see a list of valid commands write: help");
                break;
        }
    }

    public static void Allocate(string[] args)
    {
        // If else statement as there should be only one type of allocation
        if (args[0] == "teacher" && args[2] == "activity" && args[4] == "hours") AllocateHandler(args[1], args[3], args[5]);
        else Console.WriteLine("Write a valid allocate command. To see a list of valid commands write: help");
    }
    
    public static void DeAllocate(string[] args)
    {
        // If else statement as there should be only one type of allocation
        if (args[0] == "teacher" && args[2] == "activity") DeAllocateHandler(args[1], args[3]);
        else Console.WriteLine("Write a valid allocate command. To see a list of valid commands write: help");
    }

    public static void Create(string[] args)
    {
        string createType = args[0];
        switch (createType)
        {
            case "activity_type" when args[1] == "name" && args[3] == "factor":
                CreateActivityTypeHandler(args[2], args[4]);
                break;
            case "activity" when args is [_, "name", _, "course", _, "hours", _]:   // can be done for the others as well
                CreateActivityHandler(args[2], args[4], args[6]);
                break;
            default:
                Console.WriteLine("Write a valid create command. To see a list of valid commands write: help");
                break;
        }
    }
    
    /*
     * The private methods are used by the CommandHandler to handle:
     * the different methods needed to be called due to different command inputs,
     * and potential exceptions
     */

    /* FindHandlers */
    private static int? ParseInteger(string argInt, string argName)
    {
        if (!int.TryParse(argInt, out var integer))
        {
            Console.WriteLine($"{argName} needs to be an integer");
            return null;
        }
        return integer;
    }
    
    private static float? ParseFloat(string argFloat, string argName)
    {
        if (!float.TryParse(argFloat, out var floatNumber))
        {
            Console.WriteLine($"{argName} needs to be a float");
            return null;
        }
        return floatNumber;
    }
    
    private static void FindCourseByIdHandler(string argId)
    {
        int? courseId = ParseInteger(argId, "id");
        if (courseId == null) return;

        try
        {
            Course? course = _cmdController.FindCourseById((int) courseId);
            if (course != null) Display.Course(course);
            else Console.WriteLine($"No course found with instance ID: {courseId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void FindCoursesByYearHandler(string argYear)
    {
        int? year = ParseInteger(argYear, "year");
        if (year == null) return;
        try
        {
            List<Course>? courses = _cmdController.FindCoursesByYear((int) year);
            if (courses != null) Display.Courses(courses);
            else Console.WriteLine($"No courses found for the year: {year}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void FindTeachingCostByIdHandler(string argId)
    {
        int? courseId = ParseInteger(argId, "id");
        if (courseId == null) return;
        try
        {
            TeachingCost? cost = _cmdController.CalculateTeachingCost((int) courseId);
            if (cost != null) Display.TeachingCost(cost);
            else Console.WriteLine($"No course instance found with instance ID: {courseId} to calculate cost: {cost}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void FindTeacherByIdHandler(string argId)
    {
        int? teacherId = ParseInteger(argId, "id");
        if (teacherId == null) return;
        try
        {
            Teacher? teacher = _cmdController.FindTeacherById((int) teacherId);
            if (teacher != null) Display.Teacher(teacher);
            else Console.WriteLine($"No teacher found with employement ID: {teacherId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void FindActivityByIdHandler(string argId, string idType)
    {
        int? id = ParseInteger(argId, "id");
        if (id == null) return;
        try
        {
            List<Activity>? activities = null;
            if (idType == "courseId") activities = _cmdController.FindActivitiesByInstanceId((int)id);
            if (idType == "teacherId") activities = _cmdController.FindActivitiesByEmployementId((int)id);
            
            if (activities != null) Display.Activities(activities, idType == "teacherId");
            else Console.WriteLine($"No activities found with {idType}: {id}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    
    private static void UpdateCourseNumStudentsHandler(string argId, string argNumStudents)
    {
        int? id = ParseInteger(argId, "id");
        int? numStudents = ParseInteger(argNumStudents, "num_students");
        if (id == null || numStudents == null) return;
        try
        {
            int? rowsAffected = _cmdController.UpdateNumStudentsById((int) id, (int) numStudents);
            if (rowsAffected != null) Console.WriteLine($"{rowsAffected} row(s) updated.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
   
    private static void AllocateHandler(string argTeacherId, string argActivityId, string argHours)
    {
        int? teacherId = ParseInteger(argTeacherId, "teacher");
        int? activityId = ParseInteger(argActivityId, "activity");
        float? hours = ParseFloat(argHours, "hours");
        if (teacherId == null || activityId == null || hours == null) return;
        try
        {
            int? rowsAffected = _cmdController.CreateEPA((int)teacherId, (int)activityId, (int)hours);
            if (rowsAffected > 0) Console.WriteLine("Allocation was successful");
            else Console.WriteLine("Allocation was unsuccessful");
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.SqlState == "23505" ? "Teacher already allocated to activity" : e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void DeAllocateHandler(string argTeacherId, string argActivityId)
    {
        int? teacherId = ParseInteger(argTeacherId, "teacher");
        int? activityId = ParseInteger(argActivityId, "activity");
        if (teacherId == null || activityId == null) return;
        try
        {
            int? rowsAffected = _cmdController.DeleteEPA((int)teacherId, (int)activityId);
            if (rowsAffected > 0) Console.WriteLine("Deallocation was successful");
            else Console.WriteLine("Deallocation was unsuccessful");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void CreateActivityTypeHandler(string argName, string argFactor)
    {
        float? factor = ParseFloat(argFactor, "factor");
        if (factor == null || argName.Length == 0) return;
        try
        {
            int? rowsAffected = _cmdController.CreateActivityType(argName, (float) factor);
            if (rowsAffected > 0) Console.WriteLine("Creation was successful");
            else Console.WriteLine("Creation was unsuccessful");
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.SqlState == "23505" ? "Activity type with that name already exists" : e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void CreateActivityHandler(string argName, string argId, string argHours)
    {
        int? courseId = ParseInteger(argId, "id");
        float?  hours = ParseFloat(argHours, "hours");
        if (courseId == null || hours == null || argName.Length == 0) return;
        try
        {
            int? rowsAffected = _cmdController.CreateActivity(argName, (int) courseId, (float) hours);
            if (rowsAffected > 0) Console.WriteLine("Creation was successful");
            else Console.WriteLine("Creation was unsuccessful");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}