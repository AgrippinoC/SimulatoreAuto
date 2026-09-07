using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Core;
using Grpc.Net.Client;
using MySqlConnector;
using Dapper;
using Test;

namespace csharp;

public record Car(string Id, string Nome, string ImagePath);
public record CarDb(string Nome);
public record Track(string Id, string Nome);
public record TrackDb(string Nome);
public record Weather(int ID, string Description);

public partial class Form1 : Form {

    public static Form1? Istanza;

    private FlowLayoutPanel? _autoPanel = null;
    private FlowLayoutPanel? _pistaPanel = null;
    private FlowLayoutPanel? _meteoPanel = null;
    private FlowLayoutPanel? _mantoPanel = null;
    private FlowLayoutPanel? _ventoPanel = null;
    private string _automobile = string.Empty;
    private string _pista = string.Empty;
    private Button? _autoButton = null;
    private Button? _Reset = null;
    private Weather? _manto = null;
    private double _vento = 0;
    private NumericUpDown? inputVento = null;

    public Form1() {
        InitializeComponent();
        Istanza = this;
        this.Load += async (s, e) => {
            SetupUI();
            await RiempiCar();
            await RiempiPiste();
            RiempiMeteo();
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
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        mainLayout.Controls.Add(_meteoPanel, 0, 7);

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
        _Reset.Click += (s, e) => Resett();
        bottomPanel.Controls.Add(_Reset);
        mainLayout.Controls.Add(bottomPanel, 0, 8);

        this.Controls.Add(mainLayout);
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
        if (_autoPanel == null) return;

        string connectionString = "Server=localhost;Port=3306;Database=simulatore;User ID=root;Password=;";
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "SELECT nome FROM veicoli LIMIT 10";
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
                Image = File.Exists(car.ImagePath) ? new Bitmap(Image.FromFile(car.ImagePath), new Size(100, 70)) : null,
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
        if (_pistaPanel == null) return;

        string connectionString = "Server=localhost;Port=3306;Database=simulatore;User ID=root;Password=;";
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "SELECT nome FROM piste LIMIT 5";
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
        if (_meteoPanel == null) return;

        _meteoPanel.Controls.Clear();
        _meteoPanel.FlowDirection = FlowDirection.TopDown;

        _meteoPanel.Controls.Add(new Label { 
            Text = "Velocità Vento", 
            AutoSize = true, 
            Font = new Font(this.Font, FontStyle.Italic),
            Margin = new Padding(0, 10, 0, 5)
        });
        inputVento = new NumericUpDown {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Increment = 5,
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 10)
        };
        inputVento.ValueChanged += async (s, e) => {
            _vento = (double)inputVento.Value;
            await VerificaEInviaMeteo();
        };
        _meteoPanel.Controls.Add(inputVento);
        
        _meteoPanel.Controls.Add(new Label { 
            Text = "\nManto Stradale", 
            AutoSize = true, 
            Font = new Font(this.Font, FontStyle.Italic) 
        });
        var conditionsManto = new List<Weather> { new(1000, "Asciutto"), new(2000, "Bagnato")};
        _mantoPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        foreach (var w in conditionsManto) {
            var btn = new Button {
                Size = new Size(120, 40),
                Text = w.Description,
                BackColor = Color.LightYellow,
                Margin = new Padding(4),
                Tag = w
            };
            btn.Click += async (s, e) => await SelezionaManto(btn, w);
            _mantoPanel.Controls.Add(btn);
        }
        _meteoPanel.Controls.Add(_mantoPanel);
    }

    private async Task SelezionaManto(Button btn, Weather w) {
        _manto = w;
        if (_mantoPanel != null) {
            foreach (Control c in _mantoPanel.Controls) {
                c.BackColor = Color.LightYellow;
            }
        }
        btn.BackColor = Color.LightGreen;
        await VerificaEInviaMeteo();
    }

    private async Task VerificaEInviaMeteo() {
        if (string.IsNullOrEmpty(_automobile) || string.IsNullOrEmpty(_pista)) {
            MessageBox.Show("Selezionare prima il veicolo e la pista", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_manto != null) {
            double meteoo = _manto.ID + _vento;
            await Invio(meteoo, null, null);
        }
    }

    private async Task carselezione(Button btn, Car car) {
        _automobile = car.Id;
        _autoButton = btn;
        if (_autoPanel != null) {
            foreach (Control c in _autoPanel.Controls) {
                c.Enabled = false;
            }
        }
        btn.Enabled = true;
        btn.BackColor = Color.LightGreen;
        await Invio(0.0, null, _automobile);
    }

    private async Task pistaSelezione(Button btn, Track track) {
        _pista = track.Id;
        if (_pistaPanel != null) {
            foreach (Control c in _pistaPanel.Controls) {
                c.Enabled = false;
            }
        }
        btn.Enabled = true;
        btn.BackColor = Color.LightGreen;
        await Invio(0.0, _pista, null);
    }

    private async Task Invio(double condizion, string? pist, string? automob) {
        if (!string.IsNullOrEmpty(automob)) {
            _automobile = automob;
        }
        if (!string.IsNullOrEmpty(pist)) {
            _pista = pist;
        }
        if (condizion != 0.0 && !string.IsNullOrEmpty(_automobile) && !string.IsNullOrEmpty(_pista)) {
            try {
                using var chan = GrpcChannel.ForAddress("http://localhost:50052");
                var client = new ServiceCtoC.ServiceCtoCClient(chan);
                var reply = await client.InvioCppAsync(new RequestCtoC { Go = condizion, Car = _automobile, Pist = _pista });
                if (_Reset != null) _Reset.Enabled = true;
            } catch (Exception e) {
                MessageBox.Show("Errore di connessione: " + e.Message);
            }
        }
    }

    private void Resett() {
        _automobile = string.Empty;
        _autoButton = null;
        if (_autoPanel != null) {
            foreach (Control c in _autoPanel.Controls) {c.Enabled = true;c.BackColor = Color.LightGray;}}
        _pista = string.Empty;
        if (_pistaPanel != null) {
            foreach (Control c in _pistaPanel.Controls) {c.Enabled = true; c.BackColor = Color.Bisque;}}
        _manto = null;
        _vento = 0;
        inputVento.Value = 0;
        if (_mantoPanel != null) { foreach (Control c in _mantoPanel.Controls) c.BackColor = Color.LightYellow;}
        if (_ventoPanel != null) { foreach (Control c in _ventoPanel.Controls) c.BackColor = Color.LightYellow;}
        if (_Reset != null) _Reset.Enabled = false;
    }

    public static void ShowReport(ReportData request) {
        Form risultat = new Form {
            Text = $"Risultati simulazione {request.Inform}",
            Size = new Size(1000, 700),
            StartPosition = FormStartPosition.CenterScreen
        };

        Label DatiRis = new Label {
            Dock = DockStyle.Top,
            Height = 200,
            Padding = new Padding(15),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Text = $"Risultati {request.Inform} dopo 90 sec.\n\n" +
                   $"|Distanza totale:    {request.Dist:F2} m    |  Velocità Max: {request.VMax:F2} km/h\n" +
                   $"|Velocità Media:    {request.VMedia:F2} km/h |  RPM Max: {request.RpmMax}\n" +
                   $"|Temperatura Media: {request.TMedia:F2} °C   | Tempo da 0 a 100 km/h: {request.TAccela} s\n" +
                   $"|Distanza Frenata: {request.DistanzaFrenata:F2} m"
        };

        FlowLayoutPanel pannel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            Padding = new Padding(4)
        };

        PictureBox graf1 = CreatePictureBox();
        PictureBox graf2 = CreatePictureBox();
        PictureBox graf3 = CreatePictureBox();

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

    private static PictureBox CreatePictureBox() {
        return new PictureBox {
            Size = new Size(500, 500),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
        };
    }
}