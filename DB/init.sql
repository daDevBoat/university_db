DROP DATABASE IF EXISTS university_db;

CREATE DATABASE university_db
  WITH ENCODING 'UTF8'
       LC_COLLATE='sv_SE.UTF-8'
       LC_CTYPE='sv_SE.UTF-8'
       TEMPLATE=template0;

\c university_db

DROP TYPE IF EXISTS period_enum;
CREATE TYPE period_enum AS ENUM('1', '2', '3', '4');

CREATE TABLE rule (
    name VARCHAR(50) UNIQUE PRIMARY KEY,
    limit_value INT
);

CREATE TABLE calculation (
    name VARCHAR(50) UNIQUE PRIMARY KEY,
    hp_factor REAL,
    students_factor REAL,
    constant REAL
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
    course_layout_id INT NOT NULL REFERENCES course_layout(course_layout_id) ON UPDATE CASCADE,
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
    instance_id INT NOT NULL REFERENCES course_instance(instance_id) ON UPDATE CASCADE,
    teaching_activity_id INT NOT NULL REFERENCES teaching_activity(teaching_activity_id) ON UPDATE CASCADE,
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
    person_id INT NOT NULL REFERENCES person(person_id) ON UPDATE CASCADE,
    department_id INT NOT NULL REFERENCES department(department_id) ON UPDATE CASCADE,
    job_title_id INT NOT NULL REFERENCES job_title(job_title_id) ON UPDATE CASCADE,
    supervisor_id INT REFERENCES employee(employement_id) ON UPDATE CASCADE,
    is_active BIT(1) DEFAULT B'1' NOT NULL
);

CREATE TABLE salary_history (
    employement_id INT NOT NULL REFERENCES employee(employement_id) ON UPDATE CASCADE,
    year INT NOT NULL,
    period period_enum NOT NULL,
    PRIMARY KEY (employement_id, year, period)
);

CREATE TABLE employee_skill_set (
    employement_id INT NOT NULL REFERENCES employee(employement_id) ON UPDATE CASCADE,
    skill_set_id INT NOT NULL REFERENCES skill_set(skill_set_id) ON UPDATE CASCADE ON DELETE CASCADE,
    PRIMARY KEY (employement_id, skill_set_id)
);

CREATE TABLE employee_planned_activity (
    employement_id INT NOT NULL REFERENCES employee(employement_id) ON UPDATE CASCADE,
    planned_activity_id INT NOT NULL REFERENCES planned_activity(planned_activity_id) ON UPDATE CASCADE,
    PRIMARY KEY (employement_id, planned_activity_id)
);

CREATE TABLE phone (
    phone_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    phone_number CHAR(15) NOT NULL UNIQUE
);

CREATE TABLE person_phone (
    person_id INT NOT NULL REFERENCES person (person_id) ON UPDATE CASCADE,
    phone_id INT  NOT NULL REFERENCES phone (phone_id) ON UPDATE CASCADE,
    PRIMARY KEY (person_id, phone_id)
);



/*
CODE GRAVEYARD NOT USED

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


CREATE TRIGGER course_instance_calc_trigger
AFTER INSERT ON course_instance
FOR EACH ROW
EXECUTE FUNCTION calculate_exam_admin_hours();

*/