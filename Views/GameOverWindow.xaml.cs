using System.Windows;

namespace DominoGameWPF.Views
{
    public partial class GameOverWindow : Window
    {
        public bool PlayAgain { get; private set; }

        public GameOverWindow(string winnerMessage)
        {
            InitializeComponent();
            WinnerText.Text = winnerMessage;
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            PlayAgain = true;
            DialogResult = true;
        }

        private void ExitToMenu_Click(object sender, RoutedEventArgs e)
        {
            PlayAgain = false;
            DialogResult = true;
        }
    }
}
