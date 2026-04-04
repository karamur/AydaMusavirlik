using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Data;
using AydaMusavirlik.Desktop.Services;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Desktop.ViewModels;

public partial class AccountingViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Account> _accounts = new();

    [ObservableProperty]
    private ObservableCollection<AccountingRecord> _records = new();

    [ObservableProperty]
    private Account? _selectedAccount;

    [ObservableProperty]
    private AccountingRecord? _selectedRecord;

    [ObservableProperty]
    private string _title = "Muhasebe";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTabIndex;

    // Mizan hesaplama
    [ObservableProperty]
    private decimal _totalDebit;

    [ObservableProperty]
    private decimal _totalCredit;

    public AccountingViewModel(AppDbContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
        LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadAccountsAsync();
            await LoadRecordsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAccountsAsync()
    {
        var accounts = await _context.Accounts
            .OrderBy(a => a.Code)
            .ToListAsync();

        Accounts = new ObservableCollection<Account>(accounts);
    }

    private async Task LoadRecordsAsync()
    {
        var records = await _context.AccountingRecords
            .Include(r => r.Company)
            .Include(r => r.Entries)
            .OrderByDescending(r => r.DocumentDate)
            .Take(100)
            .ToListAsync();

        Records = new ObservableCollection<AccountingRecord>(records);
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        var newAccount = new Account
        {
            Code = "999",
            Name = "Yeni Hesap",
            Level = 3,
            IsHeader = false,
            AllowPosting = true,
            AccountType = AccountType.Aktif,
            Nature = AccountNature.Debit,
            IsActive = true
        };

        try
        {
            await _context.Accounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();
            Accounts.Add(newAccount);
            SelectedAccount = newAccount;
            _dialogService.ShowInfo("Yeni hesap eklendi. Hesap kodunu ve adini duzenleyin.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Hesap eklenirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveAccountAsync()
    {
        if (SelectedAccount == null) return;

        try
        {
            _context.Accounts.Update(SelectedAccount);
            await _context.SaveChangesAsync();
            _dialogService.ShowInfo("Hesap kaydedildi.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Hesap kaydedilirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateRecordAsync()
    {
        var record = new AccountingRecord
        {
            DocumentNumber = $"MF-{DateTime.Now:yyyyMMddHHmmss}",
            DocumentDate = DateTime.Today,
            RecordType = RecordType.MahsupFisi,
            Description = "Yeni Mahsup Fisi",
            Status = RecordStatus.Draft
        };

        try
        {
            await _context.AccountingRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            Records.Insert(0, record);
            SelectedRecord = record;
            _dialogService.ShowInfo("Yeni fis olusturuldu.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Fis olusturulurken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CalculateMizanAsync()
    {
        try
        {
            var entries = await _context.AccountingEntries
                .Include(e => e.Account)
                .Where(e => e.AccountingRecord.Status == RecordStatus.Posted)
                .ToListAsync();

            TotalDebit = entries.Sum(e => e.Debit);
            TotalCredit = entries.Sum(e => e.Credit);

            // Hesap bakiyelerini guncelle
            var accountGroups = entries.GroupBy(e => e.AccountId);
            foreach (var group in accountGroups)
            {
                var account = Accounts.FirstOrDefault(a => a.Id == group.Key);
                if (account != null)
                {
                    var debit = group.Sum(e => e.Debit);
                    var credit = group.Sum(e => e.Credit);
                    account.CurrentBalance = account.Nature == AccountNature.Debit 
                        ? debit - credit 
                        : credit - debit;
                }
            }

            _dialogService.ShowInfo($"Mizan hesaplandi.\nToplam Borc: {TotalDebit:N2} TL\nToplam Alacak: {TotalCredit:N2} TL");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Mizan hesaplanirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PostRecordAsync()
    {
        if (SelectedRecord == null) return;

        if (SelectedRecord.TotalDebit != SelectedRecord.TotalCredit)
        {
            _dialogService.ShowWarning("Borc ve alacak toplamlari esit degil!");
            return;
        }

        try
        {
            SelectedRecord.Status = RecordStatus.Posted;
            await _context.SaveChangesAsync();
            _dialogService.ShowInfo("Fis deftere islendi.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Fis islenirken hata: {ex.Message}");
        }
    }
}
