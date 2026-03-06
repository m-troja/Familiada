using System.Diagnostics;
using System.Windows.Controls;

namespace Familiada.Views
{
    public partial class ScoreboardFinalUserControl : UserControl
    {
        public ScoreboardFinalUserControl()
        {
            Debug.WriteLine("[Debug] ScoreboardFinalUserControl DataContext: " + DataContext?.GetType().FullName);

            InitializeComponent();

            this.Loaded += ScoreboardFinalUserControl_Loaded;
            this.DataContextChanged += ScoreboardFinalUserControl_DataContextChanged;
        }

        private void ScoreboardFinalUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DebugWriteItemsCount();
            DebugItems();

        }

        private void ScoreboardFinalUserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            DebugWriteItemsCount();
            DebugItems();

        }

        private void DebugWriteItemsCount()
        {
            if (AnswersPlayer1ItemsControl != null)
            {
                Debug.WriteLine($"[Debug] Player1 ItemsControl count: {AnswersPlayer1ItemsControl.Items.Count}");
            }
            if (AnswersPlayer2ItemsControl != null)
            {
                Debug.WriteLine($"[Debug] Player2 ItemsControl count: {AnswersPlayer2ItemsControl.Items.Count}");
            }
        }

        private void DebugItems()
        {
            if (AnswersPlayer1ItemsControl != null)
                Debug.WriteLine($"[Debug] ItemsPlayer1 count: {AnswersPlayer1ItemsControl.Items.Count}");

            if (AnswersPlayer2ItemsControl != null)
                Debug.WriteLine($"[Debug] ItemsPlayer2 count: {AnswersPlayer2ItemsControl.Items.Count}");

            foreach (var item in AnswersPlayer1ItemsControl.Items)
                Debug.WriteLine($"[Debug] Player1 item type: {item.GetType().FullName}");

            foreach (var item in AnswersPlayer2ItemsControl.Items)
                Debug.WriteLine($"[Debug] Player2 item type: {item.GetType().FullName}");
        }
    }
}