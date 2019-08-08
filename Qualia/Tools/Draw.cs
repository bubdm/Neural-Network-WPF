﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tools
{
    public static class Render
    {
        static Render()
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var dpiProperty = typeof(SystemParameters).GetProperty("Dpi", flags);

            Dpi = (int)dpiProperty.GetValue(null, null);
            PixelSize = 96.0 / Dpi;
            PixelsPerDip = Dpi / 96.0;
            HalfPixelSize = PixelSize / 2;
        }

        //Размер физического пикселя в виртуальных единицах
        public static double PixelSize { get; private set; }

        //Текущее разрешение
        public static int Dpi { get; private set; }

        public static double PixelsPerDip { get; private set; }

        //Округление до границ пикселей
        static public double SnapToPixels(double value)
        {
            value += HalfPixelSize;

            //На нестандартных DPI размер пикселя в WPF-единицах дробный.
            //Перемножение на 1000 нужно из-за потерь точности
            //при представлении дробных чисел в double
            //2.4 / 0.4 = 5.9999999999999991
            //2400.0 / 400.0 = 6.0

            var div = (value * 1000) / (PixelSize * 1000);

            return (int)div * PixelSize;
        }

        static public double Scale(double x)
        {
            return x;// SnapToPixels(x);
        }

        static public double ScaleThickness(double x)
        {
          //  x *= Render.PixelSize;
            return SnapToPixels(x);// x < 1 ? 1 : 0;
        }

        private static readonly double HalfPixelSize;
    }

    public static class Draw
    {
        public static Color GetColor(byte a, Color c)
        {
            return Color.FromArgb(a, c.R, c.G, c.B);
        }

        public static Color GetColor(byte r, byte g, byte b)
        {
            return Color.FromRgb(r, g, b);
        }

        public static Color GetColor(double v, byte alpha = 255)
        {
            int s = Math.Sign(v);

            v = Math.Abs(2 / (1 + Math.Exp(-v * 4)) - 1);

            if (v > 1)
                v = 1;

            if (s >= 0)
                return Color.FromArgb(alpha, (byte)(255 * v), (byte)(50 * v), (byte)(50 * v));
            else
                return Color.FromArgb(alpha, (byte)(50 * v), (byte)(50 * v), (byte)(255 * v));
        }

        public static Color GetColorDradient(Color from, Color to, byte alpha, double fraction)
        {
            if (fraction > 1)
                fraction = 1;
            else if (fraction < 0)
                fraction = 0;

            return Color.FromArgb(alpha,
                                  (byte)(from.R - fraction * (from.R - to.R)),
                                  (byte)(from.G - fraction * (from.G - to.G)),
                                  (byte)(from.B - fraction * (from.B - to.B)));
        }

        public static Color GetColorDradient(Color from, Color zero, Color to, byte alpha, double fraction)
        {
            if (fraction < 0.5)
            {
                return GetColorDradient(from, zero, alpha, fraction * 2);
            }
            else
            {
                return GetColorDradient(zero, to, alpha, 2 * (fraction - 0.5));
            }
        }

        public static Brush GetBrush(double v, byte alpha = 255)
        {
            return GetBrush(GetColor(v, alpha));
        }

        public static Brush GetBrush(Color c)
        {
            return new SolidColorBrush(c);
        }

        public static Pen GetPen(double v, float width = 1, byte alpha = 255)
        {
            if (width == 0)
            {
                width = Math.Abs(v) <= 1 ? 1 : (float)Math.Abs(v);
                alpha = alpha == 255 ? (byte)(alpha / (1 + (width - 1) / 2)) : alpha;
            }

            return new Pen(GetBrush(v, alpha), Render.ScaleThickness(width));
        }

        public static Pen GetPen(Color c, float width = 1)
        {
            return new Pen(GetBrush(c), Render.ScaleThickness(width));
        }

        public static Color GetRandomColor(byte offsetFromTop, Color? baseColor = null)
        {
            if (baseColor == null)
            {
                baseColor = Colors.White;
            }

            var rand = (byte)Rand.Flat.Next(offsetFromTop);
            return Color.FromArgb(255,
                                  (byte)(Math.Max(baseColor.Value.R - offsetFromTop, 0) + rand),
                                  (byte)(Math.Max(baseColor.Value.G - offsetFromTop, 0) + rand),
                                  (byte)(Math.Max(baseColor.Value.B - offsetFromTop, 0) + rand));
        }
    }
}