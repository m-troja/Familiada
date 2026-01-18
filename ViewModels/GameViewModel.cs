using Familiada.Models;
using Familiada.Services;
using Familiada.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly SoundService _soundService = new SoundService();
    public ObservableCollection<int> QuestionIds
    {
        get
        {
            int i = 1;
            while (i <= GameState.Questions.Count)
            {
                QuestionIds.Add(i);
                i++;
                return QuestionIds;
            }
            return new ObservableCollection<int> { 0, 0, 0, 0, 0 };
        }
    }
    public ICommand RevealAnswerCommand { get; }
    public ICommand TriggerNextRoundCommand { get; }
    public ICommand AnswerCorrectCommand { get; }
    public ICommand AnswerIncorrectCommand { get; }
    public ICommand PlayAnswerCorrectCommand { get; }
    public ICommand PlayAnswerIncorrectCommand { get; }
    public ICommand PlayDzwonkiCommand { get; }
    public ICommand IntroMusicCommand { get; }
    public ICommand TestTeamALost3Chances { get; }
    public ICommand TeamAAnsweringCommand { get; }
    public ICommand TeamBAnsweringCommand { get; }
    public GameState GameState { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private ObservableCollection<Answer> _currentAnswers = new();
    public ObservableCollection<Answer> Answers
    {
        get => _currentAnswers;
        set
        {
            _currentAnswers = value;
            OnPropertyChanged(nameof(Answers));
        }
    }

    public string CurrentQuestionText
    {
        get
        {
            if (GameState.RoundNumber < 1) { return "Rozpocznij grę"; }
            else { return $"Pytanie {GameState.RoundNumber}: {GameState.Questions[GameState.RoundNumber].Text}"; }
        }
    }

    public string TeamAPointsText
    {
        get
        {
            if (GameState.RoundNumber < 1) { return "Punkty DrużynaA"; }
            else { return "Punkty DrużynaA"; }
        }
    }

    public string TeamBPointsText
    {
        get
        {
            if (GameState.RoundNumber < 1) { return "Punkty DrużynaB"; }
            else { return "Punkty DrużynaB"; }
        }
    }


    // Lost chances
    public bool IsAX1Lost => GameState.TeamALostChances >= 1;
    public bool IsAX2Lost => GameState.TeamALostChances >= 2;
    public bool IsAX3Lost => GameState.TeamALostChances >= 3;

    public bool IsBX1Lost => GameState.TeamBLostChances >= 1;
    public bool IsBX2Lost => GameState.TeamBLostChances >= 2;
    public bool IsBX3Lost => GameState.TeamBLostChances >= 3;


    public string TeamAX1 => IsAX1Lost ? "X" : "";
    public string TeamAX2 => IsAX2Lost ? "X" : "";
    public string TeamAX3 => IsAX3Lost ? "X" : "";

    public string TeamBX1 => IsBX1Lost ? "X" : "";
    public string TeamBX2 => IsBX2Lost ? "X" : "";
    public string TeamBX3 => IsBX3Lost ? "X" : "";

    public string TeamALostFirstWinChance => GameState.IsTeamAFirstWinsError ? "X" : "";
    public string TeamBLostFirstWinChance => GameState.IsTeamBFirstWinsError ? "X" : "";
    public string FirstWinsText => GameState.IsFirstWins ? "Kto pierwszy ten lepszy!!!" : "";
    public string TeamAnswersText
    {
        get
        {
            if (GameState.IsTeamAAnswering)
                return "Odpowiada drużyna A";
            else if (GameState.IsTeamBAnswering)
                return "Odpowiada drużyna B";
            else
                return "Odpowiada drużyna: Wybierz drużynę!!";
        }
    }


    // ROUND POINTS

    public int RoundPoints
    {
        get => GameState.RoundPoints;
        set
        {
            if (GameState.RoundPoints != value)
            {
                GameState.RoundPoints = value;
                OnPropertyChanged(nameof(RoundPoints));
            }
        }
    }

    public int TeamAPoints
    {
        get => GameState.TeamAPoints;
        set
        {
            if (GameState.TeamAPoints != value)
            {
                GameState.TeamAPoints = value;
                OnPropertyChanged(nameof(TeamAPoints));
            }
        }
    }

    public int TeamBPoints
    {
        get => GameState.TeamBPoints;
        set
        {
            if (GameState.TeamBPoints != value)
            {
                GameState.TeamBPoints = value;
                OnPropertyChanged(nameof(TeamBPoints));
            }
        }
    }

    public int FirstWinsIterations
    {
        get => GameState.FirstWinsIterations;
        set
        {
            if (GameState.FirstWinsIterations != value)
            {
                GameState.FirstWinsIterations = value;
                OnPropertyChanged(nameof(FirstWinsIterations));
            }
        }
    }

    public bool IsFirstWins => GameState.IsFirstWins;
    public bool IsTeamAFirstWinsError => GameState.IsTeamAFirstWinsError;
    public bool IsTeamBFirstWinsError => GameState.IsTeamBFirstWinsError;
    public  bool IsTeamAAnswering => GameState.IsTeamAAnswering;
    public  bool IsTeamBAnswering => GameState.IsTeamBAnswering;

    public GameViewModel(GameState gameState)
    {
        GameState = gameState;

        Answers = new ObservableCollection<Answer>(
            GameState.Questions[GameState.RoundNumber].Answers
        );


        // 🔹 KOMENDA Z PARAMETREM
        RevealAnswerCommand = new RelayCommand<Answer>(async answer =>
        {
            GameState.IsFirstWins = false;

            PlaySound("Assets/Sounds/correct.wav");
            
            if (answer == null || answer.IsRevealed)
                return;

            answer.IsRevealed = true;
            GameState.RoundPoints += answer.Points;

            // Check if all answers are revealed
            if (Answers.All(a => a.IsRevealed))
            {
                await _soundService.PlaySequenceAsync(
                  "Assets/Sounds/dzwonki.wav"
                 );
                if (IsTeamAAnswering)
                {
                    TeamAPoints += GameState.RoundPoints;
                }
                else if (IsTeamBAnswering)
                {
                    TeamBPoints += GameState.RoundPoints;
                }
                GameState.RoundPoints = 0;

                await _soundService.PlaySequenceAsync(
                  "Assets/Sounds/przed_i_po_rundzie.wav"
                 );

            }

            OnPropertyChanged(nameof(FirstWinsText));
            OnPropertyChanged(nameof(TeamAPoints));
            OnPropertyChanged(nameof(TeamBPoints));
            OnPropertyChanged(nameof(RoundPoints));

        });


        AnswerCorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/correct.wav"));

        AnswerIncorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/incorrect.wav"));

        PlayAnswerCorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/correct.wav"));

        PlayAnswerIncorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/incorrect.wav"));

        PlayDzwonkiCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/dzwonki.wav"));

        TeamAAnsweringCommand = new RelayCommand(() =>
        {

            GameState.IsTeamAAnswering = true;
            GameState.IsTeamBAnswering = false;
            OnPropertyChanged(nameof(TeamAnswersText));
        });

        TeamBAnsweringCommand = new RelayCommand(() =>
        {
            GameState.IsTeamAAnswering = false;
            GameState.IsTeamBAnswering = true;
            OnPropertyChanged(nameof(TeamAnswersText));

        } );

        // 🔹 BEZ PARAMETRU
        TriggerNextRoundCommand = new RelayCommand(() =>
        {
            GameState.IsFirstWins = true;

            if (GameState.RoundNumber + 1 >= GameState.Questions.Count)
                return;

            GameState.RoundNumber++;

            Answers = new ObservableCollection<Answer>(
                GameState.Questions[GameState.RoundNumber].Answers
            );

            PlaySound("Assets/Sounds/przed_i_po_rundzie.wav");

            OnPropertyChanged(nameof(CurrentQuestionText));
            OnPropertyChanged(nameof(FirstWinsText));
        });

        TestTeamALost3Chances = new RelayCommand(() =>
        {
            GameState.TeamALostChances = 3;
            RoundPoints = -2;
        });

        // ROUND POINTS
        GameState.PropertyChanged += GameState_PropertyChanged;

        
    }

    private void PlaySound(string relativePath)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (!File.Exists(path)) return;

        new SoundPlayer(path).Play();
    }


    // GameState event handler
    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameState.RoundPoints))
        {
            OnPropertyChanged(nameof(RoundPoints));
        }

        if (e.PropertyName == nameof(GameState.RoundNumber))
        {
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        if (e.PropertyName == nameof(GameState.TeamALostChances))
        {
            OnPropertyChanged(nameof(TeamAX1));
            OnPropertyChanged(nameof(TeamAX2));
            OnPropertyChanged(nameof(TeamAX3));
        }

        if (e.PropertyName == nameof(GameState.TeamBLostChances))
        {
            OnPropertyChanged(nameof(TeamBX1));
            OnPropertyChanged(nameof(TeamBX2));
            OnPropertyChanged(nameof(TeamBX3));
        }

        if (e.PropertyName == nameof(GameState.RoundPoints))
            OnPropertyChanged(nameof(RoundPoints));

        if (e.PropertyName == nameof(GameState.TeamAPoints))
            OnPropertyChanged(nameof(TeamAPoints));

        if (e.PropertyName == nameof(GameState.TeamBPoints))
            OnPropertyChanged(nameof(TeamBPoints));

        if (e.PropertyName == nameof(GameState.FirstWinsIterations))
            OnPropertyChanged(nameof(FirstWinsIterations));

        if (e.PropertyName == nameof(GameState.IsFirstWins))
            OnPropertyChanged(nameof(IsFirstWins));

        if (e.PropertyName == nameof(GameState.IsTeamAFirstWinsError))
            OnPropertyChanged(nameof(IsTeamAFirstWinsError));

        if (e.PropertyName == nameof(GameState.IsTeamBFirstWinsError))
            OnPropertyChanged(nameof(IsTeamBFirstWinsError));

        if (e.PropertyName == nameof(GameState.IsTeamAAnswering))
            OnPropertyChanged(nameof(IsTeamAAnswering));

        if (e.PropertyName == nameof(GameState.IsTeamBAnswering))
            OnPropertyChanged(nameof(IsTeamBAnswering));
    }


}
