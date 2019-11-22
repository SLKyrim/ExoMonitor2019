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

        //DispatcherTimer Detection = new DispatcherTimer();
        public delegate void showData(string msg);//通信窗口输出
        private TcpClient client_eeg;
        private TcpClient client_emg;
        private TcpListener server_eeg;
        private TcpListener server_emg;
        private const int bufferSize = 1;
        private double ProportionValue = 2 * Math.PI;
        private int TimeValue = 75;
        private int MiddleGaitTime = 80;
        private int LongGiatTime = 85;

        //控制逻辑
        private int state = 0; //外骨骼当前状态：0为坐下，1为直立状态，2位跨步状态，3为停止状态
        private int eeg_cm = 2; //脑电命令：在外骨骼坐下模式时，0为保持坐姿，1为站立；在外骨骼站起模式时，0为停止，1为允许肌电进行步态控制
        private int emg_cm = 0; //肌电命令：仅在外骨骼站立时且脑电命令为1时肌电可进行控制，1为走一个完整步态周期，0为不动
        private int pattern = 0; //外骨骼步态模式
        private bool main_s = false; //总开关，0为停止外骨骼，1为使能外骨骼
        private int cnt = 0; // 进入越障步态后完成正常步的周期数
        private int obstacle_cnt = 0; // 越障个数计数器
        private int normal_cnt = 0; // 正常循环步步数计数器
        private const int NORMAL_MAX_CNT = 3; // 越障后正常循环步最大步数，此处跨一小步记为一步【收步不算】
        private const int OBSTACLE_NUM = 1; // 设置越障个数
        private const int N = 0; // Demo越障前正常步步数
        private int over_normal_cnt = 0; // 越障前正常步计数器
        private const int OVER_NORMAL_MAX_CNT = 3; // 越障前正常步最大步数 

        // 测试用
        private const int ENABLE = 1; // 使能外骨骼的命令
        private const int DISABLE = 0; // 失能外骨骼的命令
        private const int RENHAO_V = 10; // 越障步态速度
        private const int OBSTACLE_SPEED = 15; // 跨越那一步的速度
        private const int NORMAL_SPEED = 13; // 正常循环步速度

        #endregion

        #region 界面初始化

        private void Window_Loaded(object sender, RoutedEventArgs e)//打开程序时执行
        {
            try
            {
                motors.motors_Init();
                //cp.plotStart(motors, statusBar, statusInfoTextBlock);
            }
            catch (Exception)
            {
                MessageBox.Show("驱动器初始化失败");
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }

            Thread logic = new Thread(eeg_emg_control);//逻辑控制线程logic
            logic.Start();
        }

        private void Window_Closed(object sender, EventArgs e)//关闭程序时执行
        {
            //server.Stop();
        }
        #endregion

        #region 控制逻辑
        private void eeg_emg_control()
        {
            Thread control_tread = new Thread(control);
            while (true)
            {
                if (main_s)
                {
                    if (control_tread.ThreadState == ThreadState.Unstarted)
                    {
                        control_tread.Start();
                    }
                }
                else
                {
                    if (control_tread.ThreadState == ThreadState.Suspended)
                    {
                        control_tread.Abort();
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void control()
        {
            // 逻辑顺序： 起坐步态 --> 越障步态 --> 走停步态
            while (true)
            {
                // 起坐步态
                if (state == 0 && eeg_cm == ENABLE && pattern == 0)
                {
                    pattern = 1; //由坐下到直立
                }

                // 走停步态
                if (state == 1 && eeg_cm == ENABLE && pattern == 0)
                {
                    normal_cnt += 1;
                    pattern = 2; //由越障并收步后到跨步，由直立状态到左腿在前的站姿
                }
                if (state == 2 && eeg_cm == ENABLE && pattern == 0 && normal_cnt < NORMAL_MAX_CNT)
                {
                    normal_cnt += 1;
                    pattern = 3; //由跨步到跨步（即走一个步态周期），由左腿在前的站姿到右腿在前的站姿
                }
                if (state == 3 && eeg_cm == ENABLE && pattern == 0 && normal_cnt < NORMAL_MAX_CNT)
                {
                    normal_cnt += 1;
                    pattern = 4; //由跨步到跨步（即走一个步态周期），由右腿在前的站姿到左腿在前的站姿
                }
                if (state == 2 && pattern == 0 && (eeg_cm == DISABLE || normal_cnt >= NORMAL_MAX_CNT))
                {
                    pattern = 5; //由跨步到停止（收步为直立状态），由右腿在前的站姿到直立状态
                }
                if (state == 3 && pattern == 0 && (eeg_cm == DISABLE || normal_cnt >= NORMAL_MAX_CNT))
                {
                    pattern = 6; //由跨步到跨步（即走一个步态周期），由左腿在前的站姿到直立状态
                }

                #region 任豪连续越障步态
                //// 越障步态
                //if (state == 4 && eeg_cm == ENABLE && pattern == 0)
                //{
                //    pattern = 7; // 由坐下到直立状态后到越障起始步迈左腿， 由直立状态到左脚在前的站姿
                //}
                //if (state == 5 && eeg_cm == ENABLE && pattern == 0)
                //{
                //    pattern = 8; // 由起始步迈左腿到正常步迈右腿， 由左脚在前的站姿到右腿在前的站姿
                //}
                //if (state == 6 && eeg_cm == ENABLE && pattern == 0)
                //{
                //    if (cnt < N)
                //    {
                //        pattern = 9; // 有正常步迈右腿到正常步迈左腿，由右腿在前的站姿到左腿在前的站姿
                //    }
                //    else
                //    {
                //        pattern = 11; // 有正常步迈右腿到接跨步前的正常步迈左腿，由右腿在前的站姿到左腿在前的站姿
                //    }

                //}
                //if (state == 7 && eeg_cm == ENABLE && pattern == 0)
                //{
                //    pattern = 10; // 由正常步迈左腿到正常步迈右腿， 由左脚在前的站姿到右腿在前的站姿
                //    cnt += 1;
                //}
                //if (state == 8 && eeg_cm == ENABLE && pattern == 0)
                //{
                //    pattern = 12; // 有接跨步前的正常步迈左腿到越障并收步，由左脚在前的站姿到直立状态
                //}
                #endregion

                #region 任豪直接越障步态【接马博正常步收步后直接进行越障】
                if (state == 4 && eeg_cm == ENABLE && pattern == 0)
                {
                    over_normal_cnt += 1;
                    pattern = 7; //由越障并收步后到跨步，由直立状态到左腿在前的站姿
                }
                if (state == 5 && eeg_cm == ENABLE && pattern == 0 && over_normal_cnt < OVER_NORMAL_MAX_CNT)
                {
                    over_normal_cnt += 1;
                    pattern = 8; //由跨步到跨步（即走一个步态周期），由左腿在前的站姿到右腿在前的站姿
                }
                if (state == 6 && eeg_cm == ENABLE && pattern == 0 && over_normal_cnt < OVER_NORMAL_MAX_CNT)
                {
                    over_normal_cnt += 1;
                    pattern = 9; //由跨步到跨步（即走一个步态周期），由右腿在前的站姿到左腿在前的站姿
                }
                if (state == 5 && pattern == 0 && (eeg_cm == ENABLE || over_normal_cnt >= OVER_NORMAL_MAX_CNT))
                {
                    pattern = 10; //由跨步到停止（收步为直立状态），由右腿在前的站姿到直立状态
                }
                if (state == 6 && pattern == 0 && (eeg_cm == ENABLE || over_normal_cnt >= OVER_NORMAL_MAX_CNT))
                {
                    pattern = 11; //由跨步到跨步（即走一个步态周期），由左腿在前的站姿到直立状态
                }
                if (state == 7 && pattern == 0 && eeg_cm == ENABLE)
                {
                    pattern = 12;
                }
                if (state == 8 && pattern == 0 && eeg_cm == ENABLE)
                {
                    pattern = 13;
                }
                #endregion


                if (pattern != 0)
                {
                    //  int walk_step = 0;
                    //Detection.Stop();
                    switch (pattern)
                    {

                        # region 起坐步态
                        case 1:
                            //由坐下到直立
                            //MessageBox.Show("1");
                            double positon = motors.ampObjAngleActual[3];
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
                            Thread.Sleep(5000);
                            obstacle_cnt += 1;
                            if (obstacle_cnt > OBSTACLE_NUM)
                            {
                                state = 1;  // 进正常循环步
                            }
                            else
                            {
                                state = 4; // 循环越障步态
                            }
                            break;
                        #endregion

                        #region 走停步态
                        case 2:
                            //由直立到跨步
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\start.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 2;  //左步在前的跨步状态1
                            break;

                        case 3:
                            //由跨步到跨步（即走一个步态周期）
                            //MessageBox.Show("3");
                            try
                            {
                                //  walk_step += 1;
                                pvt.StartPVT(motors, "..\\..\\InputData\\Rr=0.65a=0.25.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 3;    //右步在前的跨步状态
                            break;

                        case 4:
                            //由跨步到跨步（即走一个步态周期）
                            //MessageBox.Show("3");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Lr=0.65a=0.25.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 2; //左步在前的跨步状态2
                            break;

                        case 5:
                            //由跨步到停止（收步为直立状态），，，由右腿在前的站姿到直立状态
                            //MessageBox.Show("4");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Lr=1a01.15 end.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = -1;  //直立状态
                            break;

                        case 6:
                            //由跨步到停止（收步为直立状态），，，由左腿在前的站姿到直立状态
                            //MessageBox.Show("4");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Rr=1a=0.15 end.txt", NORMAL_SPEED);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = -1;
                            break;
                        #endregion

                        #region 越障步态
                        case 7:
                            //由直立到越障步态起始步
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\start.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 5;  //起始步迈左腿完成后左腿在前的状态
                            break;

                        case 8:
                            //越障步态 接起始步的正常步迈右腿
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Rr=0.65a=0.25.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 6;  // 正常步迈右腿后的右腿在前状态
                            break;

                        case 9:
                            //越障步态 正常步迈左腿
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Lr=0.65a=0.25.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            state = 5;  // 正常步迈左腿后的左腿在前状态
                            break;

                        case 10:
                            //越障步态 接正常步的正常步迈右腿
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Lr=1a01.15 end.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            Thread.Sleep(5000);
                            state = 7;  // 正常步迈右腿后的右腿在前状态
                            break;

                        case 11:
                            //越障步态 接跨步前的正常步迈左腿
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\Rr=1a=0.15 end.txt", NORMAL_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            Thread.Sleep(5000);
                            state = 8;  // 正常步迈左腿后的左腿在前并准备跨步的状态
                            break;

                        case 12:
                            //越障步态 越障并收步（右腿先跨，）
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\直接越障步态第一步-扩展.txt", OBSTACLE_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            Thread.Sleep(3000);
                            state = 8;
                            break;

                        case 13:
                            //越障步态 越障并收步（右腿先跨，）
                            //MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\直接越障步态收步-扩展.txt", OBSTACLE_SPEED);//"..\\..\\INPUT201908051539\\左脚起始步高.txt"
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            over_normal_cnt = 0;
                            state = -1;
                            break;

                        #endregion

                        default:
                            break;
                    }
                    eeg_cm = 2;
                    pattern = 0;
                    //Detection.Start();
                }
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
                    pvt.StartPVT(motors, "..\\..\\InputData\\6步新.txt", 20);
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
                        state = 0;
                    }
                    catch
                    { MessageBox.Show("sit出错"); }
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
                        state = 4;
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

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "脑肌电融合控制模式";
                bt.Content = "EEG+EMG Stop";
                state = 0;
                cnt = 0;
                obstacle_cnt = 0; // 越障个数计数器
                normal_cnt = 0; // 正常循环步步数计数器
                main_s = true;        //EEG+EMG模式的标志符         
            }

            else
            {
                main_s = false;

                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "脑肌电融合控制模式已停止";
                bt.Content = "EEG+EMG Start";
            }
        }
        #endregion

        #region Socket
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
                    byte[] exo_state = BitConverter.GetBytes(state); // 外骨骼当前状态由int转byte[]
                    sendStream.Write(exo_state, 0, exo_state.Length);

                    //接收脑机发来的命令
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n肌电客户端传来信息：" + Encoding.Default.GetString(buffer, 0, msgSize));
                    string eeg = Encoding.Default.GetString(buffer, 0, msgSize);
                    emg_cm = Convert.ToInt16(eeg); //脑电传来的命令

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
            motors.ampObj[0].Disable();
            motors.ampObj[1].Disable();
            motors.ampObj[2].Disable();
            motors.ampObj[3].Disable();
        }

        private void Enable_Click(object sender, RoutedEventArgs e)//点击【Enable】按钮时执行：使能电机
        {
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

        private void positive_Button_Click(object sender, RoutedEventArgs e)
        {
            eeg_cm = 1;  //EEG上位机操作按钮 1，表示走
        }

        private void negtive_Button_Click(object sender, RoutedEventArgs e)
        {
            eeg_cm = 0;  //EEG上位机操作按钮 0，表示收步
        }
    }
}