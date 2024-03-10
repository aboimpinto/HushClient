using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Olimpo.Controls;

public class IconButton : Button
{
    public Geometry Data 
    {
        get => (Geometry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly AvaloniaProperty DataProperty = 
        AvaloniaProperty.Register<IconButton, Geometry>(nameof(Data));
}
