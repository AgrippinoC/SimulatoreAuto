using Grpc.Net.Client;
using Test;
using System.IO;
using System.Drawing;
using System.Runtime.CompilerServices;
using Grpc.Core;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace csharp;

public class PtoCService : ServicePtoC.ServicePtoCBase {
    public override Task<ReplyCtoC> InviaReport(ReportData request, ServerCallContext context) {
        Form1.Istanza?.Invoke(() => {
            Form1.ShowReport(request);
        });
        return Task.FromResult(new ReplyCtoC { Success = true });
    }
}

public partial class Form1 : Form {
    public static Form1? Istanza; 
    private Button? PANDA, GOLF, RESET;    //messi nullable dato che potevano essere null
    private string _auto = string.Empty;

    public Form1() {
        InitializeComponent();
        Istanza = this;
        this.Load += (s, e) => {
            ZonaPuls(s, e);
            StartGrpcServer();
        };
    }

    private void StartGrpcServer() {
    Task.Run(() => {
        try {
            var server = new Grpc.Core.Server {
                Services = {ServicePtoC.BindService(new PtoCService()) },
                Ports = {new ServerPort("0.0.0.0", 50053, ServerCredentials.Insecure) }
            };
            server.Start();
        } catch (Exception ex) {
            Invoke(() => MessageBox.Show("Errore StartGrpcServer: " + ex.Message));
        }
        });
    }

    private async Task Invio(double condizion, string automob) {
        if(!string.IsNullOrEmpty(automob)) {
            _auto = automob;
            PANDA.Enabled = false; GOLF.Enabled = false;
        }

        if (condizion != 0.0 && !string.IsNullOrEmpty(_auto)) {
            try {
                using var chan = GrpcChannel.ForAddress("http://localhost:50052"); //using così non uso Dispose
                var reply = await client.InvioCppAsync(new RequestCtoC { Go = condizion, Car = _auto });
                Debug.WriteLine(reply);
                RESET.Enabled = true;
            } catch (Exception e){
                MessageBox.Show("Errore di connessione: " + e.Message);
            }
        }
    }

    private void ZonaPuls(object sender, EventArgs e) {
        //qua sono messe tutte le "cose UI"
        Label selA = new Label {
            AutoSize = true,
            Text = "SELEZIONE AUTOMOBILE",
            Location = new Point(150, 150)
        };
        this.Controls.Add(selA);

        PANDA = new Button() {
            Location = new Point(150, 200),
            Image = new Bitmap(Image.FromFile("./img/panda.png"), new Size(100, 70)),
            AutoSize = true,
            BackColor = Color.AliceBlue,
            Padding = new Padding(5)
        };
        PANDA.Click += (s, args) => { Invio(0.0, "Fiat Panda"); };
        this.Controls.Add(PANDA);

        GOLF = new Button() {
            Location = new Point(300, 200),
            Image = new Bitmap(Image.FromFile("./img/golg.png"), new Size(100, 70)),
            AutoSize = true,
            BackColor = Color.AliceBlue,
            Padding = new Padding(5)
        };
        GOLF.Click += (s, args) => { Invio(0.0, "VW Golf"); };
        this.Controls.Add(GOLF);

        Label selC = new Label {
            AutoSize = true,
            Text = "SELEZIONE CONDIZIONE METEO",
            Location = new Point(150, 300)
        };
        this.Controls.Add(selC);

        Button ANV = new Button() {
            Location = new Point(150, 350),
            Text = "Asciutto, Senza Vento",
            AutoSize = true,
            BackColor = Color.Cornsilk,
            Padding = new Padding(5)
        };
        ANV.Click += (s, args) => { Invio(1.0, null); };
        this.Controls.Add(ANV);

        Button AV = new Button() {
            Location = new Point(350, 350),
            Text = "Asciutto, Vento a 40",
            AutoSize = true,
            BackColor = Color.Cornsilk,
            Padding = new Padding(5)
        };
        AV.Click += (s, args) => { Invio(2.0, null); };
        this.Controls.Add(AV);

        Button BNV  = new Button() {
            Location = new Point(550, 350),
            Text = "Bagnato, Senza Vento",
            AutoSize = true,
            BackColor = Color.Cornsilk,
            Padding = new Padding(5)
        };
        BNV.Click += (s, args) => { Invio(3.0, null); };
        this.Controls.Add(BNV);

        RESET = new Button() {
            Location = new Point(350, 400),
            Text = "RESET",
            AutoSize = true,
            BackColor = Color.MediumSeaGreen,
            Padding = new Padding(5)
        };
        RESET.Click += (s, args) => { PANDA.Enabled = true; GOLF.Enabled = true; RESET.Enabled = false; _auto = "";};
        this.Controls.Add(RESET);
    }

    public static void ShowReport(ReportData request) {

        Form risultat = new Form {
            Text = "Risultati simulazione dopo 90 secondi",
            Size = new Size(900, 600),
            StartPosition = FormStartPosition.CenterScreen
        };

        Label DatiRis = new Label {
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(10),
            Text = $"Risultati di {request.Inform} dopo 90 secondi\n" +
                    $"Distanza percorsa: {request.Dist:F2} m\n" +
                    $"Velocità media: {request.VMedia:F2} km/h\nVelocità massima: {request.VMax:F2} km/h\n" +
                    $"Temperatura media: {request.TMedia:F2}°\n" +
                    $"Giri massimi: {request.RpmMax}\n"
        };

        FlowLayoutPanel pannel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true
        };

        PictureBox graf1 = new PictureBox {
            Size = new Size(400,400),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        PictureBox graf2 = new PictureBox {
            Size = new Size(400,400),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        if (!request.ImgData.IsEmpty) {
            using var img = new MemoryStream(request.ImgData.ToByteArray());
            graf1.Image = new Bitmap(img);
        }
        if (!request.ImgData2.IsEmpty) {
            using var ms = new MemoryStream(request.ImgData2.ToByteArray());
            graf2.Image = new Bitmap(ms);
        }

        pannel.Controls.Add(graf1);
        pannel.Controls.Add(graf2);
        risultat.Controls.Add(pannel);
        risultat.Controls.Add(DatiRis);
        risultat.ShowDialog();
    }
}
