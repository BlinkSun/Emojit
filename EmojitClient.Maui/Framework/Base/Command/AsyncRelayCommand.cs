using System.Windows.Input;

namespace EmojitClient.Maui.Framework.Base.Command;

/// <summary>
/// ICommand implementation for asynchronous operations.
/// </summary>
public partial class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Func<Task> execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private bool isExecuting;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        try
        {
            isExecuting = true;
            RaiseCanExecuteChanged();
            await execute();
        }
        finally
        {
            isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// ICommand implementation for asynchronous operations with a parameter.
/// </summary>
public partial class AsyncRelayCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private readonly Func<T, Task> execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private bool isExecuting;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (isExecuting)
            return false;

        if (canExecute == null)
            return true;

        if (parameter is T tParam)
            return canExecute(tParam);

        // si le paramètre est null et T est une struct => false
        return !(parameter is null && typeof(T).IsValueType);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        try
        {
            isExecuting = true;
            RaiseCanExecuteChanged();

            if (parameter is T tParam)
                await execute(tParam);
            else if (parameter == null && !typeof(T).IsValueType)
                await execute((T)(object?)null!);
        }
        finally
        {
            isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}