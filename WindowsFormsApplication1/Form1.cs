using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        float Roll = 0, Pitch = 0, Yaw = 0;

        float EP = 0, ER = 0;

        float Distence;

        int AX = 0, AY = 0, AZ = 0, GX = 0, GY = 0, GZ = 0, OX = 0, OY = 0, OZ = 0;

        int P = 0, I = 0, D = 0, P1 = 0, Exp_High = 0;
        Byte[] P_Buf = new Byte[6];
        Byte[] I_Buf = new Byte[6];
        Byte[] D_Buf = new Byte[6];
        Byte[] EP_Buf = new Byte[6];
        Byte[] ER_Buf = new Byte[6];
        Byte[] EW_Buf = new Byte[6];
        Byte[] EH_Buf = new Byte[6];
        Byte[] P1_Buf = new Byte[6];
        Byte[] AU_Buf = new Byte[6];

        int Speed = 0, Speed_Last = 10;
        Byte[] Speed_Buf = new Byte[6];

        int ReadDataBytes = 0;
        Byte[] readBytes;

        Image currentImage1, currentImage2;

        Queue<Byte[]> data_list = new Queue<Byte[]>();
        Queue<float> Roll_list = new Queue<float>();
        Queue<float> Pitch_list = new Queue<float>();
        Queue<float> Yaw_list = new Queue<float>();

        int Grid_Width = 0, Grid_Heigth = 0;
        int Grid_ZeroX = 0, Grid_ZeroY = 0;

        int Image_1_OK = 0, Image_2_OK = 0;

        Thread thread1, thread2, thread3;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();

            for (int i = 0; i < portList.Length; i++)
            {
                string name = portList[i];
                comboBox1.Items.Add(name);
            }

            serialPort1.DataReceived += serialPort1_DataReceived;

            Speed_Buf[0] = Convert.ToByte('P');
            Speed_Buf[1] = Convert.ToByte('W');
            Speed_Buf[4] = Convert.ToByte('\r');
            Speed_Buf[5] = Convert.ToByte('\n');

            P_Buf[0] = Convert.ToByte('S');
            P_Buf[1] = Convert.ToByte('P');
            P_Buf[4] = Convert.ToByte('\r');
            P_Buf[5] = Convert.ToByte('\n');

            I_Buf[0] = Convert.ToByte('S');
            I_Buf[1] = Convert.ToByte('I');
            I_Buf[4] = Convert.ToByte('\r');
            I_Buf[5] = Convert.ToByte('\n');

            D_Buf[0] = Convert.ToByte('S');
            D_Buf[1] = Convert.ToByte('D');
            D_Buf[4] = Convert.ToByte('\r');
            D_Buf[5] = Convert.ToByte('\n');

            EP_Buf[0] = Convert.ToByte('E');
            EP_Buf[1] = Convert.ToByte('P');
            EP_Buf[4] = Convert.ToByte('\r');
            EP_Buf[5] = Convert.ToByte('\n');

            ER_Buf[0] = Convert.ToByte('E');
            ER_Buf[1] = Convert.ToByte('R');
            ER_Buf[4] = Convert.ToByte('\r');
            ER_Buf[5] = Convert.ToByte('\n');

            EW_Buf[0] = Convert.ToByte('E');
            EW_Buf[1] = Convert.ToByte('W');
            EW_Buf[4] = Convert.ToByte('\r');
            EW_Buf[5] = Convert.ToByte('\n');

            EH_Buf[0] = Convert.ToByte('E');
            EH_Buf[1] = Convert.ToByte('H');
            EH_Buf[4] = Convert.ToByte('\r');
            EH_Buf[5] = Convert.ToByte('\n');

            P1_Buf[0] = Convert.ToByte('S');
            P1_Buf[1] = Convert.ToByte('X');
            P1_Buf[4] = Convert.ToByte('\r');
            P1_Buf[5] = Convert.ToByte('\n');

            AU_Buf[0] = Convert.ToByte('A');
            AU_Buf[1] = Convert.ToByte('U');
            AU_Buf[4] = Convert.ToByte('\r');
            AU_Buf[5] = Convert.ToByte('\n');

            Grid_Width = pictureBox1.Width;
            Grid_Heigth = pictureBox1.Height;
            Grid_ZeroX = 0;
            Grid_ZeroY = 0 + pictureBox1.Height / 2;

            thread1 = new Thread(new ThreadStart(Make_Image_1));
            thread1.IsBackground = true;
            thread1.Start();

            thread2 = new Thread(new ThreadStart(Make_Image_2));
            thread2.IsBackground = true;
            thread2.Start();

            thread3 = new Thread(new ThreadStart(Data_Process));
            thread3.IsBackground = true;
            thread3.Start();

            KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.W:
                    if (serialPort1.IsOpen)
                    {
                        try
                        {
                            AU_Buf[2] = Convert.ToByte('2');

                            serialPort1.Write(AU_Buf, 0, 6);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                    }
                    break;
                case System.Windows.Forms.Keys.A:
                    if (serialPort1.IsOpen)
                    {
                        try
                        {
                            AU_Buf[2] = Convert.ToByte('3');

                            serialPort1.Write(AU_Buf, 0, 6);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                    }
                    break;
                case System.Windows.Forms.Keys.S:
                    if (serialPort1.IsOpen)
                    {
                        try
                        {
                            AU_Buf[2] = Convert.ToByte('1');

                            serialPort1.Write(AU_Buf, 0, 6);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                    }
                    break;
                case System.Windows.Forms.Keys.D:
                    if (serialPort1.IsOpen)
                    {
                        try
                        {
                            AU_Buf[2] = Convert.ToByte('4');

                            serialPort1.Write(AU_Buf, 0, 6);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                    }
                    break;
            }
        }

        private void Draw_Back(Graphics gg)
        {
            int i = 0;
            Pen pen_Grid = new Pen(Color.Gray, 1);

            for (i = 0; i < 25; i++)
            {
                gg.DrawLine(pen_Grid, Grid_ZeroX + (i * 20), 0, Grid_ZeroX + (i * 20), Grid_Heigth);
            }
            for (i = 0; i < 10; i++)
            {
                gg.DrawLine(pen_Grid, 0, i * 20, Grid_Width, i * 20);
            }
        }

        private void Make_Image_1()
        {
            int i = 0, j = 0;

            currentImage1 = new Bitmap(Grid_Width, Grid_Heigth);

            Pen pen_Roll = new Pen(Color.Blue, 1);
            Pen pen_Back = new Pen(Color.Black, 1);
            Pen pen_Grid = new Pen(Color.Gray, 2);

            Graphics g = Graphics.FromImage(currentImage1);

            Point[] Roll_Points_Now = new Point[500];
            Point[] Roll_Points_Last = new Point[500];

            for (i = 0; i < 500; i++)
            {
                Roll_Points_Now[i].X = i;
                Roll_Points_Now[i].Y = 0 + Grid_ZeroY;
            }

            while (true)
            {
                if (Roll_list.Count > 0)
                {
                    for (j = 0; j < Roll_list.Count; j++)
                    {
                        for (i = 0; i < 499; i++)
                        {
                            Roll_Points_Now[i].Y = Roll_Points_Now[i + 1].Y;
                        }

                        Roll_Points_Now[499].Y = -Convert.ToInt32(Roll_list.Dequeue() * 2) + Grid_ZeroY;
                        g.DrawLines(pen_Back, Roll_Points_Last);

                        Draw_Back(g);

                        g.DrawLines(pen_Roll, Roll_Points_Now);

                        Image_1_OK = 1;

                        Array.Copy(Roll_Points_Now, Roll_Points_Last, 500);
                    }
                }

                Thread.Sleep(1);
            }
        }

        private void Make_Image_2()
        {
            int i = 0, j = 0;

            currentImage2 = new Bitmap(Grid_Width, Grid_Heigth);

            Pen pen_Pitch = new Pen(Color.Yellow, 1);
            Pen pen_Back = new Pen(Color.Black, 1);
            Pen pen_Grid = new Pen(Color.Gray, 1);

            Graphics g = Graphics.FromImage(currentImage2);

            Point[] Pitch_Points_Now = new Point[500];
            Point[] Pitch_Points_Last = new Point[500];

            for (i = 0; i < 500; i++)
            {
                Pitch_Points_Now[i].X = i;
                Pitch_Points_Now[i].Y = 0 + Grid_ZeroY;
            }

            while (true)
            {
                if (Pitch_list.Count > 0)
                {
                    for (j = 0; j < Pitch_list.Count; j++)
                    {
                        for (i = 0; i < 499; i++)
                        {
                            Pitch_Points_Now[i].Y = Pitch_Points_Now[i + 1].Y;
                        }
                        Pitch_Points_Now[499].Y = -Convert.ToInt32(Pitch_list.Dequeue() * 2) + Grid_ZeroY;

                        Draw_Back(g);

                        g.DrawLines(pen_Back, Pitch_Points_Last);

                        g.DrawLines(pen_Pitch, Pitch_Points_Now);

                        Image_2_OK = 1;

                        Array.Copy(Pitch_Points_Now, Pitch_Points_Last, 500);
                    }
                }

                Thread.Sleep(1);
            }
        }

        private void Data_Process()
        {
            int i = 0;
            float temp = 0;

            while (true)
            {
                if (data_list.Count > 0)
                {
                    Byte[] buffer = data_list.Dequeue();

                    for (i = 0; i < buffer.Length - 1; i++)
                    {

                        if (buffer[i] == 'A' && buffer[i + 1] == 'T')
                        {
                            try
                            {
                                temp = (buffer[i + 2] << 24) | (buffer[i + 3] << 16) | (buffer[i + 4] << 8) | (buffer[i + 5]);
                                Roll = temp / 10;
                                temp = (buffer[i + 6] << 24) | (buffer[i + 7] << 16) | (buffer[i + 8] << 8) | (buffer[i + 9]);
                                Pitch = temp / 10;
                                temp = (buffer[i + 10] << 24) | (buffer[i + 11] << 16) | (buffer[i + 12] << 8) | (buffer[i + 13]);
                                Yaw = temp / 10;
                                temp = (buffer[i + 14] << 24) | (buffer[i + 15] << 16) | (buffer[i + 16] << 8) | (buffer[i + 17]);
                                EP = temp / 100;
                                temp = (buffer[i + 18] << 24) | (buffer[i + 19] << 16) | (buffer[i + 20] << 8) | (buffer[i + 21]);
                                ER = temp / 100;
                                temp = (buffer[i + 22] << 8) | (buffer[i + 23] << 0);
                                Distence = temp / 100;

                                Roll_list.Enqueue(Roll * 20);
                                Pitch_list.Enqueue(Pitch);
                                Yaw_list.Enqueue(Yaw);
                            }
                            catch { }
                        }

                        if (buffer[i] == 'O' && buffer[i + 1] == 'F')
                        {
                            try
                            {
                                OX = (buffer[i + 2] << 8) | (buffer[i + 3]);
                                OY = (buffer[i + 4] << 8) | (buffer[i + 5]);
                                OZ = (buffer[i + 6] << 8) | (buffer[i + 7]);


                                if (OX >= 32767)
                                {
                                    OX = OX - 65536;
                                }
                                if (OY >= 32767)
                                {
                                    OY = OY - 65536;
                                }
                            }
                            catch { }
                        }

                        if (buffer[i] == 'G' && buffer[i + 1] == 'Y')
                        {
                            try
                            {
                                AX = (buffer[i + 2] << 24) | (buffer[i + 3] << 16) | (buffer[i + 4] << 8) | buffer[i + 5];
                                AY = (buffer[i + 6] << 24) | (buffer[i + 7] << 16) | (buffer[i + 8] << 8) | buffer[i + 9];
                                AZ = (buffer[i + 10] << 24) | (buffer[i + 11] << 16) | (buffer[i + 12] << 8) | buffer[i + 13];
                                GX = (buffer[i + 14] << 24) | (buffer[i + 15] << 16) | (buffer[i + 16] << 8) | buffer[i + 17];
                                GY = (buffer[i + 18] << 24) | (buffer[i + 19] << 16) | (buffer[i + 20] << 8) | buffer[i + 21];
                                GZ = (buffer[i + 22] << 24) | (buffer[i + 23] << 16) | (buffer[i + 24] << 8) | buffer[i + 25];

                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            readBytes = new Byte[serialPort1.BytesToRead];

            try
            {
                if (serialPort1.BytesToRead > 0)
                {
                    ReadDataBytes = this.serialPort1.Read(readBytes, 0, readBytes.Length);

                    data_list.Enqueue(readBytes);
                }
            }
            catch
            { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();
                    radioButton1.Checked = true;
                    button1.Text = "关闭串口";
                    comboBox1.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("串口" + serialPort1.PortName + "打开失败了！\r\n请检查端口");
                }
            }
            else
            {
                serialPort1.Close();
                radioButton1.Checked = false;
                button1.Text = "打开串口";
                comboBox1.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    Speed = Convert.ToInt32(Microsoft.VisualBasic.Interaction.InputBox("请输入", "转速设置", Convert.ToString(Speed_Last), 300, 500));

                    Speed_Last = Speed;

                    label2.Text = Convert.ToString(Speed);

                    Speed_Buf[2] = Convert.ToByte(((Speed * 10 + 1000) >> 8));
                    Speed_Buf[3] = Convert.ToByte((Speed * 10 + 1000) & 255);

                    serialPort1.Write(Speed_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("GO  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("ST  \r\n");

                    Speed = 0;

                    label2.Text = "0";
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double temp = 0;

            if (serialPort1.IsOpen)
            {
                try
                {
                    temp = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("请输入", "P参数设置", "", pictureBox1.Location.X + 250, pictureBox1.Location.Y));

                    label28.Text = Convert.ToString(temp);

                    P = Convert.ToInt32(temp * 1000);

                    P_Buf[2] = Convert.ToByte(P >> 8);
                    P_Buf[3] = Convert.ToByte(P & 255);

                    serialPort1.Write(P_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            double temp = 0;

            if (serialPort1.IsOpen)
            {
                try
                {
                    temp = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("请输入", "I参数设置", "", pictureBox1.Location.X + 250, pictureBox1.Location.Y));

                    label30.Text = Convert.ToString(temp);

                    I = Convert.ToInt32(temp * 1000);

                    I_Buf[2] = Convert.ToByte(I >> 8);
                    I_Buf[3] = Convert.ToByte(I & 255);

                    serialPort1.Write(I_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            double temp = 0;

            if (serialPort1.IsOpen)
            {
                try
                {
                    temp = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("请输入", "P参数设置", "", pictureBox1.Location.X + 250, pictureBox1.Location.Y));

                    label32.Text = Convert.ToString(temp);

                    D = Convert.ToInt32(temp * 1000);

                    D_Buf[2] = Convert.ToByte(D >> 8);
                    D_Buf[3] = Convert.ToByte(D & 255);

                    serialPort1.Write(D_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Speed--;

                Speed_Buf[2] = Convert.ToByte(((Speed * 10 + 1000) >> 8));
                Speed_Buf[3] = Convert.ToByte((Speed * 10 + 1000) & 255);

                serialPort1.Write(Speed_Buf, 0, 6);

                label2.Text = Convert.ToString(Speed);
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Speed++;

                Speed_Buf[2] = Convert.ToByte(((Speed * 10 + 1000) >> 8));
                Speed_Buf[3] = Convert.ToByte((Speed * 10 + 1000) & 255);

                serialPort1.Write(Speed_Buf, 0, 6);

                label2.Text = Convert.ToString(Speed);
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            double temp = 0;

            if (serialPort1.IsOpen)
            {
                try
                {
                    temp = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("请输入", "期望高度设置", "", button11.Location.X - 10, pictureBox1.Location.Y + 10));

                    label38.Text = Convert.ToString(temp);

                    Exp_High = Convert.ToInt32(temp);

                    EH_Buf[2] = Convert.ToByte(Exp_High >> 8);
                    EH_Buf[3] = Convert.ToByte(Exp_High & 255);

                    serialPort1.Write(EH_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P = P + 100;

                    label28.Text = Convert.ToString((float)(P) / 1000);

                    P_Buf[2] = Convert.ToByte(P >> 8);
                    P_Buf[3] = Convert.ToByte(P & 255);

                    serialPort1.Write(P_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    D = D + 100;

                    label32.Text = Convert.ToString((float)D / 1000);

                    D_Buf[2] = Convert.ToByte(D >> 8);
                    D_Buf[3] = Convert.ToByte(D & 255);

                    serialPort1.Write(D_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P = P - 100;

                    label28.Text = Convert.ToString((float)(P) / 1000);

                    P_Buf[2] = Convert.ToByte(P >> 8);
                    P_Buf[3] = Convert.ToByte(P & 255);

                    serialPort1.Write(P_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    D = D - 100;

                    label32.Text = Convert.ToString((float)D / 1000);

                    D_Buf[2] = Convert.ToByte(D >> 8);
                    D_Buf[3] = Convert.ToByte(D & 255);

                    serialPort1.Write(D_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Speed = 0;

                Speed_Buf[2] = Convert.ToByte(((Speed * 10 + 1000) >> 8));
                Speed_Buf[3] = Convert.ToByte((Speed * 10 + 1000) & 255);

                serialPort1.Write(Speed_Buf, 0, 6);

                label2.Text = Convert.ToString(Speed);
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    I = I + 1000;

                    label30.Text = Convert.ToString((float)I / 1000);

                    I_Buf[2] = Convert.ToByte(I >> 8);
                    I_Buf[3] = Convert.ToByte(I & 255);

                    serialPort1.Write(I_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    I = I - 1000;

                    label30.Text = Convert.ToString((float)I / 1000);

                    I_Buf[2] = Convert.ToByte(I >> 8);
                    I_Buf[3] = Convert.ToByte(I & 255);

                    serialPort1.Write(I_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P = P + 1000;

                    label28.Text = Convert.ToString((float)(P) / 1000);

                    P_Buf[2] = Convert.ToByte(P >> 8);
                    P_Buf[3] = Convert.ToByte(P & 255);

                    serialPort1.Write(P_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P = P - 1000;

                    label28.Text = Convert.ToString((float)(P) / 1000);

                    P_Buf[2] = Convert.ToByte(P >> 8);
                    P_Buf[3] = Convert.ToByte(P & 255);

                    serialPort1.Write(P_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    D = D + 10;

                    label32.Text = Convert.ToString((float)D / 1000);

                    D_Buf[2] = Convert.ToByte(D >> 8);
                    D_Buf[3] = Convert.ToByte(D & 255);

                    serialPort1.Write(D_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    D = D - 10;

                    label32.Text = Convert.ToString((float)D / 1000);

                    D_Buf[2] = Convert.ToByte(D >> 8);
                    D_Buf[3] = Convert.ToByte(D & 255);

                    serialPort1.Write(D_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("SH  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Image_1_OK == 1)
            {
                pictureBox1.Image = currentImage1;
                Image_1_OK = 0;
            }
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /*label5.Text = Convert.ToString(AX);
            label8.Text = Convert.ToString(AY);
            label10.Text = Convert.ToString(AZ);
            label12.Text = Convert.ToString(GX);
            label14.Text = Convert.ToString(GY);
            label16.Text = Convert.ToString(GZ);*/
            label18.Text = Convert.ToString(Roll);
            label20.Text = Convert.ToString(Pitch);
            label22.Text = Convert.ToString(Yaw);
            label40.Text = Convert.ToString(EP);
            label43.Text = Convert.ToString(ER);
            label33.Text = Convert.ToString(Distence);

            label48.Text = Convert.ToString(OX);
            label49.Text = Convert.ToString(OY);
            label50.Text = Convert.ToString(OZ);
            /*label40.Text = Convert.ToString(vzs);
            label43.Text = Convert.ToString(dzs);*/
        }

        private void timer3_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Image_2_OK == 1)
            {
                pictureBox2.Image = currentImage2;
                Image_2_OK = 0;
            }
        }

        private void button35_Click(object sender, EventArgs e)
        {
            double temp = 0;

            if (serialPort1.IsOpen)
            {
                try
                {
                    temp = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("请输入", "P1参数设置", "", pictureBox1.Location.X + 250, pictureBox1.Location.Y));

                    label52.Text = Convert.ToString(temp);

                    P1 = Convert.ToInt32(temp * 1000);

                    P1_Buf[2] = Convert.ToByte(P1 >> 8);
                    P1_Buf[3] = Convert.ToByte(P1 & 255);

                    serialPort1.Write(P1_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P1 = P1 + 1000;

                    label52.Text = Convert.ToString((float)(P1) / 1000);

                    P1_Buf[2] = Convert.ToByte(P1 >> 8);
                    P1_Buf[3] = Convert.ToByte(P1 & 255);

                    serialPort1.Write(P1_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P1 = P1 - 1000;

                    label52.Text = Convert.ToString((float)(P1) / 1000);

                    P1_Buf[2] = Convert.ToByte(P1 >> 8);
                    P1_Buf[3] = Convert.ToByte(P1 & 255);

                    serialPort1.Write(P1_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button44_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    I = I - 100;

                    label30.Text = Convert.ToString((float)I / 1000);

                    I_Buf[2] = Convert.ToByte(I >> 8);
                    I_Buf[3] = Convert.ToByte(I & 255);

                    serialPort1.Write(I_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button45_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    I = I + 100;

                    label30.Text = Convert.ToString((float)I / 1000);

                    I_Buf[2] = Convert.ToByte(I >> 8);
                    I_Buf[3] = Convert.ToByte(I & 255);

                    serialPort1.Write(I_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P1 = P1 - 100;

                    label52.Text = Convert.ToString((float)(P1) / 1000);

                    P1_Buf[2] = Convert.ToByte(P1 >> 8);
                    P1_Buf[3] = Convert.ToByte(P1 & 255);

                    serialPort1.Write(P1_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    P1 = P1 + 100;

                    label52.Text = Convert.ToString((float)(P1) / 1000);

                    P1_Buf[2] = Convert.ToByte(P1 >> 8);
                    P1_Buf[3] = Convert.ToByte(P1 & 255);

                    serialPort1.Write(P1_Buf, 0, 6);
                }
                catch
                { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button47_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    Exp_High = Exp_High + 10;

                    label38.Text = Convert.ToString(Exp_High);

                    EH_Buf[2] = Convert.ToByte(Exp_High >> 8);
                    EH_Buf[3] = Convert.ToByte(Exp_High & 255);

                    serialPort1.Write(EH_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button46_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    Exp_High = Exp_High - 10;

                    label38.Text = Convert.ToString(Exp_High);

                    EH_Buf[2] = Convert.ToByte(Exp_High >> 8);
                    EH_Buf[3] = Convert.ToByte(Exp_High & 255);

                    serialPort1.Write(EH_Buf, 0, 6);
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("PS  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("PT  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("FL  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write("LA  \r\n");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }
    }
}
