

INSERT INTO course_layout (course_code, course_name, min_students, max_students, hp, study_period)
VALUES ('IV1351', 'Data Storage Paradigms', 20, 300, 7.5, '1');

INSERT INTO course_instance (course_layout_id, num_students, study_year)
SELECT course_layout_id, 200, 2025
FROM course_layout
WHERE course_code = 'IV1351';
