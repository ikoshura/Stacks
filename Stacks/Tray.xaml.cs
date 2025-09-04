// FILE: Stacks/Tray.xaml.cs
using System.Windows.Controls;

namespace Stacks;
public partial class Tray
{
    public Tray(TrayViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}