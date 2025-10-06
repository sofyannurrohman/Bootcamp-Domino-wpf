using DominoGame.Helpers;
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
            SoundManager.PlayBackgroundMusic("backsound.mp3", 0.3);
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var setupWindow = new PlayerSelectionWindow();
            if (setupWindow.ShowDialog() == true)
            {
                var vm = App.Services.GetService(typeof(GameViewModel)) as GameViewModel;
                vm.StartGame(setupWindow.TotalPlayers, setupWindow.AiPlayers, setupWindow.MaxRounds, setupWindow.MatchPoints);

                var mainGame = new MainWindow(vm);
                mainGame.Show();

                this.Close();
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow { Owner = this };
            optionsWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.StopBackgroundMusic();
            Application.Current.Shutdown();
        }
    }
}
