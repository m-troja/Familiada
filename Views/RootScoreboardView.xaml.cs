using System.Diagnostics;
using System.Windows;

namespace Familiada.Views
{
    public partial class RootScoreboardView : Window
    {
        public RootScoreboardView(ViewModels.RootViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            this.Width = viewModel.HostWidth;
            this.Height = viewModel.HostHeight;

            // Debugowanie DataContext i CurrentViewModel
            Debug.WriteLine("[Debug] RootScoreboardView initialized.");
            Debug.WriteLine("[Debug] DataContext type: " + (DataContext?.GetType().Name ?? "null"));
            Debug.WriteLine("[Debug] CurrentViewModel type: " + (viewModel.CurrentViewModel?.GetType().Name ?? "null"));

            if (viewModel.CurrentViewModel != null)
            {
                Debug.WriteLine("[Debug] CurrentViewModel: " + viewModel.CurrentViewModel.GetType().FullName);
            }
        }

        public RootScoreboardView()
        {
            InitializeComponent();
            Debug.WriteLine("[Debug] RootScoreboardView parameterless constructor called.");
        }
    }
}