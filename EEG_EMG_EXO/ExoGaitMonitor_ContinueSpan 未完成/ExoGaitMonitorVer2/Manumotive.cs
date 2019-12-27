using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CMLCOMLib;
using System.Windows.Controls.Primitives;

namespace ExoGaitMonitorVer2
{
    class Manumotive
    {
        #region 声明

        //设置角度
        private Motors motors = new Motors();
        private double angleSet;
        private int motorNumber;
        private AngleCoderTrans angtocoder = new AngleCoderTrans();
        StatusBar statusBar;
        TextBlock statusInfoTextBlock;
        Button angleSetButton;
        Button emergencyStopButton;
        Button getZeroPointButton;
        Button zeroPointSetButton;
        Button PVT_Button;
        TextBox angleSetTextBox;
        TextBox motorNumberTextBox;

        static DispatcherTimer timer;

        ProfileSettingsObj profileParameters = new ProfileSettingsObj();    //用于设置电机参数

        const double FAST_VEL = 30000; //设置未到目标角度时较快的速度
        const double SLOW_VEL = 3000; //设置快到目标角度时较慢的速度
        const double ORIGIN_POINT = 1; //原点阈值
        const double TURN_POINT = 10; //快速转慢速的转变点

        #endregion

        #region 设置角度

        public void angleSetStart(Motors motorsIn, double angleSetIn, int motorNumberIn, StatusBar statusBarIn, TextBlock statusInfoTextBlockIn,
                                  Button angleSetButtonIn, Button emergencyStopButtonIn, Button getZeroPointButtonIn, Button zeroPointSetButtonIn,
                                  Button PVT_ButtonIn, TextBox angleSetTextBoxIn, TextBox motorNumberTextBoxIn)//设置角度
        {
            motors = motorsIn;
            angleSet = angleSetIn;
            motorNumber = motorNumberIn;
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;
            angleSetButton = angleSetButtonIn;
            emergencyStopButton = emergencyStopButtonIn;
            getZeroPointButton = getZeroPointButtonIn;
            zeroPointSetButton = zeroPointSetButtonIn;
            PVT_Button = PVT_ButtonIn;
            angleSetTextBox = angleSetTextBoxIn;
            motorNumberTextBox = motorNumberTextBoxIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(angleSetTimer_Tick);
            timer.Start();
        }

        public void angleSetStop()
        {
            timer.Stop();
            timer.Tick -= new EventHandler(angleSetTimer_Tick);
        }

        public void angleSetTimer_Tick(object sender, EventArgs e)//电机按设置角度转动的委托
        {
            double angleSet_s = 0;
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";
            int i = motorNumber - 1;
            if (i == 0)
            {
                double[] Angleset = new double[4];
                angtocoder.AngleToCoder((Math.Abs(angleSet)).ToString(), "0", "0", "0", ref Angleset[0], ref Angleset[1], ref Angleset[2], ref Angleset[3]);
                angleSet_s = Angleset[0];
            }
            else if (i == 1)
            {
                double[] Angleset = new double[4];
                angtocoder.AngleToCoder("0", (-1 * Math.Abs(angleSet)).ToString(), "0", "0", ref Angleset[0], ref Angleset[1], ref Angleset[2], ref Angleset[3]);
                angleSet_s = Angleset[1];
            }
            else if (i == 2)
            {
                double[] Angleset = new double[4];
                angtocoder.AngleToCoder("0", "0", (Math.Abs(angleSet)).ToString(), "0", ref Angleset[0], ref Angleset[1], ref Angleset[2], ref Angleset[3]);
                angleSet_s = Angleset[2];
            }
            else if (i == 3)
            {
                double[] Angleset = new double[4];
                angtocoder.AngleToCoder("0", "0", "0", (-1 * Math.Abs(angleSet)).ToString(), ref Angleset[0], ref Angleset[1], ref Angleset[2], ref Angleset[3]);
                angleSet_s = Angleset[3];
            }
            else { }

            try
            {
                //profileParameters = motors.ampObj[i].ProfileSettings;
                profileParameters.ProfileVel = 85 * 640 * 2 * 160 / 360;//* DeltaP1[s, i] / MaxDeltaP;    //单位为°/s
                profileParameters.ProfileAccel = 50 * 6400 * 4 * 160 / 360;    //单位为°/s2
                profileParameters.ProfileDecel = 50 * 6400 * 4 * 160 / 360;    //单位为°/s
                profileParameters.ProfileJerk = 50000;
                profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_SCURVE;
                motors.ampObj[i].ProfileSettings = profileParameters;
                if (angleSet < 0)
                {
                    motors.ampObj[i].MoveRel(-1 * angleSet_s);
                }
                else
                {
                    motors.ampObj[i].MoveRel(1 * angleSet_s);
                }

            }
            catch (Exception ee)
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "电机" + (i + 1).ToString() + "出错！";
                System.Windows.MessageBox.Show(ee.ToString());
            }

            motors.ampObj[0].WaitMoveDone(10000);
            motors.ampObj[1].WaitMoveDone(10000);
            motors.ampObj[2].WaitMoveDone(10000);
            motors.ampObj[3].WaitMoveDone(10000);

            angleSetButton.IsEnabled = true;
            emergencyStopButton.IsEnabled = false;
            getZeroPointButton.IsEnabled = true;
            zeroPointSetButton.IsEnabled = true;
            PVT_Button.IsEnabled = true;

            angleSetTextBox.IsReadOnly = false;
            motorNumberTextBox.IsReadOnly = false;

            timer.Stop();
            timer.Tick -= new EventHandler(angleSetTimer_Tick);
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "执行完毕";
        }
        #endregion


        #region 回归原点
        public void getZeroPointStart(Motors motorsIn, StatusBar statusBarIn, TextBlock statusInfoTextBlockIn, Button angleSetButtonIn, Button emergencyStopButtonIn, Button getZeroPointButtonIn, Button zeroPointSetButtonIn,
                                      Button PVT_ButtonIn, TextBox angleSetTextBoxIn, TextBox motorNumberTextBoxIn)
        {
            motors = motorsIn;
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;
            angleSetButton = angleSetButtonIn;
            emergencyStopButton = emergencyStopButtonIn;
            getZeroPointButton = getZeroPointButtonIn;
            zeroPointSetButton = zeroPointSetButtonIn;
            PVT_Button = PVT_ButtonIn;
            angleSetTextBox = angleSetTextBoxIn;
            motorNumberTextBox = motorNumberTextBoxIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(getZeroPointTimer_Tick);
            timer.Start();
        }

        double[] KeyPoss = { -1, -2, -2, -1 }; // 原点位置
        public void getZeroPointTimer_Tick(object sender, EventArgs e)//回归原点的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在回归原点";

            for (int i = 0; i < motors.motor_num; i++)//电机回归原点
            {

                try
                {
                    //profileParameters = motors.ampObj[i].ProfileSettings;
                    profileParameters.ProfileVel = 85 * 640 * 2 * 160 / 360;//* DeltaP1[s, i] / MaxDeltaP;    //单位为°/s
                    profileParameters.ProfileAccel = 50 * 6400 * 4 * 160 / 360;    //单位为°/s2
                    profileParameters.ProfileDecel = 50 * 6400 * 4 * 160 / 360;    //单位为°/s
                    //profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                    profileParameters.ProfileJerk = 50000;
                    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_SCURVE;
                    motors.ampObj[i].ProfileSettings = profileParameters;
                    motors.ampObj[i].MoveAbs(KeyPoss[i]);
                }
                catch (Exception ee)
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "电机" + (i + 1).ToString() + "出错！";
                    System.Windows.MessageBox.Show(ee.ToString());
                }
            }
            motors.ampObj[0].WaitMoveDone(30000);
            motors.ampObj[1].WaitMoveDone(30000);
            motors.ampObj[2].WaitMoveDone(30000);
            motors.ampObj[3].WaitMoveDone(30000);
            //if (Math.Abs(motors.ampObjAngleActual[0]) < ORIGIN_POINT && Math.Abs(motors.ampObjAngleActual[1]) < ORIGIN_POINT && Math.Abs(motors.ampObjAngleActual[2]) < ORIGIN_POINT && Math.Abs(motors.ampObjAngleActual[3]) < ORIGIN_POINT)
            //{
            //    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            //    statusInfoTextBlock.Text = "回归原点完毕";
            //    motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
            //    for (int i = 0; i < motors.motor_num; i++)
            //    {
            //        motors.profileSettingsObj.ProfileVel = 0;
            //        motors.profileSettingsObj.ProfileAccel = 0;
            //        motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
            //        motors.ampObj[i].ProfileSettings = motors.profileSettingsObj;
            //    }

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "回归原点完毕";
            angleSetButton.IsEnabled = true;
            emergencyStopButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;
            getZeroPointButton.IsEnabled = true;
            PVT_Button.IsEnabled = true;

            angleSetTextBox.IsReadOnly = false;
            motorNumberTextBox.IsReadOnly = false;

            timer.Stop();
            timer.Tick -= new EventHandler(getZeroPointTimer_Tick);

        }
        #endregion
    }
}
