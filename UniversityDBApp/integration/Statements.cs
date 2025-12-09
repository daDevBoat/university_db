namespace UniversityDBApp.integration;

static class Statements
{ 
    public const string FindCourseInstanceById = """
                                                 SELECT * 
                                                 FROM course_instance i 
                                                 JOIN course_layout l ON l.course_layout_id = i.course_layout_id 
                                                 WHERE instance_id = @id
                                                 """;
    public const string FindCourseInstanceByIdForUpdate = """
                                                 SELECT * 
                                                 FROM course_instance i 
                                                 JOIN course_layout l ON l.course_layout_id = i.course_layout_id 
                                                 WHERE instance_id = @id FOR UPDATE
                                                 """;
    public const string FindCourseInstanceByYear = """
                                                   SELECT * 
                                                   FROM course_instance i 
                                                   JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                   WHERE study_year = @year
                                                   ORDER BY l.study_period ASC
                                                   """;
    public const string UpdateNumStudentsById = """
                                                UPDATE course_instance
                                                SET
                                                    num_students = @num_students
                                                WHERE instance_id = @id
                                                """;
    public const string FindActivitiesByInstanceId = """
                                                     SELECT pa.planned_activity_id, pa.instance_id, ta.activity_name, pa.planned_hours, ta.factor
                                                     FROM course_instance i
                                                     JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                     JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                     WHERE i.instance_id = @id
                                                     """;
    public const string FindActivitiesByInstanceIdForUpdate = """
                                                     SELECT pa.planned_activity_id, pa.instance_id, ta.activity_name, pa.planned_hours, ta.factor
                                                     FROM course_instance i
                                                     JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                     JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                     WHERE i.instance_id = @id FOR UPDATE
                                                     """;
    public const string FindActivitiesByEmployementId = """
                                                     SELECT pa.planned_activity_id, pa.instance_id, ta.activity_name, epa.allocated_hours, ta.factor
                                                     FROM employee e
                                                     JOIN employee_planned_activity epa ON epa.employement_id = e.employement_id
                                                     JOIN planned_activity pa ON pa.planned_activity_id = epa.planned_activity_id
                                                     JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                     WHERE e.employement_id = @id
                                                     ORDER BY pa.instance_id ASC
                                                     """;
    public const string FindActivitiesByEmployementIdForUpdate = """
                                                        SELECT pa.planned_activity_id, pa.instance_id, ta.activity_name, epa.allocated_hours, ta.factor
                                                        FROM employee e
                                                        JOIN employee_planned_activity epa ON epa.employement_id = e.employement_id
                                                        JOIN planned_activity pa ON pa.planned_activity_id = epa.planned_activity_id
                                                        JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                        WHERE e.employement_id = @id FOR UPDATE
                                                        """;
    public const string FindTeacherByEmployementId = """
                                                     SELECT e.employement_id, p.first_name, p.last_name, j.job_title, e.department_id, e.supervisor_id
                                                     FROM employee e
                                                     JOIN person p ON p.person_id = e.person_id
                                                     JOIN job_title j ON j.job_title_id = e.job_title_id
                                                     WHERE e.employement_id = @id
                                                     """;
    public const string FindTeacherByEmployementIdForUpdate = """
                                                     SELECT e.employement_id, p.first_name, p.last_name, j.job_title, e.department_id, e.supervisor_id
                                                     FROM employee e
                                                     JOIN person p ON p.person_id = e.person_id
                                                     JOIN job_title j ON j.job_title_id = e.job_title_id
                                                     WHERE e.employement_id = @id FOR UPDATE
                                                     """;

    public const string DeleteEmployeePlannedActivityById = """
                                                            DELETE FROM employee_planned_activity
                                                            WHERE employement_id = @employement_id AND planned_activity_id = @planned_activity_id
                                                            """;
    public const string CreateEmployeePlannedActivityById = """
                                                            INSERT INTO employee_planned_activity(employement_id, planned_activity_id, allocated_hours)
                                                            VALUES (@employement_id, @planned_activity_id, @allocated_hours)
                                                            """;
    public const string CreateActivityType = """
                                            INSERT INTO teaching_activity(activity_name, factor)
                                            VALUES (@activity_name, @factor)
                                            """;
    public const string CreateActivity = """
                                         INSERT INTO planned_activity(instance_id, teaching_activity_id, planned_hours)
                                         SELECT @instance_id, ta.teaching_activity_id, @planned_hours
                                         FROM teaching_activity ta
                                         WHERE ta.activity_name = @activity_name
                                         """;
    public const string CostCalculationById = """
                                                WITH salaries AS (
                                                    SELECT DISTINCT ON (s.employement_id)
                                                    s.employement_id, s.year, s.period, s.salary
                                                    FROM salary_history s 
                                                    WHERE 
                                                        CASE 
                                                            WHEN s.year = @year THEN s.period <= @period ::period_enum
                                                            WHEN s.year < @year THEN TRUE
                                                            ELSE FALSE
                                                        END
                                                    ORDER BY s.employement_id ASC, s.year DESC, s.period DESC
                                                ), 
                                                average_salary AS (
                                                    SELECT AVG(salaries.salary) AS salary FROM salaries
                                                ),
                                                planned_costs AS (
                                                    SELECT SUM(ROUND(COALESCE(pa.planned_hours * ta.factor * avg.salary, 0))) as total_cost
                                                    FROM course_instance i
                                                    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                    JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                    JOIN average_salary avg ON TRUE
                                                    WHERE i.study_year = @year AND i.instance_id = @id
                                                )
                                                SELECT f.course_code, f.instance_id, f.study_period, pc.total_cost AS planned_costs, f.total_cost AS actual_costs
                                                FROM (
                                                    SELECT 
                                                        l.course_code, i.instance_id, l.study_period, 
                                                        SUM(ROUND(COALESCE(epa.allocated_hours * ta.factor * s.salary, 0))) as total_cost
                                                    FROM course_instance i 
                                                    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
                                                    JOIN planned_activity pa ON pa.instance_id = i.instance_id
                                                    JOIN employee_planned_activity epa ON epa.planned_activity_id = pa.planned_activity_id
                                                    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
                                                    JOIN employee e ON e.employement_id = epa.employement_id
                                                    JOIN job_title j ON j.job_title_id = e.job_title_id
                                                    JOIN person p ON p.person_id = e.person_id
                                                    JOIN salaries s ON s.employement_id = e.employement_id
                                                    WHERE i.study_year = @year AND i.instance_id = @id
                                                    GROUP BY l.course_code, i.instance_id, l.study_period) AS f
                                                JOIN planned_costs pc ON TRUE
                                                """;
}

