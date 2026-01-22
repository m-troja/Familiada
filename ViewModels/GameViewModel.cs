using Familiada.Models;
using Familiada.Services;
using Familiada.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

namespace Familiada.ViewModels;

public class GameViewModel : INotifyPropertyChanged 
{

    private readonly RootViewModel _rootViewModel;
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
    public ICommand CheckFirstWinsCommand { get; }
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
            if (GameState.Phase == Phase.NotStarted) { return "Rozpocznij grę"; }
            else { return $"Pytanie {GameState.RoundNumber}: {GameState.Questions[GameState.RoundNumber -1].Text}"; }
        }
    }

    public string TeamAPointsText => "Punkty DrużynaA";

    public string TeamBPointsText => "Punkty DrużynaB";

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

    public string TeamAbigX => GameState.IsTeamAdisplayBigX ? "X" : "";
    public string TeamBbigX => GameState.IsTeamBdisplayBigX ? "X" : "";
    public string FirstWinsText => GameState.IsFirstWins ? $"Kto pierwszy ten lepszy!!! Iteracja {GameState.FirstWinsIterations}" : "";
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
    public bool IsTeamAdisplayBigX => GameState.IsTeamAdisplayBigX;
    public bool IsTeamBdisplayBigX => GameState.IsTeamBdisplayBigX;
    public bool IsTeamAAnswering => GameState.IsTeamAAnswering;
    public bool IsTeamBAnswering => GameState.IsTeamBAnswering;
    public bool IsLastChance => GameState.IsLastChance;
    public bool IsFinalRound => GameState.IsFinalRound;
    private Phase Phase => GameState.Phase;
    public GameViewModel() : this(new GameState(), null) { }


    public GameViewModel(GameState gameState, RootViewModel rootViewModel)
    {
        GameState = gameState;
        _rootViewModel = rootViewModel;

        Answers = new ObservableCollection<Answer>();

        RevealAnswerCommand = new RelayCommand<Answer>(async answer =>
        {
            HandleRevealAnswer(answer);
        });

        AnswerCorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/correct.wav"));

        AnswerIncorrectCommand = new RelayCommand(() =>
        {
            HandleIncorrectAnswer();
        });

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

        // 🔹 RelayCommand NoParam
        TriggerNextRoundCommand = new RelayCommand(() =>
        {
            StartNewRound();
        });

        TestTeamALost3Chances = new RelayCommand(() =>
        {
            GameState.TeamALostChances = 3;
        });

        CheckFirstWinsCommand = new RelayCommand(() =>
        {
            CheckFirstWins();
        });

        // ROUND POINTS
        GameState.PropertyChanged += GameState_PropertyChanged;
    }

    private void HandleRevealAnswer(Answer answer)
    {
        GameState.IsFirstWins = false;

        if (answer == null || answer.IsRevealed)
            return;

        answer.IsRevealed = true;

        PlaySound("Assets/Sounds/correct.wav");

        if (!GameState.RoundFinished)
        {
            GameState.IsFirstWins = false;

            RemoveFirstWinsErrors();

            GameState.RoundPoints += answer.Points;

            CheckRoundCanBeFinished();
        }
    }

    private void HandleIncorrectAnswer()
    {
        if (IsFirstWins)
        {
            if (IsTeamAAnswering)
            {
                GameState.IsTeamAdisplayBigX = true;
                UpdateFrontend();
            }
            else if (IsTeamBAnswering)
            {
                GameState.IsTeamBdisplayBigX = true;
                UpdateFrontend();
            }
        }
        else if(!IsFirstWins)
        {
            if (IsTeamAAnswering)
            {
                if (GameState.TeamBLostChances >= 3)
                {
                    GameState.IsTeamAdisplayBigX = true;
                    TeamBwinsRound();
                }
                else
                {
                    GameState.TeamALostChances++;
                }

                UpdateFrontend();
            }
            else if (IsTeamBAnswering)
            {
                if (GameState.TeamALostChances >= 3)
                {
                    GameState.IsTeamBdisplayBigX = true;
                    TeamAwinsRound();
                }
                else
                {
                    GameState.TeamBLostChances++;
                }

                UpdateFrontend();            
            }
        }
        PlaySound("Assets/Sounds/incorrect.wav");
    }

    private void TeamAwinsRound()
    {
        TeamAPoints += GameState.RoundPoints;
        EndRound();
    }
    private void TeamBwinsRound()
    {
        TeamBPoints += GameState.RoundPoints;
        EndRound();
    }
    private void UpdateFrontend() 
    {
        OnPropertyChanged(nameof(TeamAPoints));
        OnPropertyChanged(nameof(TeamBPoints));
        OnPropertyChanged(nameof(RoundPoints));
        OnPropertyChanged(nameof(TeamAX1));
        OnPropertyChanged(nameof(TeamAX2));
        OnPropertyChanged(nameof(TeamAX3));
        OnPropertyChanged(nameof(TeamBX1));
        OnPropertyChanged(nameof(TeamBX2));
        OnPropertyChanged(nameof(TeamBX3));
        OnPropertyChanged(nameof(TeamAbigX));
        OnPropertyChanged(nameof(TeamBbigX));
        OnPropertyChanged(nameof(CurrentQuestionText));
        OnPropertyChanged(nameof(IsFirstWins));
        OnPropertyChanged(nameof(FirstWinsText));
        OnPropertyChanged(nameof(FirstWinsText));
    }
    private async void CheckRoundCanBeFinished()
    {
        // Check if all answers are revealed

        if (Answers.All(a => a.IsRevealed) && GameState.RoundPoints != 0)
        {
            await _soundService.PlaySequenceAsync(
              "Assets/Sounds/dzwonki.wav"
             );

            if (IsTeamAAnswering)
            {
                TeamAwinsRound();

            }
            else if (IsTeamBAnswering)
            {
                TeamBwinsRound();
            }
        }

        // Check if any team lost 3 chances

        if (GameState.TeamALostChances == 3 || GameState.TeamBLostChances == 3) await _soundService.PlaySequenceAsync(
              "Assets/Sounds/dzwonki.wav"
             );

        if (GameState.TeamALostChances == 3)
        {
            TeamBwinsRound();
        }
        else if (GameState.TeamBLostChances == 3)
        {
            TeamAwinsRound();
        }
    }

    private void RemoveFirstWinsErrors()
    {
        GameState.IsTeamAdisplayBigX = false;
        GameState.IsTeamBdisplayBigX = false;
        UpdateFrontend();
    }
    private async void StartNewRound()
    {
        GameState.IsFirstWins = true;
        GameState.RoundFinished = false;

        if (GameState.RoundNumber + 1 > GameState.Questions.Count)
            return;

        if (GameState.Phase != Phase.MainGame && GameState.Phase != Phase.Final)
        {
            GameState.RoundNumber = 1;
            GameState.Phase = Phase.MainGame;

        }
        else
        {
            GameState.RoundNumber++;
        }

        Answers = new ObservableCollection<Answer>(
            GameState.Questions[GameState.RoundNumber -1].Answers
        );

        GameState.RoundPoints = 0;
        GameState.TeamALostChances = 0;
        GameState.TeamBLostChances = 0;
        GameState.FirstWinsIterations = 1;

        await _soundService.PlaySequenceAsync(
          "Assets/Sounds/przed_i_po_rundzie.wav"
         );

        RemoveFirstWinsErrors();
        UpdateFrontend();
    }

    private async void EndRound()
    {
       GameState.RoundFinished = true;
        RoundPoints = 0;
        await _soundService.PlaySequenceAsync(
          "Assets/Sounds/przed_i_po_rundzie.wav"
         );

        if (GameState.RoundNumber == GameState.Questions.Count)
        {
            EnterFinal();
        }
    }

    private void EnterFinal()
    {
        GameState.Phase = Phase.Final;
        UpdateFrontend();
        _rootViewModel.SwitchToFinal();
    }

    private void CheckFirstWins()
    {
        GameState.IsFirstWins = true;
        GameState.FirstWinsIterations++;
        GameState.TeamALostChances = 0;
        GameState.TeamBLostChances = 0;
        GameState.IsTeamAdisplayBigX = false;
        GameState.IsTeamBdisplayBigX = false;
        UpdateFrontend();
    }

    private void EndFirstWins()
    {
        GameState.IsFirstWins = true;
        GameState.FirstWinsIterations = 0;
        GameState.TeamALostChances = 0;
        GameState.TeamBLostChances = 0;
        GameState.IsTeamAdisplayBigX = false;
        GameState.IsTeamBdisplayBigX = false;
        UpdateFrontend();
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

        if (e.PropertyName == nameof(GameState.IsTeamAdisplayBigX))
            OnPropertyChanged(nameof(IsTeamAdisplayBigX));

        if (e.PropertyName == nameof(GameState.IsTeamBdisplayBigX))
            OnPropertyChanged(nameof(IsTeamBdisplayBigX));

        if (e.PropertyName == nameof(GameState.IsTeamAAnswering))
            OnPropertyChanged(nameof(IsTeamAAnswering));

        if (e.PropertyName == nameof(GameState.IsTeamBAnswering))
            OnPropertyChanged(nameof(IsTeamBAnswering));

        if (e.PropertyName == nameof(GameState.IsLastChance))
            OnPropertyChanged(nameof(IsLastChance));

        if (e.PropertyName == nameof(GameState.IsFinalRound))
            OnPropertyChanged(nameof(IsFinalRound));
        if (e.PropertyName == nameof(GameState.Phase))
            OnPropertyChanged(nameof(Phase));

    }
}
