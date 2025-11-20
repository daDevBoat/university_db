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

CREATE TRIGGER min_max_students_triger
BEFORE INSERT ON course_instance
FOR EACH ROW
EXECUTE FUNCTION min_max_students_check();

CREATE TRIGGER planned_activity_limit_trigger
BEFORE INSERT ON employee_planned_activity
FOR EACH ROW
EXECUTE FUNCTION planned_activity_limit_check();