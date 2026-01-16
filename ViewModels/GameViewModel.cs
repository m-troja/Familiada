using Familiada.Models;
using Familiada.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Media;
using System.Windows.Input;
using System.Windows.Navigation;

public class GameViewModel : INotifyPropertyChanged
{
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
            return new ObservableCollection<int> { 0, 0, 0, 0 ,0 };
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

    public GameViewModel(GameState gameState)
    {
        GameState = gameState;

        RevealedAnswers = new ObservableCollection<Answer>(
            GameState.Questions[GameState.RoundNumber].Answers
        );

        // 🔹 KOMENDA Z PARAMETREM
        RevealAnswerCommand = new RelayCommand<Answer>(answer =>
        {
            if (answer == null || answer.IsRevealed)
                return;

            answer.IsRevealed = true;
            GameState.TeamAPoints += answer.Points;
        });

        // 🔹 BEZ PARAMETRU
        TriggerNextRoundCommand = new RelayCommand(() =>
        {
            if (GameState.RoundNumber + 1 >= GameState.Questions.Count)
                return;

            GameState.RoundNumber++;

            RevealedAnswers = new ObservableCollection<Answer>(
                GameState.Questions[GameState.RoundNumber].Answers
            );

            OnPropertyChanged(nameof(CurrentQuestionText));
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

        IntroMusicCommand = new RelayCommand(() =>
            PlaySound("Assets/Sounds/intro.wav"));
    }

    private void PlaySound(string relativePath)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (!File.Exists(path)) return;

        new SoundPlayer(path).Play();
    }
}
