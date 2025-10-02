using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DominoGameWPF.Views
{
    public partial class LeftRightSelectionWindow : Window
    {
        public bool PlaceLeft { get; private set; }

        public LeftRightSelectionWindow()
        {
            InitializeComponent();
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            PlaceLeft = true;
            DialogResult = true;
            Close();
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            PlaceLeft = false;
            DialogResult = true;
            Close();
        }
    }
}
