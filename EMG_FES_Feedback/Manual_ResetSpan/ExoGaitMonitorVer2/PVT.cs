using System;
using System.Text;
using System.IO;
using CMLCOMLib;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;


namespace ExoGaitMonitorVer2
{
    class PVT
    {
        private AngleCoderTrans angletocoder = new AngleCoderTrans();

        public void StartPVT(Motors motors, string adress, int timevalue2)
        {
            //形参说明：
            //motors 电机类;
            //adress PVT要走的步态数据文件(txt)的路径; 
            //timevalue2 步态数据每点的时间间隔(建议取值范围14-20);

            #region 
            //计算轨迹位置，速度和时间间隔序列
            //原始数据

            string[] ral = File.ReadAllLines(adress, Encoding.Default); //相对目录是在bin/Debug下，所以要回溯到上两级目录
            int lineCounter = ral.Length; //获取步态数据行数
            string[] col = (ral[0] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int colCounter = col.Length; //获取步态数据列数
            double[,] pos0 = new double[lineCounter, colCounter]; //原始位置数据
            double[,] vel = new double[lineCounter, colCounter]; //速度
            int[] times = new int[lineCounter]; //时间间隔

            for (int i = 0; i < lineCounter; i++)
            {
                times[i] = timevalue2;
                string[] str = (ral[i] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                // for循环用于盘式电机
                //for (int j = 0; j < colCounter; j++)
                //{
                //    pos0[i, j] = double.Parse(str[j]) / (360.0 / motors.RATIO) * motors.userUnits[j] * -1;
                //}
                //angletocoder.CoderToAngle(double.Parse(str[0]), double.Parse(str[1]), double.Parse(str[2]), double.Parse(str[3]),ref pos0[i,0],ref pos0[i,1],ref pos0[i,2],ref pos0[i,3]);
                angletocoder.AngleToCoder((str[0]), (str[1]), (str[2]), (str[3]), ref pos0[i, 0], ref pos0[i, 1], ref pos0[i, 2], ref pos0[i, 3]); // 角度值转变为编码值

            }
            FileStream pw = new FileStream("positiont.txt", FileMode.Create, FileAccess.ReadWrite);
            StreamWriter psw = new StreamWriter(pw);

            for (int i = 0; i < lineCounter; i++)//输出位置
            {
                psw.WriteLine(pos0[i, 0].ToString() + "\t" + pos0[i, 1].ToString() + "\t" + pos0[i, 2].ToString() + "\t" + pos0[i, 3].ToString());
            }
            psw.Flush();
            psw.Close();
            times[lineCounter - 1] = 0;

            for (int i = 0; i < lineCounter - 1; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    vel[i, j] = (pos0[i + 1, j] - pos0[i, j]) * 1000.0 / (times[i]); // 求每一点的速度 = （当前位置 - 上一个位置）/ 时间差
                }
            }
            vel[lineCounter - 1, 0] = 0;
            vel[lineCounter - 1, 1] = 0;
            vel[lineCounter - 1, 2] = 0;
            vel[lineCounter - 1, 3] = 0;
            #endregion

            motors.Linkage.TrajectoryInitialize(pos0, vel, times, 500); //开始步态
            motors.EventObj.Start(true, 5000);
            motors.Linkage.WaitMoveDone(50000); // 20000毫秒内PVT未执行完毕则报错，并阻塞其它线程
            motors.EventObj.Stop();
            //motors.ampObj[0].WaitMoveDone(10000);
            //motors.ampObj[1].WaitMoveDone(10000);
            //motors.ampObj[2].WaitMoveDone(10000);
            //motors.ampObj[3].WaitMoveDone(10000);

        }



        public void  start_Sitdown2(Motors motor) // 位置模式控制(非PVT模式)执行坐下动作
        {
            #region
            double[,] KeyPos ={ { 5, -20, 20, -5 },
                                { 14, -52.7, 52.7, -14 },
                                { 30.7, -78.5, 78.5, -30.7 },
                                { 64, -102.8,102.8,-64},
                                { 87, -120, 120, -87 },
                                { 91, -113, 113, -91 },
                                { 100, -90, 90, -100 }}; // 坐下动作的位置点(4列)（单位：°）
            double[,] KeyPos_s = new double[7, 4]; // 对KeyPos角度值的编码值
            #endregion
            ProfileSettingsObj profileParameters = new ProfileSettingsObj();    //用于设置电机参数

            //double MotorVelocity = 230; // 电机速度(越大越快)
            //double MotorAcceleration = 100; // 电机加速度(越大越快)
            //double MotorDeceleration = 100; // 电机减速度(越大越快)

            double MotorVelocity = 280; // 电机速度(越大越快)
            double MotorAcceleration = 150; // 电机加速度(越大越快)
            double MotorDeceleration = 150; // 电机减速度(越大越快)

            double[,] DeltaP = new double[7, 4];
            for (int s = 0; s < 7; s++)
            {
                angletocoder.AngleToCoder((KeyPos[s, 0].ToString()), (KeyPos[s, 1].ToString()), (KeyPos[s, 2].ToString()), (KeyPos[s, 3].ToString()), ref KeyPos_s[s, 0], ref KeyPos_s[s, 1], ref KeyPos_s[s, 2], ref KeyPos_s[s, 3]);
                for (int j = 0; j < motor.motor_num; j++) // 求每个坐下动作位置点的差值(点与点之间的路程)
                {
                    if (s == 0)
                    {
                        DeltaP[s, j] = Math.Abs(KeyPos_s[s, j] - 0);
                    }
                    else
                    {
                        DeltaP[s, j] = Math.Abs(KeyPos_s[s, j] - KeyPos_s[s - 1, j]);
                    }

                }
                for (int i = 0; i < motor.motor_num; i++) // 求每个电机坐下动作位置点间差值的最大值（以便后续归一化）
                {
                    double MaxDeltaP = DeltaP[s, 0];
                    if (MaxDeltaP < DeltaP[s, i])
                    {
                        MaxDeltaP = DeltaP[s, i];
                    }
                    //profileParameters = motor.ampObj[i].ProfileSettings;
                    profileParameters.ProfileVel = MotorVelocity * 640 * 4 * 160 / 360 * DeltaP[s, i] / MaxDeltaP;    //单位为°/s
                    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
                    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
                    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                    motor.ampObj[i].ProfileSettings = profileParameters;
                    motor.ampObj[i].MoveAbs(KeyPos_s[s, i]); // 位置控制模式，让电机按以上速度，加速度移动到KeyPos_s的位置
                }

            }
            //motors.Linkage.WaitMoveDone(10000);
            motor.ampObj[0].WaitMoveDone(10000);
            motor.ampObj[1].WaitMoveDone(10000);
            motor.ampObj[2].WaitMoveDone(10000);
            motor.ampObj[3].WaitMoveDone(10000);
        }
    }
}
