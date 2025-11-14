CREATE DATABASE KthDB;  

CREATE TYPE period_enum AS ENUM(1, 2, 3, 4);

CREATE TABLE course_layout (
    course_layout_id INT GENERATED ALWAYS AS INDENTITY PRIMARY KEY,
    course_code VARCHAR(10) NOT NULL,
    course_name VARCHAR(100) NOT NULL,
    min_students INT NOT NULL,
    max_students INT NOT NULL,
    hp REAL NOT NULL, 
    study_period period_enum NOT NULL
);

CREATE TABLE course_instance (
    instance_id INT GENERATED AS INDENTITY PRIMARY KEY,
    course_layout_id INT REFERENCES course_layout(course_layout_id),
    num_students INT NOT NULL,
    study_year INT NOT NULL
);

CREATE TABLE teaching_activity (
    teaching_activity_id INT GENERATED ALWAYS AS INDENTITY PRIMARY KEY,
    activity_name VARCHAR(50) NOT NULL UNIQUE,
    factor REAL NOT NULL
);

