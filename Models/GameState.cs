using Familiada.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Familiada.Models;

public class GameState : INotifyPropertyChanged
{
    public ObservableCollection<Question> Questions { get; set; } = new();
    public ObservableCollection<Answer> Answers { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    private int _teamAPoints = 1;
    private int _teamBPoints = 2;
    private int _roundPoints = 3;
    public int RoundNumber { get; set; } 
    public int RoundPoints
    {
        get => _roundPoints;
        set
        {
            if (_roundPoints != value)
            {
                _roundPoints = value;
                OnPropertyChanged(nameof(RoundPoints));
            }
        }
    }

    public int TeamAPoints
    {
        get => _teamAPoints;
        set
        {
            if (_teamAPoints != value)
            {
                _teamAPoints = value;
                OnPropertyChanged(nameof(TeamAPoints));
            }
        }
    }


    public int TeamBPoints
    {
        get => _teamBPoints;
        set
        {
            if (_teamBPoints != value)
            {
                _teamBPoints = value;
                OnPropertyChanged(nameof(TeamBPoints));
            }
        }
    }
    public GameState()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Answers", "answers1.json");
        var json = File.ReadAllText(path);

        var wrapper = JsonSerializer.Deserialize<QuestionsWrapper>(json) ?? throw new Exception("Error reading json of questions");
        Questions = new ObservableCollection<Question>(wrapper.Questions) ;

        RoundNumber = 0;
        Answers = new ObservableCollection<Answer>(Questions[RoundNumber].Answers);

    }
}
