using Familiada.ViewModels;
using System.Windows;
using System.Windows.Controls;


namespace Familiada.Views;

public partial class FinalScoreboard : UserControl
{
    public FinalScoreboard(FinalViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel; 
    }
    public FinalScoreboard()
    {
        InitializeComponent();
    }
}
