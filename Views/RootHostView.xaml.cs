using System.Diagnostics;
using System.Windows;

namespace Familiada.Views
{
    public partial class RootHostView : Window
    {
        public RootHostView(ViewModels.RootViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.Width = viewModel.HostWidth;
            this.Height = viewModel.HostHeight;
        }
        public RootHostView()
        {
            InitializeComponent();
        }
    }
}
