/*

UNUSED AFTER TRIGGER WAS ADDED BACK TO SOLVE TASK 2


CREATE OR REPLACE VIEW exam_and_admin_hours AS
SELECT 
    l.course_code,
    i.instance_id,
    ROUND (l.hp * exam_calc.hp_factor + i.num_students * exam_calc.students_factor + exam_calc.constant) AS exam_hours,
    ROUND (l.hp * admin_calc.hp_factor + i.num_students * admin_calc.students_factor + admin_calc.constant) AS admin_hours
FROM course_instance i
JOIN course_layout l ON l.course_layout_id = i.course_layout_id
JOIN calculation exam_calc ON exam_calc.name = 'exam_calc'
JOIN calculation admin_calc ON admin_calc.name = 'admin_calc';
*/