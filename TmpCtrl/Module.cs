using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TmpCtrl
{
    public class Module
    {
        public event EventHandler<ModuleDataArg> moduleBack;
        private byte[] quest4TCmd = new byte[] {0xFE,0x04,0x00,0x01,0x00,0x04,0xB4,0x06};
        private byte[] tapLock = new byte[] {0xFE,0x05,0x00,0x00,0xFF,0x00,0x98,0x35};
        private byte[] tapUnlock = new byte[]{0xFE,0x05,0x00,0x00,0x00,0x00,0xD9,0xC5};
        private byte[] uplock = new byte[] { 0xFE, 0x05, 0x00, 0x01, 0xFF, 0x00, 0xC9, 0xF5 };
        private byte[] upUnlock = new byte[] { 0xFE, 0x05, 0x00, 0x01, 0x00, 0x00, 0x88, 0x05 };

        private SerialPort moduleCOM;


        public Module()
        {
            moduleCOM = new SerialPort(ConfigurationSettings.AppSettings.Get("modulePort"));
            moduleCOM.BaudRate = 9600;
            moduleCOM.Parity = Parity.None;
            moduleCOM.StopBits = StopBits.One;
            moduleCOM.DataBits = 8;
            moduleCOM.Handshake = Handshake.None;
            moduleCOM.DataReceived += handleData;
            moduleCOM.Open();
        }

        private void handleData(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen == false) return;
            byte[] data = new byte[128];
            sp.Read(data, 0, data.Length);
            sp.DiscardInBuffer();
            ModuleDataArg arg = new ModuleDataArg();
            if (data[1]==0x04)
            {
                arg.type = ModuleDataArg.EventType.T4;
                //计算温度
                arg.t4[0] = (data[3] * 256 + data[4]) / 10.0f;
                arg.t4[1] = (data[5] * 256 + data[6]) / 10.0f;
                arg.t4[2] = (data[7] * 256 + data[8]) / 10.0f;
                arg.t4[3] = (data[9] * 256 + data[10]) / 10.0f;
            }
            else if(data[1] == 0x05)
            {
                arg.type = data[4]==0xFF?ModuleDataArg.EventType.LOCK:ModuleDataArg.EventType.ULOCK;
            }
            else if (data[1] == 0x10)
            {
                arg.type = ModuleDataArg.EventType.OUTPUT;
            }
            else
            {
                arg.type = ModuleDataArg.EventType.ERROR;
            }
            moduleBack(this, arg);
        }

        public void quest4T()
        {
            moduleCOM.Write(quest4TCmd, 0, quest4TCmd.Length);
        }

        public void outputT(float t)
        {
            int x = (int)(t+400);
            byte[] cmd = new byte[] {0x01, 0x10, 0x00, 0x07, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00};
            byte[] cmdWithoutCRC = new byte[] { 0x01, 0x10, 0x00, 0x07, 0x00, 0x01, 0x02, 0x00, 0x00 };
            cmdWithoutCRC[7]=cmd[7] = (byte)(x / 256);
            cmdWithoutCRC[8]=cmd[8] = (byte)(x % 256);
            byte[] crc= CRC.StringToHexByte(CRC.ToModbusCRC16(cmdWithoutCRC, true));
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            moduleCOM.Write(cmd, 0, cmd.Length);
        }

        public void outputTap(int p)
        {
            p = p / 16 + 400;
            if (p > 2000) p=2000;
            byte[] cmd = new byte[] { 0x01, 0x10, 0x00, 0x06, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00 };
            byte[] cmdWithoutCRC = new byte[] { 0x01, 0x10, 0x00, 0x06, 0x00, 0x01, 0x02, 0x00, 0x00 };
            cmdWithoutCRC[7] = cmd[7] = (byte)(p / 256);
            cmdWithoutCRC[8] = cmd[8] = (byte)(p % 256);
            byte[] crc = CRC.StringToHexByte(CRC.ToModbusCRC16(cmdWithoutCRC, true));
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            moduleCOM.Write(cmd, 0, cmd.Length);
        }

        public void lockTap(bool needLock)
        {
            byte[] cmd = needLock ? tapLock : tapUnlock;
            moduleCOM.Write(cmd, 0, cmd.Length);
        }

        public void down(bool notFinish)
        {
            lockTap(notFinish);
        }

        public void up(bool notFinish)
        {
            moduleCOM.Write(notFinish?uplock:upUnlock, 0, uplock.Length);
        }

        public void close()
        {
            moduleCOM.Close();
        }

        public class ModuleDataArg : EventArgs
        {
            public enum EventType {T4,OUTPUT,LOCK,ULOCK,ERROR};
            public EventType type;
            public float[] t4 = new float[] { 0, 0, 0, 0 };
        }
    }
}
