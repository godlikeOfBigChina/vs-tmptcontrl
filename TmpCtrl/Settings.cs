using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TmpCtrl
{
    public partial class Settings : Form
    {
        Module m;
        Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        public Settings(Module module)
        {
            InitializeComponent();
            tbpath.Text = cfg.AppSettings.Settings["logPath"].Value;
            pidcom.Text = cfg.AppSettings.Settings["pidPort"].Value;
            modulecom.Text = cfg.AppSettings.Settings["modulePort"].Value;
            tbmintap.Text = cfg.AppSettings.Settings["minTap"].Value;
            tbinterval.Text= cfg.AppSettings.Settings["interval"].Value;
            m = module;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cfg.AppSettings.Settings["logPath"].Value=tbpath.Text.ToString();
            cfg.AppSettings.Settings["pidPort"].Value=pidcom.Text.ToString();
            cfg.AppSettings.Settings["modulePort"].Value=modulecom.Text.ToString();
            cfg.AppSettings.Settings["minTap"].Value=tbmintap.Text.ToString();
            cfg.AppSettings.Settings["interval"].Value=tbinterval.Text.ToString();
            cfg.Save();
            MessageBox.Show("参数修改成功！");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m.down(true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            m.down(false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m.up(true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            m.up(false);
        }
    }
}
