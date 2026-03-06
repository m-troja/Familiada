using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }
        public RootScoreboardView()
        {
            InitializeComponent();
        }
    }
}
