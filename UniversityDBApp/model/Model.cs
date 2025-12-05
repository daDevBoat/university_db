using System.Globalization;
using ConsoleTables;
namespace UniversityDBApp.model;

public class TeachingActivity
{
    public int TeachingActivityId { get; }
    public string TeachingActivityName { get; }
    public float Factor { get; }

    public TeachingActivity(int teachingActivityId, string teachingActivityName, float factor)
    {
        this.TeachingActivityId = teachingActivityId;
        this.TeachingActivityName = teachingActivityName;
        this.Factor = factor;
    }

    public override string ToString()
    {
        return $"""
                {this.TeachingActivityId.ToString().PadRight(5)}| 
                {this.TeachingActivityName.PadRight(20)}| 
                {this.Factor.ToString(CultureInfo.InvariantCulture)}
                """;
    }
}

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

    public Course(int instanceId, string courseCode, string courseName, int numStudents, int studyYear, string studyPeriod, float hp)
    {
        this.InstanceId = instanceId;
        this.CourseCode = courseCode;
        this.CourseName = courseName;
        this.NumStudents = numStudents;
        this.StudyYear = studyYear;
        this.StudyPeriod = studyPeriod;
        this.Hp = hp;
    }

    public override string ToString()
    {
        var table = new ConsoleTable("instance id", "course code", "course name", "num of students", "study year", "study period", "hp");
        table.AddRow(this.InstanceId, this.CourseCode, this.CourseName, this.NumStudents, this.StudyYear, this.StudyPeriod, this.Hp);
        return table.ToString();
    }

    public void AddRow(ref ConsoleTable table)
    {
        table.AddRow(this.InstanceId, this.CourseCode, this.CourseName, this.NumStudents, this.StudyYear, this.StudyPeriod, this.Hp);
    }
}