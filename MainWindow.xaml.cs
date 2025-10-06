
using DominoGame.Helpers;
using DominoGameWPF.ViewModels;
using System.Windows;

namespace DominoGameWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(GameViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
