using Microsoft.Maui.Layouts;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;

namespace EmojitClient.Maui.Framework.Controls;

/// <summary>
/// A robust and highly customizable circular layout for .NET MAUI.
///
/// ✅ Supports <see cref="ItemsSource"/> and <see cref="DataTemplate"/> binding  
/// ✅ Optional centered item (<see cref="HasCentralItem"/>)  
/// ✅ Random per-item rotation and scaling (persistent once assigned)  
/// ✅ Optional automatic rotation of the entire layout  
/// ✅ Tap command binding support  
///
/// Designed for visual dynamism with predictable and stable layout behavior.
/// </summary>
public sealed partial class CircularItemsLayout : Layout
{
    #region === PRIVATE FIELDS ===

    /// <summary>
    /// Stores the persistent random rotation and scale values assigned to each view.
    /// Prevents random “flashing” or re-randomization on each ArrangeChildren() call.
    /// </summary>
    private readonly Dictionary<IView, (double Rotation, double Scale)> itemTransforms = [];

    private CancellationTokenSource? autoRotateCts;

    #endregion

    #region === BINDABLE PROPERTIES ===

    public static readonly BindableProperty HasCentralItemProperty =
        BindableProperty.Create(
            nameof(HasCentralItem),
            typeof(bool),
            typeof(CircularItemsLayout),
            false,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty IsRandomItemRotationEnabledProperty =
        BindableProperty.Create(
            nameof(IsRandomItemRotationEnabled),
            typeof(bool),
            typeof(CircularItemsLayout),
            false,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty IsRandomItemScaleEnabledProperty =
        BindableProperty.Create(
            nameof(IsRandomItemScaleEnabled),
            typeof(bool),
            typeof(CircularItemsLayout),
            false,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty IsAutoRotateEnabledProperty =
        BindableProperty.Create(
            nameof(IsAutoRotateEnabled),
            typeof(bool),
            typeof(CircularItemsLayout),
            false,
            propertyChanged: OnAutoRotateChanged);

    public static readonly BindableProperty AutoRotateSpeedProperty =
        BindableProperty.Create(
            nameof(AutoRotateSpeed),
            typeof(double),
            typeof(CircularItemsLayout),
            0.01,
            propertyChanged: OnAutoRotateSpeedChanged);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(CircularItemsLayout),
            null,
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(CircularItemsLayout),
            null,
            propertyChanged: OnItemTemplateChanged);

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(
            nameof(Radius),
            typeof(double),
            typeof(CircularItemsLayout),
            0.0,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty StartAngleProperty =
        BindableProperty.Create(
            nameof(StartAngle),
            typeof(double),
            typeof(CircularItemsLayout),
            -Math.PI / 2,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty RotationOffsetProperty =
        BindableProperty.Create(
            nameof(RotationOffset),
            typeof(double),
            typeof(CircularItemsLayout),
            0.0,
            propertyChanged: (b, _, _) => ((CircularItemsLayout)b).InvalidateMeasure());

    public static readonly BindableProperty ItemTappedCommandProperty =
        BindableProperty.Create(
            nameof(ItemTappedCommand),
            typeof(ICommand),
            typeof(CircularItemsLayout),
            null);

    #endregion

    #region === PUBLIC PROPERTIES ===

    /// <summary>Places the first item at the center if true.</summary>
    public bool HasCentralItem
    {
        get => (bool)GetValue(HasCentralItemProperty);
        set => SetValue(HasCentralItemProperty, value);
    }

    /// <summary>Whether each item has a random self-rotation (persistent).</summary>
    public bool IsRandomItemRotationEnabled
    {
        get => (bool)GetValue(IsRandomItemRotationEnabledProperty);
        set => SetValue(IsRandomItemRotationEnabledProperty, value);
    }

    /// <summary>Whether each item has a random scale variation (persistent).</summary>
    public bool IsRandomItemScaleEnabled
    {
        get => (bool)GetValue(IsRandomItemScaleEnabledProperty);
        set => SetValue(IsRandomItemScaleEnabledProperty, value);
    }

    /// <summary>If true, the entire layout auto-rotates over time.</summary>
    public bool IsAutoRotateEnabled
    {
        get => (bool)GetValue(IsAutoRotateEnabledProperty);
        set => SetValue(IsAutoRotateEnabledProperty, value);
    }

    /// <summary>Rotation speed in radians per frame (60fps).</summary>
    public double AutoRotateSpeed
    {
        get => (double)GetValue(AutoRotateSpeedProperty);
        set => SetValue(AutoRotateSpeedProperty, value);
    }

    /// <summary>Collection used to generate child views.</summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Data template used to generate each item view.</summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Fixed layout radius. ≤ 0 = automatic based on bounds.</summary>
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>Base angular offset in radians. Default = -π/2 (12 o’clock).</summary>
    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>Additional rotation offset applied to all items.</summary>
    public double RotationOffset
    {
        get => (double)GetValue(RotationOffsetProperty);
        set => SetValue(RotationOffsetProperty, value);
    }

    /// <summary>Command executed when an item is tapped.</summary>
    public ICommand ItemTappedCommand
    {
        get => (ICommand)GetValue(ItemTappedCommandProperty);
        set => SetValue(ItemTappedCommandProperty, value);
    }

    #endregion

    #region === AUTO ROTATION HANDLING ===

    private static void OnAutoRotateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        CircularItemsLayout layout = (CircularItemsLayout)bindable;
        if ((bool)newValue)
            layout.StartAutoRotate();
        else
            layout.StopAutoRotate();
    }

    private static void OnAutoRotateSpeedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        CircularItemsLayout layout = (CircularItemsLayout)bindable;
        if (layout.IsAutoRotateEnabled)
        {
            layout.StopAutoRotate();
            layout.StartAutoRotate();
        }
    }

    private void StartAutoRotate()
    {
        StopAutoRotate();
        autoRotateCts = new CancellationTokenSource();
        CancellationToken token = autoRotateCts.Token;

        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Math.Abs(AutoRotateSpeed) > double.Epsilon)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            RotationOffset += AutoRotateSpeed;
                        });
                    }

                    await Task.Delay(16, token); // ≈ 60 FPS
                }
            }
            catch (TaskCanceledException)
            {
                // normal when stopped
            }
        }, token);
    }

    private void StopAutoRotate()
    {
        if (autoRotateCts == null)
            return;

        autoRotateCts.Cancel();
        autoRotateCts.Dispose();
        autoRotateCts = null;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler == null)
            StopAutoRotate();
    }

    #endregion

    #region === ITEM MANAGEMENT ===

    protected override ILayoutManager CreateLayoutManager() => new CircularLayoutManager(this);

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        => ((CircularItemsLayout)bindable).RebuildItems();

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        => ((CircularItemsLayout)bindable).RebuildItems();

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => Application.Current?.Dispatcher.Dispatch(RebuildItems);

    /// <summary>
    /// Safely rebuilds all children when data or template changes.
    /// </summary>
    private void RebuildItems()
    {
        try
        {
            if (ItemsSource is INotifyCollectionChanged oldObs)
                oldObs.CollectionChanged -= OnCollectionChanged;

            Children.Clear();
            itemTransforms.Clear();

            if (ItemsSource == null || ItemTemplate == null)
                return;

            if (ItemsSource is INotifyCollectionChanged obs)
            {
                obs.CollectionChanged -= OnCollectionChanged;
                obs.CollectionChanged += OnCollectionChanged;
            }

            Random random = new();

            foreach (object item in ItemsSource)
            {
                View? view = CreateView(item);
                if (view == null)
                    continue;

                // Assign persistent random transform (if enabled)
                double rotation = IsRandomItemRotationEnabled ? random.NextDouble() * 360 : 0;
                double scale = IsRandomItemScaleEnabled
                    ? 1.0 + ((random.NextDouble() - 0.5) * 0.2) // ±10%
                    : 1.0;

                itemTransforms[view] = (rotation, scale);

                if (ItemTappedCommand != null)
                {
                    view.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            if (ItemTappedCommand.CanExecute(item))
                                ItemTappedCommand.Execute(item);
                        })
                    });
                }

                Children.Add(view);
            }

            InvalidateMeasure();

            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(16), () =>
            {
                InvalidateMeasure();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CircularItemsLayout] RebuildItems crash: {ex}");
        }
    }

    private View? CreateView(object item)
    {
        try
        {
            if (item is View existingView)
                return existingView;

            DataTemplate template = ItemTemplate is DataTemplateSelector selector
                ? selector.SelectTemplate(item, this)
                : ItemTemplate;

            object content = template.CreateContent();
            //object content = template.LoadTemplate();

            if (content is View view)
            {
                view.BindingContext = item;
                PropagateBindingContext(view);
                return view;
            }

            if (content is ViewCell cell)
            {
                cell.BindingContext = item;
                PropagateBindingContext(cell.View);
                return cell.View;
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CircularItemsLayout] Error creating view: {ex.Message}");
        }

        return null;
    }

    private static void PropagateBindingContext(Element parent)
    {
        if (parent is IElementController controller)
        {
            foreach (Element child in controller.LogicalChildren)
            {
                child.BindingContext = parent.BindingContext;
                PropagateBindingContext(child);
            }
        }
    }

    #endregion

    #region === INTERNAL LAYOUT MANAGER ===

    private sealed class CircularLayoutManager(CircularItemsLayout layout) : LayoutManager(layout)
    {
        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            double maxChildWidth = 0;
            double maxChildHeight = 0;

            foreach (IView child in layout.Children)
            {
                if (child.Visibility != Visibility.Visible)
                    continue;

                Size desired = child.Measure(double.PositiveInfinity, double.PositiveInfinity);
                maxChildWidth = Math.Max(maxChildWidth, desired.Width);
                maxChildHeight = Math.Max(maxChildHeight, desired.Height);

                //Debug.WriteLine($"{child.GetType().Name} => {desired.Width}x{desired.Height} => {maxChildWidth}x{maxChildHeight}");
            }

            double effectiveRadius = GetEffectiveRadius(layout, widthConstraint, heightConstraint, maxChildWidth, maxChildHeight);
            double diameter = (effectiveRadius + (Math.Max(maxChildWidth, maxChildHeight) / 2)) * 2;

            return new Size(diameter, diameter);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            List<IView> visibleChildren = layout.Children.Where(c => c.Visibility == Visibility.Visible).ToList();
            int count = visibleChildren.Count;
            if (count == 0)
                return bounds.Size;

            double centerX = bounds.X + (bounds.Width / 2);
            double centerY = bounds.Y + (bounds.Height / 2);

            double maxChild = visibleChildren
                .Select(c => Math.Max(c.DesiredSize.Width, c.DesiredSize.Height))
                .DefaultIfEmpty(0)
                .Max();

            double radius = GetEffectiveRadius(layout, bounds.Width, bounds.Height, maxChild, maxChild);
            double angleStep = 2 * Math.PI / (layout.HasCentralItem ? Math.Max(1, count - 1) : count);
            double angle = layout.StartAngle + layout.RotationOffset;

            bool centerPlaced = !layout.HasCentralItem;

            foreach (IView child in visibleChildren)
            {
                Size desired = child.DesiredSize;
                double x, y;

                if (layout.HasCentralItem && !centerPlaced)
                {
                    x = centerX - (desired.Width / 2);
                    y = centerY - (desired.Height / 2);
                    centerPlaced = true;
                }
                else
                {
                    x = centerX + (radius * Math.Cos(angle)) - (desired.Width / 2);
                    y = centerY + (radius * Math.Sin(angle)) - (desired.Height / 2);
                    angle += angleStep;
                }

                if (child is VisualElement ve)
                {
                    if (layout.itemTransforms.TryGetValue(child, out (double Rotation, double Scale) t))
                    {
                        ve.Rotation = t.Rotation;
                        ve.Scale = t.Scale;
                    }
                    else
                    {
                        ve.Rotation = 0;
                        ve.Scale = 1;
                    }
                }

                child.Arrange(new Rect(x, y, desired.Width, desired.Height));
            }

            return bounds.Size;
        }

        private static double GetEffectiveRadius(CircularItemsLayout layout, double width, double height, double maxW, double maxH)
        {
            double radius = layout.Radius <= 0
                ? (Math.Min(width, height) / 2.0) - (Math.Max(maxW, maxH) / 2.0)
                : layout.Radius - (Math.Max(maxW, maxH) / 2.0);

            return Math.Max(0, radius);
        }
    }

    #endregion
}