using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TmpCtrl
{
    public partial class TestForm : Form
    {
        private PidCalculator pid;
        private Module module;

        public TestForm()
        {
            InitializeComponent();
            pid = new PidCalculator();
            pid.getFeed += Pid_getFeed;
            module = new Module();
            module.moduleBack += Module_moduleBack;
        }

        private void Module_moduleBack(object sender, Module.ModuleDataArg e)
        {

            if (e.type == Module.ModuleDataArg.EventType.T4)
            {
                System.Console.Out.WriteLine(e.t4[0]);
                System.Console.Out.WriteLine(e.t4[1]);
                System.Console.Out.WriteLine(e.t4[2]);
                System.Console.Out.WriteLine(e.t4[3]);
            }
            else if (e.type== Module.ModuleDataArg.EventType.LOCK)
            {
                System.Console.Out.WriteLine("关闭锁定");
            }
            else if (e.type == Module.ModuleDataArg.EventType.ULOCK)
            {
                System.Console.Out.WriteLine("解锁");
            }
            else if (e.type == Module.ModuleDataArg.EventType.OUTPUT)
            {
                System.Console.Out.WriteLine("输出成功");
            }
            else if (e.type == Module.ModuleDataArg.EventType.ERROR)
            {
                System.Console.Out.WriteLine("发生错误");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pid.setTarget(100);
        }

        private void Pid_getFeed(object sender, EventArgs e)
        {
            PidCalculator.FeedbackArg arg = (PidCalculator.FeedbackArg)e;
            System.Console.Out.WriteLine(arg.kind);
            System.Console.Out.WriteLine(arg.output);
            System.Console.Out.WriteLine(arg.targetIsUpdated);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pid.getOutput();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            module.quest4T();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            module.outputT(20);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            module.lockTap(true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            module.lockTap(false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            module.outputTap(2000);
        }
    }
}
