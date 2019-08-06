using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Timers;
using System.IO.Ports;
using System.Threading;
using System.Configuration;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace TmpCtrl
{
    public partial class Form1 : Form
    {
        Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        delegate void UpdateChart(float y);
        delegate void UpdateLog(String content);
        delegate void Update4T(float[] t4,float average);
        PidCalculator pid;
        Module module;
        int thresh;
        float tmpt,target;
        int t=0;
        Boolean started = false;

        public Form1()
        {
            InitializeComponent();
            // 设置温度曲线
            Series seriesTmp = chart1.Series[0];
            seriesTmp.ChartType = SeriesChartType.Line;
            seriesTmp.BorderWidth = 2;
            seriesTmp.Color = System.Drawing.Color.Red;
            seriesTmp.LegendText = "温度曲线";
            //设置目标曲线
            Series seriesTgt = chart1.Series[1];
            seriesTgt.ChartType = SeriesChartType.Line;
            seriesTgt.BorderWidth = 2;
            seriesTgt.Color = System.Drawing.Color.Blue;
            seriesTgt.LegendText = "目标曲线";

            //设置定时器
            timer1.Enabled = false;
            timer1.Interval = int.Parse(ConfigurationSettings.AppSettings.Get("interval"));
            timer1.Tick += new EventHandler(timer1_Tick);

            pid = new PidCalculator();
            pid.getFeed += Pid_getFeed;
            module = new Module();
            module.moduleBack += Module_moduleBack;
            module.lockTap(false);
            
            //设置最小开度
            thresh= int.Parse(ConfigurationSettings.AppSettings.Get("minTap"));

            this.button2.Enabled = false;

        }

        private void Module_moduleBack(object sender, Module.ModuleDataArg e)
        {
            switch (e.type)
            {
                case Module.ModuleDataArg.EventType.T4:
                    float average = (e.t4[0] + e.t4[1] + e.t4[2] + e.t4[3]) / 4;
                    module.outputT(average);
                    this.Invoke(new Update4T(update4T), e.t4, average);
                    break;
                case Module.ModuleDataArg.EventType.OUTPUT:
                    //this.Invoke(new UpdateLog(updateLog), "输出成功");
                    pid.getOutput();
                    break;
                case Module.ModuleDataArg.EventType.LOCK:
                    this.Invoke(new UpdateLog(updateLog), "锁定成功");
                    break;
                case Module.ModuleDataArg.EventType.ULOCK:
                    this.Invoke(new UpdateLog(updateLog), "解锁成功");
                    break;
                case Module.ModuleDataArg.EventType.ERROR:
                    this.Invoke(new UpdateLog(updateLog), "采集模块通信错误");
                    break;
                default:
                    break;
            }
        }

        private void Pid_getFeed(object sender, PidCalculator.FeedbackArg e)
        {
            if (e.kind == PidCalculator.FeedbackArg.Kind.READOUTPUT)
            {
                module.outputTap(e.output > thresh ? e.output : thresh);
                this.Invoke(new UpdateLog(updateLog),e.output.ToString());
            }
            else if(e.kind == PidCalculator.FeedbackArg.Kind.WRITETARGET)
            {
                if (e.targetIsUpdated)
                {
                    this.Invoke(new UpdateLog(updateLog), "目标温度更新成功");
                    this.Invoke(new UpdateChart(updateTarget), target);
                }
                else
                {
                    this.Invoke(new UpdateLog(updateLog), "目标温度更新失败");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (t % 6 == 0)
            {
                int index = t / 30;
                float tgt = Data.getTarget(index);
                if (tgt!=2222)
                {
                    target = tgt;
                    pid.setTarget(target);
                }
                else
                {
                    button1.PerformClick();
                }
            }
            else
            {
                if (!started&&tmpt < target)
                {
                    t = -1;
                }
                else
                {
                    started = true;
                }
                module.quest4T();
            }
        }

        private void updateChart(float y)
        {
            tmpt = y;
            this.realT.Text = tmpt.ToString();
            this.chart1.Series[0].Points.AddXY(t, tmpt);
            this.chart1.Series[1].Points.AddXY(t, target);
            write2Excel(t, tmpt, target);
            t++;
        }

        private void updateTarget(float x)
        {
            this.tmp.Text = target.ToString();
            t++;
        }

        private void updateLog(String txt)
        {
            log.Text = txt;
        }

        private void update4T(float[] t4,float average)
        {
            labelt1.Text = t4[0].ToString();
            labelt2.Text = t4[1].ToString();
            labelt3.Text = t4[2].ToString();
            labelt4.Text = t4[3].ToString();
            updateChart(average);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            t = 0;
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            File.Delete(cfg.AppSettings.Settings["logPath"].Value);
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            pid.close();
            module.close();
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(this.module);
            settings.Show();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本系统由GAMI技术研发中心研发，仅供试验使用。");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text.Equals("开始"))
                {
                    target = Data.t0[0];
                    timer1.Enabled = true;
                    timer1.Start();
                    button1.Text = "结束";
                    this.button2.Enabled = false;
                }
                else if (button1.Text.Equals("结束"))
                {
                    //此处彻底关闭阀，
                    module.lockTap(true);
                    timer1.Enabled = false;
                    timer1.Stop();
                    button1.Text = "开始";
                    this.button2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            
        }

        private void write2Excel(int t,float tmp,float tar)
        {
            if (!File.Exists(cfg.AppSettings.Settings["logPath"].Value))
            {
                XSSFWorkbook workBook = new XSSFWorkbook();
                XSSFSheet sheet = (XSSFSheet)workBook.CreateSheet();  //创建一个sheet
                IRow frow0 = sheet.CreateRow(0);  // 添加一行（一般第一行是表头）
                frow0.CreateCell(0).SetCellValue("时间");
                frow0.CreateCell(1).SetCellValue("实际温度");
                frow0.CreateCell(2).SetCellValue("目标温度");   //表头内容
                FileStream sw = File.OpenWrite(cfg.AppSettings.Settings["logPath"].Value);
                workBook.Write(sw);
                workBook.Close();
            }
            else
            {
                FileStream rs = new FileStream(cfg.AppSettings.Settings["logPath"].Value,FileMode.Open,FileAccess.Read);
                XSSFWorkbook workBook = new XSSFWorkbook(rs);
                ISheet sheet = (XSSFSheet)workBook.GetSheetAt(0);
                IRow row = sheet.CreateRow(sheet.LastRowNum + 1);
                row.CreateCell(0).SetCellValue(t);
                row.CreateCell(1).SetCellValue(tmp);
                row.CreateCell(2).SetCellValue(tar);
                rs.Close();
                rs.Dispose();
                FileStream sw = new FileStream(cfg.AppSettings.Settings["logPath"].Value, FileMode.Create,FileAccess.Write);
                workBook.Write(sw);
                workBook.Close();
                sw.Close();
                sw.Dispose();
            }
        }     
    }
}
