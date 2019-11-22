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
using System.Diagnostics;

namespace ExoGaitMonitorVer2
{
    public class Motors
    {
        #region 声明
        //电机类，包括实例化电机对象和电机联动对象Linkage；
        //包括操作电机向设置方向移动到设置角度；设置外骨骼原点及回归原点
        //CMO
        public AmpObj[] ampObj; //声明驱动器
        public ProfileSettingsObj profileSettingsObj; //声明驱动器属性
        public LinkageSettingsObj linkageSettingsObj;
        public canOpenObj canObj; //声明网络接口
        public LinkageObj Linkage; //连接一组电机，能够按输入序列同时操作
        public ampSettingsObj ampsetingsObj;//
        public eventObj EventObj;

        private const int MOTOR_NUM = 4; //设置电机个数
        public int motor_num = MOTOR_NUM; //供调用
        public int RATIO = 160; //减速比

        public double[] userUnits = new double[MOTOR_NUM]; // 用户定义单位：编码器每圈计数

        public double[] ampObjAngleActual = new double[MOTOR_NUM];//减速器的转角，单位：°
        public double[] ampObjAngleVelActual = new double[MOTOR_NUM];//减速器的角速度，单位：rad/s
        public double[] ampObjAngleAccActual = new double[MOTOR_NUM];//减速器的角加速度，单位：rad/s^2

        private DispatcherTimer motorsTimer;
        private AngleCoderTrans angletocoder = new AngleCoderTrans(); // 使程序同时适用于盘式电机和丝杠电机外骨骼的类

        // 
        private CML_EVENT_STATUS readstatus0;
        private CML_EVENT_STATUS readstatus1;
        private CML_EVENT_STATUS readstatus2;
        private CML_EVENT_STATUS readstatus3;

        // 包含电机错误的类
        private CML_AMP_FAULT readfault0;
        private CML_AMP_FAULT readfault1;
        private CML_AMP_FAULT readfault2;
        private CML_AMP_FAULT readfault3;
        #endregion

        public void motors_Init()//电机初始化
        {
            try
            {
                canObj = new canOpenObj(); //实例化网络接口
                profileSettingsObj = new ProfileSettingsObj(); //实例化驱动器属性

                //canObj.BitRate = CML_BIT_RATES.BITRATE_500_Kbit_per_sec;
                canObj.BitRate = CML_BIT_RATES.BITRATE_1_Mbit_per_sec; //设置CAN传输速率为1M/s
                canObj.Initialize(); //网络接口初始化

                ampObj = new AmpObj[MOTOR_NUM]; //实例化四个驱动器（盘式电机）
                ampsetingsObj = new ampSettingsObj();

                ampsetingsObj.enableOnInit = true;
                ampsetingsObj.guardTime = 2000; // 防止时间过长工控机电机失能
                //ampsetingsObj.heartbeatPeriod = 1000;
                //ampsetingsObj.heartbeatPeriod = 300;
                ampsetingsObj.lifeFactor = 1000;

                for (int i = 0; i < MOTOR_NUM; i++)//初始化四个驱动器
                {
                    ampObj[i] = new AmpObj();
                    //ampObj[i].Initialize(canObj, (short)(i + 1));
                    ampObj[i].InitializeExt(canObj, (short)(i + 1), ampsetingsObj);
                    ampObj[i].HaltMode = CML_HALT_MODE.HALT_DECEL; //选择通过减速来停止电机的方式
                    ampObj[i].CountsPerUnit = 1; //电机转一圈编码器默认计数25600次，设置为4后转一圈计数6400次
                    userUnits[i] = ampObj[i].MotorInfo.ctsPerRev / ampObj[i].CountsPerUnit; //用户定义单位，counts/圈
                }


                //ampObj[0].PositionActual = -1;
                //ampObj[1].PositionActual = -2;
                //ampObj[2].PositionActual = -2;
                //ampObj[3].PositionActual = -1;

                Linkage = new LinkageObj();
                linkageSettingsObj = new LinkageSettingsObj();
                linkageSettingsObj.moveAckTimeout = 2000; // 代替WaitMoveDone
                //Linkage.Initialize(ampObj);
                Linkage.InitializeExt(ampObj, linkageSettingsObj); //电机联动初始化
                Linkage.SetMoveLimits(200000, 3000000, 3000000, 200000); // 设置电机速度，加速度等极限值
                EventObj = Linkage.CreateEvent(CML_LINK_EVENT.LINKEVENT_TRJDONE | CML_LINK_EVENT.LINKEVENT_MOVEDONE | CML_LINK_EVENT.LINKEVENT_ABORT, CML_EVENT_CONDITION.CML_EVENT_ANY);
                EventObj.EventNotify += new eventObj.EventHandler(EventObj_EventNotify);


                motorsTimer = new DispatcherTimer();
                motorsTimer.Tick += new EventHandler(motorsTimer_Tick); //Tick是超过计时器间隔时发生事件，此处为Tick增加了一个叫ShowCurTimer的取当前时间并扫描串口的委托
                motorsTimer.Interval = TimeSpan.FromMilliseconds(100); ; //设置刻度之间的时间值，设定为1秒（即文本框会1秒改变一次输出文本）
                motorsTimer.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("驱动器初始化错误");
            }


        }

        private void EventObj_EventNotify(CML_AMP_EVENT match, bool hasError) // 检查并输出电机的错误。错误是16进制的数，需转成2进制与指导书表格对比，以此找出具体的电机错误
        {// Handles EventObj.EventNotify
            try
            {
                if (hasError)
                {
                    //Linkage.HaltMove();
                    ampObj[0].ReadEventLatch(ref readstatus0);
                    ampObj[1].ReadEventLatch(ref readstatus1);
                    ampObj[2].ReadEventLatch(ref readstatus2);
                    ampObj[3].ReadEventLatch(ref readstatus3);
                    ampObj[0].ReadFaults(ref readfault0);
                    ampObj[1].ReadFaults(ref readfault1);
                    ampObj[2].ReadFaults(ref readfault2);
                    ampObj[3].ReadFaults(ref readfault3);
                    ampObj[0].ClearFaults();
                    ampObj[1].ClearFaults();
                    ampObj[2].ClearFaults();
                    ampObj[3].ClearFaults();
                    Trace.WriteLine(readstatus0 + "; " + readfault0 + "; " + match + "; " + "Num:0");
                    Trace.WriteLine(readstatus1 + "; " + readfault1 + "; " + match + "; " + "Num:1");
                    Trace.WriteLine(readstatus2 + "; " + readfault2 + "; " + match + "; " + "Num:2");
                    Trace.WriteLine(readstatus3 + "; " + readfault3 + "; " + match + "; " + "Num:3");
                    double a = 0;
                    return;
                }
                if ((match & CML_AMP_EVENT.AMPEVENT_TRAJECTORY_DONE) == CML_AMP_EVENT.AMPEVENT_TRAJECTORY_DONE)
                {
                    double a = 0;
                }
                if ((match & CML_AMP_EVENT.AMPEVENT_MOVE_DONE) == CML_AMP_EVENT.AMPEVENT_MOVE_DONE)
                {
                    double a = 0;
                }
                Trace.WriteLine("Enter EventObj_EventNotify!");
            }
            catch (Exception ex)
            {

            }
        }

        private void motorsTimer_Tick(object sender, EventArgs e) // 实时获取电机当前的位置（角度值）
        {
            //for (int i = 0; i < MOTOR_NUM; i++)
            //{
            //    ampObjAngleActual[i] = (ampObj[i].PositionActual / userUnits[i]) * (360.0 / RATIO);//角度单位从counts转化为°

            //    ampObjAngleVelActual[i] = (ampObj[i].VelocityActual * 0.1 / userUnits[i]) * 2.0 * Math.PI / RATIO;//角速度单位从counts/s转化为rad/s

            //    ampObjAngleAccActual[i] = (ampObj[i].TrajectoryAcc * 10 / userUnits[i]) * 2.0 * Math.PI / RATIO;//角加速度单位从counts/s^2转化为rad/s^2
            //}
            // 当前电机的位置编码值PositionActual转化成角度值ampObjAngleActual
            angletocoder.CoderToAngle(ampObj[0].PositionActual, ampObj[1].PositionActual, ampObj[2].PositionActual, ampObj[3].PositionActual, ref ampObjAngleActual[0], ref ampObjAngleActual[1], ref ampObjAngleActual[2], ref ampObjAngleActual[3]);

        }
    }
}
