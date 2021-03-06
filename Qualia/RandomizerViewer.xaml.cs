using Qualia.Controls;
using System;
using System.Drawing;
using System.Windows;
using Tools;

namespace Qualia
{
    public partial class RandomizerViewer : WindowResizeControl
    {
        readonly string Randomizer;
        readonly double? A;

        Font Font;

        NetworkDataModel Model = new NetworkDataModel(Const.UnknownId, new int[] { 100, 100, 100, 100, 100, 100 });

        public RandomizerViewer()
        {
            InitializeComponent();
        }

        public RandomizerViewer(string randomizer, double? a)
        {
            InitializeComponent();
            Font = new Font("Tahoma", 6.5f, System.Drawing.FontStyle.Bold);
            CtlName.Text = randomizer;

            Randomizer = randomizer;
            A = a;

            RandomizeMode.Helper.Invoke(Randomizer, Model, A);

            CtlPresenter.Width = SystemParameters.PrimaryScreenWidth;
            CtlPresenter.Height = SystemParameters.PrimaryScreenHeight;
            RandomizerViewer_SizeChanged(null, null);
            SizeChanged += RandomizerViewer_SizeChanged;
            Loaded += RandomizerViewer_Loaded;
        }

        private void RandomizerViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void RandomizerViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {   
            CtlPresenter.SetValue(System.Windows.Controls.Canvas.LeftProperty, (CtlCanvas.ActualWidth - CtlPresenter.ActualWidth) / 2);
            CtlPresenter.SetValue(System.Windows.Controls.Canvas.TopProperty, (CtlCanvas.ActualHeight - CtlPresenter.ActualHeight) / 2);
        }

        private int GetModelWeightsMaxValue()
        {
            double max = 0;

            var layer = Model.Layers[0];
            while (layer != Model.Layers.Last())
            {
                var neuron = layer.Neurons[0];
                while (neuron != null)
                {
                    var weight = neuron.Weights[0];
                    while (weight != null)
                    {
                        if (Math.Abs(weight.Weight) > max)
                        {
                            max = Math.Abs(weight.Weight);
                        }

                        weight = weight.Next;
                    }

                    neuron = neuron.Next;
                }

                layer = layer.Next;
            }

            return (int)Math.Ceiling(Math.Max(1, max));
        }

        private void Render()
        {
            CtlPresenter.CtlPresenter.StartRender();

            const int layersDistance = 200;

            int left = CtlPresenter.CtlPresenter.Width / 2 - (Model.Layers.Count - 1) * (layersDistance - 100 / 2) / 2;
            int top = CtlPresenter.CtlPresenter.Height / 2 - Model.Layers[0].Height / 2;

            byte alpha = 100;
            int heightOf1 = (int)((CtlPresenter.CtlPresenter.Height - 250) / 2 / GetModelWeightsMaxValue());

            var zeroColor = new Pen(Color.FromArgb(alpha, Color.Gray));

            for (int layer = 0; layer < Model.Layers.Count - 1; ++layer)
            {
                var neuronsCount = Model.Layers[layer].Neurons.Count;
                for (int neuron = 0; neuron < neuronsCount; ++neuron)
                {
                    CtlPresenter.CtlPresenter.G.DrawLine(zeroColor, left - neuron + layer * layersDistance, top + neuron, 100 + left - neuron + layer * layersDistance, top + neuron);

                    for (int weight = 0; weight < Model.Layers[layer].Neurons[neuron].Weights.Count; ++weight)
                    {
                        var value = Model.Layers[layer].Neurons[neuron].Weights[weight].Weight;
                        var hover = value == 0 ? 0 : 30 * Math.Sign(value);
                        var p = Draw.GetPen(value, 0, alpha);

                        using (var pen = new Pen(Draw.MediaColorToSystemColor(p.Brush.GetColor())))
                        {
                            CtlPresenter.CtlPresenter.G.DrawLine(pen,
                                                    left - neuron + layer * layersDistance + weight,
                                                    top + neuron - hover,
                                                    left - neuron + layer * layersDistance + weight,
                                                    top + neuron - hover - (float)(heightOf1 * value));

                            CtlPresenter.CtlPresenter.G.FillEllipse(Brushes.Orange,
                                                       left - neuron + layer * layersDistance + weight - 1,
                                                       top + neuron - hover - (float)(heightOf1 * value),
                                                       2,
                                                       2);
                        }
                    }
                }

                // 1 line
                CtlPresenter.CtlPresenter.G.DrawLine(Pens.Black,
                                        left - 105,
                                        top + 100 - 30,
                                        left - 105,
                                        top + 100 - 30 - heightOf1);

                // 1 text
                CtlPresenter.CtlPresenter.G.DrawString("1", Font, Brushes.Black,
                                          left - 115,
                                          top + 100 - Font.Height - 30);

                CtlPresenter.CtlPresenter.Invalidate();
            }

            zeroColor.Dispose();
        }
    }
}