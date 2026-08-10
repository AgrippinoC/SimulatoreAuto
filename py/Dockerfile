FROM python:3.10-slim

RUN pip install --no-cache-dir grpcio grpcio-tools numpy matplotlib

WORKDIR /app
COPY py/ . 
COPY proto/ ./proto

RUN python -m grpc_tools.protoc -I ./proto --python_out=. --grpc_python_out=. ./proto/test.proto

CMD ["python", "main.py"]