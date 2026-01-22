using Familiada.Models;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Familiada.ViewModels
{
    public class FinalViewModel : INotifyPropertyChanged
    {
        public ICommand RevealAnswerCommand { get; }
        public ICommand TriggerNextPlayerCommand { get; }
        public ICommand AnswerCorrectCommand { get; }
        public ICommand AnswerIncorrectCommand { get; }
        public ICommand PlayAnswerCorrectCommand { get; }
        public ICommand PlayAnswerIncorrectCommand { get; }
        public ICommand PlayDzwonkiCommand { get; }
        public ICommand IntroMusicCommand { get; }
        public FinalViewModel() : this(new GameState()) { }
        public GameState GameState { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public FinalViewModel(GameState gameState)
        {
            GameState = gameState;
            GameState.PropertyChanged += GameState_PropertyChanged;

            TriggerNextPlayerCommand = new RelayCommand(() =>
            {
                GameState.IsFinalSecondPlayerAnswering = true;
                UpdateFrontend();

            });

            Init();

        }


        public string FinalText => "Jestesmy w finalowej rundzie!";
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

        private void Init()
        {
            UpdateFrontend();
        }

        private void UpdateFrontend()
        {
            OnPropertyChanged(nameof(IsFinalSecondPlayerAnswering));
            OnPropertyChanged(nameof(FinalText));
        }

        private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameState.IsFinalSecondPlayerAnswering))
            {
                OnPropertyChanged(nameof(IsFinalSecondPlayerAnswering));
            }
        }

    }
}
