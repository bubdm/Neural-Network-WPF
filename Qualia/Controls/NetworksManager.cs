using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tools;

namespace Qualia.Controls
{
    public class NetworksManager
    {
        public readonly Config Config;
        public ListX<NetworkDataModel> Models;

        Action<Notification.ParameterChanged> OnNetworkUIChanged;

        TabControl CtlTabs;
        INetworkTask Task;

        public NetworksManager(TabControl tabs, string name, Action<Notification.ParameterChanged> onNetworkUIChanged)
        {
            OnNetworkUIChanged = onNetworkUIChanged;
            CtlTabs = tabs;

            Config = String.IsNullOrEmpty(name) ? CreateNewManager() : new Config(name);
            if (Config != null)
            {
                ClearNetworks();
                LoadConfig();
            }
        }

        NetworkDataModel _prevSelectedNetworkModel;

        public NetworkControl SelectedNetwork => CtlTabs.SelectedContent as NetworkControl;
        public NetworkDataModel SelectedNetworkModel
        {
            get
            {
                var selected = SelectedNetwork == null ? _prevSelectedNetworkModel : Models.FirstOrDefault(m => m.VisualId == SelectedNetwork.Id);
                _prevSelectedNetworkModel = selected;
                return selected;
            }
        }

        List<NetworkControl> Networks
        {
            get
            {
                var result = new List<NetworkControl>();
                for (int i = 1; i < CtlTabs.Items.Count; ++i)
                {
                    result.Add(CtlTabs.Tab(i).Content as NetworkControl);
                }
                return result;
            }
        }

        public Config CreateNewManager()
        {
            var saveDialog = new SaveFileDialog
            {
                InitialDirectory = Path.GetFullPath("Networks\\"),
                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            {
                if (saveDialog.ShowDialog() == true)
                {
                    if (File.Exists(saveDialog.FileName))
                    {
                        File.Delete(saveDialog.FileName);
                    }

                    var config = new Config(saveDialog.FileName);
                    Config.Main.Set(Const.Param.NetworksManagerName, saveDialog.FileName);
                    return config;
                }
            }

            return null;
        }

        private void ClearNetworks()
        {
            while (CtlTabs.Items.Count > 1)
            {
                CtlTabs.Items.RemoveAt(1);
            }
        }

        private void LoadConfig()
        {
            var networks = Config.GetArray(Const.Param.Networks);
            if (networks.Length == 0)
            {
                networks = new long[] { Const.UnknownId };
            }
            Range.For(networks.Length, i => AddNetwork(networks[i]));
            CtlTabs.SelectedIndex = (int)Config.GetInt(Const.Param.SelectedNetworkIndex, 0).Value + 1;
            RefreshNetworksDataModels();
        }

        public void RebuildNetworksForTask(INetworkTask task)
        {
            Task = task;
            Networks.ForEach(n => n.OnTaskChanged(task));
            OnNetworkUIChanged(Notification.ParameterChanged.NeuronsCount);
        }

        public void AddNetwork()
        {
            AddNetwork(Const.UnknownId);
        }

        private void AddNetwork(long id)
        {
            var network = new NetworkControl(id, Config, OnNetworkUIChanged);
            var tab = new TabItem();
            tab.Header = $"Network {CtlTabs.Items.Count}";
            tab.Content = network;
            CtlTabs.Items.Add(tab);
            CtlTabs.SelectedItem = tab;

            if (id == Const.UnknownId)
            {
                network.InputLayer.OnTaskChanged(Task);
                network.ResetLayersTabsNames();
            }
        }

        public void DeleteNetwork()
        {
            if (MessageBox.Show($"Would you really like to delete Network {CtlTabs.SelectedIndex}?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                SelectedNetwork.VanishConfig();
                var index = CtlTabs.Items.IndexOf(CtlTabs.SelectedTab());
                CtlTabs.Items.Remove(CtlTabs.SelectedTab());
                CtlTabs.SelectedIndex = index - 1;
                ResetNetworksTabsNames();
                OnNetworkUIChanged(Notification.ParameterChanged.Structure);
            }
        }

        private void ResetNetworksTabsNames()
        {
            for (int i = 1; i < CtlTabs.Items.Count; ++i)
            {
                CtlTabs.Tab(i).Header = $"Network {i}";
            }
        }

        public bool IsValid()
        {
            return !Networks.Any() || Networks.All(n => n.IsValid());
        }

        public void SaveConfig()
        {
            Config.Set(Const.Param.Networks, Networks.Select(l => l.Id));
            Config.Set(Const.Param.SelectedNetworkIndex, CtlTabs.SelectedIndex - 1);
            Networks.ForEach(n => n.SaveConfig());
        }

        public void ResetLayersTabsNames()
        {
            Networks.ForEach(n => n.ResetLayersTabsNames());
        }

        public void SaveAs()
        {
            var saveDialog = new SaveFileDialog
            {
                InitialDirectory = Path.GetFullPath("Networks\\"),
                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            {
                if (saveDialog.ShowDialog() == true)
                {
                    if (File.Exists(saveDialog.FileName))
                    {
                        File.Delete(saveDialog.FileName);
                    }

                    File.Copy(Config.Main.GetString(Const.Param.NetworksManagerName), saveDialog.FileName);
                }
            }
        }

        public ListX<NetworkDataModel> CreateNetworksDataModels()
        {
            var result = new ListX<NetworkDataModel>(Networks.Count);
            Networks.ForEach(n => result.Add(n.CreateNetworkDataModel(Task, false)));
            return result;
        }

        public void RefreshNetworksDataModels()
        {
            Models = CreateNetworksDataModels();
        }

        public void MergeModels(ListX<NetworkDataModel> models)
        {
            var newModels = new ListX<NetworkDataModel>(models.Count);

            foreach (var newModel in models)
            {
                var model = Models.Find(m => m.VisualId == newModel.VisualId);
                if (model != null)
                {
                    newModels.Add(model.Merge(newModel));
                }
                else
                {
                    newModels.Add(newModel);
                }
            }

            Models = newModels;
        }

        public void PrepareModelsForRun()
        {
            Models.ForEach(m => m.ActivateFirstLayer());
            ResetModelsDynamicStatistics();
            ResetModelsStatistics();
            ResetErrorMatrix();
        }

        public void PrepareModelsForRound()
        {
            Task.Do(Models[0]);

            // copy first layer state and last layer targets to other networks

            var model = Models.Count > 1 ? Models[1] : null;          

            while (model != null)
            {
                var neuronFirstModelFirstLayer = Models[0].Layers[0].Neurons[0];
                var neuron = model.Layers[0].Neurons[0];

                while (neuron != null)
                {
                    if (!neuron.IsBias)
                    {
                        neuron.Activation = neuronFirstModelFirstLayer.Activation;
                    }

                    neuron = neuron.Next;
                    neuronFirstModelFirstLayer = neuronFirstModelFirstLayer.Next;
                }

                var neuronFirstModelLastLayer = Models[0].Layers.Last().Neurons[0];
                neuron = model.Layers.Last().Neurons[0];

                while (neuron != null)
                {
                    neuron.Target = neuronFirstModelLastLayer.Target;

                    neuron = neuron.Next;
                    neuronFirstModelLastLayer = neuronFirstModelLastLayer.Next;
                }

                model.TargetOutput = Models[0].TargetOutput;

                model = model.Next;
            }
        }

        public void FeedForward()
        {
            Models.ForEach(m => m.FeedForward());
        }

        public void ResetModelsStatistics()
        {
            Models.ForEach(m => m.Statistics = new Statistics());
        }

        private void ResetModelsDynamicStatistics()
        {
            Models.ForEach(m => m.DynamicStatistics = new DynamicStatistics());
        }

        public void ResetErrorMatrix()
        {
            Models.ForEach(m =>
            {
                m.ErrorMatrix.ClearData();
                m.ErrorMatrix.Next.ClearData();
            });
        }
    }
}
