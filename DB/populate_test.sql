/* 
THIS SCRIPT WAS USED ORIGINALY FOR TESTING.
IS NOT USED ANYMORE. USE populate.sql!
*/




INSERT INTO course_layout (course_code, course_name, min_students, max_students, hp, study_period)
VALUES 
    ('IV1351', 'Data Storage Paradigms', 20, 300, 7.5, '1'),
    ('IV1352', 'Data Cloud Storage', 20, 300, 7.5, '1'),
    ('IV1353', 'Data Storage Design', 20, 300, 7.5, '1'),
    ('IV1354', 'Data Modeling', 20, 300, 7.5, '1'),
    ('IV1355', 'Data Handling', 20, 300, 7.5, '1'),
    ('IV1356', 'Data Analysing', 20, 300, 7.5, '2');

INSERT INTO course_instance (course_layout_id, num_students, study_year)
SELECT course_layout_id, 300, 2025
FROM course_layout
WHERE course_code = 'IV1351'
UNION ALL
SELECT course_layout_id, 200, 2025
FROM course_layout
WHERE course_code = 'IV1352'
UNION ALL
SELECT course_layout_id, 200, 2025
FROM course_layout
WHERE course_code = 'IV1353'
UNION ALL
SELECT course_layout_id, 200, 2025
FROM course_layout
WHERE course_code = 'IV1354'
UNION ALL
SELECT course_layout_id, 20, 2025
FROM course_layout
WHERE course_code = 'IV1355';


INSERT INTO person (personal_number, first_name, last_name, street, city, zip)
VALUES ('200407119999', 'Elias', 'Næss', 'Armégatan', 'Solna', '17172');

INSERT INTO department (department_name)
VALUES ('EECS');

INSERT INTO job_title (job_title)
VALUES ('teacher');

INSERT INTO teaching_activity (activity_name, factor)
VALUES 
    ('lecture', 3.6),
    ('seminar', 2.2);

INSERT INTO employee (person_id, department_id, job_title_id, supervisor_id)
SELECT p.person_id, d.department_id, j.job_title_id, NULL
FROM person p
JOIN department d ON d.department_name = 'EECS'
JOIN job_title j ON j.job_title = 'teacher'
WHERE p.personal_number = '200407119999';

INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
SELECT i.instance_id, t.teaching_activity_id, 20
FROM course_instance i
JOIN teaching_activity t ON t.activity_name = 'lecture'
JOIN course_layout l ON i.course_layout_id = l.course_layout_id
WHERE l.course_code = 'IV1351'
UNION ALL
SELECT i.instance_id, t.teaching_activity_id, 20
FROM course_instance i
JOIN teaching_activity t ON t.activity_name = 'lecture'
JOIN course_layout l ON i.course_layout_id = l.course_layout_id
WHERE l.course_code = 'IV1352'
UNION ALL
SELECT i.instance_id, t.teaching_activity_id, 20
FROM course_instance i
JOIN teaching_activity t ON t.activity_name = 'lecture'
JOIN course_layout l ON i.course_layout_id = l.course_layout_id
WHERE l.course_code = 'IV1353'
UNION ALL
SELECT i.instance_id, t.teaching_activity_id, 20
FROM course_instance i
JOIN teaching_activity t ON t.activity_name = 'lecture'
JOIN course_layout l ON i.course_layout_id = l.course_layout_id
WHERE l.course_code = 'IV1354'
UNION ALL
SELECT i.instance_id, t.teaching_activity_id, 20
FROM course_instance i
JOIN teaching_activity t ON t.activity_name = 'lecture'
JOIN course_layout l ON i.course_layout_id = l.course_layout_id
WHERE l.course_code = 'IV1355';

INSERT INTO employee_planned_activity (employement_id, planned_activity_id)
SELECT 1, p.planned_activity_id
FROM planned_activity p
JOIN course_instance i ON i.instance_id = p.instance_id 
JOIN course_layout l  ON i.course_layout_id = l.course_layout_id
JOIN teaching_activity t ON t.teaching_activity_id = p.teaching_activity_id
WHERE l.course_code = 'IV1351' and t.activity_name = 'lecture'
UNION ALL
SELECT 1, p.planned_activity_id
FROM planned_activity p
JOIN course_instance i ON i.instance_id = p.instance_id 
JOIN course_layout l  ON i.course_layout_id = l.course_layout_id
JOIN teaching_activity t ON t.teaching_activity_id = p.teaching_activity_id
WHERE l.course_code = 'IV1352' and t.activity_name = 'lecture'
UNION ALL
SELECT 1, p.planned_activity_id
FROM planned_activity p
JOIN course_instance i ON i.instance_id = p.instance_id 
JOIN course_layout l  ON i.course_layout_id = l.course_layout_id
JOIN teaching_activity t ON t.teaching_activity_id = p.teaching_activity_id
WHERE l.course_code = 'IV1353' and t.activity_name = 'lecture'
UNION ALL
SELECT 1, p.planned_activity_id
FROM planned_activity p
JOIN course_instance i ON i.instance_id = p.instance_id 
JOIN course_layout l  ON i.course_layout_id = l.course_layout_id
JOIN teaching_activity t ON t.teaching_activity_id = p.teaching_activity_id
WHERE l.course_code = 'IV1354' and t.activity_name = 'lecture';


