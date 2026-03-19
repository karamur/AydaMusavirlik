using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AydaMusavirlik.Desktop.Views.ArGe;

public partial class ArGeProjectListView : UserControl
{
    private ObservableCollection<ArGeProjectViewModel> _projects;

    public ArGeProjectListView()
    {
        InitializeComponent();
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        _projects = new ObservableCollection<ArGeProjectViewModel>
        {
            new ArGeProjectViewModel 
            { 
                ProjectCode = "ARG-001", 
                ProjectName = "Yapay Zeka Destekli Muhasebe Sistemi", 
                ProjectType = "AR-GE",
                Status = "Aktif",
                StartDate = new DateTime(2024, 1, 1),
                PlannedEndDate = new DateTime(2025, 6, 30),
                PlannedBudget = 2500000,
                ActualCost = 850000,
                HasIncentive = true
            },
            new ArGeProjectViewModel 
            { 
                ProjectCode = "ARG-002", 
                ProjectName = "Blockchain Tabanli Fatura Takip", 
                ProjectType = "Yazilim",
                Status = "Planlama",
                StartDate = new DateTime(2024, 4, 1),
                PlannedEndDate = new DateTime(2024, 12, 31),
                PlannedBudget = 800000,
                ActualCost = 0,
                HasIncentive = false
            },
            new ArGeProjectViewModel 
            { 
                ProjectCode = "TSR-001", 
                ProjectName = "Ergonomik Ofis Mobilya Tasarimi", 
                ProjectType = "Tasarim",
                Status = "Aktif",
                StartDate = new DateTime(2023, 9, 1),
                PlannedEndDate = new DateTime(2024, 8, 31),
                PlannedBudget = 450000,
                ActualCost = 320000,
                HasIncentive = true
            },
            new ArGeProjectViewModel 
            { 
                ProjectCode = "ARG-003", 
                ProjectName = "IoT Sensor Agi Gelistirme", 
                ProjectType = "AR-GE",
                Status = "Tamamlandi",
                StartDate = new DateTime(2022, 6, 1),
                PlannedEndDate = new DateTime(2023, 12, 31),
                PlannedBudget = 1200000,
                ActualCost = 1150000,
                HasIncentive = true
            },
        };

        dgProjects.ItemsSource = _projects;
        UpdateStats();
    }

    private void UpdateStats()
    {
        txtTotalProjects.Text = _projects.Count.ToString();
        txtActiveProjects.Text = _projects.Count(p => p.Status == "Aktif").ToString();
        txtTotalBudget.Text = $"{_projects.Sum(p => p.PlannedBudget):N0} TL";
        txtIncentiveProjects.Text = _projects.Count(p => p.HasIncentive).ToString();
    }

    private void YeniProje_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yeni AR-GE projesi ekleme formu acilacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void TesvikRaporu_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("AR-GE Tesvik Hesaplama Raporu olusturulacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Duzenle_Click(object sender, RoutedEventArgs e)
    {
        var project = (sender as Button)?.DataContext as ArGeProjectViewModel;
        if (project != null)
        {
            MessageBox.Show($"'{project.ProjectName}' projesi duzenlenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Giderler_Click(object sender, RoutedEventArgs e)
    {
        var project = (sender as Button)?.DataContext as ArGeProjectViewModel;
        if (project != null)
        {
            MessageBox.Show($"'{project.ProjectName}' projesi giderleri listelenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public class ArGeProjectViewModel
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal ActualCost { get; set; }
    public bool HasIncentive { get; set; }
    public string IncentiveIcon => HasIncentive ? "Evet" : "Hayir";
}