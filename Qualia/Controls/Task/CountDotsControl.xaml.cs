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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tools;

namespace Qualia.Controls
{
    public partial class CountDotsControl : UserControl, IConfigValue
    {
        event Action OnChange = delegate { };

        public CountDotsControl()
        {
            InitializeComponent();
        }

        public int InputCount => CtlTaskInputCount.Value;
        public bool IsSymmetric => CtlIsSymmetric.IsOn;
        public int MaxNumber => CtlTaskMaxNumber.Value;
        public int MinNumber => CtlTaskMinNumber.Value;

        private void Changed()
        {
            if (IsValid())
            {
                OnChange();
            }
        }

        public void Load(Config config)
        {
            Range.ForEach(this.FindVisualChildren<IConfigValue>(), c => c.Load(config));
        }

        public void Save(Config config)
        {
            Range.ForEach(this.FindVisualChildren<IConfigValue>(), c => c.Save(config));
        }

        public void Vanish(Config config)
        {
            Range.ForEach(this.FindVisualChildren<IConfigValue>(), c => c.Vanish(config));
        }

        public bool IsValid()
        {
            if (this.FindVisualChildren<IConfigValue>().All(c => c.IsValid()))
            {
                if (CtlTaskInputCount.Value >= CtlTaskMaxNumber.Value)
                {
                    return true;
                }

                CtlTaskInputCount.InvalidateValue();
            }
            return false;
        }

        public void SetChangeEvent(Action onChange)
        {
            OnChange -= onChange;
            OnChange += onChange;
            Range.ForEach(this.FindVisualChildren<IConfigValue>(), c => c.SetChangeEvent(Changed));
        }

        public void InvalidateValue()
        {
            throw new NotImplementedException();
        }
    }
}
