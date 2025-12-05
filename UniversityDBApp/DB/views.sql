
/* The view for the first query for the higher grade part */
CREATE OR REPLACE VIEW planned_hours_per_course_instance AS
SELECT 
    f.course_code, f.instance_id, f.hp, f.study_period, f.study_year, f.num_students, f.lecture_hours, f.seminar_hours, f.lab_hours, f.tutorial_hours, f.other_hours, f.exam_hours, f.admin_hours,
    f.lecture_hours + f.seminar_hours + f.lab_hours + f.tutorial_hours + f.other_hours + f.exam_hours + f.admin_hours AS total_hours
FROM (
    SELECT 
        l.course_code, i.instance_id, l.hp, l.study_period, i.study_year, i.num_students,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lecture' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS lecture_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Seminar' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS seminar_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lab' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS lab_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Tutorial' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS tutorial_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Other' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS other_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Exam' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS exam_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Admin' THEN ROUND((pa.planned_hours * ta.factor)::numeric, 2) END, 0)) AS admin_hours
    FROM course_instance i
    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
    JOIN planned_activity pa ON pa.instance_id = i.instance_id
    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
    WHERE i.study_year = EXTRACT(YEAR FROM CURRENT_DATE)::int
    GROUP BY l.course_code, i.instance_id, l.hp, l.study_period, i.study_year, i.num_students) AS f
ORDER BY f.study_period;


/* The materialized view for the second and third query for the higher grade*/
DROP MATERIALIZED VIEW IF EXISTS allocated_hours_per_teacher;
CREATE MATERIALIZED VIEW allocated_hours_per_teacher AS
SELECT 
    f.course_code, f.instance_id, f.hp, f.study_period, f.teacher_name, f.job_title, f.lecture_hours, f.seminar_hours, f.lab_hours, f.tutorial_hours, f.other_hours, f.exam_hours, f.admin_hours,
    f.lecture_hours + f.seminar_hours + f.lab_hours + f.tutorial_hours + f.other_hours + f.exam_hours + f.admin_hours AS total_hours
FROM (
    SELECT 
        l.course_code, i.instance_id, l.hp, l.study_period, p.first_name || ' ' || p.last_name AS teacher_name, j.job_title,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lecture' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS lecture_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Seminar' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS seminar_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lab' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS lab_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Tutorial' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS tutorial_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Other' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS other_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Exam' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS exam_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Admin' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS admin_hours
    FROM course_instance i 
    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
    JOIN planned_activity pa ON pa.instance_id = i.instance_id
    JOIN employee_planned_activity epa ON epa.planned_activity_id = pa.planned_activity_id
    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
    JOIN employee e ON e.employement_id = epa.employement_id
    JOIN job_title j ON j.job_title_id = e.job_title_id
    JOIN person p ON p.person_id = e.person_id
    WHERE i.study_year = EXTRACT(YEAR FROM CURRENT_DATE)::int
    GROUP BY l.course_code, i.instance_id, l.hp, l.study_period, teacher_name, j.job_title) AS f
ORDER BY f.study_period ASC;

/* Same as above, just as a view */
CREATE OR REPLACE VIEW allocated_hours_per_teacher_view AS
SELECT 
    f.course_code, f.instance_id, f.hp, f.study_period, f.teacher_name, f.job_title, f.lecture_hours, f.seminar_hours, f.lab_hours, f.tutorial_hours, f.other_hours, f.exam_hours, f.admin_hours,
    f.lecture_hours + f.seminar_hours + f.lab_hours + f.tutorial_hours + f.other_hours + f.exam_hours + f.admin_hours AS total_hours
FROM (
    SELECT 
        l.course_code, i.instance_id, l.hp, l.study_period, p.first_name || ' ' || p.last_name AS teacher_name, j.job_title,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lecture' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS lecture_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Seminar' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS seminar_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Lab' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS lab_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Tutorial' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS tutorial_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Other' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS other_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Exam' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS exam_hours,
        SUM(COALESCE(CASE WHEN ta.activity_name = 'Admin' THEN ROUND((epa.allocated_hours * ta.factor)::numeric, 2) END, 0)) AS admin_hours
    FROM course_instance i 
    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
    JOIN planned_activity pa ON pa.instance_id = i.instance_id
    JOIN employee_planned_activity epa ON epa.planned_activity_id = pa.planned_activity_id
    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
    JOIN employee e ON e.employement_id = epa.employement_id
    JOIN job_title j ON j.job_title_id = e.job_title_id
    JOIN person p ON p.person_id = e.person_id
    WHERE i.study_year = EXTRACT(YEAR FROM CURRENT_DATE)::int
    GROUP BY l.course_code, i.instance_id, l.hp, l.study_period, teacher_name, j.job_title) AS f
ORDER BY f.study_period ASC;

/* The view for the fourth query for the higher grade part */
CREATE OR REPLACE VIEW planned_hours_vs_allocated_hours_per_instance AS
SELECT f.course_code, f.instance_id, f.total_planned_hours, f.total_allocated_hours, ROUND((ABS(f.total_planned_hours - f.total_allocated_hours) / f.total_planned_hours)::numeric, 4) AS variance
FROM (
    SELECT l.course_code, i.instance_id, ROUND(SUM(DISTINCT pa.planned_hours * ta.factor)::numeric, 4) AS total_planned_hours, ROUND(SUM(epa.allocated_hours * ta.factor)::numeric, 4) AS total_allocated_hours
    FROM course_instance i
    JOIN planned_activity pa ON pa.instance_id = i.instance_id
    JOIN employee_planned_activity epa ON epa.planned_activity_id = pa.planned_activity_id
    JOIN teaching_activity ta ON ta.teaching_activity_id = pa.teaching_activity_id
    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
    GROUP BY l.course_code, i.instance_id
    ORDER BY i.instance_id) AS f
WHERE ROUND((ABS(f.total_planned_hours - f.total_allocated_hours) / f.total_planned_hours)::numeric, 4) > 0.15;



/* The materialized view for the fifth query for the higher grade part */

DROP MATERIALIZED VIEW IF EXISTS teachers_allocated_more_than_N_courses;
CREATE MATERIALIZED VIEW teachers_allocated_more_than_N_courses AS
SELECT i.instance_id, e.employement_id, p.first_name || ' ' || p.last_name AS teacher_name, l.study_period
FROM employee e
JOIN person p ON p.person_id = e.person_id
JOIN job_title j ON j.job_title_id = e.job_title_id
JOIN employee_planned_activity epa ON epa.employement_id = e.employement_id
JOIN planned_activity pa ON pa.planned_activity_id = epa.planned_activity_id
JOIN course_instance i ON i.instance_id = pa.instance_id
JOIN course_layout l ON l.course_layout_id = i.course_layout_id
WHERE j.job_title = 'Teacher' AND i.study_year = EXTRACT(YEAR FROM CURRENT_DATE)::int AND l.study_period::char = get_current_period()::char;

/*
DROP INDEX IF EXISTS idx_mv_teacher_grouping;
CREATE INDEX idx_mv_teacher_grouping
ON teachers_allocated_more_than_N_courses (employement_id, teacher_name, study_period, instance_id);
*/

/* NOT USED
/* INDEXES */
DROP INDEX IF EXISTS idx_person_full_name;
CREATE INDEX idx_person_full_name 
ON person ((first_name || ' ' || last_name));


DROP INDEX IF EXISTS idx_pa_instance_id;
CREATE INDEX idx_pa_instance_id
ON planned_activity(instance_id);

DROP INDEX IF EXISTS idx_pa_teaching_activity_id;
CREATE INDEX idx_pa_teaching_activity_id
ON planned_activity(teaching_activity_id);

DROP INDEX IF EXISTS idx_ci_course_layout_id;
CREATE INDEX idx_ci_course_layout_id
ON course_instance(course_layout_id);

DROP INDEX IF EXISTS idx_epa_employement_id;
CREATE INDEX idx_epa_employement_id
ON employee_planned_activity(employement_id);

DROP INDEX IF EXISTS idx_epa_planned_activity_id;
CREATE INDEX idx_epa_planned_activity_id
ON employee_planned_activity(planned_activity_id);

DROP INDEX IF EXISTS idx_e_person_id;
CREATE INDEX idx_e_person_id
ON employee(person_id);

DROP INDEX IF EXISTS idx_e_job_title_id;
CREATE INDEX idx_e_job_title_id
ON employee(job_title_id);

DROP INDEX IF EXISTS idx_pa_instance_planned    ;
CREATE INDEX idx_pa_instance_planned
ON planned_activity (instance_id, planned_activity_id);

DROP INDEX IF EXISTS idx_epa_planned_employement;
CREATE INDEX idx_epa_planned_employement
ON employee_planned_activity (planned_activity_id, employement_id);

*/

/*

CODE GRAVEYARD:
DROP MATERIALIZED VIEW IF EXISTS teachers_allocated_more_than_N_courses;
CREATE MATERIALIZED VIEW teachers_allocated_more_than_N_courses AS
SELECT i.instance_id, e.employement_id, p.first_name || ' ' || p.last_name AS teacher_name, l.study_period
FROM employee e
JOIN person p ON p.person_id = e.person_id
JOIN job_title j ON j.job_title_id = e.job_title_id
JOIN employee_planned_activity epa ON epa.employement_id = e.employement_id
JOIN planned_activity pa ON pa.planned_activity_id = epa.planned_activity_id
JOIN course_instance i ON i.instance_id = pa.instance_id
JOIN course_layout l ON l.course_layout_id = i.course_layout_id
WHERE j.job_title = 'Teacher' AND i.study_year = EXTRACT(YEAR FROM CURRENT_DATE)::int AND l.study_period::char = get_current_period()::char;

*/