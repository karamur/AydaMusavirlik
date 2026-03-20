using System.Windows;
using System.Windows.Controls;

namespace AydaMusavirlik.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += DashboardView_Loaded;
    }

    private void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        // Veriler yukleniyor - API'den gelecek
    }

    private void FirmaSec_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Firma secim ekrani acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void FisGirisi_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Fis girisi ekrani acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BordroHesapla_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Bordro hesaplama ekrani acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Mizan_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mizan ekrani acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void MaliAnaliz_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mali analiz ekrani acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PdfRapor_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("PDF rapor olusturma baslatildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}