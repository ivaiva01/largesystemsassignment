
DROP TABLE IF EXISTS Occurrences
DROP TABLE IF EXISTS Words
DROP TABLE IF EXISTS Files

CREATE TABLE Words
(
    word_id INT IDENTITY (1,1) PRIMARY KEY,
    word    NVARCHAR(255) UNIQUE
)

CREATE TABLE Files
(
    file_id   INT IDENTITY (1,1) PRIMARY KEY,
    file_name VARCHAR(255),
    content   BINARY
)

CREATE TABLE Occurrences
(
    word_id INT NOT NULL,
    file_id INT NOT NULL,
    count   INT,

    PRIMARY KEY (word_id, file_id),
    FOREIGN KEY (word_id) REFERENCES Words (word_id),
    FOREIGN KEY (file_id) REFERENCES Files (file_id)
)