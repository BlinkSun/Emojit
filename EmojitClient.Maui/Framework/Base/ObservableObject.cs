using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EmojitClient.Maui.Framework.Base;

/// <summary>
/// Base class that implements INotifyPropertyChanged for MVVM bindings.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a property and notifies listeners if the value changed.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Triggers a property change notification.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}