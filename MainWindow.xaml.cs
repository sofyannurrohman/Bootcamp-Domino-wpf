
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
            SoundManager.PlayBackgroundMusic("backsound.mp3");
        }
        protected override void OnClosed(EventArgs e)
        {
            SoundManager.StopBackgroundMusic();
            base.OnClosed(e);
        }
    }
}
