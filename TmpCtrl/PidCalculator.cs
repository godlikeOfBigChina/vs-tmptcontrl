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
    public class PidCalculator
    {
        public event EventHandler<FeedbackArg> getFeed;
        private byte[] questOutput = new byte[] { 0x01, 0x03, 0x01, 0x01, 0x00, 0x01, 0xD4, 0x36 };
        private byte[] setAuto = new byte[] { 0x01, 0x06, 0x00, 0x38, 0x00, 0x04, 0x09, 0xC4 };
        private SerialPort pidCom;
        private int target;

        public PidCalculator()
        {
            pidCom = new SerialPort(ConfigurationSettings.AppSettings.Get("pidPort"));
            pidCom.BaudRate = 9600;
            pidCom.Parity = Parity.None;
            pidCom.StopBits = StopBits.One;
            pidCom.DataBits = 8;
            pidCom.Handshake = Handshake.None;
            pidCom.DataReceived += handleData;
            pidCom.Open();
        }

        private void handleData(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen == false) return;
            byte[] data = new byte[128];
            sp.Read(data, 0, data.Length);
            sp.DiscardInBuffer();
            FeedbackArg arg = new FeedbackArg();
            if (data[1] == 0x03)//get output
            {
                arg.kind = FeedbackArg.Kind.READOUTPUT;
                arg.output = (data[3] * 256 + data[4]);
            }
            else if (data[1] == 0x06)//set ok
            {
                arg.kind = FeedbackArg.Kind.WRITETARGET;
                float t = (data[4] * 256 + data[5]) / 1.0f;
                if (this.target==t)
                {
                    arg.targetIsUpdated = true;
                }
                else
                {
                    arg.targetIsUpdated = false;
                }
            }
            getFeed(this, arg);
            System.Console.Out.WriteLine("getFeed occured");
        }

        public void setTarget(float target)
        {
            this.target = (int)target;
            pidCom.Write(getCmdByTmpt(this.target), 0, 8);
        }

        public void getOutput()
        {
            pidCom.Write(questOutput, 0, 8);
        }

        private byte[] getCmdByTmpt(int x)
        {
            byte[] rtv = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] data = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x00, 0x00 };
            data[4] = (byte)(x / 256);
            data[5] = (byte)(x % 256);
            rtv[4] = data[4];
            rtv[5] = data[5];
            byte[] crc16 = CRC.StringToHexByte(CRC.ToModbusCRC16(data, true));
            rtv[6] = crc16[0];
            rtv[7] = crc16[1];
            return rtv;
        }

        public void close()
        {
            pidCom.Close();
        }


        public class FeedbackArg: EventArgs
        {
            public enum Kind {READOUTPUT,WRITETARGET};
            public Kind kind;
            public int output;
            public Boolean targetIsUpdated;
        }
    }
}
