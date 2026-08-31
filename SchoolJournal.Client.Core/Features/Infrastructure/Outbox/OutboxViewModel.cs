using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

namespace SchoolJournal.Client.Core.Features.Infrastructure.Outbox;

public sealed partial class OutboxViewModel : ObservableObject
{
    private readonly IOutboxApi _outboxApi;

    public OutboxViewModel(IOutboxApi outboxApi)
    {
        _outboxApi = outboxApi;
        _ = LoadMessagesCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<OutboxMessageResponse> _messages = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _pageNumber = 1;

    [ObservableProperty]
    private string? _filterType;

    [ObservableProperty]
    private bool? _filterHasError;

    [ObservableProperty]
    private OutboxMessageResponse? _selectedMessage;

    [RelayCommand]
    private async Task NextPageAsync(CancellationToken ct)
    {
        PageNumber++;
        await LoadMessagesAsync(ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task PreviousPageAsync(CancellationToken ct)
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadMessagesAsync(ct).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task LoadMessagesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _outboxApi.GetOutboxMessagesAsync(PageNumber, 20, FilterType, FilterHasError, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Messages = new ObservableCollection<OutboxMessageResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження повідомлень Outbox: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task MarkProcessedAsync(OutboxMessageResponse message, CancellationToken ct)
    {
        var response = await _outboxApi.MarkAsProcessedAsync(message.Id, ct).ConfigureAwait(true);
        if (response.IsSuccessStatusCode) await LoadMessagesAsync(ct).ConfigureAwait(true);
    }
}