using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Familiada.Models;

public class GameState : INotifyPropertyChanged
{
    public ObservableCollection<Question> QuestionsGame { get; set; } = new();
    public ObservableCollection<Question> QuestionsFinal { get; set; } = new();
    private ObservableCollection<Answer> _answersFinalPlayer1  = new();
    public ObservableCollection<Answer> AnswersFinalPlayer1
    {
        get => _answersFinalPlayer1;
        set
        {
            if (_answersFinalPlayer1 != value)
            {
                _answersFinalPlayer1 = value;
                OnPropertyChanged(nameof(AnswersFinalPlayer1));
            }
        }
    }
    private ObservableCollection<Answer> _answersFinalPlayer2  = new();
    public ObservableCollection<Answer> AnswersFinalPlayer2
    {
        get => _answersFinalPlayer2;
        set
        {
            if (_answersFinalPlayer2 != value)
            {
                _answersFinalPlayer2 = value;
                OnPropertyChanged(nameof(AnswersFinalPlayer2));
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    public Phase Phase { get; set; } = Phase.NotStarted;
    public string Team1Name = null!;
    public string Team2Name = null!;
    private int _teamAPoints = 0;
    private int _teamBPoints = 0;
    private int _roundPoints = 0;
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

    // First Wins
    private int _firstWinsIterations = 1;
    public int FirstWinsIterations
    {
        get => _firstWinsIterations;
        set
        {
            if (_firstWinsIterations != value)
            {
                _firstWinsIterations = value;
                OnPropertyChanged(nameof(FirstWinsIterations));
            }
        }
    }

    private bool _isFirstWins = false;
    public bool IsFirstWins
    {
        get => _isFirstWins;
        set
        {

            if (_isFirstWins != value)
            {
                _isFirstWins = value;
                if (_isFirstWins == false)
                {
                    OnPropertyChanged(nameof(IsTeamAdisplayBigX));
                    OnPropertyChanged(nameof(IsTeamBdisplayBigX));
                }
                OnPropertyChanged(nameof(IsFirstWins));
            }
        }
    }

    private bool _isTeamAdisplayBigX = false;
    public bool IsTeamAdisplayBigX
    {
        get => _isTeamAdisplayBigX;
        set
        {
            if (_isTeamAdisplayBigX != value)
            {
                _isTeamAdisplayBigX = value;
                OnPropertyChanged(nameof(IsTeamAdisplayBigX));
            }
        }
    }

    private bool _isTeamBdisplayBigX = false;
    public bool IsTeamBdisplayBigX
    {
        get => _isTeamBdisplayBigX;
        set
        {
            if (_isTeamBdisplayBigX != value)
            {
                _isTeamBdisplayBigX = value;
                OnPropertyChanged(nameof(IsTeamBdisplayBigX));
            }
        }
    }

    // Team Answers
    private bool _isTeamAAnswering = false;
    public bool IsTeamAAnswering
    {
        get => _isTeamAAnswering;
        set
        {
            if (_isTeamAAnswering != value)
            {
                _isTeamAAnswering = value;
                OnPropertyChanged(nameof(IsTeamAAnswering));
            }
        }
    }
    private bool _isTeamBAnswering = false;
    public bool IsTeamBAnswering
    {
        get => _isTeamBAnswering;
        set
        {
            if (_isTeamBAnswering != value)
            {
                _isTeamBAnswering = value;
                OnPropertyChanged(nameof(IsTeamBAnswering));
            }
        }
    }
    // Lost changes
    private int _teamALostChances = 0;
    public int TeamALostChances
    {
        get => _teamALostChances;
        set
        {
            if (_teamALostChances != value)
            {
                _teamALostChances = value;
                OnPropertyChanged(nameof(TeamALostChances));
            }
        }
    }

    private int _teamBLostChances = 0;
    public int TeamBLostChances
    {
        get => _teamBLostChances;
        set
        {
            if (_teamBLostChances != value)
            {
                _teamBLostChances = value;
                OnPropertyChanged(nameof(TeamBLostChances));
            }
        }
    }

    private bool _roundFinished = false;
    public bool RoundFinished
    {
        get => _roundFinished;
        set
        {
            if (_roundFinished != value)
            {
                _roundFinished = value;
                OnPropertyChanged(nameof(RoundFinished));
            }
        }
    }

    // LAST CHANCE
    private bool _isLastChance = false;
    public bool IsLastChance
    {
        get => _isLastChance;
        set
        {
            if (_isLastChance != value)
            {
                _isLastChance = value;
                OnPropertyChanged(nameof(IsLastChance));
            }
        }
    }

    // FINAL
    private bool _isFinalRound = false;
    public bool IsFinalRound
    {
        get => _isFinalRound;
        set
        {
            if (_isFinalRound != value)
            {
                _isFinalRound = value;
                OnPropertyChanged(nameof(IsFinalRound));
            }
        }
    }

    public bool _isFinalSecondPlayerAnswering = false;
    public bool IsFinalSecondPlayerAnswering
    {
        get => _isFinalSecondPlayerAnswering;
        set
        {
            if (_isFinalSecondPlayerAnswering != value)
            {
                _isFinalSecondPlayerAnswering = value;
                OnPropertyChanged(nameof(IsFinalSecondPlayerAnswering));
            }
        }
    }
    public GameState()
    {
        LoadEnvironmentVariables();
        var pathGameAnswers = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Answers", "answers2.json");
        var pathFinalAnswers = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Answers", "final2.json");
        var jsonGameAnswers = File.ReadAllText(pathGameAnswers);
        var jsonFinalAnswers = File.ReadAllText(pathFinalAnswers);

        var wrapperGame = JsonSerializer.Deserialize<QuestionsWrapper>(jsonGameAnswers) ?? throw new Exception("Error reading game json of questions");
        var wrapperFinal = JsonSerializer.Deserialize<QuestionsWrapper>(jsonFinalAnswers) ?? throw new Exception("Error reading final json of questions");
        QuestionsGame = new ObservableCollection<Question>(wrapperGame.Questions) ;
        QuestionsFinal = new ObservableCollection<Question>(wrapperFinal.Questions);
        AnswersFinalPlayer1 = new ObservableCollection<Answer>(new List<Answer>  { new(), new(), new(), new(), new() });
        AnswersFinalPlayer2 = new ObservableCollection<Answer>(new List<Answer> { new(),  new(),  new(), new(), new()  } );
        RoundNumber = -1;
        Phase = Phase.NotStarted;
    }

    private void LoadEnvironmentVariables()
    {
        string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dev.env");
        DotNetEnv.Env.Load(envFilePath);
        Team1Name = Environment.GetEnvironmentVariable("FAMILIADA_TEAM_A_NAME") ?? "Druzyna A";
        Team2Name = Environment.GetEnvironmentVariable("FAMILIADA_TEAM_B_NAME") ?? "Druzyna B";

        Debug.WriteLine("Parsed Team1Name: " + Team1Name);
        Debug.WriteLine("Parsed Team2Name: " + Team2Name);
    }
}
