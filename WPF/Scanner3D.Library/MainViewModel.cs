using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Scanner3D.Library
{
    public class MainViewModel : ViewModelBase
    {
        private BitmapSource _currentOriginalImage;
        private BitmapSource _currentLaserImage;
        private WebcamCapture _webcamCapture;
        private IEnumerable<Slice> _lastScan;

        public MainViewModel()
        {
            StartScanCommand = new RelayCommand(StartScan);
            SaveScanCommand = new RelayCommand(SaveLastScan);
        }

        public ICommand StartScanCommand { get; set; }
        public ICommand SaveScanCommand { get; private set; }

        public WebcamCapture WebcamCapture
        {
            get { return _webcamCapture; }
            set
            {
                if (value != null)
                {
                    value.StartCapture();
                }
                _webcamCapture = value;
            }
        }

        public BitmapSource CurrentOriginalImage
        {
            get { return _currentOriginalImage; }
            set { Set(ref _currentOriginalImage, value); }
        }

        public BitmapSource CurrentLaserImage
        {
            get { return _currentLaserImage; }
            set { Set(ref _currentLaserImage, value); }
        }

        private void StartScan()
        {
            var scanner = new Scanner();
            scanner.StartScan(CaptureImage, ScanDone);
        }

        private void ScanDone(IEnumerable<Slice> lastScan)
        {
            _lastScan = lastScan;
        }

        private void SaveLastScan()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Point Cloud (*.ply)|*.ply";
            dialog.AddExtension = true;
            dialog.DefaultExt = "ply";
            var selected = dialog.ShowDialog();
            if (selected ?? false)
            {
                var filePath = dialog.FileName;

                var rawData = _lastScan == null ? 
                    new List<string>() :
                    _lastScan.SelectMany(
                        s =>
                            s.Depths.Select(
                                d =>
                                    string.Format("{0} {1} {2}", d.Depth*Math.Sin(s.Angle.ToRadians()), d.Height,
                                        d.Depth*Math.Cos(s.Angle.ToRadians())))).ToList();

                var data = new List<string>
                {
                    "ply",
                    "format ascii 1.0",
                    "element vertex " + (rawData.Count()),
                    "property float x",
                    "property float y",
                    "property float z",
                    "end_header"
                };

                if (_lastScan != null)
                {
                    data.AddRange(rawData);
                }

                File.WriteAllLines(filePath, data, Encoding.ASCII);
            }
        }

        private Image CaptureImage(bool original)
        {
            var source = WebcamCapture.GrabImage();
            if (original)
            {
                CurrentOriginalImage = WebcamCapture.GrabBitmapSource();
            }
            else
            {
                CurrentLaserImage = WebcamCapture.GrabBitmapSource();
            }
            return source;
        }
    }
}
