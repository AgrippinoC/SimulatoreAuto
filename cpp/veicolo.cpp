#include <algorithm>
#include <cmath>
#include "funzioni.h"

using namespace Eigen;
constexpr double pi = 3.14;
constexpr double g = 9.81;

void Motore::LogicaMotor(Stato& stato, double v, double t){
    
    //calcolo RPM
    double circ = 2.0 * pi * stato.ruota;
    double rotazioni = ( v / circ ) * 60.0;
    stato.rpm = rotazioni * stato.rapporto[stato.marcia] * stato.differenziale;
    stato.rpm = std::max(stato.rpm, 800.0);
            
    //cambio marcia
    if(stato.rpm > stato.rCambio && stato.marcia < 4){
        stato.marcia++;
    } else if (stato.rpm < 1200 && stato.marcia > 0) {
        stato.marcia--;
    }
    stato.rpm = rotazioni * stato.rapporto[stato.marcia] * stato.differenziale;
    stato.rpm = std::max(stato.rpm, 800.0);

    //temperatura
    double risc = (stato.rpm / stato.rMax) * 0.5;
    double raff = (stato.temper - f.ambiente) * 0.02;
    stato.temper += (risc - raff) * t;
}

Vector3d Fisica::Formule(Stato& stato, double coppia, const Vector3d& dir, double v, double theta, double mu, int vento, double frenata){

    double coppiaR = coppia * stato.rapporto[stato.marcia] * stato.differenziale * 0.92;
    double Fmotrice_max = stato.Pmax / std::max(std::abs(v), 5.0);
    double trazione = coppiaR / stato.ruota;
    switch(vento){
        case 1: vel = v - 40.0; break; //vento in coda
        case 2: vel = v + 40.0; break; //vento frontale
        default: vel = v; break;
    }
    Vector3d Fmotrice = dir * std::min(trazione, Fmotrice_max);
    Vector3d Fattrito = -dir * (f.av * stato.mass * g * std::cos(theta));
    Vector3d ResAerod = -dir * (0.5 * f.rho * f.cd * f.a * vel * std::abs(vel));
    Vector3d Fgravit = -dir * (stato.mass * g * std::sin(theta));
    double maxFrenata = 8000.0; //Newton
    Vector3d Ffreno = Vector3d::Zero();
    
    if (std::abs(v) > 0.1) { //se si muove
        Ffreno = -dir * (frenata * maxFrenata * (v > 0 ? 1.0 : -1.0));
    }

    Vector3d Ftot = Fmotrice + Fattrito + ResAerod + Ffreno + Fgravit;
    double Fopposta = Ftot.dot(dir);
    Ftot = dir * Fopposta;
    double Aderenza = mu * stato.mass * g * std::cos(theta);
    if(Ftot.norm() > Aderenza) Ftot = Ftot.normalized() * Aderenza;

    return Ftot;
}

Veicolo::Veicolo(const Vector3d startP, double ton, double r, double copp, const std::array<double, 5>& marce, double dif, double rm, double rc, double pm){
    stato.timer = 0.0;
    stato.pos = startP;
    stato.vel = stato.acc = Vector3d::Zero();
    stato.mass = ton;
    stato.ruota = r;
    stato.rpm = 800.0;
    std::copy(marce.begin(), marce.end(), stato.rapporto);
    stato.differenziale = dif;
    stato.rMax = rm, stato.rCambio = rc, stato.Pmax = pm;
    stato.marcia = 0;
    stato.temper = f.ambiente;
    c_ = copp;
}
    
void Veicolo::update(double t, double pendenza, bool bagnato, int vento){

    //preliminari
    if (bagnato) { mu = f.mu_b; } else { mu = f.mu;}
    double theta = std::atan(pendenza);
            
    Vector3d dir(std::cos(theta), 0.0, std::sin(theta));
    dir.normalize();
    double v_longit = stato.vel.dot(dir);

    double pedaleAcceleratore = 1.0;
    double frenata = 0.0;

    //se discesa oltre 27.7 ms si frena
    double v_attuale = stato.vel.norm();
    if (pendenza < 0 && v_attuale > 27.7) {
        pedaleAcceleratore = 0.0;
        frenata = 0.6;//freno 60%
    }
    double ridCoppia = std::max(0.3, std::min(stato.rpm / 2500.0, 1.0));
    double c = c_ * ridCoppia * pedaleAcceleratore;

    Vector3d Ftot(fisica.Formule(stato, c, dir, v_longit, theta, mu, vento, frenata));

    //cinematica
    stato.acc = Ftot / stato.mass;
    stato.pos += stato.vel * t + 0.5 * stato.acc * t * t;
    stato.vel += stato.acc * t;
    stato.vel = dir * stato.vel.dot(dir);

    if (frenata > 0.1 && std::abs(v_longit) < 0.2) {
        stato.vel = Vector3d::Zero();
        stato.acc = Vector3d::Zero();
    }

    motore.LogicaMotor(stato, v_longit, t);
    stato.timer += t;
}