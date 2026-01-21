using Familiada.ViewModels;
using System.Windows;

namespace Familiada.Views;

public partial class FinalHostPanel : Window
{
    public FinalHostPanel(FinalViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

}
