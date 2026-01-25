using Familiada.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Familiada.ViewModels
{
    public class FinalViewModel : INotifyPropertyChanged
    {
        public ICommand RevealAnswerCommand { get; }
        public ICommand TriggerNextPlayerAnsweringCommand { get; }
        public ICommand AnswerCorrectCommand { get; }
        public ICommand AnswerIncorrectCommand { get; }
        public ICommand PlayAnswerCorrectCommand { get; }
        public ICommand PlayAnswerIncorrectCommand { get; }
        public ICommand PlayDzwonkiCommand { get; }
        public ICommand IntroMusicCommand { get; }
        public ICommand StartFinalRoundCommand { get; }
        public FinalViewModel() : this(new GameState()) { }
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
        public ObservableCollection<Answer> AnswersFinalPlayer1 => GameState.AnswersFinalPlayer1;
        public ObservableCollection<Answer> AnswersFinalPlayer2 => GameState.AnswersFinalPlayer2;

        public GameState GameState { get; }
        public int RoundNumber => GameState.RoundNumber;
        public string FinalText => $"Finał! Runda {RoundNumber}, odpowiada gracz {(GameState.IsFinalSecondPlayerAnswering ? 2 : 1)}";
        public bool IsFinalSecondPlayerAnswering => GameState.IsFinalSecondPlayerAnswering;
        public string PlayerAnsweringText
        {
            get
            {
                if (IsFinalSecondPlayerAnswering)
                {
                    return "Drugi gracz odpowiada";
                }
                else
                {
                    return "Pierwszy gracz odpowiada";
                }
            }
        }
        public int _timerSeconds;
        public int TimerSeconds
        {
            get => _timerSeconds;
            set => _timerSeconds = value;
        }

        public ObservableCollection<Question> AllQuestionsInFinal => GameState.QuestionsFinal;

        public FinalViewModel(GameState gameState)
        {
            GameState = gameState;
            GameState.PropertyChanged += GameState_PropertyChanged;
            GameState.RoundNumber = 0;
            Answers = new ObservableCollection<Answer>(GameState.QuestionsFinal[RoundNumber].Answers); // single question


            Init();


            TriggerNextPlayerAnsweringCommand = new RelayCommand(() =>
            {
                GameState.IsFinalSecondPlayerAnswering =  GameState.IsFinalSecondPlayerAnswering == true ? 
                    GameState.IsFinalSecondPlayerAnswering = false : 
                    GameState.IsFinalSecondPlayerAnswering = true;
                UpdateFrontend();
            });

            StartFinalRoundCommand = new RelayCommand( async () =>
            {
                await StartFinalRound();
            });
        }


        private async Task<Task> StartFinalRound()
        {
            int playerAnswering = IsFinalSecondPlayerAnswering ? 2 : 1;

            _ =  StartTimerAsync(playerAnswering);

            GameState.RoundNumber++;
            UpdateFrontend();
            return Task.CompletedTask;

        }

        private async Task StartTimerAsync(int playerAnswering)
        {
            int TimerDuration = playerAnswering == 1 ? 15 : 20;
            
            for (int secondsLeft = TimerDuration;  secondsLeft >= 0; secondsLeft--)
            {
                TimerSeconds = secondsLeft;
                UpdateFrontend();
                await Task.Delay(1000);
            }
            TimerSeconds = 0;
            UpdateFrontend();
        }
        private void Init()
        {
            UpdateFrontend();
        }

        private void UpdateFrontend()
        {
            OnPropertyChanged(nameof(IsFinalSecondPlayerAnswering));
            OnPropertyChanged(nameof(FinalText));
            OnPropertyChanged(nameof(TimerSeconds));
            OnPropertyChanged(nameof(AnswersFinalPlayer1));
            OnPropertyChanged(nameof(AnswersFinalPlayer2));
            OnPropertyChanged(nameof(AllQuestionsInFinal));
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameState.IsFinalSecondPlayerAnswering))
            {
                OnPropertyChanged(nameof(IsFinalSecondPlayerAnswering));
            }
            if (e.PropertyName == nameof(GameState.RoundNumber))
            {
                OnPropertyChanged(nameof(RoundNumber));
            }
            if (e.PropertyName == nameof(GameState.AnswersFinalPlayer1))
            {
                OnPropertyChanged(nameof(AnswersFinalPlayer1));
            }
            if (e.PropertyName == nameof(GameState.AnswersFinalPlayer2))
            {
                OnPropertyChanged(nameof(AnswersFinalPlayer2));
            }
            if (e.PropertyName == nameof(GameState.QuestionsFinal))
            {
                OnPropertyChanged(nameof(AllQuestionsInFinal));
            }
        }

    }
}
