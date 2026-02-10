using Familiada.Models;
using Familiada.Services;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Familiada.ViewModels
{
    public class FinalViewModel : INotifyPropertyChanged
    {
        public ICommand RevealAnswerCommand { get; }
        public ICommand TriggerNextPlayerAnsweringCommand { get; }
        public ICommand PlayAnswerIncorrectCommand { get; }
        public ICommand InsertWrongAnswerCommand { get; }
        public ICommand StartTimerCommand { get; }
        public ICommand ResetTimerCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand PlayOutroCommand { get; }
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
        public string FinalText => $"Odpowiada gracz {(GameState.IsFinalSecondPlayerAnswering ? 2 : 1)}";
        public bool IsFinalSecondPlayerAnswering => GameState.IsFinalSecondPlayerAnswering;
        public string RoundPointsText => $"Punkty razem: {GameState.RoundPoints}";

        public int _timerSeconds;
        public int TimerSeconds
        {
            get => _timerSeconds;
            set 
            {
                _timerSeconds = value;
                OnPropertyChanged(nameof(TimerSeconds));
            }
        }

        private CancellationTokenSource? _timerCts;
        private SoundService _soundService;

        public ObservableCollection<Question> AllQuestionsInFinal => GameState.QuestionsFinal;
        public string TotalPointsText => $"SUMA: {GameState.RoundPoints}";

        public FinalViewModel(GameState gameState)
        {
            GameState = gameState;
            _soundService = new SoundService();
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

            RevealAnswerCommand = new RelayCommand<Answer>(async answer =>
            {
                if (answer != null) HandleRevealAnswer(answer);
            });

            PlayAnswerIncorrectCommand = new RelayCommand(() =>
                PlaySound("Assets/Sounds/final_powtorzenie.wav"));

            InsertWrongAnswerCommand = new RelayCommand(() =>
            {
                HandleWrongAnswer();
            });

            StartTimerCommand = new RelayCommand(() =>
            {
                int playerAnswering = IsFinalSecondPlayerAnswering ? 2 : 1;
                _timerCts = new CancellationTokenSource();
                _ = StartTimerAsync(playerAnswering, _timerCts.Token);

                UpdateFrontend();
            });
            
            ResetTimerCommand = new RelayCommand(() =>
            {
                ResetTimer();
            });
           
            ResetCommand = new RelayCommand(() =>
            {
                Reset();
            });

            PlayOutroCommand = new RelayCommand(() =>
            {
                PlaySound("Assets/Sounds/outro.wav");
            });

        }

        private void Reset()
        {
            foreach (var answer in GameState.QuestionsFinal.SelectMany(q => q.Answers))
            {
                answer.IsRevealed = false;
            }

            GameState.AnswersFinalPlayer1 = new ObservableCollection<Answer>(new List<Answer> { new(), new(), new(), new(), new() });
            GameState.AnswersFinalPlayer2 = new ObservableCollection<Answer>(new List<Answer> { new(), new(), new(), new(), new() });
            GameState.IsFinalSecondPlayerAnswering = false;
            GameState.RoundNumber = 0;
            GameState.RoundPoints = 0;
            GameState.Phase = Phase.Final;

            ResetTimer();

            UpdateFrontend();
        }

        private void ResetTimer()
        {
            if (_timerCts != null) _timerCts.Cancel();

            TimerSeconds = 0;
            UpdateFrontend();
        }

        private void HandleRevealAnswer(Answer answer)
        {
            if (answer.IsRevealed) return;

            if (!IsFinalSecondPlayerAnswering)
            {
                RevealAnswerByPlayer(answer, false);
            }
            else if (IsFinalSecondPlayerAnswering)
            {
                RevealAnswerByPlayer(answer, true);
            }
        }

        private async void RevealAnswerByPlayer(Answer answer, bool IsFinalSecondPlayerAnswering)
        {
            if (!IsFinalSecondPlayerAnswering)
            {
                var answerInCollection = GameState.AnswersFinalPlayer1.Where(a => a.DisplayPoints == "@@").FirstOrDefault();
                if (answerInCollection == null) return;
                answerInCollection.Text = answer.Text;
                answerInCollection.Points = answer.Points;
                answerInCollection.IsRevealed = true;
            }
            else
            {
                var answerInCollection = GameState.AnswersFinalPlayer2.Where(a => a.DisplayPoints == "@@").FirstOrDefault();
                if (answerInCollection == null) return;
                answerInCollection.Text = answer.Text;
                answerInCollection.Points = answer.Points;
                answerInCollection.IsRevealed = true;
            }

            answer.IsRevealed = true;
            if (GameState.Phase == Phase.Ended) return;

            GameState.RoundPoints += answer.Points;
            UpdateFrontend();

            await _soundService.PlaySequenceAsync("Assets/Sounds/correct.wav");

            if (GameState.RoundPoints >= 200)
            {
                GameState.Phase = Phase.Ended;
                await _soundService.PlaySequenceAsync("Assets/Sounds/dzwonki.wav");
            }

        }
        private void HandleWrongAnswer()
        {
            if (!IsFinalSecondPlayerAnswering)
            {
                var answerInCollection = GameState.AnswersFinalPlayer1.Where(a => a.DisplayPoints == "@@").FirstOrDefault();
                if (answerInCollection != null)
                {
                    answerInCollection.Text = "-------------------------------------------";
                    answerInCollection.Points = 0;
                    answerInCollection.IsRevealed = true;
                }
                UpdateFrontend();
            }
            else
            {
                var answerInCollection = GameState.AnswersFinalPlayer2.Where(a => a.DisplayPoints == "@@").FirstOrDefault();
                if (answerInCollection != null)
                {
                    answerInCollection.Text = "-------------------------------------------";
                    answerInCollection.Points = 0;
                    answerInCollection.IsRevealed = true;
                }
            }

            PlaySound("Assets/Sounds/incorrect.wav");
            UpdateFrontend();


        }
        private async Task StartTimerAsync(int playerAnswering, CancellationToken token)
        {
            int timerDuration = playerAnswering == 1 ? 15 : 20;

            try
            {
                for (int secondsLeft = timerDuration; secondsLeft >= 0; secondsLeft--)
                {
                    token.ThrowIfCancellationRequested();

                    TimerSeconds = secondsLeft;
                    UpdateFrontend();

                    await Task.Delay(1000, token);
                }

            }
            catch (OperationCanceledException)
            {
                TimerSeconds = 0;
                UpdateFrontend();
            }
        }

        private void Init()
        {
            GameState.RoundPoints = 0;
            GameState.Phase = Phase.Final;
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
            OnPropertyChanged(nameof(TotalPointsText));
        }

        private void PlaySound(string relativePath)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (!File.Exists(path)) return;

            new SoundPlayer(path).Play();
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameState.IsFinalSecondPlayerAnswering))
            {
                PlaySound("Assets/Sounds/po_1_rundzie_finalu.wav");
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
            if (e.PropertyName == nameof(GameState.RoundPoints))
            {
                OnPropertyChanged(nameof(RoundPointsText));
                OnPropertyChanged(nameof(TotalPointsText));
            }

        }
    }

}
