using System.Globalization;
using ConsoleTables;
namespace UniversityDBApp.model;


/* Merges course instance and layout into one class */
public class Course
{
    public int InstanceId { get; }
    public string CourseCode { get; }
    public string CourseName { get; }
    public int NumStudents { get; set; }
    public int StudyYear { get; }
    public string StudyPeriod { get; }
    public float Hp { get; set;  }
    public List<Activity>? TeachingActivities { get; set;  }

    public Course(int instanceId, string courseCode, string courseName, int numStudents, int studyYear, string studyPeriod, float hp)
    {
        this.InstanceId = instanceId;
        this.CourseCode = courseCode;
        this.CourseName = courseName;
        this.NumStudents = numStudents;
        this.StudyYear = studyYear;
        this.StudyPeriod = studyPeriod;
        this.Hp = hp;
        this.TeachingActivities = null;
    }

    public override string ToString()
    {
        var table = new ConsoleTable("instance id", "course code", "course name", "num of students", "study year", "study period", "hp");
        table.AddRow(this.InstanceId, this.CourseCode, this.CourseName, this.NumStudents, this.StudyYear, this.StudyPeriod, this.Hp);
        return table.ToString();
    }

    public void AddRow(ConsoleTable table)
    {
        table.AddRow(this.InstanceId, this.CourseCode, this.CourseName, this.NumStudents, this.StudyYear, this.StudyPeriod, this.Hp);
    }
}

public class TeachingCost
{
    public int InstanceId { get; }
    public string CourseCode { get; }
    public string StudyPeriod { get; }
    public float PlannedCosts { get; }
    public float ActualCosts { get; }

    public TeachingCost(int instanceId, string courseCode, string studyPeriod, float plannedCosts, float actualCosts)
    {
        this.InstanceId = instanceId;
        this.CourseCode = courseCode;
        this.StudyPeriod = studyPeriod;
        this.PlannedCosts = plannedCosts;
        this.ActualCosts = actualCosts;
    }
    
    public override string ToString()
    {
        var table = new ConsoleTable("course code", "instance id", "study period", "planned costs", "actual costs");
        table.AddRow(this.CourseCode, this.InstanceId, this.StudyPeriod, this.PlannedCosts, this.ActualCosts);
        return table.ToString();
    }
}

public class Activity
{
    public int ActivityId { get; }
    public int InstanceId { get; }
    public string ActivityName { get; }
    public float PlannedHours { get; set; }
    public float Factor { get;}

    public Activity(int activityId, int instanceId, string activityName, float plannedHours, float factor)
    {
        this.ActivityId = activityId;
        this.InstanceId =  instanceId;
        this.ActivityName = activityName;
        this.PlannedHours = plannedHours;
        this.Factor = factor;
    }
    
    public void AddRow(ConsoleTable table)
    {
        table.AddRow(this.ActivityId, this.InstanceId, this.ActivityName, this.PlannedHours, this.Factor);
    }
}

public class Teacher
{
    public int EmployementId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string JobTitle { get; set; }
    public int DepartmentId { get; set; }
    public int SupervisorId { get; set; }
    public List<Activity> TeachingActivities { get; }

    public Teacher(int employementId, string firstName, string lastName, string jobTitle, int departmentId, int supervisorId)
    {
        this.EmployementId = employementId;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.JobTitle = jobTitle;
        this.DepartmentId = departmentId;
        this.SupervisorId = supervisorId;
        this.TeachingActivities = new List<Activity>();
    }

    public Activity? GetActivity(string activityName)
    {
        foreach (var activity in this.TeachingActivities) 
        {
            if (activity.ActivityName == activityName) return activity;
        }
        return null;
    }
    
    public override string ToString()
    {
        var table = new ConsoleTable("employement id", "first name", "last name", "job title", "department id", "supervisor id");
        table.AddRow(this.EmployementId, this.FirstName, this.LastName, this.JobTitle, this.DepartmentId, this.SupervisorId);
        return table.ToString();
    }
}