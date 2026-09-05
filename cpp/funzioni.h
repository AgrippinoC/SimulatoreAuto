#ifndef FUNZIONI_H
#define FUNZIONI_H
#include <Eigen/Dense>
#include <array>
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
        Eigen::Vector3d Formule(Stato& stato, double coppia, const Eigen::Vector3d& dir, double v, double theta, double mu, int vento, double frenata);
};

class Percorso {
    private:
        std::vector<Tratto> tratti;
    public:
        Percorso(const DatiPercorso& dati);
        double getPendenza(double posX) const;
};

class Veicolo {
    //composizione
    private:
        Stato stato;
        Coefficenti f;
        Motore motore;
        Fisica fisica;
        double c_, mu;

    public:
        Veicolo(const Vector3d startP, double ton, double r, double copp, const std::array<double, 5>& marce, double dif, double rm, double rc, double pm);
        void update(double t, double pendenza, bool bagnato, int vento);
        const Stato& getStato() const { return stato; };
};

#endif