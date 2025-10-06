using System.Windows;
using DominoGame.Helpers;

namespace DominoGameWPF.Views
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            VolumeSlider.Value = SoundManager.GetBackgroundVolume();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SoundManager.SetBackgroundVolume(e.NewValue);
        }

        private void MuteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SoundManager.SetBackgroundVolume(0);
        }

        private void MuteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SoundManager.SetBackgroundVolume(VolumeSlider.Value);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
