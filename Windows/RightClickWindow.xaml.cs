﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace loki_bms_csharp.Windows
{
    /// <summary>
    /// Interaction logic for RightClickWindow.xaml
    /// </summary>
    public partial class RightClickWindow : Window
    {
        public RightClickWindow()
        {
            InitializeComponent();
            ButtonPane.ParentWindow = this;
        }

        public void Popup (Point screenpos, Vector64 worldPos)
        {
            Left = screenpos.X;
            Top = screenpos.Y;

            ButtonPane.Reinit(worldPos);

            Show();
        }
    }
}
