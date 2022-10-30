using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueModels;
using MultiQueueTesting;

namespace MultiQueueSimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            groupBox3.Hide();
            groupBox4.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void fillGrid(List<SimulationCase>tapl)
        {
            dataGridView1.Rows.Clear();
            foreach (var item in tapl)
                dataGridView1.Rows.Add(item.CustomerNumber, item.RandomInterArrival, item.InterArrival, item.ArrivalTime, item.AssignedServer.ID, item.RandomService, item.ServiceTime, item.StartTime, item.EndTime, item.TimeInQueue);
        }
        public static SimulationSystem sys;
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "openFileDialog1")
            {
                SimulationSystem system = new SimulationSystem();
                string[] s = openFileDialog1.FileName.Split('\\');
                system.readfile(openFileDialog1.FileName);
                system.StartSimu();
                system.set_perfMes();
                string result = TestingManager.Test(system, s[s.Length - 1]);
                MessageBox.Show(result);
                fillGrid(system.SimulationTable);
                PerformanceMeasuresButton.Show();
                GraphsButton.Show();
                sys = new SimulationSystem();
                sys = system;
            }
            else
                MessageBox.Show("choose valid test file");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = sys.PerformanceMeasures.AverageWaitingTime.ToString();
            textBox2.Text = sys.PerformanceMeasures.MaxQueueLength.ToString();
            textBox3.Text = sys.PerformanceMeasures.WaitingProbability.ToString();
            groupBox3.Show();
            groupBox1.Hide();
            groupBox4.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Show();
            groupBox3.Hide();
            groupBox4.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;
            chart1.Series["Server Busy Time"].Points.Clear();
            
            comboBox1.Items.Clear();
            foreach(var item in sys.Servers)
            {
                comboBox1.Items.Add("server" + item.ID);
            }
            comboBox1.SelectedIndex = 0;
            int last = 0;
            foreach (var item in sys.SimulationTable)
                if (item.EndTime > last)
                    last = item.EndTime;
            List<int> time = new List<int>();
            List<int> busy = new List<int>();
            for (int i=1;i<=last;i++)
            {
                time.Add(i);
                busy.Add(0);
                foreach(var item in sys.SimulationTable)
                {
                    if (item.AssignedServer.ID == 1 && item.StartTime <= i && item.EndTime >= i)
                    {
                        busy[i - 1] = 1;
                        break;
                    }
                }
            }
            for(int i=0;i<time.Count;i++)
            chart1.Series["Server Busy Time"].Points.AddXY(time[i], busy[i]);
            groupBox1.Hide();
            groupBox3.Hide();
            groupBox4.Show();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;
            chart1.Series["Server Busy Time"].Points.Clear();
            int last = 0;
            foreach (var item in sys.SimulationTable)
                if (item.EndTime > last)
                    last = item.EndTime;
            List<int> time = new List<int>();
            List<int> busy = new List<int>();
            for (int i = 1; i <= last; i++)
            {
                time.Add(i);
                busy.Add(0);
                foreach (var item in sys.SimulationTable)
                {
                    if (item.AssignedServer.ID == int.Parse(comboBox1.Text[comboBox1.Text.Length-1].ToString()) && item.StartTime <= i && item.EndTime >= i)
                    {
                        busy[i - 1] = 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < time.Count; i++)
                chart1.Series["Server Busy Time"].Points.AddXY(time[i], busy[i]);
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }
    }
}
