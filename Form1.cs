using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RacunApp
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal TaxRate { get; set; }

        public Product(int id, string name, decimal price, decimal taxRate)
        {
            Id = id;
            Name = name;
            Price = price;
            TaxRate = taxRate;
        }
    }

    public class InvoiceItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public InvoiceItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public decimal AmountWithoutTax
        {
            get { return Product.Price * Quantity; }
        }

        public decimal TaxAmount
        {
            get { return AmountWithoutTax * Product.TaxRate; }
        }

        public decimal AmountWithTax
        {
            get { return AmountWithoutTax + TaxAmount; }
        }
    }

    public class Invoice
    {
        public List<InvoiceItem> Items { get; private set; }

        public Invoice()
        {
            Items = new List<InvoiceItem>();
        }

        public void AddItem(Product product, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Količina mora biti veća od 0.");

            Items.Add(new InvoiceItem(product, quantity));
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public decimal TotalWithoutTax
        {
            get { return Items.Sum(i => i.AmountWithoutTax); }
        }

        public decimal TotalTax
        {
            get { return Items.Sum(i => i.TaxAmount); }
        }

        public decimal TotalWithTax
        {
            get { return Items.Sum(i => i.AmountWithTax); }
        }
    }

    public partial class Form1 : Form
    {
        private List<Product> products;
        private Invoice invoice;

        private ComboBox cmbProizvodi;
        private NumericUpDown nudKolicina;
        private Button btnDodaj;
        private Button btnIzracunaj;
        private Button btnOcisti;
        private DataGridView dgvRacun;
        private Label lblUkupnoBezPDV;
        private Label lblUkupnoPDV;
        private Label lblUkupnoSaPDV;
        private Label lblStatus;

        public Form1()
        {
            InitializeForm();
            InicijalizirajProizvode();
            invoice = new Invoice();
        }

        private void InitializeForm()
        {
            this.Text = "Račun sa PDV-om - Detaljan prikaz";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.LightGray;

            // GORNJI DIO - UNOS PROIZVODA
            Label lblProizvod = new Label();
            lblProizvod.Text = "Proizvod:";
            lblProizvod.Location = new Point(20, 20);
            lblProizvod.Size = new Size(80, 20);
            lblProizvod.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            cmbProizvodi = new ComboBox();
            cmbProizvodi.Location = new Point(110, 20);
            cmbProizvodi.Size = new Size(250, 25);
            cmbProizvodi.DropDownStyle = ComboBoxStyle.DropDownList;

            Label lblKolicina = new Label();
            lblKolicina.Text = "Količina:";
            lblKolicina.Location = new Point(385, 20);
            lblKolicina.Size = new Size(70, 20);
            lblKolicina.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            nudKolicina = new NumericUpDown();
            nudKolicina.Location = new Point(460, 18);
            nudKolicina.Size = new Size(80, 25);
            nudKolicina.Minimum = 1;
            nudKolicina.Maximum = 1000;
            nudKolicina.Value = 1;

            btnDodaj = new Button();
            btnDodaj.Text = "Dodaj artikl";
            btnDodaj.Location = new Point(560, 18);
            btnDodaj.Size = new Size(120, 28);
            btnDodaj.BackColor = Color.LimeGreen;
            btnDodaj.ForeColor = Color.White;
            btnDodaj.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnDodaj.Click += BtnDodaj_Click;

            btnOcisti = new Button();
            btnOcisti.Text = "Očisti račun";
            btnOcisti.Location = new Point(700, 18);
            btnOcisti.Size = new Size(120, 28);
            btnOcisti.BackColor = Color.Red;
            btnOcisti.ForeColor = Color.White;
            btnOcisti.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnOcisti.Click += BtnOcisti_Click;

            // TABELA SA RACUNOM
            dgvRacun = new DataGridView();
            dgvRacun.Location = new Point(20, 70);
            dgvRacun.Size = new Size(950, 320);
            dgvRacun.ReadOnly = true;
            dgvRacun.AllowUserToAddRows = false;
            dgvRacun.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRacun.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvRacun.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkBlue;
            dgvRacun.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRacun.AlternatingRowsDefaultCellStyle.BackColor = Color.LightYellow;
            dgvRacun.RowTemplate.Height = 30;

            dgvRacun.Columns.Add("Artikal", "Artikal");
            dgvRacun.Columns.Add("Cijena", "Cijena (KM)");
            dgvRacun.Columns.Add("Količina", "Količina");
            dgvRacun.Columns.Add("Iznos", "Iznos bez PDV-a (KM)");
            dgvRacun.Columns.Add("PDV%", "PDV %");
            dgvRacun.Columns.Add("PDV", "PDV (KM)");
            dgvRacun.Columns.Add("SaPDV", "Iznos sa PDV-om (KM)");

            // SUMA I TOTALI
            Label lblSeparator1 = new Label();
            lblSeparator1.Location = new Point(20, 410);
            lblSeparator1.Size = new Size(950, 2);
            lblSeparator1.BackColor = Color.Black;

            lblUkupnoBezPDV = new Label();
            lblUkupnoBezPDV.Location = new Point(20, 430);
            lblUkupnoBezPDV.Size = new Size(450, 35);
            lblUkupnoBezPDV.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblUkupnoBezPDV.Text = "UKUPNO BEZ PDV-a: 0.00 KM";
            lblUkupnoBezPDV.BackColor = Color.White;
            lblUkupnoBezPDV.Padding = new Padding(10, 5, 10, 5);
            lblUkupnoBezPDV.ForeColor = Color.DarkBlue;

            lblUkupnoPDV = new Label();
            lblUkupnoPDV.Location = new Point(20, 475);
            lblUkupnoPDV.Size = new Size(450, 35);
            lblUkupnoPDV.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblUkupnoPDV.Text = "UKUPNO PDV: 0.00 KM";
            lblUkupnoPDV.BackColor = Color.LightYellow;
            lblUkupnoPDV.Padding = new Padding(10, 5, 10, 5);
            lblUkupnoPDV.ForeColor = Color.OrangeRed;

            lblUkupnoSaPDV = new Label();
            lblUkupnoSaPDV.Location = new Point(20, 520);
            lblUkupnoSaPDV.Size = new Size(450, 40);
            lblUkupnoSaPDV.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblUkupnoSaPDV.Text = "UKUPNO SA PDV-om: 0.00 KM";
            lblUkupnoSaPDV.BackColor = Color.LimeGreen;
            lblUkupnoSaPDV.Padding = new Padding(10, 5, 10, 5);
            lblUkupnoSaPDV.ForeColor = Color.White;
            lblUkupnoSaPDV.BorderStyle = BorderStyle.FixedSingle;

            btnIzracunaj = new Button();
            btnIzracunaj.Text = "IZRAČUNAJ RAČUN";
            btnIzracunaj.Location = new Point(500, 520);
            btnIzracunaj.Size = new Size(200, 40);
            btnIzracunaj.BackColor = Color.DodgerBlue;
            btnIzracunaj.ForeColor = Color.White;
            btnIzracunaj.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnIzracunaj.Click += BtnIzracunaj_Click;

            lblStatus = new Label();
            lblStatus.Location = new Point(20, 575);
            lblStatus.Size = new Size(950, 60);
            lblStatus.Font = new Font("Segoe UI", 11);
            lblStatus.Text = "Status: Očekujem da dodate proizvode...";
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.BackColor = Color.WhiteSmoke;
            lblStatus.Padding = new Padding(10, 10, 10, 10);

            this.Controls.Add(lblProizvod);
            this.Controls.Add(cmbProizvodi);
            this.Controls.Add(lblKolicina);
            this.Controls.Add(nudKolicina);
            this.Controls.Add(btnDodaj);
            this.Controls.Add(btnOcisti);
            this.Controls.Add(dgvRacun);
            this.Controls.Add(lblSeparator1);
            this.Controls.Add(lblUkupnoBezPDV);
            this.Controls.Add(lblUkupnoPDV);
            this.Controls.Add(lblUkupnoSaPDV);
            this.Controls.Add(btnIzracunaj);
            this.Controls.Add(lblStatus);
        }

        private void InicijalizirajProizvode()
        {
            products = new List<Product>
            {
                new Product(1, "Laptop Dell", 1200m, 0.17m),
                new Product(2, "Monitor LG", 350m, 0.17m),
                new Product(3, "Tipkovnica Logitech", 90m, 0.17m),
                new Product(4, "Miš Razer", 45m, 0.17m),
                new Product(5, "Printer HP", 280m, 0.17m)
            };

            cmbProizvodi.DataSource = products;
            cmbProizvodi.DisplayMember = "Name";
        }

        private void BtnDodaj_Click(object sender, EventArgs e)
        {
            var proizvod = (Product)cmbProizvodi.SelectedItem;
            int kolicina = (int)nudKolicina.Value;

            invoice.AddItem(proizvod, kolicina);

            decimal cijena = proizvod.Price;
            decimal iznosBezPDV = cijena * kolicina;
            decimal pdvProcenat = proizvod.TaxRate * 100;
            decimal pdv = iznosBezPDV * proizvod.TaxRate;
            decimal iznosSaPDV = iznosBezPDV + pdv;

            dgvRacun.Rows.Add(
                proizvod.Name,
                cijena.ToString("0.00"),
                kolicina,
                iznosBezPDV.ToString("0.00"),
                pdvProcenat.ToString("0"),
                pdv.ToString("0.00"),
                iznosSaPDV.ToString("0.00")
            );

            lblStatus.Text = $"Status: Dodan proizvod '{proizvod.Name}' - količina: {kolicina} kom. | Ukupno stavki u računu: {invoice.Items.Count}";
            nudKolicina.Value = 1;
        }

        private void BtnOcisti_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Jeste li sigurni da želite očistiti račun?", "Potvrda", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                dgvRacun.Rows.Clear();
                invoice.ClearItems();
                lblUkupnoBezPDV.Text = "UKUPNO BEZ PDV-a: 0.00 KM";
                lblUkupnoPDV.Text = "UKUPNO PDV: 0.00 KM";
                lblUkupnoSaPDV.Text = "UKUPNO SA PDV-om: 0.00 KM";
                lblStatus.Text = "Status: Račun je očišćen. Očekujem da dodate proizvode...";
            }
        }

        private void BtnIzracunaj_Click(object sender, EventArgs e)
        {
            if (invoice.Items.Count == 0)
            {
                MessageBox.Show("Dodajte barem jedan proizvod.");
                return;
            }

            decimal totalBezPDV = invoice.TotalWithoutTax;
            decimal totalPDV = invoice.TotalTax;
            decimal totalSaPDV = invoice.TotalWithTax;

            lblUkupnoBezPDV.Text = $"UKUPNO BEZ PDV-a: {totalBezPDV.ToString("0.00")} KM";
            lblUkupnoPDV.Text = $"UKUPNO PDV: {totalPDV.ToString("0.00")} KM";
            lblUkupnoSaPDV.Text = $"UKUPNO SA PDV-om: {totalSaPDV.ToString("0.00")} KM";
            
            lblStatus.Text = $"Status: ✓ RAČUN IZRAČUNAT | Stavki: {invoice.Items.Count} | Bez PDV-a: {totalBezPDV.ToString("0.00")} KM | PDV: {totalPDV.ToString("0.00")} KM | SA PDV-om: {totalSaPDV.ToString("0.00")} KM";
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
