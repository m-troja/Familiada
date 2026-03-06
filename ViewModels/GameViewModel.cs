using Familiada.Models;
using Familiada.Services;
using Familiada.ViewModels;
using irnin.FirstPressDevice;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows.Input;

namespace Familiada.ViewModels;

public class GameViewModel : INotifyPropertyChanged 
{

    private readonly RootViewModel _rootViewModel;
    private readonly SoundService _soundService = new SoundService();

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
    public ICommand TestSwitchToFinal { get; }
    public GameState GameState { get; }
    private FirstPressDevice device;

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
            else if (GameState.Phase == Phase.MainGame)
            {
                return $"Pytanie {GameState.RoundNumber}: {GameState.QuestionsGame[GameState.RoundNumber - 1].Text}";
            }
            return "";
        }

    }

    public double FontsizeGameScoreboard => _rootViewModel?.FontsizeGameScoreboard ?? 66;
    public double FontsizeGameScoreboardX => _rootViewModel?.FontsizeGameScoreboardX ?? 100;

    public string TeamAPointsText => "Drużyna 1";

    public string TeamBPointsText => "Drużyna 2";

    // Lost chances
    public bool IsAX1Lost => GameState.TeamALostChances >= 1;
    public bool IsAX2Lost => GameState.TeamALostChances >= 2;
    public bool IsAX3Lost => GameState.TeamALostChances >= 3;
    public bool IsBX1Lost => GameState.TeamBLostChances >= 1;
    public bool IsBX2Lost => GameState.TeamBLostChances >= 2;
    public bool IsBX3Lost => GameState.TeamBLostChances >= 3;

    public string Team1Name => GameState.Team1Name;
    public string Team2Name => GameState.Team2Name;

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
                return $"Odpowiadają {Team1Name}";

            if (GameState.IsTeamBAnswering)
                return $"Odpowiadają {Team2Name}";

            return "";
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
            }
            OnPropertyChanged(nameof(RoundPoints));
        }
    }
    public string TotalPointsText => $"SUMA: {GameState.RoundPoints}";
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
    public GameViewModel() : this(new GameState(), null) {
    }


    public GameViewModel(GameState gameState, RootViewModel rootViewModel)
    {
        InitDevice();

        GameState = gameState;
        _rootViewModel = rootViewModel;

        Answers = new ObservableCollection<Answer>();

        RevealAnswerCommand = new RelayCommand<Answer>(async answer =>
        {
            if (answer != null) HandleRevealAnswer(answer);
        });


        //  RelayCommand NoParam
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

        TestSwitchToFinal = new RelayCommand(() =>
        {
            EnterFinal();
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
            HandleTeamAAnswering();
        });

        TeamBAnsweringCommand = new RelayCommand(() =>
        {
            HandleTeamBAnswering();
        });
        IntroMusicCommand = new RelayCommand(() =>
        {
            PlaySound("Assets/Sounds/intro.wav");
        });
        GameState.PropertyChanged += GameState_PropertyChanged;
    }

    private void HandleTeamAAnswering()
    {
        GameState.IsTeamAAnswering = true;
        GameState.IsTeamBAnswering = false;

        UpdateFrontend();
    }
    private void HandleTeamBAnswering()
    {
        GameState.IsTeamAAnswering = false;
        GameState.IsTeamBAnswering = true;

        UpdateFrontend();
    }
    private void HandleRevealAnswer(Answer answer)
    {
        if (!GameState.IsTeamAAnswering && !GameState.IsTeamBAnswering)
        {
            return;
        }

        device.Reset();

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
            }
            else if (IsTeamBAnswering)
            {
                GameState.IsTeamBdisplayBigX = true;
            }
            device.Reset();
            SwitchTeamAnswering();
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
                else if (GameState.TeamBLostChances == 2)
                {
                    GameState.IsTeamAdisplayBigX = true;
                    GameState.TeamBLostChances++;
                    SwitchTeamAnswering();
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
                else if (GameState.TeamALostChances == 2)
                {
                    GameState.IsTeamBdisplayBigX = true;
                    GameState.TeamBLostChances++;
                    SwitchTeamAnswering();
                }
                else
                {
                    GameState.TeamBLostChances++;
                }

                UpdateFrontend();            
            }
        }
        UpdateFrontend();
        PlaySound("Assets/Sounds/incorrect.wav");
    }

    private void TeamAwinsRound()
    {
        if (GameState.RoundNumber == 3)
        {
            TeamAPoints += GameState.RoundPoints * 2;
        }
        else if (GameState.RoundNumber == 4)
        {
            TeamAPoints += GameState.RoundPoints * 3;
        }
        else
        {
            TeamAPoints += GameState.RoundPoints;
        }
        EndRound();
    }
    private void TeamBwinsRound()
    {
        if (GameState.RoundNumber == 5)
        {
            TeamBPoints += GameState.RoundPoints * 2;
        }
        else if (GameState.RoundNumber == 6)
        {
            TeamBPoints += GameState.RoundPoints * 3;
        }
        else
        {
            TeamBPoints += GameState.RoundPoints;
        }
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
        OnPropertyChanged(nameof(TotalPointsText));
        OnPropertyChanged(nameof(TeamAnswersText));
        OnPropertyChanged(nameof(IsTeamAdisplayBigX));
        OnPropertyChanged(nameof(IsTeamBdisplayBigX));
    }
    private async void CheckRoundCanBeFinished()
    {
        // Check if all answers are revealed
        bool roundFinished = false;

        if (Answers.All(a => a.IsRevealed) && GameState.RoundPoints != 0)
        {
            roundFinished = true;
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

        if (!roundFinished && (GameState.TeamALostChances == 3 || GameState.TeamBLostChances == 3)) 
            await _soundService.PlaySequenceAsync(
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

    private void SwitchTeamAnswering()
    {
        if (GameState.IsTeamAAnswering)
        {
            GameState.IsTeamAAnswering = false;
            GameState.IsTeamBAnswering = true;
        }
        else if (GameState.IsTeamBAnswering)
        {
            GameState.IsTeamAAnswering = true;
            GameState.IsTeamBAnswering = false;
        }
        UpdateFrontend();
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

        if (GameState.RoundNumber + 1 > GameState.QuestionsGame.Count)
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
            GameState.QuestionsGame[GameState.RoundNumber -1].Answers
        );

        GameState.RoundPoints = 0;
        GameState.TeamALostChances = 0;
        GameState.TeamBLostChances = 0;
        GameState.FirstWinsIterations = 1;

        await _soundService.PlaySequenceAsync(
          "Assets/Sounds/przed_i_po_rundzie.wav"
         );

        RemoveFirstWinsErrors();
        device.Reset();
        UpdateFrontend();
    }

    private async void EndRound()
    {
       GameState.RoundFinished = true;
        RoundPoints = 0;
        await _soundService.PlaySequenceAsync(
          "Assets/Sounds/przed_i_po_rundzie.wav"
         );
    }

    private void EnterFinal()
    {
        GameState.RoundNumber = 0;
        GameState.RoundPoints = 0;
        GameState.Phase = Phase.Final;
        UpdateFrontend();
        _rootViewModel.SwitchToFinal();
    }

    private void CheckFirstWins()
    {
        GameState.IsTeamAAnswering = false;
        GameState.IsTeamBAnswering = false;
        device.Reset();
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
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.RoundNumber)) 
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.TeamALostChances))   
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.TeamBLostChances))  
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.RoundPoints))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.TeamAPoints))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.TeamBPoints))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.FirstWinsIterations))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsFirstWins))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsTeamAdisplayBigX))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsTeamBdisplayBigX))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsTeamAAnswering))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsTeamBAnswering))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsLastChance))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.IsFinalRound))
            UpdateFrontend();

        if (e.PropertyName == nameof(GameState.Phase))
            UpdateFrontend();
    }

    private async Task InitDevice()
    {
        device = new FirstPressDevice();
        var comPortName = System.Environment.GetEnvironmentVariable("SERIAL_PORT_NAME") ?? "COM5";

        if (!device.Connect(comPortName))
        {
            Console.WriteLine("Device not connected!!!!!!!!!!");
            return;
        }

        await Task.Delay(2000); 
        device.Reset();

        device.OnTeamAPressed = () =>
        {
            HandleTeamAAnswering();
        }
        ;
        device.OnTeamBPressed = () =>
        {
            HandleTeamBAnswering();
        };
    }
}
