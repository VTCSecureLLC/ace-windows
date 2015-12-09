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
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for FeedbackView.xaml
    /// </summary>
    public partial class FeedbackView : Window
    {
        private FeedbackViewModel _viewModel;
        public FeedbackView()
        {
            InitializeComponent();
            _viewModel = new FeedbackViewModel();
            DataContext = _viewModel;
        }

        private void OnSendFeedback(object sender, RoutedEventArgs e)
        {
            _viewModel.SendFeedback(_viewModel);
        }
    }
}
