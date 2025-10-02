using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DominoGameWPF.Views
{
    public partial class PlayerSelectionWindow : Window
    {
        public int TotalPlayers { get; private set; }
        public int AiPlayers { get; private set; }
        public int MaxRounds { get; private set; }
        public int MatchPoints { get; private set; }

        public PlayerSelectionWindow()
        {
            InitializeComponent();

            TotalPlayersCombo.SelectionChanged += ValidateSelection;
            AiPlayersCombo.SelectionChanged += ValidateSelection;
        }

        private void ValidateSelection(object sender, SelectionChangedEventArgs e)
        {
            if (TotalPlayersCombo.SelectedItem is ComboBoxItem totalItem &&
                AiPlayersCombo.SelectedItem is ComboBoxItem aiItem)
            {
                int totalPlayers = int.Parse(totalItem.Content.ToString());
                int aiPlayers = int.Parse(aiItem.Content.ToString());

                if (aiPlayers >= totalPlayers)
                {
                    int correctedAI = totalPlayers - 1;
                    if (correctedAI < 0) correctedAI = 0;

                    AiPlayersCombo.SelectedItem = AiPlayersCombo.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(x => x.Content.ToString() == correctedAI.ToString());

                    MessageBox.Show(
                        "AI players cannot be equal to or exceed the total number of players.\nAt least one human is required.",
                        "Invalid Selection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            TotalPlayers = int.Parse((TotalPlayersCombo.SelectedItem as ComboBoxItem)?.Content.ToString());
            AiPlayers = int.Parse((AiPlayersCombo.SelectedItem as ComboBoxItem)?.Content.ToString());
            MaxRounds = int.Parse((RoundsCombo.SelectedItem as ComboBoxItem)?.Content.ToString());
            MatchPoints = int.Parse((MatchPointsCombo.SelectedItem as ComboBoxItem)?.Content.ToString()); // ✅ NEW

            if (AiPlayers >= TotalPlayers)
            {
                MessageBox.Show(
                    "Cannot start the game: At least one human player is required.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
