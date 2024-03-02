using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace HushClient.Behaviors;

public class SetFocusBehavior : Behavior<Control>
{
    public bool SetFocus
    {
        get => (bool)GetValue(SetFocusProperty);
        set => SetValue(SetFocusProperty, value);
    }

    public static AvaloniaProperty SetFocusProperty =
        AvaloniaProperty.Register<SetFocusBehavior, bool>(nameof(SetFocus));

    protected override void OnAttached()
    {
        base.OnAttached();
    }

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject != null)
        {
            if (this.SetFocus)
            {
                AssociatedObject.Focus();
            }
        }
        else
        {
            throw new InvalidOperationException("AssociatedObject is null");
        }
    }
}
