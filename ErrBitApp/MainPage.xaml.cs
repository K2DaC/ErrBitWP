using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace ErrBitApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const String DIR = "ErrBitNotify";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;

        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            doStuff();
           
        }

        private void doStuff()
        {
            System.Diagnostics.Debug.WriteLine("sdf");
            throw new SystemException();
        }
    }
}