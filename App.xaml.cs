using Familiada.Models;
using Familiada.ViewModels;
using Familiada.Views;
using System.Windows;

namespace Familiada
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var gameState = new GameState();
            var viewModel = new RootViewModel(gameState);
            var hostPanel = new RootHostView(viewModel);
            var scoreboard = new RootScoreboardView(viewModel);

            hostPanel.Show();
            scoreboard.Show();
        }
    }
}
