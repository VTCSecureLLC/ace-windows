using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderSelectorScreen.xaml
    /// </summary>
    public partial class ProviderSelectorScreen : UserControl
    {

        #region Memebers
        private readonly MainWindow _mainWnd;
        #endregion

        #region Properties
        #endregion
        public ProviderSelectorScreen(MainWindow theMain)
        {
            _mainWnd = theMain;
            InitializeComponent();
        }

    }
}
