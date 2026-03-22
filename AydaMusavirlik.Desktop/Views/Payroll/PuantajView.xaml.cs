using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class PuantajView : UserControl
{
    private readonly IEmployeeService _employeeService;
    private readonly ICompanyService _companyService;
    private ObservableCollection<PuantajGridItem> _puantajItems = new();

    public PuantajView()
    {
        InitializeComponent();
        _employeeService = App.GetService<IEmployeeService>();
        _companyService = App.GetService<ICompanyService>();

        Loaded += PuantajView_Loaded;
    }

    private async void PuantajView_Loaded(object sender, RoutedEventArgs e)
    {
        // Yil combobox
        var currentYear = DateTime.Now.Year;
        for (int y = currentYear - 2; y <= currentYear + 1; y++)
            cmbYil.Items.Add(y);
        cmbYil.SelectedItem = currentYear;

        // Ay combobox
        var aylar = new[] { "Ocak", "Subat", "Mart", "Nisan", "Mayis", "Haziran", 
                           "Temmuz", "Agustos", "Eylul", "Ekim", "Kasim", "Aralik" };
        for (int i = 0; i < aylar.Length; i++)
            cmbAy.Items.Add(new ComboBoxItem { Content = aylar[i], Tag = i + 1 });
        cmbAy.SelectedIndex = DateTime.Now.Month - 1;

        // Firma listesi
        try
        {
            var firmalar = await _companyService.GetAllAsync();
            cmbFirma.ItemsSource = firmalar;
            if (firmalar.Any())
                cmbFirma.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Firma listesi yuklenemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnYukle_Click(object sender, RoutedEventArgs e)
    {
        if (cmbFirma.SelectedItem == null)
        {
            MessageBox.Show("Lutfen firma secin.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var firma = (CompanyDto)cmbFirma.SelectedItem;
            var employees = await _employeeService.GetActiveByCompanyAsync(firma.Id);

            _puantajItems.Clear();
            foreach (var emp in employees)
            {
                _puantajItems.Add(new PuantajGridItem
                {
                    EmployeeId = emp.Id,
                    EmployeeNumber = emp.EmployeeNumber,
                    EmployeeName = emp.FullName,
                    ToplamCalisilanGun = 30,
                    HaftaSonuCalisilanGun = 0,
                    ResmiTatilCalisilanGun = 0,
                    UcretliIzinGun = 0,
                    UcretsizIzinGun = 0,
                    RaporluGun = 0,
                    DevamsizlikGun = 0,
                    FazlaMesaiHaftaIci = 0,
                    FazlaMesaiHaftaSonu = 0,
                    FazlaMesaiTatil = 0,
                    SgkPrimGunu = 30
                });
            }

            dgPuantaj.ItemsSource = _puantajItems;
            MessageBox.Show($"{employees.Count()} personel yuklendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnTumunuDoldur_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _puantajItems)
        {
            item.ToplamCalisilanGun = 30;
            item.SgkPrimGunu = 30;
            item.HaftaSonuCalisilanGun = 0;
            item.ResmiTatilCalisilanGun = 0;
            item.UcretliIzinGun = 0;
            item.UcretsizIzinGun = 0;
            item.RaporluGun = 0;
            item.DevamsizlikGun = 0;
        }
        dgPuantaj.Items.Refresh();
    }

    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Veritabanina kaydet
        MessageBox.Show("Puantaj bilgileri kaydedildi.", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnBordroHesapla_Click(object sender, RoutedEventArgs e)
    {
        if (!_puantajItems.Any())
        {
            MessageBox.Show("Once personel listesini yukleyin.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var yil = (int)cmbYil.SelectedItem;
        var ay = (int)((ComboBoxItem)cmbAy.SelectedItem).Tag;
        var firma = (CompanyDto)cmbFirma.SelectedItem;

        // Bordro hesaplama ekranina git
        var bordroView = new BordroDetayView(firma.Id, yil, ay, _puantajItems.ToList());

        // Ana pencereye tab olarak ekle
        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.OpenTabWithControl($"Bordro {ay:D2}/{yil}", bordroView);
        }
    }
}

public class PuantajGridItem
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int ToplamCalisilanGun { get; set; }
    public int HaftaSonuCalisilanGun { get; set; }
    public int ResmiTatilCalisilanGun { get; set; }
    public int UcretliIzinGun { get; set; }
    public int UcretsizIzinGun { get; set; }
    public int RaporluGun { get; set; }
    public int DevamsizlikGun { get; set; }
    public decimal FazlaMesaiHaftaIci { get; set; }
    public decimal FazlaMesaiHaftaSonu { get; set; }
    public decimal FazlaMesaiTatil { get; set; }
    public int SgkPrimGunu { get; set; }
}