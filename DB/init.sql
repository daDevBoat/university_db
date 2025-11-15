DROP DATABASE IF EXISTS university_db;
CREATE DATABASE university_db;  

\c university_db

DROP TYPE IF EXISTS period_enum;
CREATE TYPE period_enum AS ENUM('1', '2', '3', '4');

CREATE TABLE rule (
    rule_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) UNIQUE,
    limit_value INT,
    target_name VARCHAR(50),
    CHECK (limit_value IS NOT NULL OR target_name IS NOT NULL)
);

CREATE TABLE course_layout (
    course_layout_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    course_code VARCHAR(10) NOT NULL,
    course_name VARCHAR(100) NOT NULL,
    min_students INT NOT NULL,
    max_students INT NOT NULL,
    hp REAL NOT NULL, 
    study_period period_enum NOT NULL
);

CREATE TABLE course_instance (
    instance_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    course_layout_id INT REFERENCES course_layout(course_layout_id) NOT NULL,
    num_students INT NOT NULL,
    study_year INT NOT NULL
);

CREATE TABLE teaching_activity (
    teaching_activity_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    activity_name VARCHAR(50) NOT NULL UNIQUE,
    factor REAL NOT NULL
);

CREATE TABLE planned_activity (
    planned_activity_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    instance_id INT REFERENCES course_instance(instance_id) NOT NULL,
    teaching_activity_id INT REFERENCES teaching_activity(teaching_activity_id) NOT NULL,
    planned_hours REAL NOT NULL
);

CREATE TABLE department (
    department_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    department_name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE person (
    person_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    personal_number CHAR(12) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    street VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    zip CHAR(5) NOT NULL
);

CREATE TABLE job_title (
    job_title_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_title VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE skill_set (
    skill_set_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    skill VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE employee (
    employement_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    person_id INT REFERENCES person(person_id) NOT NULL,
    department_id INT REFERENCES department(department_id) NOT NULL,
    job_title_id INT REFERENCES job_title(job_title_id) NOT NULL,
    supervisor_id INT REFERENCES employee(employement_id),
    is_active BIT(1) DEFAULT B'1' NOT NULL
);

CREATE TABLE salary_history (
    employement_id INT REFERENCES employee(employement_id) NOT NULL,
    year INT NOT NULL,
    period period_enum NOT NULL,
    PRIMARY KEY (employement_id, year, period)
);

CREATE TABLE employee_skill_set (
    employement_id INT REFERENCES employee(employement_id) NOT NULL,
    skill_set_id INT REFERENCES skill_set(skill_set_id) NOT NULL,
    PRIMARY KEY (employement_id, skill_set_id)
);

CREATE TABLE employee_planned_activity (
    employement_id INT REFERENCES employee(employement_id) NOT NULL,
    planned_activity_id INT REFERENCES planned_activity(planned_activity_id) NOT NULL,
    PRIMARY KEY (employement_id, planned_activity_id)
);

CREATE TABLE phone (
    phone_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    phone_number CHAR(15) NOT NULL UNIQUE
);

CREATE TABLE person_phone (
    person_id INT REFERENCES person (person_id) NOT NULL,
    phone_id INT REFERENCES phone (phone_id) NOT NULL,
    PRIMARY KEY (person_id, phone_id)
);

/* TRIGGERS and FUNCTIONS */

-- First add the neccesay trigger rules
INSERT INTO rule (name, limit_value, target_name)
VALUES 
    ('exam_attr_name', NULL, 'exam'),
    ('admin_attr_name', NULL, 'admin'),
    ('employee_instance_limit', 4, NULL);

-- Insert exam and admin into teaching_activity (so they are always there) 
INSERT INTO teaching_activity (activity_name, factor)
VALUES 
    ('exam', 1),
    ('admin', 1);


/* Assumes that the start of the academic year is 1st of Jaunary and each period is 3 months */
CREATE OR REPLACE FUNCTION get_current_period()
RETURNS INT AS $$
DECLARE
    period INT;
    date_no_year CHAR(5);
BEGIN
    SELECT TO_CHAR(CURRENT_DATE, 'MM-DD') INTO date_no_year;
    RAISE NOTICE 'date: %', date_no_year;
    
    IF date_no_year < '04-01' THEN
        RETURN 1;
    ELSIF date_no_year < '07-01' THEN
        RETURN 2;
    ELSIF date_no_year < '10-01' THEN 
        RETURN 3;
    ELSE
        RETURN 4;
    END IF;
END;
$$ LANGUAGE plpgsql;

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


CREATE OR REPLACE FUNCTION calculate_exam_admin_hours() RETURNS trigger AS $$
DECLARE
    num_students INT;
    hp_value REAL;
    exam_ta_id INT;
    admin_ta_id INT;
BEGIN
    num_students := NEW.num_students;

    SELECT hp INTO hp_value
    FROM course_layout
    WHERE course_layout_id = NEW.course_layout_id;

    SELECT t.teaching_activity_id INTO exam_ta_id 
    FROM teaching_activity t
    JOIN rule r ON r.target_name = t.activity_name
    WHERE r.name = 'exam_attr_name';

    SELECT t.teaching_activity_id INTO admin_ta_id 
    FROM teaching_activity t
    JOIN rule r ON r.target_name = t.activity_name
    WHERE r.name = 'admin_attr_name';

    -- Examination hour = 32+ 0.725* #Students 
    INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
    VALUES (NEW.instance_id, exam_ta_id, 32 + 0.725 * num_students);

    -- Admin hours = 2*HP+ 28+ 0.2* #Students 
    INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
    VALUES (NEW.instance_id, admin_ta_id, 2 * hp_value + 28 + 0.2 * num_students);

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
    JOIN planned_activity p ON f.planned_activity_id = p.planned_activity_id;


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
    
    RAISE NOTICE '# of instances for employee: %', num_of_instances + 1;

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

CREATE TRIGGER course_instance_calc_trigger
AFTER INSERT ON course_instance
FOR EACH ROW
EXECUTE FUNCTION calculate_exam_admin_hours();


/*
QUERIES:

INSERT INTO teaching_activity (activity_name, factor)
VALUES ('exam', 1);

INSERT INTO teaching_activity (activity_name, factor)
VALUES ('admin', 1);

INSERT INTO course_layout (course_code, course_name, min_students, max_students, hp, study_period)
VALUES ('IV1351', 'Data Storage Paradigms', 20, 300, 7.5, '1');

INSERT INTO course_instance (course_layout_id, num_students, study_year)
SELECT course_layout_id, 200, 2025
FROM course_layout
WHERE course_code = 'IV1351';


SELECT l.course_code, l.course_name, t.activity_name, p.planned_hours
FROM planned_activity p
JOIN course_instance i ON i.instance_id = p.instance_id
JOIN course_layout l ON l.course_layout_id = i.course_layout_id
JOIN teaching_activity t ON t.teaching_activity_id = p.teaching_activity_id
WHERE l.course_code = 'IV1351';


*/