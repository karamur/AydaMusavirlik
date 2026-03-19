using System.Windows;
using System.Windows.Controls;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class AccountPlanView : UserControl
{
    public AccountPlanView()
    {
        InitializeComponent();
    }

    private void YeniHesap_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yeni hesap ekleme formu acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void StandartPlanYukle_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Tekduzen Hesap Plani yuklensin mi?", "Standart Plan Yukle", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            MessageBox.Show("Tekduzen Hesap Plani basariyla yuklendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}