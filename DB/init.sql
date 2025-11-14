DROP DATABASE IF EXISTS university_db;
CREATE DATABASE university_db;  

\c university_db

DROP TYPE IF EXISTS period_enum;
CREATE TYPE period_enum AS ENUM('1', '2', '3', '4');

CREATE TABLE rules (
    name VARCHAR(50) PRIMARY KEY,
    limit_value INT NOT NULL
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

-- ADD MORE HERE
CREATE TABLE planned_activity (
    planned_activity_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY
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
    phoneNumber VARCHAR(15) NOT NULL,
    address VARCHAR(100) NOT NULL
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


/* TRIGGERS and FUNCTIONS */

