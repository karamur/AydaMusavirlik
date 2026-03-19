using System.Windows;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Companies;

public partial class CompanyEditWindow : Window
{
    private readonly ICompanyService _companyService;
    private CompanyDto? _company;
    private bool _isEditMode;

    public CompanyEditWindow(CompanyDto? company = null)
    {
        InitializeComponent();
        _companyService = App.GetService<ICompanyService>();
        _company = company;
        _isEditMode = company != null;

        if (_isEditMode)
        {
            txtTitle.Text = "Firma Düzenle";
            LoadCompanyData();
        }
    }

    private void LoadCompanyData()
    {
        if (_company == null) return;
        txtName.Text = _company.Name;
        txtTaxNumber.Text = _company.TaxNumber;
        txtTaxOffice.Text = _company.TaxOffice;
        txtTradeRegistryNumber.Text = _company.TradeRegistryNumber;
        txtMersisNumber.Text = _company.MersisNumber;
        txtAddress.Text = _company.Address;
        txtCity.Text = _company.City;
        txtDistrict.Text = _company.District;
        txtPhone.Text = _company.Phone;
        txtEmail.Text = _company.Email;
        txtCapital.Text = _company.Capital?.ToString();
    }

    private async void Kaydet_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Firma adý zorunludur.", "Uyarý", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtName.Focus();
            return;
        }

        var dto = new CreateCompanyDto
        {
            Name = txtName.Text,
            TaxNumber = txtTaxNumber.Text,
            TaxOffice = txtTaxOffice.Text,
            TradeRegistryNumber = txtTradeRegistryNumber.Text,
            MersisNumber = txtMersisNumber.Text,
            Address = txtAddress.Text,
            City = txtCity.Text,
            District = txtDistrict.Text,
            Phone = txtPhone.Text,
            Email = txtEmail.Text
        };

        if (decimal.TryParse(txtCapital.Text, out var capital))
            dto.Capital = capital;

        try
        {
            bool success;
            if (_isEditMode && _company != null)
            {
                success = await _companyService.UpdateAsync(_company.Id, dto);
            }
            else
            {
                var result = await _companyService.CreateAsync(dto);
                success = result != null;
            }

            if (success)
            {
                MessageBox.Show("Firma baþarýyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Firma kaydedilirken hata oluþtu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Iptal_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}