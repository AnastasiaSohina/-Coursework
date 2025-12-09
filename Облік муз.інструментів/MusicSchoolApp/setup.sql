CREATE DATABASE IF NOT EXISTS music_school;
USE music_school;

-- ==========================
-- 1. Учні
-- ==========================
CREATE TABLE students (
    student_id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    birth_date DATE,
    class VARCHAR(20),
    enrollment_year INT,
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT,
    notes TEXT
);

-- ==========================
-- 2. Викладачі
-- ==========================
CREATE TABLE teachers (
    teacher_id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    specialization VARCHAR(100),   -- інструмент/дисципліна
    phone VARCHAR(20),
    email VARCHAR(100),
    notes TEXT
);

-- ==========================
-- 3. Інструменти
-- ==========================
CREATE TABLE instruments (
    instrument_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    category VARCHAR(100),           -- струнні, духові, клавішні...
    brand VARCHAR(100),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    purchase_date DATE,
    condition_status VARCHAR(50),
    price DECIMAL(10,2),
    location VARCHAR(100),           -- клас/склад/кабінет
    notes TEXT
);

-- ==========================
-- 4. Видача інструментів
-- ==========================
CREATE TABLE instrument_assignments (
    assignment_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id INT NOT NULL,
    instrument_id INT NOT NULL,
    issue_date DATE NOT NULL,
    expected_return_date DATE,
    return_date DATE,
    teacher_id INT,                   -- хто видав
    notes TEXT,

    FOREIGN KEY (student_id) REFERENCES students(student_id) ON DELETE CASCADE,
    FOREIGN KEY (instrument_id) REFERENCES instruments(instrument_id) ON DELETE CASCADE,
    FOREIGN KEY (teacher_id) REFERENCES teachers(teacher_id) ON DELETE SET NULL
);

-- ==========================
-- 5. Ремонт інструментів
-- ==========================
CREATE TABLE instrument_repairs (
    repair_id INT AUTO_INCREMENT PRIMARY KEY,
    instrument_id INT NOT NULL,
    repair_date DATE NOT NULL,
    description TEXT,
    cost DECIMAL(10,2),
    repair_company VARCHAR(100),
    responsible_person VARCHAR(100),

    FOREIGN KEY (instrument_id) REFERENCES instruments(instrument_id) ON DELETE CASCADE
);

-- ==========================
-- 6. Постачальники інструментів
-- ==========================
CREATE TABLE suppliers (
    supplier_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT,
    notes TEXT
);

-- ==========================
-- 7. Закупівлі інструментів
-- ==========================
CREATE TABLE instrument_purchases (
    purchase_id INT AUTO_INCREMENT PRIMARY KEY,
    supplier_id INT NOT NULL,
    instrument_id INT NOT NULL,
    purchase_date DATE NOT NULL,
    quantity INT DEFAULT 1,
    total_cost DECIMAL(10,2),

    FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id),
    FOREIGN KEY (instrument_id) REFERENCES instruments(instrument_id)
);

-- ==========================
-- 8. Навчальні дисципліни
-- ==========================
CREATE TABLE subjects (
    subject_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT
);

-- ==========================
-- 9. Запис учнів на предмети
-- ==========================
CREATE TABLE student_subjects (
    id INT AUTO_INCREMENT PRIMARY KEY,
    student_id INT NOT NULL,
    subject_id INT NOT NULL,
    teacher_id INT,
    start_date DATE,
    end_date DATE,

    FOREIGN KEY (student_id) REFERENCES students(student_id) ON DELETE CASCADE,
    FOREIGN KEY (subject_id) REFERENCES subjects(subject_id) ON DELETE CASCADE,
    FOREIGN KEY (teacher_id) REFERENCES teachers(teacher_id) ON DELETE SET NULL
);

-- ==========================
-- 10. Аудиторії / Кабінети
-- ==========================
CREATE TABLE rooms (
    room_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL,      -- кабінет №3
    type VARCHAR(50),               -- клас, склад, репетиційна
    capacity INT,
    notes TEXT
);

-- ==========================
-- 11. Розклад занять
-- ==========================
CREATE TABLE schedule (
    schedule_id INT AUTO_INCREMENT PRIMARY KEY,
    subject_id INT NOT NULL,
    room_id INT NOT NULL,
    teacher_id INT NOT NULL,
    lesson_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,

    FOREIGN KEY (subject_id) REFERENCES subjects(subject_id),
    FOREIGN KEY (room_id) REFERENCES rooms(room_id),
    FOREIGN KEY (teacher_id) REFERENCES teachers(teacher_id)
);

-- ==========================
-- 12. Користувачі системи (адміни/персонал)
-- ==========================
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    role VARCHAR(50),               -- admin, staff
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
