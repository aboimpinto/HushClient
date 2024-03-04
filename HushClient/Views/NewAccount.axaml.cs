using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HushClient.Views;

public partial class NewAccount : UserControl
{
    public NewAccount()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}