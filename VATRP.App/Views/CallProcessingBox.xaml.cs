using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Structs;
using Timer = System.Timers.Timer;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallProcessingBox.xaml
    /// </summary>
    public partial class CallProcessingBox
    {
        #region Members

        #endregion

        public CallProcessingBox() : base(VATRPWindowType.CALL_VIEW)
        {
            DataContext = new CallViewModel();
            InitializeComponent();
        }
    }
}
