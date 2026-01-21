using System.Windows;
using Familiada.ViewModels;

namespace Familiada.Views;

public partial class HostPanel : Window
{
    public HostPanel(GameViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var vm = DataContext as GameViewModel;
        if (vm == null) return;

        if (e.Key == System.Windows.Input.Key.Q)
        {
            vm.PlayAnswerCorrectCommand.Execute(null);
        }
        else if (e.Key == System.Windows.Input.Key.W)
        {
            vm.PlayAnswerIncorrectCommand.Execute(null);
        }
        else if (e.Key == System.Windows.Input.Key.E)
        {
            vm.PlayDzwonkiCommand.Execute(null);
        }
    }
}
