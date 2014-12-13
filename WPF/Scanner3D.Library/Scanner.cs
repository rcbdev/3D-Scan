using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Scanner3D.Library
{
    internal class Scanner
    {
        private const double LaserAngle = (Math.PI / 180) * 30;

        private readonly ArduinoControl _arduinoControl;
        private Func<bool, Image> _getImage;
        private List<Slice> _slices;

        public Scanner()
        {
            _arduinoControl = new ArduinoControl();
            _arduinoControl.OpenConnection();
        }

        public void StartScan(Func<bool, Image> getImage, Action<IEnumerable<Slice>> scanDone)
        {
            _getImage = getImage;
            _slices = new List<Slice>();

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 360; i += 2)
                {
                    await ScanSlice(i);
                }

                Dispatcher.CurrentDispatcher.Invoke(() => scanDone(_slices));
            }, TaskCreationOptions.LongRunning);
        }

        private async Task ScanSlice(int angle)
        {
            try
            {
                var originalImage = CaptureImage(true);
                LaserOn();
                await Task.Delay(300);
                var laserImage = CaptureImage(false);
                LaserOff();
                TurnMotor();
                await Task.Delay(300);
                ProcessImage(originalImage.ToBitmapSource(), laserImage.ToBitmapSource(), angle);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private void ProcessImage(BitmapSource originalImage, BitmapSource laserImage, int angle)
        {
            var height = originalImage.PixelHeight;
            var width = originalImage.PixelWidth;
            var nStride = (width * originalImage.Format.BitsPerPixel + 7) / 8;
            var originalPixels = new byte[height*nStride];
            originalImage.CopyPixels(originalPixels, nStride, 0);
            var laserPixels = new byte[height*nStride];
            laserImage.CopyPixels(laserPixels, nStride, 0);

            var matches = new List<int>();

            for (var i = 0; i < height*width; i++)
            {
                var diff = laserPixels[i*4 + 1] - originalPixels[i*4 + 1];
                if (diff > 20)
                {
                    matches.Add(i);
                    i = ((i/width) + 1)*width;
                }
            }

            var slice = new Slice
            {
                Angle = angle,
                Depths = matches.Select(m =>
                {
                    var y = m/width;
                    var x = m - y*width;
                    var xFromCenter = width/2 - x;
                    return new
                    {
                        x = xFromCenter,
                        y = y
                    };
                })
                    .Where(m => m.x >= 0)
                    .Select(m =>
                    {
                        var depth = ((double)m.x) / Math.Sin(LaserAngle);
                        return new DepthPoint
                        {
                            Depth = depth,
                            Height = height - m.y
                        };
                    }).ToList()
            };

            _slices.Add(slice);
        }

        private void TurnMotor()
        {
            _arduinoControl.TurnMotor();
        }

        private void LaserOn()
        {
            _arduinoControl.LaserOn();
        }

        private void LaserOff()
        {
            _arduinoControl.LaserOff();
        }

        private Image CaptureImage(bool original)
        {
            return _getImage(original);
        }
    }

    public static class NumericExtensions
    {
        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}