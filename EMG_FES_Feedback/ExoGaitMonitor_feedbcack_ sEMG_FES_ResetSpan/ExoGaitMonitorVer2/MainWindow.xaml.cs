using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;
using CMLCOMLib;
using System.Windows.Input;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ExoGaitMonitorVer2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 声明
        //主界面窗口

        //CMO
        public Motors motors = new Motors(); //声明电机类

        //手动操作设置
        private Manumotive manumotive = new Manumotive();

        //PVT模式
        private PVT pvt = new PVT();

        //Stand up
        private Standup2 stand2 = new Standup2();


        //Motionstim8
        static Motionstim8 stimlator = new Motionstim8();
        static private Timer StimTimer;
        static int  PulseChannel, PulseCurrent, PulseWidth, PulseFreq;
        static int StimNum = 0;

        //DispatcherTimer Detection = new DispatcherTimer();
        public delegate void showData(string msg);//通信窗口输出
        private TcpClient client_eeg;
        private TcpClient client_emg;
        private TcpListener server_eeg;
        private TcpListener server_emg;
        private const int bufferSize = 1;
        //private double ProportionValue = 2 * Math.PI;
        //private int TimeValue = 75;
        //private int MiddleGaitTime = 80;
        //private int LongGiatTime = 85;

        // 控制逻辑
        private int state = 2; //外骨骼当前状态，2为直立状态
        private int eeg_cm = 2; //脑电命令：1为行走（正常走和跨障碍物），0为收步
        private int emg_cm = 0; //肌电命令：0为正常行走，1为执行起立，2为跨障碍物
        private int pattern = 0; //外骨骼步态模式，非0时触发外骨骼执行对应步态
        private bool main_s = false; //【EEG+EMG Start】总开关，0为停止外骨骼，1为使能外骨骼
        // 正常循环步流程：直立 -> 起始步迈左腿 -> 接起始步的正常步迈右腿 -> 正常步迈左腿 -> 【接正常步的正常步迈右腿 -> 正常步迈左腿】 -> 接正常步的迈右腿收步\迈左腿收步
  

        // 测试用 
        private const int ENABLE = 1; // 使能外骨骼的命令
        private const int DISABLE = 0; // 失能外骨骼的命令
        private const int RENHAO_V = 10; // 越障步态速度
        private const int OBSTACLE_SPEED = 12; // 跨越那一步的速度
        private const int NORMAL_SPEED = 9; // 正常循环步速度

        #endregion

        #region 界面初始化

        private void Window_Loaded(object sender, RoutedEventArgs e)//打开程序时执行
        {
            try
            {
                motors.motors_Init();
                //cp.plotStart(motors, statusBar, statusInfoTextBlock);
                         
                //stimlator.Send_Single_Pulse(PulseChannel, 0, 0);
                //stimlator.OpenSetup_serial();
                //stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);
                //Motionstim8.StimSerialPort.Close();

            }
            catch (Exception)
            {
                MessageBox.Show("驱动器初始化失败");
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }

            Thread logic = new Thread(Emg_control);//逻辑控制线程logic           
            logic.Start();

        }

        private void Window_Closed(object sender, EventArgs e)//关闭程序时执行
        {
            //server.Stop();
             
        }
        #endregion

        #region 控制逻辑
        private void Emg_control()
        {
            Thread control_thread = new Thread(control);
            Thread FeedbackThread = new Thread(ElecFeedback);
        

            while (true)
            {
                if (main_s)
                {
                    if (control_thread.ThreadState == ThreadState.Unstarted)
                    {
                        control_thread.Start();
                        FeedbackThread.Start();
                        stimlator.OpenSetup_serial();
                        stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);

                    }

                }
                else
                {
                    if (control_thread.ThreadState == ThreadState.Suspended)
                    {
                        control_thread.Abort();
                        FeedbackThread.Abort();
                        Motionstim8.StimSerialPort.Close();
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void control()
        {

            
           
            //int pre_emg_cm = 0;
            //int emg_cm1count = 0;
            //int emg_cm4count = 0;
            //int emg_cm5count = 0;
            // 逻辑顺序： 起坐步态 --> 正常循环步 --> 复位越障步态
            while (true)
            {
                //if (emg_cm == 1)  //起立Emg指令计数
                //{
                //    if (pre_emg_cm == 1)
                //    {
                //        emg_cm1count = emg_cm1count + 1;
                //    }
                //    else
                //    {
                //        emg_cm1count = 0;
                //       // pre_emg_cm = emg_cm;
                //    }
                //}
                //else
                //{
                //    emg_cm1count = 0;
                //    pre_emg_cm = emg_cm;
                //}

                // 起立步态
                if (state == 1 && pattern == 0 && emg_cm == 1 ) //&& emg_cm1count == 3)  //为坐立状态，且连续五次emg=1时，执行起立  
                {
                    pattern = 1; //由坐下到直立
                    //emg_cm1count = 0;
                }

                // 正常循环步
                if (state == 2 && pattern == 0 && (emg_cm == 2 || emg_cm == 3))  //若满足 直立状态，肌电为2或3状态，迈起始步
                {
                    pattern = 2; //起始步，由直立状态到左腿在前的站姿


                }
                if (state == 3 && pattern == 0 && emg_cm == 3)  //若满足 起始步完成后左腿在前的状态，肌电为3状态
                {
                    pattern = 3; //接起始步迈左腿的正常步迈右腿

                }
                if (state == 4 && pattern == 0 && emg_cm == 2)  //若满足 正常步右腿在前的状态，脑电为行走命令，肌电为2状态
                {
                    pattern = 4; //正常步迈左腿

                }
                if (state == 5 && pattern == 0 && emg_cm == 3)  //若满足 正常步左腿在前的状态，脑电为行走命令，肌电为0状态
                {
                    pattern = 5; //接正常步的正常步迈右腿

                }

                //if (emg_cm == 4)  //收步Emg指令计数
                //{
                //    if (pre_emg_cm == 4)
                //    {
                //        emg_cm4count = emg_cm4count + 1;
                //    }
                //    else
                //    {
                //        emg_cm4count = 0;
                //        pre_emg_cm = emg_cm;
                //    }
                //}
                //else
                //{
                //    emg_cm4count = 0;
                //    pre_emg_cm = emg_cm;
                //}

                if (state == 4 && pattern == 0 && emg_cm == 4)  //&& emg_cm4count == 3)  //若满足 正常步右腿在前的状态，脑电为收步命令，肌电为0状态
                {
                    pattern = 6; //接正常步的正常步迈左腿收步
                    //emg_cm4count = 0;

                }
                if (state == 5 && pattern == 0 && emg_cm == 4)  //若满足 正常步左腿在前的状态，脑电为收步命令，肌电为0状态
                {
                    pattern = 7; //接正常步的正常步迈右腿收步
                    //emg_cm4count = 0;
                }

                // 复位越障步态
                //if (emg_cm == 5)  //跨障Emg指令计数
                //{
                //    if (pre_emg_cm == 5)
                //    {
                //        emg_cm5count = emg_cm5count + 1;
                //    }
                //    else
                //    {
                //        emg_cm5count = 0;
                //        pre_emg_cm = emg_cm;
                //    }
                //}
                //else
                //{
                //    emg_cm5count = 0;
                //    pre_emg_cm = emg_cm;
                //}
                
                if (state == 2 && pattern == 0 && emg_cm == 5) // && emg_cm5count == 3)  //若满足 直立状态，且连续五次emg=5时，执行跨障
                {
                    pattern = 8; //直接越障并收步
                    //emg_cm5count = 0;

                }

                ////正常循环步
                //if (state == 2 && pattern == 0 && emg_cm == 1)  //若满足 直立状态，脑电为行走命令，肌电为0状态
                //{
                //    pattern = 2; //起始步，由直立状态到左腿在前的站姿
                //}
                //if (state == 3 && pattern == 0 && emg_cm == 1)  //若满足 起始步完成后左腿在前的状态，脑电为行走命令，肌电为0状态
                //{
                //    pattern = 3; //接起始步迈左腿的正常步迈右腿
                //}
                //if (state == 4 && pattern == 0 && emg_cm == 1)  //若满足 正常步右腿在前的状态，脑电为行走命令，肌电为0状态
                //{
                //    pattern = 4; //正常步迈左腿
                //}
                //if (state == 5 && pattern == 0 && emg_cm == 1)  //若满足 正常步左腿在前的状态，脑电为行走命令，肌电为0状态
                //{
                //    pattern = 5; //接正常步的正常步迈右腿
                //}
                //if (state == 4 && pattern == 0 && emg_cm == 0)  //若满足 正常步右腿在前的状态，脑电为收步命令，肌电为0状态
                //{
                //    pattern = 6; //接正常步的正常步迈左腿收步
                //}
                //if (state == 5 && pattern == 0 && emg_cm == 0)  //若满足 正常步左腿在前的状态，脑电为收步命令，肌电为0状态
                //{
                //    pattern = 7; //接正常步的正常步迈右腿收步
                //}

                ////复位越障步态
                //if (state == 2 && pattern == 0 && emg_cm == 2)  //若满足 直立状态，脑电为行走命令，肌电为2状态
                //{
                //    pattern = 8; //直接越障并收步
                //}


                if (pattern != 0)
                {
                    switch (pattern)
                    {
                        # region 起坐步态
                        case 1:
                            //由坐下到直立
                            //MessageBox.Show("1");
                            double positon = motors.ampObjAngleActual[3];
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "直立状态\n");
                            if (Math.Abs(positon) > 60)
                            {
                                try
                                {
                                    stand2.start_Standup2(motors);
                                }
                                catch (Exception ee)
                                {
                                    MessageBox.Show("stand up 出错");
                                }
                            }                            
                            state = 2;  // 进正常循环步
                            break;
                        #endregion

                        #region 越障前正常循环步
                        case 2:
                            //由直立到跨步
                            //MessageBox.Show("2");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "起始步迈左腿\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\起始步迈左腿-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 3;  //左步在前的跨步状态1
                            break;

                        case 3:
                            //MessageBox.Show("3");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "接起始步的正常步迈右腿\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\接起始步的正常步迈右腿-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 4;    //右步在前的跨步状态
                            break;

                        case 4:
                            //由跨步到跨步（即走一个步态周期）
                            //MessageBox.Show("4");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "正常步迈左腿\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\正常步迈左腿-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 5;    //左步在前的跨步状态
                            break;

                        case 5:
                            //MessageBox.Show("5");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "接正常步的正常步迈右腿\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\接正常步的正常步迈右腿-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 4;   //右步在前的跨步状态
                            break;

                        case 6:
                            //MessageBox.Show("6");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "接正常步的迈左腿收步\n");                           
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\接正常步的迈左腿收步-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 2;
                            break;
                        case 7:
                            //MessageBox.Show("7");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "接正常步的迈右腿收步\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\接正常步的迈右腿收步-扩展.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 2;
                            break;
                        #endregion

                        #region 复位越障步态
                        case 8:
                            //由直立到越障步态起始步
                            //MessageBox.Show("8");
                            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "直接越障并收步\n");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\EEG_EMG直接越障并收步.txt", OBSTACLE_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 2;  //跨障碍物完成，并恢复到直立状态
                            break;


                        #endregion

                        default:
                            break;
                    }
                    //pre_emg_cm = emg_cm;
                    pattern = 0;               
                }
                emg_cm = -1; // 恢复到肌电静止状态
                Thread.Sleep(100);
            }

             
        }
        #endregion

        #region 手动操作设置 Manumotive

        private void angleSetButton_Click(object sender, RoutedEventArgs e)//点击【执行命令】按钮时执行
        {
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = true;
            getZeroPointButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;
            PVT_Button.IsEnabled = false;

            angleSetTextBox.IsReadOnly = true;
            motorNumberTextBox.IsReadOnly = true;

            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            motors.ampObj[i].PositionActual = 0;

            manumotive.angleSetStart(motors, Convert.ToDouble(angleSetTextBox.Text), Convert.ToInt16(motorNumberTextBox.Text), statusBar, statusInfoTextBlock,
                                     angleSetButton, emergencyStopButton, getZeroPointButton, zeroPointSetButton, PVT_Button, angleSetTextBox, motorNumberTextBox);
        }

        private void emergencyStopButton_Click(object sender, RoutedEventArgs e)//点击【紧急停止】按钮时执行
        {
            emergencyStopButton.IsEnabled = false;
            angleSetButton.IsEnabled = true;
            getZeroPointButton.IsEnabled = true;
            angleSetTextBox.IsReadOnly = false;
            motorNumberTextBox.IsReadOnly = false;
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            motors.ampObj[i].HaltMove();
            manumotive.angleSetStop();
        }

        private void zeroPointSetButton_Click(object sender, RoutedEventArgs e)//点击【设置原点】按钮时执行
        {
            motors.ampObj[0].PositionActual = -1;
            motors.ampObj[1].PositionActual = -2;
            motors.ampObj[2].PositionActual = -2;
            motors.ampObj[3].PositionActual = -1;

            zeroPointSetButton.IsEnabled = true;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "原点设置完毕";
        }

        private void getZeroPointButton_Click(object sender, RoutedEventArgs e)//点击【回归原点】按钮时执行
        {
            angleSetTextBox.IsReadOnly = true;
            motorNumberTextBox.IsReadOnly = true;
            PVT_Button.IsEnabled = false;
            getZeroPointButton.IsEnabled = false;
            //manumotive.getZeroPointTimer_Tick(sender,e);
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;
            //PositionState = 0;
            manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
            zeroPointSetButton, PVT_Button, angleSetTextBox, motorNumberTextBox);
        }

        #endregion

        #region 控制模式选择
        private void PVT_Button_Click(object sender, RoutedEventArgs e)//点击【PVT Mode】按钮时执行:进入PVT模式
        {
            Button bt = sender as Button;
            double positon = motors.ampObjAngleActual[3];
            if (bt.Content.ToString() == "PVT Mode")
            {
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "PVT模式";
                bt.Content = "Stop";
                if (positon < 10)
                {
                    pvt.StartPVT(motors, "..\\..\\InputData\\6步新.txt", 25);
                }

            }

            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "PVT控制模式已停止";
                bt.Content = "PVT Mode";
            }
        }

        private void Sit_button_Click(object sender, RoutedEventArgs e)//点击【Sit Down】按钮时执行:由站立姿势坐下
        {
            Button bt = sender as Button;
            double positon = motors.ampObjAngleActual[3];
            if (bt.Content.ToString() == "Sit Down")
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "坐下\n");
                PVT_Button.IsEnabled = false;
                Stand_up_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "坐下模式";
                bt.Content = "Stop";
                if (positon < 10)
                {
                    try
                    {
                        pvt.start_Sitdown2(motors);
                        state = 1;
                        eeg_cm = 2;  // 保证不会因为脑电错分类使得坐下后马上站起来
                        emg_cm = -1; // 保证不会因肌电误动作使得坐下后马上站起来
                    }
                    catch
                    {
                        MessageBox.Show("sit出错");
                    }
                }

            }
            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;
                PVT_Button.IsEnabled = true;
                Stand_up_Button.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "坐下模式已停止";
                bt.Content = "Sit Down";
            }
        }

        private void Stand_up_Button_Click(object sender, RoutedEventArgs e)//点击【Stand up】按钮时执行:由坐下姿势起立
        {
            Button bt = sender as Button;
            double positon = motors.ampObjAngleActual[3];
            if (bt.Content.ToString() == "Stand Up")
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "起立\n");
                PVT_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                Sit_button.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "起立模式";
                bt.Content = "Stop";
                if (Math.Abs(positon) > 60)
                {
                    try
                    {
                        stand2.start_Standup2(motors);
                        state = 1;
                        eeg_cm = 2;  // 保证不会因为脑电错分类使得坐下后马上行走
                        emg_cm = -1; // 保证不会因肌电误动作使得坐下后马上行走

                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("stand up 出错");
                    }
                }

            }
            else
            {
                PVT_Button.IsEnabled = true;
                angleSetButton.IsEnabled = true;

                Sit_button.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "起立模式已结束";
                bt.Content = "Stand Up";
            }
        }

        private void EEG_EMG_Button_Click(object sender, RoutedEventArgs e)//点击【EEG+EMG Start】按钮时进入EEG+EMG模式，关闭其他模式
        {
            Button bt = sender as Button;
            double positon = motors.ampObjAngleActual[3];
            if (bt.Content.ToString() == "EEG+EMG Start")
            {
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;
                Sit_button.IsEnabled = false;
                Stand_up_Button.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "脑肌电融合控制模式";
                bt.Content = "EEG+EMG Stop";

                eeg_cm = 2;  // 保证不会因为脑电错分类使得坐下后马上动作
                emg_cm = -1; // 保证不会因肌电误动作使得坐下后马上动作
                main_s = true;        //EEG+EMG模式的标志符         
            }

            else
            {
                main_s = false;

                state = 1;
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;
                Sit_button.IsEnabled = true;
                Stand_up_Button.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "脑肌电融合控制模式已停止";
                bt.Content = "EEG+EMG Start";
            }
        }
        #endregion

        #region Socket  进程通讯机制，socket 协议下 客户端与服务端会有三次握手，稳定可靠
        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        private void switch_Click(object sender, RoutedEventArgs e)
        {
            if (IPAdressTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "请填入服务器IP地址\n");
                return;
            }
            if (PortTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "请填入服务器端口号\n");
                return;
            }
            Thread thread = new Thread(reciveAndListener);
            IpAndPort ipHePort = new IpAndPort();
            ipHePort.Ip = IPAdressTextBox.Text;
            ipHePort.Port = PortTextBox.Text;
            thread.Start((object)ipHePort);

            Thread thread_emg = new Thread(reciveAndListener_EMG);

            IpAndPort ipPort_emg = new IpAndPort();
            ipPort_emg.Ip = IPAdressTextBox.Text;
            ipPort_emg.Port = "4485";
            thread_emg.Start((object)ipPort_emg);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (stxtSendMsg.Text.Trim() != string.Empty)
            {
                NetworkStream sendStream = client_eeg.GetStream();//获得用于数据传输的流
                byte[] buffer = Encoding.Default.GetBytes(stxtSendMsg.Text.Trim());//将数据存在缓冲中
                sendStream.Write(buffer, 0, buffer.Length);//最终写入流中
                string showmsg = Encoding.Default.GetString(buffer, 0, buffer.Length);
                //ComWinTextBox1.AppendText("发送给服务端数据：" + showmsg + "\n");
                stxtSendMsg.Text = string.Empty;
            }
        }

        private void ComWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComWinTextBox.ScrollToEnd();//当通信窗口内容变化时滚动条定位在最下面
        }
        private void reciveAndListener(object ipAndPort)
        {
            IpAndPort ipHePort = (IpAndPort)ipAndPort;
            IPAddress ip = IPAddress.Parse(ipHePort.Ip);
            server_eeg = new TcpListener(ip, int.Parse(ipHePort.Port));
            Socket socketserver = server_eeg.Server;
            bool conma = !((socketserver.Poll(1000, SelectMode.SelectRead) && (socketserver.Available == 0)) || !socketserver.Connected);
            server_eeg.Start();//启动监听

            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "脑电服务端开启侦听....\n");

            client_eeg = server_eeg.AcceptTcpClient();
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有脑电客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步

            //获取流
            NetworkStream reciveStream = client_eeg.GetStream();

            do
            {
                //获取连接的客户d端的对象
                //if (socketserver.Poll(10, SelectMode.SelectRead) == false)
                //{
                //    client = server.AcceptTcpClient();
                //    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步
                //    reciveStream = client.GetStream();
                //}


                byte[] buffer = new byte[bufferSize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSize);
                    }

                    if (msgSize == 0)
                    {
                        //获取连接的客户d端的对象
                        client_eeg = server_eeg.AcceptTcpClient();
                        //ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有脑电客户端请求连接，连接已建立！"); //AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步
                        reciveStream = client_eeg.GetStream();
                        continue;
                    }
                    //将外骨骼当前状态发给脑机，使其能够选择对应模式的分类器
                    NetworkStream sendStream = client_eeg.GetStream();
                    byte[] exo_state = BitConverter.GetBytes(state); // 外骨骼当前状态由int转byte[]
                    sendStream.Write(exo_state, 0, exo_state.Length);

                    //接收脑机发来的命令
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n脑电客户端传来信息：" + Encoding.Default.GetString(buffer, 0, msgSize));
                    string eeg = Encoding.Default.GetString(buffer, 0, msgSize);
                    eeg_cm = Convert.ToInt16(eeg); //脑电传来的命令
                   

                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n脑电出现异常：连接被迫关闭");
                    break;
                }
            } while (true);

            Thread.Sleep(100);
        }

        private void reciveAndListener_EMG(object ipAndPort)
        {
            IpAndPort ipHePort = (IpAndPort)ipAndPort;
            IPAddress ip = IPAddress.Parse(ipHePort.Ip);
            server_emg = new TcpListener(ip, int.Parse(ipHePort.Port));
            Socket socketserver = server_emg.Server;
            bool conma = !((socketserver.Poll(1000, SelectMode.SelectRead) && (socketserver.Available == 0)) || !socketserver.Connected);
            server_emg.Start();//启动监听

            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "肌电服务端开启侦听....\n");

            client_emg = server_emg.AcceptTcpClient();
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有肌电客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步

            //获取流
            NetworkStream reciveStream = client_emg.GetStream();

            do
            {
                //获取连接的客户d端的对象
                //if (socketserver.Poll(10, SelectMode.SelectRead) == false)
                //{
                //    client = server.AcceptTcpClient();
                //    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步
                //    reciveStream = client.GetStream();
                //}


                byte[] buffer = new byte[bufferSize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSize);
                    }

                    if (msgSize == 0)
                    {
                        //获取连接的客户d端的对象
                        client_emg = server_emg.AcceptTcpClient();
                        //ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有脑电客户端请求连接，连接已建立！"); //AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步
                        reciveStream = client_emg.GetStream();
                        continue;
                    }
                    //将外骨骼当前状态发给脑机，使其能够选择对应模式的分类器
                    NetworkStream sendStream = client_emg.GetStream();
                    byte[] exo_state = BitConverter.GetBytes(4); // 外骨骼当前状态由int转byte[]
                    sendStream.Write(exo_state, 0, exo_state.Length);

                    //接收脑机发来的命令
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n肌电客户端传来信息：" + Encoding.Default.GetString(buffer, 0, msgSize));
                    // string eeg = Encoding.Default.GetString(buffer, 0, msgSize);
                    emg_cm = Convert.ToInt16(Encoding.Default.GetString(buffer, 0, msgSize)); //脑电传来的命令
                    
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n肌电出现异常：连接被迫关闭");
                    break;
                }
            } while (true);

            Thread.Sleep(100);
        }
        #endregion

        #region 电机状态
        private void Disable_Click(object sender, RoutedEventArgs e)//点击【Disable】按钮时执行：失能电机
        {
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "电机Disable\n");
            motors.ampObj[0].Disable();
            motors.ampObj[1].Disable();
            motors.ampObj[2].Disable();
            motors.ampObj[3].Disable();
        }

        private void Enable_Click(object sender, RoutedEventArgs e)//点击【Enable】按钮时执行：使能电机
        {
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "电机Enable\n");
            motors.ampObj[0].Enable();
            motors.ampObj[1].Enable();
            motors.ampObj[2].Enable();
            motors.ampObj[3].Enable();
        }

        private void Clear_Fault_Button_Click(object sender, RoutedEventArgs e)//点击【Clear Fault】按钮时执行：清除电机错误
        {
            motors.ampObj[0].ClearFaults();
            motors.ampObj[1].ClearFaults();
            motors.ampObj[2].ClearFaults();
            motors.ampObj[3].ClearFaults();
        }
        #endregion


        #region Electrical Feedback
        private void ElecFeedback()
        {
            //NetworkStream sendStream = client_eeg.GetStream();//获得用于数据传输的流  
            while (main_s)
            {
                motors.angletocoder.CoderToAngle(-motors.ampObj[0].PositionActual, motors.ampObj[1].PositionActual, -motors.ampObj[2].PositionActual, motors.ampObj[3].PositionActual, ref motors.ampObjAngleActual[0], ref motors.ampObjAngleActual[1], ref motors.ampObjAngleActual[2], ref motors.ampObjAngleActual[3]);
                motors.angletocoder.GetVel(motors.ampObj[0].VelocityActual, motors.ampObj[1].VelocityActual, motors.ampObj[2].VelocityActual, motors.ampObj[3].VelocityActual, ref motors.ampObjAngleVelActual[0], ref motors.ampObjAngleVelActual[1], ref motors.ampObjAngleVelActual[2], ref motors.ampObjAngleVelActual[3]);

                double[] AngVel = { motors.ampObjAngleActual[0], motors.ampObjAngleActual[1], motors.ampObjAngleActual[2], motors.ampObjAngleActual[3],
                                    motors.ampObjAngleVelActual[0], motors.ampObjAngleVelActual[1],motors.ampObjAngleVelActual[2], motors.ampObjAngleVelActual[3]};
                outputSensor(AngVel[0], AngVel[1], AngVel[2], AngVel[3], AngVel[4], AngVel[5], AngVel[6], AngVel[7]);  //保存实时数据


                int PulseWidthF = 400;
                int PulseCurrentF = 7;
                int PulseFreF = 25;
               
                if ((Math.Abs(AngVel[4])+ Math.Abs(AngVel[5])+ Math.Abs(AngVel[6])+Math.Abs(AngVel[7]))<0.0001  )
                {
                    
                    if( 1<AngVel[0]&&AngVel[0]<3   &&   -11 <AngVel[1]&&AngVel[1]<-9   &&   -6.5<AngVel[2]&&AngVel[2]<-4.5   &&  -9<AngVel[3]&&AngVel[3] < -7)
                    {
                        //byte[] AngVelByte = BitConverter.GetBytes(1);  //起始步完成
                        //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                        SingleChannel(0, PulseWidthF, PulseCurrentF-2, PulseFreF);
                        Console.WriteLine("起始步完成");
                    }
                    else if (5.8<AngVel[0]&&AngVel[0]<9.5  &&  4.5<AngVel[1]&&AngVel[1]<7  &&  11.5<AngVel[2]&&AngVel[2]<13.8   &&   -7.5<AngVel[3]&&AngVel[3]<-5.5)
                    {
                        //byte[] AngVelByte = BitConverter.GetBytes(2);  //右脚在前
                        //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                        SingleChannel(1, PulseWidthF, PulseCurrentF-2, PulseFreF);
                        Console.WriteLine("右脚在前");
                    }
                    else if (5<AngVel[0]&&AngVel[0]<8   &&   -13.5<AngVel[1]&&AngVel[1]<-11   &&   -6.5<AngVel[2]&&AngVel[2]<-4.5   &&   -9<AngVel[3]&&AngVel[3]<-7)
                    {
                        //byte[] AngVelByte = BitConverter.GetBytes(3);  //左脚在前
                        //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                        SingleChannel(0, PulseWidthF, PulseCurrentF-2, PulseFreF);
                        Console.WriteLine("左脚在前");
                    }
                    else if( (Math.Abs(AngVel[0]) + Math.Abs(AngVel[1]) + Math.Abs(AngVel[2]) + Math.Abs(AngVel[3]))< 3)
                    {
                        //byte[] AngVelByte = BitConverter.GetBytes(4);  //收步完成,直立状态
                        //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                        TwoChannel(PulseWidthF, PulseCurrentF-2, PulseFreF);  //channel 0 1 Light 
                        Console.WriteLine("直立状态");
                    }
                    else if(98 < AngVel[0] && AngVel[0]<102 && -93 < AngVel[1] && AngVel[1] < -88 && 88< AngVel[2] && AngVel[2] < 92 && -102 < AngVel[3] && AngVel[3] < -98)
                    {
                        //坐下完成后状态为 100, -90, 90, -100
                        //坐立状态不执行动作，但需要防止误判
                        Console.WriteLine("坐立状态");
                    }
                    else
                    {
                        //byte[] AngVelByte = BitConverter.GetBytes(0);  //电机错误
                        //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                        TwoChannel(PulseWidthF, PulseCurrentF, PulseFreF); //channel 0 1 Heavy
                        Console.WriteLine("电机错误");
                    }
                }
                else
                {
                    //byte[] AngVelByte = BitConverter.GetBytes(6);  //电机运行中
                    //sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                    //TwoChannel2(400, 6, 25);  // 1 Light,  0 Heavy
                    SingleChannel(0, PulseWidthF, PulseCurrentF, PulseFreF);  //0 Heavy
                    Console.WriteLine("电机运行中...");
                }
                //for (int i = 0; i < 8; i++)
                //{
                //    Console.WriteLine(AngVel[i]);
                //    byte[] AngVelByte = BitConverter.GetBytes(AngVel[i]);
                //    NetworkStream sendStream = client_eeg.GetStream();//获得用于数据传输的流
                //                                                      //byte[] buffer = Encoding.Default.GetBytes(AngVel.ToCharArray());//将数据存在缓冲中
                //    sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中
                //}

                if (eeg_cm == 1)
                {
                    // TwoChannel(400, 3, 25);  //channel 0 1 Light
                    SingleChannel(1, PulseWidthF, PulseCurrentF, PulseFreF);  //1 Heavy 
                    Console.WriteLine("疲劳预警");
                }
                Thread.Sleep(50);
            }
        }

        public void outputSensor(double AlKnee, double AlHip, double ArHip, double ArKnee, double VlKnee, double VlHip, double VrHip, double VrKnee)
        { //输出几列数据
            //StreamWriter sw = new StreamWriter("gait.txt");
            FileStream F = new FileStream("AngVel.txt", FileMode.Append, FileAccess.Write, FileShare.Write);
            StreamWriter sw = new StreamWriter(F);
            sw.Write(AlKnee + "\t" + AlHip + "\t" + ArHip + "\t" + ArKnee + "\t" + VlKnee + "\t" + VlHip + "\t" + VrHip + "\t" + VrKnee + "\n");
            sw.Flush();
            sw.Close();
            F.Close();
        }

        #endregion



        #region EMG_EEG调试
        private void EEG_positive_Button_Click(object sender, RoutedEventArgs e)
        {
            if (1==1)      //只有EEG_EMG_Start开启，这个按键才会启用
            {
                eeg_cm = 1;  //EEG上位机操作按钮 1，表示走
                //double[] AngVel = {1, 2, 10, 1.1, 1.2, 3.3, 34, 0.3};
                ////byte[] AngVelByte = new byte[8];
                //for (int i = 0; i < 8; i++)
                //{
                //    Console.WriteLine(AngVel[i]);
                //    byte[] AngVelByte = BitConverter.GetBytes(AngVel[i]);
                //    NetworkStream sendStream = client_eeg.GetStream();//获得用于数据传输的流
                //  //byte[] buffer = Encoding.Default.GetBytes(AngVel.ToCharArray());//将数据存在缓冲中
                //    sendStream.Write(AngVelByte, 0, AngVelByte.Length);//最终写入流中

                //}
                //Thread.Sleep(400);
            }  
        }

        private void EEG_negtive_Button_Click(object sender, RoutedEventArgs e)
        {
            eeg_cm = 0;  //EEG上位机操作按钮 0，表示收步
        }

        private void EMG_0Button_Click(object sender, RoutedEventArgs e)
        {
            if (main_s)
            {
                emg_cm = 0;  //Emg上位机操作按钮 1，表示起立
            }
        }
        private void EMG_1Button_Click(object sender, RoutedEventArgs e)
        {
            if (main_s)
            {
                emg_cm = 1;  //Emg上位机操作按钮 1，表示起立
            }
        }

        private void EMG_2Button_Click(object sender, RoutedEventArgs e)
        {
            if (main_s)
            {
               // eeg_cm = 1;  //EEG上位机操作按钮 1，表示走
                emg_cm = 2;  //EMG上位机操作按钮2，表示跨大步
            }
        }

        #endregion

        #region 电刺激调试
        public void StimSerialButton_Click(object sender, RoutedEventArgs e)
        {
            StimSerialButton.IsEnabled = false;
            InitFesButton.IsEnabled = true;
            SinglePulseButton.IsEnabled = false;
            TestButton.IsEnabled = false;
            FinishFesButton.IsEnabled = false;

            //FesChannelTextBox.IsReadOnly = true;
            //FesCurrentTextBox.IsReadOnly = true;
            //FesWidthTextBox.IsReadOnly = true;
            //FesFrequenceTextBox.IsReadOnly = true;
            
            stimlator.OpenSetup_serial();

        }

        public void InitFesButton_Click(object sender, RoutedEventArgs e)
        {
            StimSerialButton.IsEnabled = false;
            InitFesButton.IsEnabled = false;
            SinglePulseButton.IsEnabled = true;
            TestButton.IsEnabled = true;
            FinishFesButton.IsEnabled = false;

            //FesChannelTextBox.IsReadOnly = true;
            //FesCurrentTextBox.IsReadOnly = true;
            //FesWidthTextBox.IsReadOnly = true;
            //FesFrequenceTextBox.IsReadOnly = true;

            PulseFreq = Convert.ToInt16(FesFrequenceTextBox.Text);
            PulseCurrent = Convert.ToInt32(FesCurrentTextBox.Text);
            PulseChannel = Convert.ToInt16(FesChannelTextBox.Text);
            PulseWidth = Convert.ToInt16(FesWidthTextBox.Text);
            
            stimlator.Send_Init_Param(PulseChannel, 0xff, 0x00, 50, 20, 0);
            //uint nc,
            //uint Channel_Stim,
            //uint Channel_Lf,
            //uint Main_Time,
            //uint Group_Time,
            //uint N_Factor
        }

        public void SinglePulseButton_Click(object sender, RoutedEventArgs e)
        {

            StimSerialButton.IsEnabled = false;
            InitFesButton.IsEnabled = false;
            SinglePulseButton.IsEnabled = false;
            TestButton.IsEnabled = false;
            FinishFesButton.IsEnabled = true;

            Stim(PulseFreq);

            //for(int i=0;i<100;i++)
            //{
            //    stimlator.Send_Single_Pulse(PulseChannel, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
            //    Thread.Sleep(1000 / PulseFreq);
            //}

            //while (true)
            //{
            //    //stimlator.Send_Single_Pulse(0, 400, 8);  //通道，Pulse_Width, Pulse_Current


            //    stimlator.Send_Single_Pulse(uint.Parse(Channel), uint.Parse(Width), uint.Parse(Current));  //channel，Pulse_Width, Pulse_Current

             //stimlator.Send_Single_Pulse(0, 400, 8);  //通道，Pulse_Width, Pulse_Current
            //    //stimlator.Send_Single_Pulse(1, 500, 8);
            //    //stimlator.Send_Single_Pulse(2, 500, 8);
            //    //stimlator.Send_Single_Pulse(3, 500, 8);
            //      Thread.Sleep(10);
            //}

        }

        public void FinishFesButton_Click(object sender, RoutedEventArgs e)
        {
            StimSerialButton.IsEnabled = true;
            InitFesButton.IsEnabled = true;
            SinglePulseButton.IsEnabled = true;
            TestButton.IsEnabled = true;
            FinishFesButton.IsEnabled = false;

            StimTimer.Dispose();  // StimTimer退出
            Thread.Sleep(1000);
            //stimlator.Send_Single_Pulse(PulseChannel, 0,0);
            stimlator.Send_Init_Param(0, 0x00, 0x00, 50, 20, 0);
            Motionstim8.StimSerialPort.Close();

        }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        public void Stim(int freq)
        {
            StimTimer = new Timer(SinglePulse, null, 0, 1000 / freq); //0表示直接开始，time=40表示40ms调用一次，即是25Hz采样频率
        }

        public void SinglePulse(object sender )
        {    
            for(int i=0; i< PulseChannel;i++)
            {  
                stimlator.Send_Single_Pulse(i, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current            
            }
            if(PulseChannel==0)
            {
                stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current    
            }

            StimNum++;
            while (StimNum >= 1.5*PulseFreq)  //执行1秒钟退出
            {
                StimTimer.Dispose();
                StimNum = 0;
            }
        }

        public void SingleChannel(int SingPulseChannel, int SingPulseWidth, int SingPulseCurrent, int SingPulseFreq)  //调用单通道函数
        {
            //stimlator.OpenSetup_serial();
            //stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);

            while (StimNum <= 1 * SingPulseFreq)  //执行1秒钟退出
            { 
                stimlator.Send_Single_Pulse(SingPulseChannel, SingPulseWidth, SingPulseCurrent);  //channel，Pulse_Width, Pulse_Current
                StimNum++;
                Thread.Sleep(1000/ SingPulseFreq);
            }
            StimNum = 0;
            //stimlator.Send_Init_Param(0, 0x00, 0x00, 50, 20, 0);
            //Motionstim8.StimSerialPort.Close();
        }
        public void TwoChannel(int PulseWidth, int PulseCurrent, int  PulseFreq)  //调用单通道函数
        {
            //stimlator.OpenSetup_serial();
            //stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);
            while (StimNum <= 1 *  PulseFreq)  //执行1秒钟退出
            {
                stimlator.Send_Single_Pulse( 0,  PulseWidth,  PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                stimlator.Send_Single_Pulse( 1,  PulseWidth,  PulseCurrent);
                StimNum++;
                Thread.Sleep( 1000/PulseFreq );
            }
            StimNum = 0;
            //stimlator.Send_Init_Param(0, 0x00, 0x00, 50, 20, 0);
            //Motionstim8.StimSerialPort.Close();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            StimSerialButton.IsEnabled = false;
            InitFesButton.IsEnabled = false;
            SinglePulseButton.IsEnabled = false;
            TestButton.IsEnabled = false;
            FinishFesButton.IsEnabled = true;

            for (int k = 0; k < 5; k++)
            {
                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出  00
                {
                    stimlator.Send_Single_Pulse(0, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出  11
                {
                    stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出  00 11
                {
                    stimlator.Send_Single_Pulse(0, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                    stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出  0 1
                {
                    stimlator.Send_Single_Pulse(0, PulseWidth, PulseCurrent - 2);  //channel，Pulse_Width, Pulse_Current
                    stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent - 2);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出 0
                {
                    stimlator.Send_Single_Pulse(0, PulseWidth, PulseCurrent - 2);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

                while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出  1
                {
                    stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent - 2);  //channel，Pulse_Width, Pulse_Current
                    StimNum++;
                    Thread.Sleep(1000 / PulseFreq);
                }
                StimNum = 0;
                Thread.Sleep(3000);

            }
        }

        public void TwoChannel2(int PulseWidth, int PulseCurrent, int PulseFreq)  //调用单通道函数
        {
            //stimlator.OpenSetup_serial();
            //stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);

            while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出
            {
                stimlator.Send_Single_Pulse(0, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent-2);
                StimNum++;
                Thread.Sleep(1000 / PulseFreq);
            }
            StimNum = 0;
            //stimlator.Send_Init_Param(0, 0x00, 0x00, 50, 20, 0);
            //Motionstim8.StimSerialPort.Close();
        }

        public void TwoChannel3(int PulseWidth, int PulseCurrent, int PulseFreq)  //调用双通道函数
        {
            //stimlator.OpenSetup_serial();
            //stimlator.Send_Init_Param(2, 0xff, 0x00, 50, 20, 0);

            while (StimNum <= 1 * PulseFreq)  //执行1秒钟退出
            {
                stimlator.Send_Single_Pulse(1, PulseWidth, PulseCurrent);  //channel，Pulse_Width, Pulse_Current
                stimlator.Send_Single_Pulse(0, PulseWidth - 2, PulseCurrent - 200);
                StimNum++;
                Thread.Sleep(1000 / PulseFreq);
            }
            StimNum = 0;
            //stimlator.Send_Init_Param(0, 0x00, 0x00, 50, 20, 0);
            //Motionstim8.StimSerialPort.Close();

        }
        #endregion


    }
 
}