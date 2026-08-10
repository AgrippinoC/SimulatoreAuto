#include "grpc_client.h"
#include <iostream>

PythonClient::PythonClient(std::shared_ptr<grpc::Channel> channel) : stub_(test::ServiceCtoP::NewStub(channel)) {}

void PythonClient::InviaPython(const test::RequestCtoP& msg) {

    test::ReplyCtoP reply;
    grpc::ClientContext context;

    grpc::Status status = stub_->InvioPy(&context, msg, &reply);

    if (!status.ok()) {
        std::cerr << "Errore gRPC lato Python: " << status.error_code() << " : " << status.error_message() << std::endl;
    }
}

void PythonClient::FinePython(const test::Empty& ok) {

    test::ReplyCtoP reply;
    grpc::ClientContext context;
    stub_->FinePy(&context, ok, &reply);

}