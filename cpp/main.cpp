#include <mysql_driver.h>
#include <mysql_connection.h>
#include <cppconn/statement.h>
#include <cppconn/resultset.h>
#include <cppconn/exception.h>
#include <Eigen/Dense>
#include <iostream>
#include <iomanip>
#include <array>

#include "grpc_client.h"
#include "test.grpc.pb.h"
#include "struct.h"
#include "funzioni.h"

using namespace Eigen;
using namespace test;


Dati DBveicolo(const std::string& nome) {

    sql::mysql::MySQL_Driver *driver;
    driver = sql::mysql::get_mysql_driver_instance();    
    std::string host = std::getenv("DB_HOST");
    std::string schema = std::getenv("MYSQL_DATABASE");
    std::unique_ptr<sql::Connection> con(driver->connect("tcp://" + host + ":3306", "root", "" ));
    con->setSchema(schema);
    std::unique_ptr<sql::Statement> stmt(con->createStatement());
    std::unique_ptr<sql::ResultSet> res(stmt->executeQuery("SELECT * FROM veicoli WHERE nome = '" + nome + "'"));
    if (!res->next()) throw std::runtime_error("Veicolo " + nome +" non trovato nel DB");

    return { res->getString("nome"),
             static_cast<double>(res->getDouble("peso")),
             static_cast<double>(res->getDouble("coppia")),
             static_cast<double>(res->getDouble("raggio_ruota")),
             static_cast<double>(res->getDouble("m1")),
             static_cast<double>(res->getDouble("m2")),
             static_cast<double>(res->getDouble("m3")),
             static_cast<double>(res->getDouble("m4")),
             static_cast<double>(res->getDouble("m5")),
             static_cast<double>(res->getDouble("differenziale")),
             static_cast<double>(res->getDouble("rapp_max")),
             static_cast<double>(res->getDouble("rapp_cambio")),
             static_cast<double>(res->getDouble("pot_max"))
    };
}

DatiPercorso DBpercorso(const std::string& nome) {
    sql::mysql::MySQL_Driver *driver;
    driver = sql::mysql::get_mysql_driver_instance();
    std::string host = std::getenv("DB_HOST");
    std::string schema = std::getenv("MYSQL_DATABASE");
    std::unique_ptr<sql::Connection> con(driver->connect("tcp://" + host + ":3306", "root", ""));
    con->setSchema(schema);
    std::unique_ptr<sql::Statement> stmt(con->createStatement());
    std::unique_ptr<sql::ResultSet> res(stmt->executeQuery("SELECT id, nome FROM piste WHERE nome = '" + nome + "'"));
    if (!res->next()) throw std::runtime_error("Percorso " + nome + " non trovato nel DB");

    int Id = res->getInt("id");
    DatiPercorso dati;
    dati.nome = res->getString("nome");
    std::unique_ptr<sql::Statement> stmt2(con->createStatement());
    std::unique_ptr<sql::ResultSet> res2(stmt2->executeQuery("SELECT * FROM tratti_pista WHERE pista_id = " + std::to_string(Id) + " ORDER BY x_inizio"));

    while (res2->next()) {
        Tratto tratto;
        tratto.x_inizio = res2->getDouble("x_inizio");
        tratto.x_fine = res2->getDouble("x_fine");
        tratto.pendenza = res2->getDouble("pendenza");
        dati.tratti.push_back(tratto);
    }
    if (dati.tratti.empty()) throw std::runtime_error("Nessun tratto trovato per il percorso " + nome);

    return dati;
}

class Simulazione {
    private:
        double t, t_tot;
        std::unique_ptr<Veicolo> cars;
        Percorso percorso;
        PythonClient grpcPy;
    
    public:
        static constexpr double step = 0.5;
        static constexpr double duration = 90.0;

        Simulazione(double step, double durata, const Dati& dati, const DatiPercorso& datiPercorso) : t(step), t_tot(durata), percorso(datiPercorso),
                    grpcPy(grpc::CreateChannel("py_calcolatore:50051", grpc::InsecureChannelCredentials())) {
            std::array<double, 5> marce{dati.m1, dati.m2, dati.m3, dati.m4, dati.m5};
            
            cars = std::make_unique<Veicolo>(Vector3d::Zero(), dati.peso, dati.raggio_ruota, dati.coppia,
                    marce, dati.differenziale, dati.rapp_max, dati.rapp_cambio, dati.pot_max);
        }

        void run(bool bagnato, int vento, std::string& inform) {

            while (cars->getStato().timer <= t_tot) {
                double x = cars->getStato().pos.x();
                double pendenza = percorso.getPendenza(x);
                cars->update(t, pendenza, bagnato, vento);
                const Stato& s = cars->getStato();
                test::RequestCtoP msg;
                    msg.set_tempo(s.timer);
                    msg.set_x(s.pos.x());
                    msg.set_y(s.pos.y());
                    msg.set_z(s.pos.z());
                    msg.set_vel(s.vel.x());
                    msg.set_marcia(s.marcia);
                    msg.set_rpm(s.rpm);
                    msg.set_temperatura(s.temper);
                    msg.set_inform(inform);
                grpcPy.InviaPython(msg);
            }
            test::Empty ok;
            grpcPy.FinePython(ok);
        }
};

class ServiceCtoCImpl final : public ServiceCtoC::Service {
    private:
        Dati dat;
        DatiPercorso datP;
        int tiposimul;
    public:
        ServiceCtoCImpl() {}

        grpc::Status InvioCpp(grpc::ServerContext* context, const RequestCtoC* request, ReplyCtoC* response) override {
            tiposimul = request->go();
            std::string nome = request->car();
            std::string pista = request->pist();
            std::string inform;
            try{
                dat = DBveicolo(nome);
                datP = DBpercorso(pista);
                Simulazione sim(Simulazione::step, Simulazione::duration, dat, datP);
                switch(tiposimul){
                case 1: 
                    inform = "Simulazione di una " + nome + " in asciutto e senza vento";
                    sim.run(false, 0, inform);
                    response->set_success(true);
                    break;
                case 2: 
                    inform = "Simulazione di una " + nome + " in asciutto con vento frontale";
                    sim.run(false, 2, inform);
                    response->set_success(true);
                    break;
                case 3: 
                    inform = "Simulazione di una " + nome + " sul bagnato e senza vento";
                    sim.run(true, 0, inform);
                    response->set_success(true);
                    break;
                default:
                    response->set_success(false);
                }
                return grpc::Status::OK;

            } catch (const std::exception& e) {
                std::cerr << "Errore in ServiceCtoCImpl" << std::endl;
                response->set_success(false);
                return grpc::Status(grpc::StatusCode::INTERNAL, e.what());
            }
        }
};

int main() {
    std::string server_address("0.0.0.0:50052");
    ServiceCtoCImpl service;

    grpc::ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(&service);
    
    std::unique_ptr<grpc::Server> server(builder.BuildAndStart());
    server->Wait();
    return 0;
}