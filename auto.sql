USE simulatore;

CREATE TABLE veicoli (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(20),
    peso DOUBLE,
    coppia DOUBLE,
    raggio_ruota DOUBLE,
    m1 DOUBLE,
    m2 DOUBLE,
    m3 DOUBLE,
    m4 DOUBLE,
    m5 DOUBLE,
    differenziale DOUBLE,
    rapp_max DOUBLE,
    rapp_cambio DOUBLE,
    pot_max DOUBLE
);

INSERT INTO veicoli (nome, peso, coppia, raggio_ruota, m1, m2, m3, m4, m5, differenziale, rapp_max, rapp_cambio, pot_max) 
                    VALUES ("Fiat Panda", 920.0, 92.0, 0.32, 3.5, 2.1, 1.4, 1.0, 0.8, 3.7, 6500.0, 5500.0, 55000.0),
                           ("VW Golf", 1250.0, 200.0, 0.33, 3.8, 2.0, 1.3, 0.9, 0.7, 3.9, 6000.0, 5000.0, 81000.0);