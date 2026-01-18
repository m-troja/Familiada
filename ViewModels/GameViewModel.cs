using Familiada.Models;
using Familiada.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows.Input;

public class GameViewModel : INotifyPropertyChanged
{
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
    public GameState GameState { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private ObservableCollection<Answer> _currentAnswers = new();
    public ObservableCollection<Answer> RevealedAnswers
    {
        get => _currentAnswers;
        set
        {
            _currentAnswers = value;
            OnPropertyChanged(nameof(RevealedAnswers));
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


    public GameViewModel(GameState gameState)
    {
        GameState = gameState;

        RevealedAnswers = new ObservableCollection<Answer>(
            GameState.Questions[GameState.RoundNumber].Answers
        );


        // 🔹 KOMENDA Z PARAMETREM
        RevealAnswerCommand = new RelayCommand<Answer>(answer =>
        {
            PlaySound("Assets/Sounds/correct.wav");
            if (answer == null || answer.IsRevealed)
                return;

            answer.IsRevealed = true;
            GameState.TeamAPoints += answer.Points;
        });



        AnswerIncorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/incorrect.wav"));

        PlayAnswerCorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/correct.wav"));

        PlayAnswerIncorrectCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/incorrect.wav"));

        PlayDzwonkiCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/dzwonki.wav"));

        IntroMusicCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/intro.wav"));

        // 🔹 BEZ PARAMETRU
        TriggerNextRoundCommand = new RelayCommand(() =>
        {
            if (GameState.RoundNumber + 1 >= GameState.Questions.Count)
                return;

            GameState.RoundNumber++;

            RevealedAnswers = new ObservableCollection<Answer>(
                GameState.Questions[GameState.RoundNumber].Answers
            );

            PlaySound("Assets/Sounds/przed_i_po_rundzie.wav");

            OnPropertyChanged(nameof(CurrentQuestionText));
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
    }


}
