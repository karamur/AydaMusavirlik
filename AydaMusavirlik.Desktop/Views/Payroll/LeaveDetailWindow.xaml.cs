using System.Windows;
using System.Windows.Media;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class LeaveDetailWindow : Window
{
    public LeaveDetailWindow(IzinTalebiViewModel talep)
    {
        InitializeComponent();
        LoadData(talep);
    }

    private void LoadData(IzinTalebiViewModel talep)
    {
        txtFormNo.Text = talep.FormNo;
        txtDurum.Text = talep.Durum;

        txtPersonelAdi.Text = talep.PersonelAdi;
        txtDepartman.Text = "Muhasebe"; // Demo
        txtPozisyon.Text = "Uzman"; // Demo

        txtIzinTuru.Text = talep.IzinTuru;
        txtBaslangic.Text = talep.BaslangicTarihi.ToString("dd.MM.yyyy dddd");
        txtBitis.Text = talep.BitisTarihi.ToString("dd.MM.yyyy dddd");
        txtGunSayisi.Text = $"{talep.GunSayisi} gün";
        txtTalepTarihi.Text = talep.TalepTarihi.ToString("dd.MM.yyyy HH:mm");

        txtOnayDurum.Text = talep.Durum;

        switch (talep.Durum)
        {
            case "Onaylandý":
                brdOnay.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                txtOnayDurum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                txtOnaylayan.Text = talep.OnaylayanAdi ?? "-";
                txtOnayTarihi.Text = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy HH:mm");
                break;
            case "Reddedildi":
                brdOnay.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                txtOnayDurum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                txtOnaylayan.Text = talep.OnaylayanAdi ?? "-";
                txtOnayTarihi.Text = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy HH:mm");
                break;
            default:
                brdOnay.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                txtOnayDurum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                txtOnaylayan.Text = "Bekliyor...";
                txtOnayTarihi.Text = "-";
                break;
        }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}