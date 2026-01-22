using System.Windows;

namespace Familiada.Views
{
    public partial class RootHostView : Window
    {
        public RootHostView(ViewModels.RootViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        public RootHostView()
        {
            InitializeComponent();
        }
    }
}
