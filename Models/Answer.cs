using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Familiada.Models;

public  class Answer : INotifyPropertyChanged
{
    public string Text { get;set; } = string.Empty;
    public int Points { get; set; }
    public int DisplayPoints => IsRevealed ? Points : 0;

    public string DisplayText => IsRevealed ? Text : "......................................";

    private bool _isRevealed = false;
    public bool IsRevealed
    {
        get => _isRevealed;

        set
        {
            if (_isRevealed == value) return;
            else
            {
                _isRevealed = value;
                OnPropertyChanged(nameof(IsRevealed));
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(DisplayPoints));
            }
        }
    }

    public int Position { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
    => PropertyChanged?.Invoke(this, new (name));

}
