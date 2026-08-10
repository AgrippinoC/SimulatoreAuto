#ifndef GRPC_CLIENT_H
#define GRPC_CLIENT_H

#include <grpcpp/grpcpp.h>
#include "test.grpc.pb.h"
#include <memory>

class PythonClient {
public:
    explicit PythonClient(std::shared_ptr<grpc::Channel> channel);
    void InviaPython(const test::RequestCtoP& msg);
    void FinePython(const test::Empty& stop);

private:
    std::unique_ptr<test::ServiceCtoP::Stub> stub_;
};

#endif