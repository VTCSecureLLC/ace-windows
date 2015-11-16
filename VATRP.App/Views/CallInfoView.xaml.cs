using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallInfoView.xaml
    /// </summary>
    public partial class CallInfoView
    {

        public CallInfoView()
            : base(VATRPWindowType.CALL_INFO_VIEW)
        {
            InitializeComponent();
        }

        public void SetViewModel(CallInfoViewModel infoModel)
        {
            DataContext = infoModel;
        }

  

    }
}
