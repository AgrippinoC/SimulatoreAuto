#ifndef FUNZIONI_H
#define FUNZIONI_H
#include <Eigen/Dense>
#include "struct.h"

class Motore {
    private:
        Coefficenti f;
    public:
        void LogicaMotor(Stato& stato, double v, double t);
};

class Fisica {
    private:
        Coefficenti f;
        double mu, vel;
    public:
        Eigen::Vector3d Formule(Stato& stato, double coppia, Eigen::Vector3d dir, double v, double theta, double mu, int vento, double frenata);
};

class Veicolo {
    private:
        Stato stato;
        Coefficenti f;
        Motore motore;
        Fisica fisica;
        double c_, mu, pendenza;

    public:
        Veicolo(const Vector3d startP, double ton, double r, double copp, double marce[5], double dif, double rm, double rc, double pm);
        void update(double t, bool bagnato, int vento);
        const Stato& getStato() const { return stato; };
};

#endif