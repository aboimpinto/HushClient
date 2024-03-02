using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace HushClient.Behaviors;

public class AutoScrollBehavior : Behavior<ScrollViewer>
{
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public static AvaloniaProperty AutoScrollProperty =
        AvaloniaProperty.Register<AutoScrollBehavior, bool>(nameof(AutoScroll));

    protected override void OnAttached()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.ScrollChanged += this.OnScrollChanged;
        }
        base.OnAttached();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (AssociatedObject != null)
        {
            if (this.AutoScroll)
            {
                AssociatedObject.ScrollToEnd();
            }
        }
    }
}
