using System.Windows.Input;

namespace EmojitClient.Maui.Framework.Base.Command;

/// <summary>
/// Basic ICommand implementation for synchronous actions.
/// </summary>
public partial class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
{
    private readonly Action<object?> execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute) : this(_ => execute(), null) { }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);

    public void Execute(object? parameter) => execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}