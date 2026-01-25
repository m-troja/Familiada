using Familiada.Models;
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

            RevealAnswerCommand = new RelayCommand<Answer>(async answer =>
            {
                if (answer != null) HandleRevealAnswer(answer);
            });

            PlayAnswerIncorrectCommand = new RelayCommand(() =>
                PlaySound("Assets/Sounds/final_powtorzenie.wav"));

            InsertWrongAnswerCommand = new RelayCommand(() =>
            {
                if (!IsFinalSecondPlayerAnswering && GameState.AnswersFinalPlayer1.Count < 5)
                {
                    GameState.AnswersFinalPlayer1.Add(new Answer { Text = "..............................", Points = 0 });
                    UpdateFrontend();
                }
                else if (IsFinalSecondPlayerAnswering && GameState.AnswersFinalPlayer2.Count < 5)
                {
                    GameState.AnswersFinalPlayer2.Add(new Answer { Text = "..............................", Points = 0 });
                    UpdateFrontend();
                }
                PlaySound("Assets/Sounds/incorrect.wav");
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

            GameState.AnswersFinalPlayer1.Clear();
            GameState.AnswersFinalPlayer2.Clear();
            GameState.IsFinalSecondPlayerAnswering = false;
            GameState.RoundPoints = 0;

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
            if (!IsFinalSecondPlayerAnswering && GameState.AnswersFinalPlayer1.Count < 5)
            {
                GameState.AnswersFinalPlayer1.Add(answer);
            }
            else if (IsFinalSecondPlayerAnswering && GameState.AnswersFinalPlayer2.Count < 5)
            {
                GameState.AnswersFinalPlayer2.Add(answer);
            }

            if (GameState.RoundPoints >= 200)
            {
                PlaySound("Assets/Sounds/dzwonki.wav");
            }
            answer.IsRevealed = true;
            UpdateFrontend();
            
            GameState.RoundPoints += answer.Points;

            PlaySound("Assets/Sounds/correct.wav");
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

                if (!IsFinalSecondPlayerAnswering)
                {
                    PlaySound("Assets/Sounds/po_1_rundzie_finalu.wav");
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
            }
        }
    }

}
