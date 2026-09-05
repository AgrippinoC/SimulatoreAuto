#include <algorithm>
#include <cmath>
#include <numbers>
#include "funzioni.h"

Percorso::Percorso(const DatiPercorso& dati) : tratti(dati.tratti) {
}

double Percorso::getPendenza(double posX) const {
    for (const auto& tratto : tratti) {
        if (posX >= tratto.x_inizio && posX < tratto.x_fine) {
            return tratto.pendenza;
        }
    }

    return 0.0;
}