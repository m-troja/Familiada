using Familiada.Models;
using Familiada.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Familiada;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var gameState = new GameState();

        var viewModel = new GameViewModel(gameState);

        var hostPanel = new HostPanel(viewModel);
        var scoreboard = new Scoreboard(viewModel);

        hostPanel.Show();
        scoreboard.Show();
    }
}
