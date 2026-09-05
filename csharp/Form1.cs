using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Core;
using Grpc.Net.Client;
using System.Data.SqlClient;
using MySqlConnector;
using Dapper;
using Test;

namespace csharp;

public record Car(string Id, string Nome, string ImagePath);
public record CarDb(string Nome);
public record Track(string Id, string Nome);
public record TrackDb(string Nome);
public record Weather(double ID, string Description, Color ButtonColor);

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

    private FlowLayoutPanel? _autoPanel = null;
    private FlowLayoutPanel? _pistaPanel = null;
    private FlowLayoutPanel? _meteoPanel = null;
    private string _automobile = string.Empty;
    private string _pista = string.Empty;
    private Button? _autoButton = null;
    private Button? _Reset = null;


    public Form1() {
        InitializeComponent();
        Istanza = this;
        this.Load += async(s, e) => {
          SetupUI();
          StartGrpcServer();  
        };
    }

    private void StartGrpcServer() {
        Task.Run(() => {
            try {
                var server = new Grpc.Core.Server {
                    Services = { ServicePtoC.BindService(new PtoCService()) },
                    Ports = { new ServerPort("0.0.0.0", 50053, ServerCredentials.Insecure) }
                };
                server.Start();
            } catch (Exception ex) {
                Invoke(() => MessageBox.Show("Errore StartGrpcServer: " + ex.Message));
            }
        });
    }

    private void SetupUI() {

        this.Text = "Simulatore di Guida";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        var mainLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 9,
            ColumnCount = 1,
            Padding = new Padding(30)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label per le auto, pista, meteo e poi reset
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        
        mainLayout.Controls.Add(Titolo("SELEZIONE AUTOMOBILE"), 0, 0);
        _autoPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true
        };
        mainLayout.Controls.Add(_autoPanel, 0, 1);

        mainLayout.Controls.Add(Titolo("SELEZIONE PERCORSO"), 0, 2);
        _pistaPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true
        };
        mainLayout.Controls.Add(_pistaPanel, 0, 3);

        mainLayout.Controls.Add(Titolo("SELEZIONE CONDIZIONE METEO "), 0, 5);
        _meteoPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true
        };
        mainLayout.Controls.Add(_meteoPanel, 0, 6);

        FlowLayoutPanel bottomPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        _Reset = new Button {
            Text = "RESET",
            Size = new Size(160, 40),
            BackColor = Color.MediumSeaGreen,
            Padding = new Padding(5),
            Enabled = false
        };
        _Reset.Click += async (s, e) => Resett();
        bottomPanel.Controls.Add(_Reset);
        mainLayout.Controls.Add(bottomPanel, 0, 8);

        this.Controls.Add(mainLayout);

        RiempiCar();
        RiempiPiste();
        RiempiMeteo();
    }

    private Label Titolo(string title) {
        return new Label {
            Text = title,
            Font = new Font(this.Font.FontFamily, 15, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 5)
        };
    }

    private async Task RiempiCar() {

        string connectionString = "Server=localhost;Port=3306;Database=simulatore;User ID=root;Password=;";
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "SELECT nome FROM veicoli";
        var veicoliDb = await connection.QueryAsync<CarDb>(query);
        var cars = veicoliDb.Select(v => new Car(
            Id: v.Nome,
            Nome: v.Nome,
            ImagePath: $"./img/{v.Nome}.png"
        )).ToList();

        _autoPanel.Controls.Clear();

        foreach (var car in cars) {
            var btn = new Button {
                Size = new Size(120, 80),
                Text = car.Nome,
                TextImageRelation = TextImageRelation.ImageAboveText,
                TextAlign = ContentAlignment.BottomCenter,
                Image = new Bitmap(Image.FromFile(car.ImagePath), new Size(100, 70)),
                ImageAlign = ContentAlignment.TopCenter,
                BackColor = Color.LightGray,
                Margin = new Padding(5),
                Tag = car
            };
            btn.Click += async (s, e) => await carselezione(btn, car);
            _autoPanel.Controls.Add(btn);
        }
    }
    
    private async Task RiempiPiste() {

        string connectionString = "Server=localhost;Port=3306;Database=simulatore;User ID=root;Password=;";
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "SELECT nome FROM piste";
        var pistaDb = await connection.QueryAsync<TrackDb>(query);
        var tracks = pistaDb.Select(v => new Track(
            Id: v.Nome,
            Nome: v.Nome
        )).ToList();

        _pistaPanel.Controls.Clear();

        foreach (var track in tracks) {
            var btn = new Button {
                Size = new Size(100, 80),
                Text = track.Nome,
                BackColor = Color.Bisque,
                Margin = new Padding(5),
                Tag = track
            };
            btn.Click += async (s, e) => await pistaSelezione(btn, track);
            _pistaPanel.Controls.Add(btn);
        }
    }

    private void RiempiMeteo() {
        var Conditions = new List<Weather> {
            new(1.0, "Asciutto, Senza Vento", Color.LightYellow),
            new(2.0, "Asciutto, Vento 40 km/h", Color.LightYellow),
            new(3.0, "Bagnato, Senza Vento", Color.LightYellow)
        };

        _meteoPanel.Controls.Clear();

        foreach (var w in Conditions) {
            var btn = new Button {
                Size = new Size(180, 50),
                Text = w.Description,
                BackColor = w.ButtonColor,
                Margin = new Padding(8),
                Tag = w
            };

            btn.Click += async (s, e) => await meteoselezione(w);
            _meteoPanel.Controls.Add(btn);
        }
    }

    private async Task carselezione(Button btn, Car car) {
        _automobile = car.Id;
        _autoButton = btn;
        foreach (Control c in _autoPanel.Controls) {
            c.Enabled = false;
        }
        btn.Enabled = true;
        btn.BackColor = Color.LightGreen;
        await Invio(0.0, null, _automobile);
    }

    private async Task pistaSelezione(Button btn, Track track) {
        _pista = track.Id;
        foreach (Control c in _pistaPanel.Controls) {
            c.Enabled = false;
        }
        btn.Enabled = true;
        btn.BackColor = Color.LightGreen;
        await Invio(0.0, _pista, null);
    }

    private async Task meteoselezione(Weather weather) {
        if (string.IsNullOrEmpty(_automobile) || string.IsNullOrEmpty(_pista)) {
            MessageBox.Show("Selezionare prima il veicolo e la pista", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await Invio(weather.ID, null, null);
    }

    private async Task Invio(double condizion, string? pist, string? automob) {
        if(!string.IsNullOrEmpty(automob)) {
            _automobile = automob;
        }
        if(!string.IsNullOrEmpty(pist)) {
            _pista = pist;
        }
        if (condizion != 0.0 && !string.IsNullOrEmpty(_automobile) && !string.IsNullOrEmpty(_pista)) {
            try {
                using var chan = GrpcChannel.ForAddress("http://localhost:50052");
                var client = new ServiceCtoC.ServiceCtoCClient(chan);
                var reply = await client.InvioCppAsync(new RequestCtoC { Go = condizion, Car = _automobile, Pist = _pista});
                _Reset.Enabled = true;
            } catch (Exception e){
                MessageBox.Show("Errore di connessione: " + e.Message);
            }
        }
    }

    private void Resett() {
        _automobile = string.Empty;
        _autoButton = null;
        foreach (Control c in _autoPanel.Controls) {
            c.Enabled = true;
            c.BackColor = Color.LightGray;
        }
        _pista = string.Empty;
        foreach (Control c in _pistaPanel.Controls) {
            c.Enabled = true;
            c.BackColor = Color.Bisque;
        }
        _Reset.Enabled = false;
    }

    public static void ShowReport(ReportData request) {
        
        Form risultat = new Form {
            Text = $"Risultati simulazione {request.Inform}",
            Size = new Size(950, 650),
            StartPosition = FormStartPosition.CenterScreen
        };

        Label DatiRis = new Label {

            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(15),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Text = $"Risurlati {request.Inform} dopo 90 sec.\n\n" +
                   $"|Distanza totale:    {request.Dist:F2} m    |  Velocità Max: {request.VMax:F2} km/h\n" +
                   $"|Velocità Media:    {request.VMedia:F2} km/h |  RPM Max: {request.RpmMax}\n" +
                   $"|Temperatura Media: {request.TMedia:F2} °C | Tempo da 0 a 100 km/h: {request.TAccela}"
        };

        FlowLayoutPanel pannel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            Padding = new Padding(4)
        };

        PictureBox graf1 = new PictureBox {
            Size = new Size(320, 320),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
        };
        PictureBox graf2 = new PictureBox {
            Size = new Size(320, 320),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
        };
        PictureBox graf3 = new PictureBox {
            Size = new Size(320, 320),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
        };
        if (!request.ImgData.IsEmpty) {
            using var img = new MemoryStream(request.ImgData.ToByteArray());
            graf1.Image = new Bitmap(img);
        }
        if (!request.ImgData2.IsEmpty) {
            using var ms = new MemoryStream(request.ImgData2.ToByteArray());
            graf2.Image = new Bitmap(ms);
        }
        if (!request.ImgData3.IsEmpty) {
            using var gm = new MemoryStream(request.ImgData3.ToByteArray());
            graf3.Image = new Bitmap(gm);
        }

        pannel.Controls.Add(graf1);
        pannel.Controls.Add(graf2);
        pannel.Controls.Add(graf3);
        risultat.Controls.Add(pannel);
        risultat.Controls.Add(DatiRis);
        risultat.ShowDialog();
    }

}