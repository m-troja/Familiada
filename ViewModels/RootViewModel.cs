using Familiada.Models;
using System.ComponentModel;

namespace Familiada.ViewModels
{
    public class RootViewModel : INotifyPropertyChanged
    {
        public GameState GameState { get; }

        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }

        public RootViewModel()
        {
            GameState = new GameState();
            CurrentViewModel = new GameViewModel(GameState, this);
        }

        public void SwitchToFinal()
        {
            CurrentViewModel = new FinalViewModel(GameState);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
