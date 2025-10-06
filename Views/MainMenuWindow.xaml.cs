using DominoGame;
using DominoGameWPF.ViewModels;
using System.Windows;

namespace DominoGameWPF.Views
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var setupWindow = new PlayerSelectionWindow();
            if (setupWindow.ShowDialog() == true)
            {
                // ✅ Resolve GameViewModel from DI
                var vm = App.Services.GetService(typeof(GameViewModel)) as GameViewModel;

                // ✅ Start game based on setup selection
                vm.StartGame(setupWindow.TotalPlayers, setupWindow.AiPlayers, setupWindow.MaxRounds, setupWindow.MatchPoints);

                // ✅ Open Main Game Window
                var mainGame = new MainWindow(vm);
                mainGame.Show();

                this.Close(); // Optional: close the menu
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
