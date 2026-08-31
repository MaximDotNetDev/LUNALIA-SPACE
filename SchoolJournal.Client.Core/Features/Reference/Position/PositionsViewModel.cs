using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.Positions;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace SchoolJournal.Client.Core.Features.Reference.Position;

public sealed partial class PositionsViewModel : ObservableObject
{
    private readonly IPositionApi _positionApi;
    private readonly IIdentityService _identityService;

    public PositionsViewModel(IPositionApi positionApi, IIdentityService identityService)
    {
        _positionApi = positionApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<PositionResponse> _positions = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isAdminOrDirector;

    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private Guid? _formPositionId;

    [ObservableProperty]
    private string _formPositionName = string.Empty;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await LoadPositionsAsync(ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadPositionsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _positionApi.GetPositionsAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Positions = new ObservableCollection<PositionResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження посад: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Створення посади";
        FormPositionId = null;
        FormPositionName = string.Empty;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(PositionResponse position, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _positionApi.GetPositionByIdAsync(position.PositionId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshPosition = response.Content;
                IsEditing = true;
                FormTitle = "Редагування посади";
                FormPositionId = freshPosition.PositionId;
                FormPositionName = freshPosition.PositionName;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані посади.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження посади: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormPositionName))
        {
            ErrorMessage = "Назва посади не може бути порожньою.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormPositionId.HasValue)
            {
                var request = new UpdatePositionRequest(FormPositionName);
                response = await _positionApi.UpdatePositionAsync(FormPositionId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreatePositionRequest(FormPositionName);
                response = await _positionApi.CreatePositionAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadPositionsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані посади.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Посада з такою назвою вже існує.",
                    _ => $"Помилка API: {response.Error.Message}"
                };
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeletePositionAsync(PositionResponse position, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _positionApi.DeletePositionAsync(position.PositionId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Positions.Remove(position);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректний запит на видалення.",
                    System.Net.HttpStatusCode.NotFound => "Посаду не знайдено або вже видалено.",
                    _ => $"Помилка видалення: {response.Error.Message}"
                };
            }
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Запит було скасовано.";
        }
        catch (Exception ex)
        {
            if (ex is OutOfMemoryException or StackOverflowException or AccessViolationException) throw;
            ErrorMessage = $"Помилка системи: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}