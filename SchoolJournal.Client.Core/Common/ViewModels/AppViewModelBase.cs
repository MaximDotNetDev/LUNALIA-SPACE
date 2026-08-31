using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolJournal.Client.Core.Common.ViewModels;

public abstract class AppViewModelBase : ObservableObject, IDisposable
{
    private readonly SemaphoreSlim _actionSemaphore = new(1, 1);
    private bool _disposedValue;

    protected async Task ExecuteLockedAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!await _actionSemaphore.WaitAsync(0).ConfigureAwait(true))
        {
            return;
        }

        try
        {
            await action().ConfigureAwait(true);
        }
        finally
        {
            _actionSemaphore.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _actionSemaphore.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}