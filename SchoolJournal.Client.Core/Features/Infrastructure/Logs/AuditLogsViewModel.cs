using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Infrastructure.Logs;

public sealed partial class AuditLogsViewModel(IAuditLogsApi infrastructureApi) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AuditLogResponse> _logs = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [RelayCommand]
    private async Task LoadLogsAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            DateTimeOffset? apiFromDate = FromDate.HasValue ? FromDate.Value : null;

            DateTimeOffset? apiToDate = ToDate.HasValue ? ToDate.Value.Date.AddDays(1).AddTicks(-1) : null;

            var result = await infrastructureApi.GetAuditLogsAsync(
                userId: null,
                fromDate: apiFromDate,
                toDate: apiToDate,
                cancellationToken: cancellationToken).ConfigureAwait(true);
            Logs = new ObservableCollection<AuditLogResponse>(result);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.StatusCode == System.Net.HttpStatusCode.Forbidden
                ? "У вас немає прав адміністратора для перегляду логів."
                : $"Помилка API: {ex.Message}";
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Запит було скасовано.";
        }
        catch (Exception ex)
        {
            if (ex is OutOfMemoryException or StackOverflowException or AccessViolationException)
            {
                throw;
            }

            ErrorMessage = $"Помилка з'єднання: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}