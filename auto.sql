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
                           ("VW Golf", 1250.0, 200.0, 0.33, 3.8, 2.0, 1.3, 0.9, 0.7, 3.9, 6000.0, 5000.0, 81000.0),
                           ("AlfaRomeo Giulietta", 1365.0, 250.0, 0.3, 3.9, 2.1, 1.4, 1.1, 0.9, 3.5, 6500.0, 5500.0, 125000.0),
                           ("BMW M3", 1655.0, 400.0, 0.3, 4.0, 2.4, 1.5, 1.2, 1.0, 3.8, 8400.0, 8300.0, 309000.0);

CREATE TABLE piste (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(20)
);
INSERT INTO piste (nome) 
                    VALUES ("Pianura"),
                           ("Collina"),
                           ("Conca");

CREATE TABLE tratti_pista (
    id INT PRIMARY KEY AUTO_INCREMENT,
    pista_id INT NOT NULL,
    x_inizio DOUBLE NOT NULL,
    x_fine DOUBLE NOT NULL,
    pendenza DOUBLE NOT NULL,
    FOREIGN KEY (pista_id) REFERENCES piste(id)
);
INSERT INTO tratti_pista (pista_id, x_inizio, x_fine, pendenza)
                VALUES (1, 0, 1000, 0.0),
                
                        (2, 0, 150, 0.0),
                        (2, 150, 800, 0.06),
                        (2, 800, 1500, 0.0),
                        (2, 1500, 2000, 0.09),
                        (2, 2000, 2400, 0.00),
                        (2, 2400, 2700, -0.05),
                        (2, 2700, 4000, 0.0),
                        (2, 4000, 5000, 0.05),
                        (2, 5000, 6000, 0.0),
                
                        (3, 0, 500, 0.0),
                        (3, 500, 800, -0.02),
                        (3, 800, 1100, 0.00),
                        (3, 1100, 1500, -0.05),
                        (3, 1500, 2500, 0.0),
                        (3, 2500, 2900, 0.05),
                        (3, 2900, 3200, 0.0),
                        (3, 3200, 3500, 0.02),
                        (3, 3500, 4000, 0.0);