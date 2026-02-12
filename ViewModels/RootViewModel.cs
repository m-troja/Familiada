using DotNetEnv;
using Familiada.Models;
using irnin.FirstPressDevice;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Familiada.ViewModels;
public class RootViewModel : INotifyPropertyChanged
{
    public GameState GameState { get; }

    private object? _currentViewModel;

    private double _scoreboardHeight;
    private double _scoreboardWidth;
    private double _hostHeight;
    private double _hostWidth;

    public double ScoreboardHeight
    {
        get => _scoreboardHeight;
        set
        {
            if (_scoreboardHeight != value)
            {
                _scoreboardHeight = value;
                OnPropertyChanged(nameof(ScoreboardHeight));
            }
        }
    }

    public double ScoreboardWidth
    {
        get => _scoreboardWidth;
        set
        {
            if (_scoreboardWidth != value)
            {
                _scoreboardWidth = value;
                OnPropertyChanged(nameof(ScoreboardWidth));
            }
        }
    }

    public double HostHeight
    {
        get => _hostHeight;
        set
        {
            if (_hostHeight != value)
            {
                _hostHeight = value;
                OnPropertyChanged(nameof(HostHeight));
            }
        }
    }

    public double HostWidth
    {
        get => _hostWidth;
        set
        {
            if (_hostWidth != value)
            {
                _hostWidth = value;
                OnPropertyChanged(nameof(HostWidth));
            }
        }
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }
    }

    private void LoadEnvironmentVariables()
    {
        string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dev.env");
        DotNetEnv.Env.Load(envFilePath);
        string? envScoreboardWidth = Environment.GetEnvironmentVariable("FAMILIADA_SCOREBOARD_WIDTH");
        string? envScoreboardHeight = Environment.GetEnvironmentVariable("FAMILIADA_SCOREBOARD_HEIGHT");
        string? envHostWidth = Environment.GetEnvironmentVariable("FAMILIADA_HOST_WIDTH");
        string? envHostHeight = Environment.GetEnvironmentVariable("FAMILIADA_HOST_HEIGHT");

        _scoreboardWidth = double.TryParse(envScoreboardWidth, out double scoreboardWidth) ? scoreboardWidth : 501;
        _scoreboardHeight = double.TryParse(envScoreboardHeight, out double scoreboardHeight) ? scoreboardHeight : 502;
        _hostWidth = double.TryParse(envHostWidth, out double hostWidth) ? hostWidth : 503;
        _hostHeight = double.TryParse(envHostHeight, out double hostHeight) ? hostHeight : 504;

        Debug.WriteLine("Parsed ScoreboardWidth: " + _scoreboardWidth);
        Debug.WriteLine("Parsed ScoreboardHeight: " + _scoreboardHeight);
        Debug.WriteLine("Parsed HostWidth: " + _hostWidth);
        Debug.WriteLine("Parsed HostHeight: " + _hostHeight);
    }

    public RootViewModel(GameState gameState)
    {
        LoadEnvironmentVariables();
        GameState = new GameState();
        CurrentViewModel = new GameViewModel(GameState, this);
        GameState = gameState;
    }
    public RootViewModel()
    {
        LoadEnvironmentVariables();
        GameState = new GameState(); 
        CurrentViewModel = new GameViewModel(GameState, this);
    }

    public void SwitchToFinal()
    {
        CurrentViewModel = new FinalViewModel(GameState);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}