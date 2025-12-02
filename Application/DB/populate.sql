-- 0. Teaching activities and job titles
INSERT INTO teaching_activity (activity_name, factor)
VALUES 
    ('Lecture', 3.6),
    ('Lab', 2.4),
    ('Tutorial', 2.4),
    ('Seminar', 1.8),
    ('Other', 1),
    ('Exam', 1),
    ('Admin', 1)
ON CONFLICT (activity_name) DO NOTHING;

INSERT INTO job_title (job_title)
VALUES 
    ('Teacher'),
    ('Manager'),
    ('Cleaner'),
    ('Secretary'),
    ('Assistant')
ON CONFLICT (job_title) DO NOTHING;

-- 1. Departments
INSERT INTO department (department_name) VALUES
('Computer Science'),
('Mathematics'),
('Electrical Engineering'),
('Mechanical Engineering'),
('Physics'),
('Chemistry'),
('Information Technology'),
('Industrial Engineering'),
('Software Engineering'),
('Humanities')
ON CONFLICT (department_name) DO NOTHING;

-- 2. Skill set
INSERT INTO skill_set (skill) VALUES
('Database Design'),
('Theoretical Math'),
('Numerical Methods'),
('Algorithms'),
('Concurrent Programming'),
('Operating Systems'),
('Network Security'),
('Machine Learning'),
('Data Analysis'),
('Embedded Systems'),
('Signal Processing'),
('Software Architecture'),
('Cloud Computing'),
('User Experience'),
('Project Management'),
('Hardware Design'),
('Control Systems'),
('Compiler Construction'),
('Cryptography'),
('Testing & QA')
ON CONFLICT (skill) DO NOTHING;

-- 3. Persons (ASCII-only names), phones, person_phone
DO $$
DECLARE
    first_names VARCHAR(100)[] := ARRAY[
        'Anna','Erik','Maria','Lars','Karin','Per','Elin','Johan','Sofia','Anders',
        'Eva','Fredrik','Lisa','Magnus','Irene','Oskar','Helena','Nils','Asta','Pontus',
        'Mikael','Ingrid','Simon','Therese','Gunnar','Camilla','Martin','Julia','Henrik','Sara',
        'Viktor','Rebecka','Jonas','Stina','Carl','Matilda','Fred','Emilia','Linda','Kristian',
        'Malin','Patrik','Linnea','Tobias','Amanda','Daniel','Josefin','Bertil','Elisabeth','Sebastian'
    ];
    last_names VARCHAR(100)[] := ARRAY[
        'Svensson','Johansson','Karlsson','Nilsson','Eriksson','Larsson','Olsson','Persson','Berg','Gustafsson',
        'Pettersson','Jonsson','Soderberg','Lindberg','Lundgren','Bergstrom','Hansson','Axelsson','Lindqvist','Bergman',
        'Lund','Berglund','Akesson','Holm','Bengtsson','Hedlund','Forsberg','Sandberg','Blom','Engstrom'
    ];
    streets VARCHAR(100)[] := ARRAY[
        'Storgatan 1','Drottninggatan 12','Kungsgatan 3','Sodra Vagen 45','Vastra Hamngatan 2',
        'Nygatan 7','Ostra Langgatan 9','Kvarnvagen 4','Parkgatan 16','Skomakaregatan 5'
    ];
    cities VARCHAR(100)[] := ARRAY['Stockholm','Goteborg','Malmo','Uppsala','Lund','Linkoping','Vasteras','Orebro','Helsingborg','Norrkoping'];
    zip_base INT := 10000;
    v_idx INT;
    v_person_cnt INT := 120;
    v_personal_number CHAR(12);
    v_first VARCHAR(100);
    v_last VARCHAR(100);
    v_street VARCHAR(100);
    v_city VARCHAR(100);
    v_zip CHAR(5);
    v_phone CHAR(15);
    v_phone_id INT;
    cur_person RECORD;
BEGIN
    FOR v_idx IN 1..v_person_cnt LOOP
        v_first := first_names[1 + ((v_idx * 3) % array_length(first_names,1))];
        v_last := last_names[1 + ((v_idx * 5) % array_length(last_names,1))];
        v_personal_number := lpad((19650101 + v_idx)::text,12,'0');
        v_street := streets[1 + ((v_idx*7) % array_length(streets,1))];
        v_city := cities[1 + ((v_idx*11) % array_length(cities,1))];
        v_zip := lpad((zip_base + (v_idx % 9000))::text,5,'0');

        INSERT INTO person (personal_number, first_name, last_name, street, city, zip)
        VALUES (v_personal_number, v_first, v_last, v_street, v_city, v_zip)
        ON CONFLICT (personal_number) DO NOTHING;
    END LOOP;

    FOR cur_person IN SELECT person_id FROM person ORDER BY person_id LIMIT v_person_cnt LOOP
        v_phone := '+46' || lpad((700000000 + cur_person.person_id)::text,9,'0');
        INSERT INTO phone (phone_number) VALUES (v_phone) ON CONFLICT (phone_number) DO NOTHING;
        SELECT phone_id INTO v_phone_id FROM phone WHERE phone_number = v_phone LIMIT 1;
        IF v_phone_id IS NOT NULL THEN
            INSERT INTO person_phone (person_id, phone_id)
            VALUES (cur_person.person_id, v_phone_id)
            ON CONFLICT (person_id, phone_id) DO NOTHING;
        END IF;
    END LOOP;
END
$$;

-- 4. Employees (fixed assignment of employement_id to variable)
DO $$
DECLARE
    total_emps INT := 100;
    persons_arr INT[] := ARRAY(SELECT person_id FROM person ORDER BY person_id LIMIT total_emps);
    dept_ids INT[] := ARRAY(SELECT department_id FROM department ORDER BY department_id);
    dept_count INT := array_length(dept_ids,1);
    teacher_jid INT := (SELECT job_title_id FROM job_title WHERE job_title='Teacher');
    manager_jid INT := (SELECT job_title_id FROM job_title WHERE job_title='Manager');
    cleaner_jid INT := (SELECT job_title_id FROM job_title WHERE job_title='Cleaner');
    secretary_jid INT := (SELECT job_title_id FROM job_title WHERE job_title='Secretary');
    assistant_jid INT := (SELECT job_title_id FROM job_title WHERE job_title='Assistant');
    created_emps INT[] := ARRAY[]::INT[];
    manager_list INT[] := ARRAY[]::INT[];
    i INT;
    emp_id INT;
BEGIN
    FOR i IN 1..total_emps LOOP
        IF i <= 85 THEN
            INSERT INTO employee(person_id, department_id, job_title_id, supervisor_id)
            VALUES (persons_arr[i], dept_ids[1 + (i % dept_count)], teacher_jid, NULL)
            RETURNING employement_id INTO emp_id;
            created_emps := array_append(created_emps, emp_id);
        ELSIF i <= 95 THEN
            INSERT INTO employee(person_id, department_id, job_title_id, supervisor_id)
            VALUES (persons_arr[i], dept_ids[1 + (i % dept_count)], manager_jid, NULL)
            RETURNING employement_id INTO emp_id;
            created_emps := array_append(created_emps, emp_id);
            manager_list := array_append(manager_list, emp_id);
        ELSE
            INSERT INTO employee(person_id, department_id, job_title_id, supervisor_id)
            VALUES (persons_arr[i], dept_ids[1 + (i % dept_count)],
                CASE WHEN i % 3=0 THEN cleaner_jid
                     WHEN i % 3=1 THEN secretary_jid
                     ELSE assistant_jid END,
                NULL)
            RETURNING employement_id INTO emp_id;
            created_emps := array_append(created_emps, emp_id);
        END IF;
    END LOOP;

    -- Assign supervisors
    IF array_length(manager_list,1) >= 2 THEN
        FOR i IN 3..array_length(manager_list,1) LOOP
            UPDATE employee SET supervisor_id = manager_list[1 + ((i+1) % 2)] WHERE employement_id = manager_list[i];
        END LOOP;
    END IF;

    -- Non-managers assigned to managers round-robin
    FOR i IN 1..array_length(created_emps,1) LOOP
        IF NOT created_emps[i] = ANY(manager_list) THEN
            UPDATE employee SET supervisor_id = manager_list[1 + ((i-1) % array_length(manager_list,1))] WHERE employement_id = created_emps[i];
        END IF;
    END LOOP;
END
$$;


-- 5. Salary history: deterministic, at least two entries per employee, always increasing, 200-600
DO $$
DECLARE
    emp_rec RECORD;
    salary_val REAL;
    base_salary REAL;
    years INT[] := ARRAY[2021,2022,2023];
    periods period_enum[] := ARRAY['1'::period_enum,'2'::period_enum,'3'::period_enum,'4'::period_enum];
    yr INT;
    per period_enum;
    incr REAL;
BEGIN
    FOR emp_rec IN SELECT employement_id FROM employee LOOP
        -- Base salary for each employee, deterministic
        base_salary := 200 + ((emp_rec.employement_id * 37) % 201); -- 200..400
        salary_val := base_salary;

        -- Loop through years
        FOREACH yr IN ARRAY years LOOP
            -- Loop through all 4 periods
            FOREACH per IN ARRAY periods LOOP
                -- Increase salary per entry deterministically: 5..20
                incr := 5 + ((emp_rec.employement_id + yr + (per::text)::int) % 16);
                salary_val := LEAST(salary_val + incr, 600); -- cap at 600

                -- Only insert at least two entries per employee, optional third for some
                IF (yr = 2021 AND per = '1'::period_enum) OR
                   (yr = 2022 AND per = '3'::period_enum) OR
                   ((emp_rec.employement_id % 5 = 0) AND yr = 2023 AND per = '2'::period_enum) THEN

                    INSERT INTO salary_history (employement_id, year, period, salary)
                    VALUES (emp_rec.employement_id, yr, per, salary_val)
                    ON CONFLICT DO NOTHING;
                END IF;

            END LOOP;
        END LOOP;
    END LOOP;
END
$$;





-- 6. Skills (1-3 per employee)
DO $$
DECLARE
    emp_rec RECORD;
    skill_ids INT[] := ARRAY(SELECT skill_set_id FROM skill_set ORDER BY skill_set_id);
    s_count INT;
    k INT;
BEGIN
    FOR emp_rec IN SELECT employement_id FROM employee LOOP
        s_count := 1 + (emp_rec.employement_id % 3);
        FOR k IN 0..(s_count-1) LOOP
            INSERT INTO employee_skill_set (employement_id, skill_set_id)
            VALUES (emp_rec.employement_id, skill_ids[1 + ((emp_rec.employement_id + k) % array_length(skill_ids,1))])
            ON CONFLICT DO NOTHING;
        END LOOP;
    END LOOP;
END
$$;

-- 7. Create 50 courses, each with 1-3 layouts. Deterministic min/max and HP values
DO $$
DECLARE
    course_names TEXT[] := ARRAY[
        'Data Storage Paradigms','Calculus 1','Concurrent Programming','Algorithms and Data Structures',
        'Operating Systems','Computer Networks','Machine Learning Fundamentals','Numerical Analysis',
        'Compiler Construction','Database Systems','Software Engineering','Embedded Systems',
        'Digital Signal Processing','Control Systems','Network Security','Human-Computer Interaction',
        'Cloud Infrastructure','Parallel Computing','Formal Methods','Cryptography',
        'Computer Graphics','Distributed Systems','Information Retrieval','AI Planning',
        'Programming Languages','Linear Algebra','Probability and Statistics','Optimization',
        'Real-Time Systems','Computer Vision','Natural Language Processing','Microelectronics',
        'Analog Circuits','Robotics','High Performance Computing','Quantum Computing Basics',
        'Discrete Mathematics','Data Mining','Secure Programming','Mobile Application Development',
        'Web Technologies','Big Data Systems','Ethics in Technology','Project Management for IT',
        'Advanced Algorithms','Computational Geometry','Signal Processing for Communications',
        'Software Testing','DevOps Practices','User Experience Design','IoT Systems'
    ];

    base_course_count INT := 50;
    idx INT;
    layouts INT;
    code TEXT;
    cname TEXT;
    min_vals INT[] := ARRAY[10,20,30,40,50];
    max_vals INT[] := ARRAY[150,200,250,300,400,500,600];
    lay_min INT;
    lay_max INT;
    hp_vals REAL[] := ARRAY[7.5, 7.5, 7.5, 7.5, 7.5, 7.5, 6, 9, 15, 4.5]; -- mostly 7.5
    hp_val REAL;
    study_periods period_enum[] := ARRAY['1'::period_enum,'2'::period_enum,'3'::period_enum,'4'::period_enum];
    v_layout_id INT;
BEGIN
    FOR idx IN 1..base_course_count LOOP
        cname := course_names[1 + ((idx*3) % array_length(course_names,1))];
        layouts := 1 + ((idx * 7) % 3); -- 1..3 layouts
        code := chr(65 + ((idx) % 26))
                || chr(65 + ((idx*5) % 26))
                || lpad(((1000 + ((idx*31 + 13) % 9000))::text),4,'0');

        FOR i IN 1..layouts LOOP

            -- deterministic min/max
            lay_min := min_vals[1 + ((idx + i) % array_length(min_vals,1))];
            lay_max := max_vals[1 + ((idx + i*2) % array_length(max_vals,1))];

            -- deterministic HP: mostly 7.5
            hp_val := hp_vals[1 + ((idx + i) % array_length(hp_vals,1))];

            INSERT INTO course_layout (
                course_code, course_name,
                min_students, max_students, hp, study_period
            )
            VALUES (
                code,  -- same code for multiple layouts
                cname,
                lay_min, lay_max, hp_val,
                study_periods[1 + ((idx + i) % 4)]
            )
            RETURNING course_layout_id INTO v_layout_id;

            -- instance 1
            INSERT INTO course_instance (course_layout_id, num_students, study_year)
            VALUES (
                v_layout_id,
                lay_min + ((idx*13 + i*7) % GREATEST(1, lay_max - lay_min + 1)),
                2020 + ((idx + i) % 7)
            );

            -- optional 2nd instance
            IF (idx + i) % 2 = 0 THEN
                INSERT INTO course_instance (course_layout_id, num_students, study_year)
                VALUES (
                    v_layout_id,
                    lay_min + ((idx*17 + i*11) % GREATEST(1, lay_max - lay_min + 1)),
                    2020 + ((idx + i*3) % 7)
                );
            END IF;
        END LOOP;
    END LOOP;
END
$$;


-- 8. For each course_instance create 1..3 planned_activity records (use teaching_activity table)
DO $$
DECLARE
    inst_rec RECORD;
    ta_ids INT[] := ARRAY(SELECT teaching_activity_id FROM teaching_activity ORDER BY teaching_activity_id);
    ta_count INT := array_length(ta_ids,1);
    pa_hours REAL;
    num_pa INT;
    ta_choice INT;
BEGIN
    IF ta_count IS NULL THEN
        RAISE EXCEPTION 'No teaching activities found.';
    END IF;

    FOR inst_rec IN SELECT instance_id FROM course_instance ORDER BY instance_id LOOP
        num_pa := 5; -- 1 + (inst_rec.instance_id % 3); -- 1..3 planned activities
        FOR ta_choice IN 1..num_pa LOOP
            pa_hours := ROUND(10 + ((inst_rec.instance_id + ta_choice) % 40)); -- planned hours between 10..49
            INSERT INTO planned_activity (instance_id, teaching_activity_id, planned_hours)
            VALUES (inst_rec.instance_id, ta_choice, pa_hours);
        END LOOP;
    END LOOP;
END
$$;

-----------------------------------------------------------------------
-- 9. Assign teachers to planned activities
--    Allocated hours REAL; sum per PA between 50% and 150% of planned_hours
-----------------------------------------------------------------------
DO $$
DECLARE
    teacher_ids INT[] := ARRAY(
        SELECT e.employement_id
        FROM employee e
        JOIN job_title j ON e.job_title_id = j.job_title_id
        WHERE j.job_title = 'Teacher'
        ORDER BY e.employement_id
    );
    teacher_count INT := array_length(teacher_ids, 1);

    pa RECORD;
    v_instance_id INT;
    v_study_year INT;
    v_course_layout_id INT;
    v_study_period period_enum;

    instance_limit INT;

    instance_teachers INT[] := ARRAY[]::INT[];
    tid INT;
    teacher_load INT;

    pa_teachers INT[] := ARRAY[]::INT[];
    num_pa_teachers INT := 0;

    idx INT;
    alloc_hours REAL;
BEGIN
    IF teacher_count IS NULL OR teacher_count = 0 THEN
        RAISE EXCEPTION 'No teachers found.';
    END IF;

    SELECT limit_value INTO instance_limit
    FROM rule WHERE name = 'employee_instance_limit';

    IF instance_limit IS NULL THEN
        RAISE EXCEPTION 'Missing rule employee_instance_limit';
    END IF;

    -------------------------------------------------------------------
    -- Loop over course instances
    -------------------------------------------------------------------
    FOR v_instance_id, v_study_year, v_course_layout_id, v_study_period IN
        SELECT ci.instance_id, ci.study_year, ci.course_layout_id, cl.study_period
        FROM course_instance ci
        JOIN course_layout cl ON cl.course_layout_id = ci.course_layout_id
        ORDER BY ci.instance_id
    LOOP
        -- Select up to 5 teachers for this instance
        instance_teachers := ARRAY[]::INT[];
        FOR idx IN 0..GREATEST(teacher_count-1,0) LOOP
            tid := teacher_ids[((v_instance_id + idx) % teacher_count) + 1];

            SELECT COUNT(*) INTO teacher_load
            FROM employee_planned_activity epa
            JOIN planned_activity p ON p.planned_activity_id = epa.planned_activity_id
            JOIN course_instance ci ON ci.instance_id = p.instance_id
            JOIN course_layout cl ON cl.course_layout_id = ci.course_layout_id
            WHERE epa.employement_id = tid
              AND ci.study_year = v_study_year
              AND cl.study_period = v_study_period;

            IF teacher_load < instance_limit THEN
                instance_teachers := array_append(instance_teachers, tid);
            END IF;

            EXIT WHEN array_length(instance_teachers,1) >= 5;
        END LOOP;

        -- fallback: ensure at least 2 teachers
        IF array_length(instance_teachers,1) IS NULL OR array_length(instance_teachers,1) < 2 THEN
            instance_teachers := ARRAY[ teacher_ids[1], teacher_ids[ LEAST(2, teacher_count) ] ]::INT[];
        END IF;

        -------------------------------------------------------------------
        -- Assign teachers to planned activities for this instance
        -------------------------------------------------------------------
        FOR pa IN
            SELECT planned_activity_id, planned_hours
            FROM planned_activity
            WHERE instance_id = v_instance_id
            ORDER BY planned_activity_id
        LOOP
            -- Choose teachers for this activity (max 3, respecting max 3 activities per teacher)
            pa_teachers := ARRAY[]::INT[];
            FOREACH tid IN ARRAY instance_teachers LOOP
                SELECT COUNT(*) INTO teacher_load
                FROM employee_planned_activity epa
                JOIN planned_activity p ON p.planned_activity_id = epa.planned_activity_id
                JOIN course_instance ci ON ci.instance_id = p.instance_id
                JOIN course_layout cl ON cl.course_layout_id = ci.course_layout_id
                WHERE epa.employement_id = tid
                  AND ci.study_year = v_study_year
                  AND cl.study_period = v_study_period;

                IF teacher_load < 3 THEN
                    pa_teachers := array_append(pa_teachers, tid);
                END IF;

                EXIT WHEN array_length(pa_teachers,1) >= 3;
            END LOOP;

            -- If none eligible, pick first teacher
            IF array_length(pa_teachers,1) IS NULL OR array_length(pa_teachers,1) = 0 THEN
                pa_teachers := ARRAY[instance_teachers[1]];
            END IF;

            -- Allocate hours: sum between 50% and 150% of planned_hours
            FOR idx IN 1..array_length(pa_teachers,1) LOOP
                -- Random fraction between 50% and 150% distributed among teachers
                alloc_hours := pa.planned_hours / array_length(pa_teachers,1) * (0.5 + random());
                INSERT INTO employee_planned_activity (employement_id, planned_activity_id, allocated_hours)
                VALUES (pa_teachers[idx], pa.planned_activity_id, alloc_hours)
                ON CONFLICT DO NOTHING;
            END LOOP;
        END LOOP; -- planned_activity
    END LOOP; -- instance
END $$;
-----------------------------------------------------------------------






-- 10. Salary history: at least two entries per employee (using enum type for period)
DO $$
DECLARE
    emp_rec RECORD;
BEGIN
    FOR emp_rec IN SELECT employement_id FROM employee LOOP
        INSERT INTO salary_history (employement_id, year, period)
        VALUES (emp_rec.employement_id, 2021, '1'::period_enum) ON CONFLICT DO NOTHING;

        INSERT INTO salary_history (employement_id, year, period)
        VALUES (emp_rec.employement_id, 2022, '3'::period_enum) ON CONFLICT DO NOTHING;

        IF (emp_rec.employement_id % 5) = 0 THEN
            INSERT INTO salary_history (employement_id, year, period)
            VALUES (emp_rec.employement_id, 2023, '2'::period_enum) ON CONFLICT DO NOTHING;
        END IF;
    END LOOP;
END
$$;

-- 11. Assign skills to employees (1–3 skills each)
DO $$
DECLARE
    emp_rec RECORD;
    skill_ids INT[] := ARRAY(SELECT skill_set_id FROM skill_set ORDER BY skill_set_id);
    s_count INT;
    k INT;
BEGIN
    IF array_length(skill_ids,1) IS NULL THEN
        RAISE NOTICE 'No skills found; skipping employee_skill_set assignment.';
        RETURN;
    END IF;

    FOR emp_rec IN SELECT employement_id FROM employee LOOP
        s_count := 1 + (emp_rec.employement_id % 3); -- 1..3 skills
        FOR k IN 0..(s_count-1) LOOP
            INSERT INTO employee_skill_set (employement_id, skill_set_id)
            VALUES (emp_rec.employement_id, skill_ids[1 + ((emp_rec.employement_id + k) % array_length(skill_ids,1))])
            ON CONFLICT DO NOTHING;
        END LOOP;
    END LOOP;
END
$$;
