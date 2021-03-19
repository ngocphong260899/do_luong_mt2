using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;
using System.Xml;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string InputData = String.Empty;
        string InputDataTime = String.Empty;
        SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-L0CUBTA\SQLEXPRESS;Initial Catalog=done_doluong;Integrated Security=True");
        SqlDataAdapter da;
        DataTable dt;
        SqlCommand cmd;
        static int i = 1;
        //int rowIndex = 0;
       // int light;
        delegate void SetTextCallback(string text);
        public Form1()
        {
            InitializeComponent();
            
            getAvailblePort();
            KetNoiCSDL();
            if (dataGridView1.Rows.Count - 1 > 0)
            {
                i = Int32.Parse(dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[0].Value.ToString());
                i++;
            }
            else i = 1;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
            timer1.Start();
        }
        private void KetNoiCSDL()
        {

            string sql = "select * from doluong2";
            SqlCommand com = new SqlCommand(sql, con);
            com.CommandType = CommandType.Text;
            da = new SqlDataAdapter(com);
            dt = new DataTable();
            da.Fill(dt);
            con.Close();
            dataGridView1.DataSource = dt;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
        }
        void getAvailblePort()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            string[] portsCOM = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(portsCOM);
            string[] BaudRate = { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
            comboBox2.Items.AddRange(BaudRate);
        }
        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'done_doluongDataSet.doluong2' table. You can move, or remove it, as needed.
            this.doluong2TableAdapter.Fill(this.done_doluongDataSet.doluong2);

        }

        private void DataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                InputData = serialPort1.ReadLine();
                InputDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return;
            }

            if (InputData != String.Empty)
            {
                SetText(InputData);
            }

            AutoUpdateDatabases(i, InputDataTime, null);
            i++;
        }
        private delegate void dlgAutoUpdateDatabases(int id, string time, string soluong);
        private void AutoUpdateDatabases(int id, string time, string soluong)
        {
            if (this.dataGridView1.InvokeRequired)
            {
                this.Invoke(new dlgAutoUpdateDatabases(AutoUpdateDatabases), id, time, soluong);
            }
            else
            {
                con.Open();
                cmd = new SqlCommand("INSERT INTO doluong2 (id,date_time,so_luong) VALUES (@id,@date_time,@so_luong)", con);
                cmd.Parameters.Add("@id", id);
                cmd.Parameters.Add("@date_time", time);
                cmd.Parameters.Add("@so_luong", textBox1.Text);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    con.Close();
                    return;
                }
                KetNoiCSDL();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           label10.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getAvailblePort();
            KetNoiCSDL();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                if (comboBox2.Text.Length != 0)
                {
                    if (comboBox1.Text.Length != 0)
                    {
                        try
                        {
                            serialPort1.PortName = comboBox1.Text;
                            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                            serialPort1.Open();
                        }
                        catch
                        {
                            MessageBox.Show(comboBox1.Text + " is denied", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (serialPort1.IsOpen)
                        {
                            button1.Text = ("Ngắt Kết nối");
                            label9.Text = ("Đã kết nối");
                            label9.ForeColor = Color.Green;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Access to the port 'COM1' is denied", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Hãy chọn Baud Rate", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {

                serialPort1.Close();
                button1.Text = ("Kết nối");
                label9.Text = ("Chưa kết nối");
                label9.ForeColor = Color.Red;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox1.ReadOnly = true;
            int i;
            i = dataGridView1.CurrentRow.Index;
            textBox2.Text = dataGridView1.Rows[i].Cells[0].Value.ToString();
            textBox3.Text = dataGridView1.Rows[i].Cells[1].Value.ToString();
            textBox5.Text = dataGridView1.Rows[i].Cells[2].Value.ToString();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            con.Open();
            cmd = new SqlCommand("delete from doluong2", con); // xoá hết dữ liệu Table
            cmd.ExecuteNonQuery();
            KetNoiCSDL();
            i = 0;//Nếu xoá sạch database thì lưu dữ liệu với STT từ đầu
        }

        private void button4_Click(object sender, EventArgs e)
        {
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "delete from doluong2 where id = '" + textBox2.Text + "'";
            cmd.ExecuteNonQuery();
            KetNoiCSDL();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "update doluong2 set  date_time ='" + textBox3.Text + "',  so_luong = '" + textBox5.Text + "' where id = '" + textBox2.Text + "'";
            cmd.ExecuteNonQuery();
            KetNoiCSDL();
        }
    }
}
