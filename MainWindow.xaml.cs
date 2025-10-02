using DominoGame.Controllers;
using DominoGame.Interfaces;
using DominoGameWPF.ViewModels;
using DominoGameWPF.Views;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoGameWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(GameViewModel vm)
        {
            InitializeComponent();
            var selectionWindow = new PlayerSelectionWindow();
            var result = selectionWindow.ShowDialog();

            if (result == true)
            {
                vm.StartGame(
                    numberOfPlayers: selectionWindow.TotalPlayers,
                    numberOfAI: selectionWindow.AiPlayers,
                    maxRounds: selectionWindow.MaxRounds
                );
            }
            else
            {
                Application.Current.Shutdown();
            }

                DataContext = vm;
        }
    }
}
