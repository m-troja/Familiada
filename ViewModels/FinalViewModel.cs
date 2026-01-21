using Familiada.Models;
using System.ComponentModel;

namespace Familiada.ViewModels
{
    public class FinalViewModel : INotifyPropertyChanged
    {
        public GameState GameState { get; }

        public FinalViewModel() : this(new GameState()) { }

        public FinalViewModel(GameState gameState)
        {
            GameState = gameState;
            // inicjalizacja Commands itd.
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
