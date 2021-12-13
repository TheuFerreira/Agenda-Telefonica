CREATE DATABASE agenda;
USE agenda;

CREATE TABLE contato(
id_contato INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
nome VARCHAR(100) NOT NULL
);

CREATE TABLE telefone(
id_telefone INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
numero BIGINT(15) NOT NULL
);

CREATE TABLE contato_tem_telefone(
id_contato INT NOT NULL,
id_telefone INT NOT NULL,

FOREIGN KEY (id_contato) REFERENCES contato(id_contato),
FOREIGN KEY (id_telefone) REFERENCES telefone(id_telefone),

PRIMARY KEY (id_contato, id_telefone) 
);