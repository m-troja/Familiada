using Familiada.Models;
using Familiada.ViewModels;
using Familiada.Views;
using System.Collections;
using System.Diagnostics;
using System.Windows;

namespace Familiada
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DotNetEnv.Env.Load("familiada.dev.env");

            Debug.WriteLine("===== ENV DEBUG =====");

            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                if (env.Key.ToString()!.StartsWith("FAMILIADA"))
                {
                    Debug.WriteLine($"{env.Key} = {env.Value}");
                }
            }

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
