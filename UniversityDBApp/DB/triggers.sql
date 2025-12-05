/* TRIGGERS and FUNCTIONS */

-- First add the neccesay trigger rules and calculations
INSERT INTO rule (name, limit_value)
VALUES ('employee_instance_limit', 4);

INSERT INTO calculation (name, hp_factor, students_factor, constant)
VALUES 
    ('exam_calc', 0, 0.725, 32),
    ('admin_calc', 2, 0.2, 28);


CREATE OR REPLACE FUNCTION min_max_students_check() RETURNS trigger AS $$  
DECLARE
    min_s INT;
    max_s INT;
BEGIN
    SELECT min_students, max_students INTO min_s, max_s
    FROM course_layout
    WHERE course_layout_id = NEW.course_layout_id;

    IF NEW.num_students > max_s THEN
        RAISE EXCEPTION '% number of students is greater than layout max of % students', NEW.num_students, max_s;
    ELSIF NEW.num_students < min_s THEN
        RAISE EXCEPTION '% number of students is less than layout min of % students', NEW.num_students, min_s;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION planned_activity_limit_check() RETURNS trigger AS $$
DECLARE
    in_id INT;
    lay_id INT;
    num_of_instances INT;
    year INT;
    period period_enum;
    instance_limit INT;
    job_t VARCHAR(100);
BEGIN
    SELECT instance_id INTO in_id
    FROM planned_activity
    WHERE planned_activity_id = NEW.planned_activity_id;

    SELECT study_year, course_layout_id INTO year, lay_id
    FROM course_instance
    WHERE instance_id = in_id;

    SELECT study_period INTO period
    FROM course_layout
    WHERE course_layout_id = lay_id;

    SELECT limit_value INTO instance_limit
    FROM rule
    WHERE name = 'employee_instance_limit';

    -- RAISE NOTICE 'year: % period: %', year, period;

    SELECT COUNT(DISTINCT p.instance_id) INTO num_of_instances
    FROM(
        SELECT employement_id, planned_activity_id
        FROM employee_planned_activity
        WHERE employement_id = NEW.employement_id) AS f
    JOIN planned_activity p ON f.planned_activity_id = p.planned_activity_id
    JOIN course_instance i ON i.instance_id = p.instance_id
    JOIN course_layout l ON l.course_layout_id = i.course_layout_id
    WHERE l.study_period = period AND i.study_year = year;
    
    IF num_of_instances >= instance_limit THEN
        RAISE EXCEPTION 'An employee can only be allocated to max % courses in the same period', instance_limit;
    END IF;
    
    -- RAISE NOTICE '# of instances for employee: %', num_of_instances + 1;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION calculate_exam_admin_hours() RETURNS trigger AS $$
DECLARE
    update_flag BOOLEAN;
    hp_value REAL;
BEGIN
    IF TG_NARGS > 0 THEN
        update_flag := TG_ARGV[0];
    ELSE
        RAISE EXCEPTION 'Trigger needs update flag';
    END IF;

    SELECT hp INTO hp_value
    FROM course_layout
    WHERE course_layout_id = NEW.course_layout_id;

    IF update_flag THEN
        -- Examination
        UPDATE planned_activity pa
        SET planned_hours = hp_value * exam_calc.hp_factor + NEW.num_students * exam_calc.students_factor + exam_calc.constant
        FROM teaching_activity ta
        JOIN calculation exam_calc ON exam_calc.name = 'exam_calc'
        WHERE 
            pa.instance_id = NEW.instance_id
            AND pa.teaching_activity_id = ta.teaching_activity_id
            AND ta.activity_name = 'Exam';

        -- Admin
        UPDATE planned_activity pa
        SET planned_hours = hp_value * admin_calc.hp_factor + NEW.num_students * admin_calc.students_factor + admin_calc.constant
        FROM teaching_activity ta
        JOIN calculation admin_calc ON admin_calc.name = 'admin_calc'
        WHERE 
            pa.instance_id = NEW.instance_id
            AND pa.teaching_activity_id = ta.teaching_activity_id
            AND ta.activity_name = 'Admin';
    ELSE -- INSERT
        -- Examination
        INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
        SELECT NEW.instance_id, ta.teaching_activity_id, hp_value * exam_calc.hp_factor + NEW.num_students * exam_calc.students_factor + exam_calc.constant
        FROM teaching_activity ta
        JOIN calculation exam_calc ON exam_calc.name = 'exam_calc'
        WHERE ta.activity_name = 'Exam';

        -- Admin
        INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
        SELECT NEW.instance_id, ta.teaching_activity_id, hp_value * admin_calc.hp_factor + NEW.num_students * admin_calc.students_factor + admin_calc.constant
        FROM teaching_activity ta
        JOIN calculation admin_calc ON admin_calc.name = 'admin_calc'
        WHERE ta.activity_name = 'Admin';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER course_instance_calc_insert_trigger
AFTER INSERT ON course_instance
FOR EACH ROW
EXECUTE FUNCTION calculate_exam_admin_hours(FALSE);

CREATE TRIGGER course_instance_calc_update_trigger
AFTER UPDATE ON course_instance
FOR EACH ROW
EXECUTE FUNCTION calculate_exam_admin_hours(TRUE);

CREATE TRIGGER min_max_students_trigger
BEFORE INSERT OR UPDATE ON course_instance
FOR EACH ROW
EXECUTE FUNCTION min_max_students_check();

CREATE TRIGGER planned_activity_limit_trigger
BEFORE INSERT OR UPDATE ON employee_planned_activity
FOR EACH ROW
EXECUTE FUNCTION planned_activity_limit_check();