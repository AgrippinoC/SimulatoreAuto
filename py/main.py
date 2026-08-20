import grpc
import logging
from concurrent import futures
import test_pb2, test_pb2_grpc
from analisi import telemetria
logging.basicConfig(filename='py.log', level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class ServiceCtoP(test_pb2_grpc.ServiceCtoPServicer):
    def __init__(self):
        self.reset_data()

    def reset_data(self):
        #dict di list
        self.data = { 'tempo': [], 'vel': [], 'marcia': [], 'rpm': [], 'temp': [], 'x': [], 'y': [], 'z': [] }
        self.descr = ""

    def InvioPy(self, request, context):

        self.data['x'].append(request.x)
        self.data['y'].append(request.y)
        self.data['z'].append(request.z)
        self.data['tempo'].append(request.tempo)
        self.data['vel'].append(request.vel)
        self.data['marcia'].append(request.marcia)
        self.data['rpm'].append(request.rpm)
        self.data['temp'].append(request.temperatura)
        self.descr = request.inform
        return test_pb2.ReplyCtoP(rep=True)

    def FinePy(self, request, context):
        logging.info("Fine simulazione ricevuta, avvio analisi in corso...")
        
        ris = telemetria.analizza(self.data)
        
        if ris:
            report = test_pb2.ReportData(
                **ris, #dictionary unpacking (da verificare)
                inform = self.descr
            )

            try:
                with grpc.insecure_channel('host.docker.internal:50053') as channel:
                    stub = test_pb2_grpc.ServicePtoCStub(channel)
                    stub.InviaReport(report)
                    logging.info("Report inviato a C#")
            except Exception as e:
                logging.error(f"Errore invio a C#: {e}")

        self.reset_data()
        return test_pb2.ReplyCtoP(rep=True)

def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=4))
    test_pb2_grpc.add_ServiceCtoPServicer_to_server(ServiceCtoP(), server)
    server.add_insecure_port("[::]:50051")
    #parametri standard per gRPC
    logging.info("Server Python avviato sulla porta 50051")
    server.start()
    server.wait_for_termination()

if __name__ == "__main__":
    serve()
