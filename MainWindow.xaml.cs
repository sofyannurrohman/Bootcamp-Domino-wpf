using DominoGame.Controllers;
using DominoGame.Interfaces;
using DominoGameWPF.ViewModels;
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
            DataContext = vm;
        }
    }
}
