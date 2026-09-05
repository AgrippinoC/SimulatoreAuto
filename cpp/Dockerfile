FROM gcc:latest

#sql e tutto il resto
RUN apt-get update && apt-get install -y \
    libmysqlcppconn-dev \
    wget \
    unzip \
    protobuf-compiler protobuf-compiler-grpc \
    libgrpc++-dev libprotobuf-dev \
    pkg-config \
    && rm -rf /var/lib/apt/lists/*

#Eigen
RUN wget https://gitlab.com/libeigen/eigen/-/archive/3.4.0/eigen-3.4.0.zip \
    && unzip eigen-3.4.0.zip \
    && mv eigen-3.4.0 /usr/local/include/eigen3 \
    && rm eigen-3.4.0.zip

WORKDIR /usr/src/app

COPY cpp/*.cpp ./
COPY cpp/*.h ./
COPY proto ./proto

#gRPC
RUN protoc -I proto \
    --cpp_out=. \
    --grpc_out=. \
    --plugin=protoc-gen-grpc=`which grpc_cpp_plugin`\
    proto/test.proto

RUN g++ -std=c++17 main.cpp veicolo.cpp percorso.cpp grpc_client.cpp test.pb.cc test.grpc.pb.cc \
    -I /usr/include/cppconn -I /usr/local/include/eigen3 -I .\
    -lmysqlcppconn $(pkg-config --cflags --libs grpc++ protobuf) \
    -pthread \
    -o maincpp

CMD ["./maincpp"]