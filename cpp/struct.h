#ifndef STRUCT_H
#define STRUCT_H

#include <Eigen/Dense>
#include <string>

using namespace Eigen;

struct Dati {
    std::string nome;
    double peso;
    double coppia;
    double raggio_ruota;
    double m1, m2, m3, m4, m5;
    double differenziale, rapp_max, rapp_cambio, pot_max;
};

struct Tratto {
    double x_inizio;
    double x_fine;
    double pendenza;
};

struct DatiPercorso {
    std::string nome;
    std::vector<Tratto> tratti;
};

struct Stato {
    double timer;
    Vector3d pos, vel, acc;
    double mass, ruota, rpm;
    double rapporto[5], differenziale;
    double rMax, rCambio, Pmax;
    int marcia;
    double temper;
    double angolo, velAng;
};

struct Coefficenti {
    double rho = 1.225; //densità aria
    double cd = 0.32; //coefficiente resistenza aerodinamica
    double a = 2.2; //area frontale
    double av = 0.012; //coefficiente resistenza rotolamento
    double mu = 0.9; //attrito asfalto-gomme
    double mu_b = 0.4; //attrito asfalto-gomme BAGNATO
    double ambiente = 25.0; //temperatura amniente
};
#endif