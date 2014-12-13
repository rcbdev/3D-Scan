using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner3D.Library
{
    public class ArduinoControl
    {
        private readonly SerialPort _arduinoSerialPort = new SerialPort();
        private readonly byte[] _turnMotorBytes = { 0 };
        private readonly byte[] _laserOnBytes = { 1 };
        private readonly byte[] _laserOffBytes = { 2 };

        public void OpenConnection()
        {
            if (!_arduinoSerialPort.IsOpen)
            {
                _arduinoSerialPort.PortName = ConfigurationManager.AppSettings["ArduinoPort"];
                _arduinoSerialPort.BaudRate = 9600;
                _arduinoSerialPort.Open();
            }
        }

        public void TurnMotor()
        {
            if (_arduinoSerialPort.IsOpen)
            {
                _arduinoSerialPort.Write(_turnMotorBytes, 0, _turnMotorBytes.Length);
            }
        }

        public void LaserOn()
        {
            if (_arduinoSerialPort.IsOpen)
            {
                _arduinoSerialPort.Write(_laserOnBytes, 0, _laserOnBytes.Length);
            }
        }

        public void LaserOff()
        {
            if (_arduinoSerialPort.IsOpen)
            {
                _arduinoSerialPort.Write(_laserOffBytes, 0, _laserOffBytes.Length);
            }
        }
    }
}
