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
    using PointFunc = Func<DynamicStatistic.PlotPoints, DynamicStatistic.PlotPoint, long, Point>;

    public partial class PlotterPresenter : UserControl
    {
        int AxisOffset;

        public PlotterPresenter()
        {
            InitializeComponent();
            AxisOffset = Config.Main.GetInt(Const.Param.AxisOffset, 6).Value;
        }

        public void Draw(List<NetworkDataModel> models, NetworkDataModel selectedModel)
        {
            //StartRender();
            CtlPresenter.Clear();

            DrawPlotter();

            foreach (var model in models)
            {
                Vanish(model.DynamicStatistic.PercentData, GetPointPercentData);
                Vanish(model.DynamicStatistic.CostData, GetPointCostData);

                DrawData(model.DynamicStatistic.PercentData, model.Color, GetPointPercentData, false);
                DrawData(model.DynamicStatistic.CostData, Tools.Draw.GetColor(100, model.Color), GetPointCostData, true);
            }

            if (selectedModel != null)
            {
                DrawLabel(selectedModel.DynamicStatistic.PercentData, selectedModel.Color);
            }

            //CtlBox.Invalidate();
            CtlPresenter.Update();
        }

        private void DrawPlotter()
        {
            var penBlack = Tools.Draw.GetPen(Colors.Black);
            var penLightGray = Tools.Draw.GetPen(Colors.LightGray);

            double step = ((double)ActualWidth - AxisOffset) / 10;
            double y = ActualHeight - AxisOffset - AxisOffset / 2;
            double x = 0;
            for (x = 0; x < 11; ++x)
            {
                CtlPresenter.DrawLine(penLightGray, new Point((float)(AxisOffset + step * x), (float)y), new Point((float)(AxisOffset + step * x), 0));
                CtlPresenter.DrawLine(penBlack, new Point((float)(AxisOffset + step * x), (float)y), new Point((float)(AxisOffset + step * x), (float)(y + AxisOffset)));
            }

            step = ((double)ActualHeight - AxisOffset) / 10;
            x = AxisOffset / 2;
            for (y = 0; y < 11; ++y)
            {
                CtlPresenter.DrawLine(penLightGray, new Point((float)x, (float)(ActualHeight - AxisOffset - step * y)), new Point(ActualWidth, (float)(ActualHeight - AxisOffset - step * y)));
                CtlPresenter.DrawLine(penBlack, new Point((float)x, (float)(ActualHeight - AxisOffset - step * y)), new Point((float)(x + AxisOffset), (float)(ActualHeight - AxisOffset - step * y)));
            }

            CtlPresenter.DrawLine(penBlack, new Point(AxisOffset, 0), new Point(AxisOffset, ActualHeight));
            CtlPresenter.DrawLine(penBlack, new Point(0, ActualHeight - AxisOffset), new Point(ActualWidth, ActualHeight - AxisOffset));
        }

        private void DrawData(DynamicStatistic.PlotPoints data, Color color, PointFunc func, bool isRect)
        {
            var pen = Tools.Draw.GetPen(color);
            var brush = Tools.Draw.GetBrush(color);
            
            var f = data.First();
            var l = data.Last();

            var d = l.Item2.Subtract(f.Item2).Ticks;

            var prev = f;
            foreach (var p in data)
            {
                var pp = func(data, p, d);
                CtlPresenter.DrawLine(pen, func(data, prev, d), pp);

                if (isRect)
                {
                    CtlPresenter.DrawRectangle(brush, pen, new Rect(pp.X - 6 / 2, pp.Y - 6 / 2, 6, 6));
                }
                else
                {
                    CtlPresenter.DrawEllipse(brush, pen, new Point(pp.X, pp.Y), 7 / 2, 7 / 2);
                }

                prev = p;
            }
        }

        private void DrawLabel(DynamicStatistic.PlotPoints data, Color color)
        {
            var font = new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

            var text = new FormattedText(new DateTime(data.Last().Item2.Subtract(data.First().Item2).Ticks).ToString("HH:mm:ss") + " / " + Converter.DoubleToText(data.Last().Item1, "N4") + " %", Culture.Current, FlowDirection.LeftToRight, font, 6.5f, Brushes.Black, 1.25);

            CtlPresenter.DrawText(text, new Point(AxisOffset * 3, ActualHeight - AxisOffset - 20));
        }

        private Point GetPointPercentData(DynamicStatistic.PlotPoints data, DynamicStatistic.PlotPoint point, long d)
        {
            var p0 = data.First();
            var px = d == 0 ? AxisOffset : AxisOffset + (ActualWidth - AxisOffset) * point.Item2.Subtract(p0.Item2).Ticks / d;
            var py = (ActualHeight - AxisOffset) * (1 - (point.Item1 / 100));

            return new Point((int)px, (int)py);
        }

        private Point GetPointCostData(DynamicStatistic.PlotPoints data, DynamicStatistic.PlotPoint point, long d)
        {
            var p0 = data.First();
            var px = d == 0 ? AxisOffset : AxisOffset + (ActualWidth - AxisOffset) * point.Item2.Subtract(p0.Item2).Ticks / d;
            var py = (ActualHeight - AxisOffset) * (1 - Math.Min(1, point.Item1));

            return new Point((int)px, (int)py);
        }

        private void Vanish(DynamicStatistic.PlotPoints data, PointFunc func)
        {
            int vanishArea = 16;

            if (data.Count > 10)
            {
                var pointsToRemove = new List<DynamicStatistic.PlotPoint>();
                var time = data.Last().Item2.Subtract(data.First().Item2);

                for (int i = 0; i < data.Count * 0.8; ++i)
                {
                    var d = data.Last().Item2.Subtract(data.First().Item2).Ticks;
                    var p0 = func(data, data[i], d);
                    var p1 = func(data, data[i + 1], d);
                    var p2 = func(data, data[i + 2], d);

                    if ((Math.Abs(p0.X - p2.X) < vanishArea && Math.Abs(p0.Y - p2.Y) < vanishArea) &&
                        (Math.Abs(p0.X - p1.X) < vanishArea && Math.Abs(p0.Y - p1.Y) < vanishArea) &&
                        (Math.Abs(p1.X - p2.X) < vanishArea && Math.Abs(p1.Y - p2.Y) < vanishArea))
                    {
                        pointsToRemove.Add(data[i + 1]);
                        //i += 2;
                    }
                    else
                    {
                        if ((p0.X == p1.X && p1.X == p2.X && Math.Abs(p0.Y - p1.Y) < vanishArea) || (p0.Y == p1.Y && p1.Y == p2.Y && Math.Abs(p0.X - p1.X) < vanishArea))
                        {
                            pointsToRemove.Add(data[i + 1]);
                            //i += 2;
                        }
                    }
                }

                foreach (var p in pointsToRemove)
                {
                    data.Remove(p);
                }
            }
        }
    }
}