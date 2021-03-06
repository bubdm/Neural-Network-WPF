using System;
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
    public partial class OutputNeuronControl : NeuronBase
    {
        List<IConfigValue> ConfigParams;

        public OutputNeuronControl(long id, Config config, Action<Notification.ParameterChanged> onNetworkUIChanged)
            : base(id, config, onNetworkUIChanged)
        {
            InitializeComponent();

            ConfigParams = new List<IConfigValue>()
            {
                CtlActivationFunc,
                CtlActivationFuncParamA
            };

            ConfigParams.ForEach(p => p.SetConfig(Config));
            LoadConfig();
            ConfigParams.ForEach(p => p.SetChangeEvent(OnChanged));
        }

        public override string WeightsInitializer => nameof(InitializeMode.None);
        public override double? WeightsInitializerParamA => null;
        public override bool IsBias => false;
        public override bool IsBiasConnected => false;
        public override string ActivationFunc => CtlActivationFunc.SelectedItem.ToString();
        public override double? ActivationFuncParamA => CtlActivationFuncParamA.ValueOrNull;

        public void LoadConfig()
        {
            ActivationFunction.Helper.FillComboBox(CtlActivationFunc, Config, nameof(ActivationFunction.LogisticSigmoid));
            ConfigParams.ForEach(p => p.LoadConfig());

            StateChanged();
        }

        private void OnChanged()
        {
            OnNetworkUIChanged(Notification.ParameterChanged.Structure);
        }

        public override void OrdinalNumberChanged(int number)
        {
            CtlNumber.Content = number.ToString();
        }

        public override bool IsValid()
        {
            return CtlActivationFuncParamA.IsValid();
        }

        public override void SaveConfig()
        {
            ConfigParams.ForEach(p => p.SaveConfig());
        }

        public override void VanishConfig()
        {
            ConfigParams.ForEach(p => p.VanishConfig());
        }
    }
}
