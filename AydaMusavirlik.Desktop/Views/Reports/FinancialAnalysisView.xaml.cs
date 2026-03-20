using System.Windows;
using System.Windows.Controls;

namespace AydaMusavirlik.Desktop.Views.Reports;

public partial class FinancialAnalysisView : UserControl
{
    public FinancialAnalysisView()
    {
        InitializeComponent();
    }

    private void AnalizYap_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mali analiz baslatildi. Ornek verilerle hesaplama yapilacak.", 
            "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PdfExport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("PDF export ozelligi aktif.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExcelExport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Excel export ozelligi aktif.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}