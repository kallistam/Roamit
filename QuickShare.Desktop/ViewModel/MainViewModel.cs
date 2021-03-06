﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickShare.Desktop.ViewModel
{
    class MainViewModel
    {
        public ObservableCollection<ClipboardItem> ClipboardActivities { get; } = new ObservableCollection<ClipboardItem>();

        public bool IsTrial { get; set; } = false;
        public DateTime TrialExpireTime { get; set; } = DateTime.MaxValue;
    }
}
