using Familiada.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Familiada.Views;

public partial class HostFinalUserControl : UserControl
{
    public HostFinalUserControl()
    {
        InitializeComponent();

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
        else if (e.Key == System.Windows.Input.Key.Space)
        {
            vm.PlayAnswerIncorrectCommand.Execute(null);
        }
    }
}
