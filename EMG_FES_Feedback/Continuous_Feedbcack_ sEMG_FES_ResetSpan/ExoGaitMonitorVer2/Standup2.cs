using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMLCOMLib;
using System.Windows.Threading;
using System.Threading;

namespace ExoGaitMonitorVer2
{
    #region
    //class Standup2
    //{
    //    double[] KeyPos1 = new double[4] { -1132088.889, 1024000, -1024000, 1132088.889 };
    //    double[] KeyPos2 = new double[4] { -1137777.778, 1308444.444, -1308444.444, 1137777.778 };
    //    double[] KeyPos3 = new double[4] { -1160533.333, 1353955.556, -1353955.556, 1160533.333 };
    //    //double[] KeyPos4 = new double[4] { -1160533.333, 1389588.889, -1389588.889, 1160533.333 }; // 20191120备份
    //    double[] KeyPos4 = new double[4] { -1160533.333, 2651977.777, -2651977.777, 1160533.333 };
    //    double[] KeyPos5 = new double[4] { -1072011.111, 1331200, -1331200, 1072011.111 };
    //    //double[] KeyPos6 = new double[4] { -932977.7778, 1331200, -1331200, 932977.7778 };
    //    //double[] KeyPos7 = new double[4] { -861210.1111, 1228800, -1228800, 861210.1111 };
    //    //double[] KeyPos8 = new double[4] { -789443.1111, 1126400, -1126400, 789443.1111 };
    //    double[] KeyPos9 = new double[4] { -817676.1111, 1024000, -1024000, 817676.1111 };
    //    double[] KeyPos10 = new double[4] { -645909.8889, 955733.3333, -955733.3333, 645909.8889 };
    //    //double[] KeyPos11 = new double[4] { -568888.8889, 823333, -823333, 568888.8889 };
    //    double[] KeyPos12 = new double[4] { -497121.3333, 702311.1111, -702311.1111, 497121.3333 };
    //    double[] KeyPos13 = new double[4] { -425354.3333, 599911, -599911, 425354.3333 };
    //    double[] KeyPos14 = new double[4] { -333587.3333, 487511, -487511, 333587.3333 };
    //    double[] KeyPos15 = new double[4] { -251820.7778, 482355.5556, -482355.5556, 251820.7778 };
    //    double[] KeyPos16 = new double[4] { -160053.7778, 229955, -229955, 160053.7778 };
    //    double[] KeyPos17 = new double[4] { 0, 200955.5556, -200955.5556, 0 };
    //    double[] KeyPos18 = new double[4] { 0, 0, 0, 0 };


    //    //double[] KeyPos1 = new double[4] { -1132088.889, 1228800, -1228800, 1132088.889 };
    //    //double[] KeyPos2 = new double[4] { -1120711.111, 1251555.556, -1251555.556, 1120711.111 };
    //    //double[] KeyPos3 = new double[4] { -1092266.667, 1308444.444, -1308444.444, 1092266.667 };
    //    //double[] KeyPos4 = new double[4] { -1069511.111, 1388088.889, -1388088.889, 1069511.111 };
    //    //double[] KeyPos5 = new double[4] { -1072011.111, 1389588.889, -1389588.889, 1072011.111 };
    //    //double[] KeyPos6 = new double[4] { -932977.7778, 1331200, -1331200, 932977.7778 };
    //    //double[] KeyPos7 = new double[4] { -762311.1111, 1228800, -1228800, 762311.1111 };
    //    //double[] KeyPos8 = new double[4] { -568888.8889, 955733.3333, -955733.3333, 568888.8889 };
    //    //double[] KeyPos9 = new double[4] { -443733.3333, 762311.1111, -762311.1111, 443733.3333 };
    //    //double[] KeyPos10 = new double[4] { -113777.7778, 432355.5556, -432355.5556, 113777.7778 };
    //    //double[] KeyPos11 = new double[4] { -56888.88889, 227555.5556, -227555.5556, 56888.88889 };
    //    //double[] KeyPos12 = new double[4] { 0, 0, 0, 0 };

    //    ProfileSettingsObj profileParameters = new ProfileSettingsObj();    //用于设置电机参数
    //    double MotorVelocity = 120;
    //    double MotorAcceleration = 50;
    //    double MotorDeceleration = 50;
        
        
    //    // 求取从初始位置到第一个位置的电机转角变化量的绝对值
    //    public void start_Standup2(Motors motor)
    //    {
    //        double[] DeltaP1 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP1[i] = System.Math.Abs(KeyPos1[i] - 0);
    //        }         
    //        double MaxDeltaP1 = DeltaP1.Max();   //获取变化量的绝对值的最大值
    //        //设置各电机运动参数，并在位置模式下运动到第一个下蹲位置
    //        for (int i = 0; i < motor.motor_num; i++)   
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP1[i] / MaxDeltaP1;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos1[i]);
    //        }



    //        //cm.MotorAmp_Array[9].WaitMoveDone(100000);
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP2 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP2[i] = System.Math.Abs(KeyPos2[i] - KeyPos1[i]);
    //        }
    //       double MaxDeltaP2 = DeltaP2.Max();        //获取变化量的绝对值的最大值
    //       //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //       for (int i = 0; i < motor.motor_num; i++)  
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP2[i] / MaxDeltaP2;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos2[i]);
    //        }
           
           
           
    //        //cm.MotorAmp_Array[9].WaitMoveDone(100000);       
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP3 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP3[i] = System.Math.Abs(KeyPos3[i] - KeyPos2[i]);
    //        }          
    //        double MaxDeltaP3 = DeltaP3.Max();  //获取变化量的绝对值的最大值
    //        //设置各电机运动参数,并在位置模式下运动到第左脚落地位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP3[i] / MaxDeltaP3;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos3[i]);
    //        }



    //        //cm.MotorAmp_Array[9].WaitMoveDone(100000);
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP4 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP4[i] = System.Math.Abs(KeyPos4[i] - KeyPos3[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP4 = DeltaP4.Max();

    //        //设置各电机运动参数,并在位置模式下左移重心
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP4[i] / MaxDeltaP4;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos4[i]);

    //        }
    //        Task task = Task.Run(() =>
    //        {
    //            Thread.Sleep(400);
    //        });

    //        task.Wait();
    //        //MotorAmp_Array[9].WaitMoveDone(100000);
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP5 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP5[i] = System.Math.Abs(KeyPos5[i] - KeyPos4[i]);
    //        }
    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP5 = DeltaP5.Max();

    //        //设置各电机运动参数,并在位置模式下左移重心
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = 0 * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos5[i]);

    //        }

    //        //Thread.Sleep(2000);
    //        //motor.ampObj[3].WaitMoveDone(5000);
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        //double[] DeltaP6 = new double[4];
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    DeltaP6[i] = System.Math.Abs(KeyPos6[i] - KeyPos5[i]);
    //        //}

    //        ////获取变化量的绝对值的最大值
    //        //double MaxDeltaP6 = DeltaP6.Max();

    //        ////设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    profileParameters = motor.ampObj[i].ProfileSettings;
    //        //    profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP6[i] / MaxDeltaP6;    //单位为°/s
    //        //    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //        //    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //        //    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //        //    motor.ampObj[i].ProfileSettings = profileParameters;
    //        //    motor.ampObj[i].MoveAbs(KeyPos6[i]);
    //        //}
    //        //cm.MotorAmp_Array[9].WaitMoveDone(100000);
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        //double[] DeltaP7 = new double[4];
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    DeltaP7[i] = System.Math.Abs(KeyPos7[i] - KeyPos5[i]);
    //        //}

    //        ////获取变化量的绝对值的最大值
    //        //double MaxDeltaP7 = DeltaP7.Max();

    //        ////设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    profileParameters = motor.ampObj[i].ProfileSettings;
    //        //    profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP7[i] / MaxDeltaP7;    //单位为°/s
    //        //    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //        //    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //        //    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //        //    motor.ampObj[i].ProfileSettings = profileParameters;
    //        //    motor.ampObj[i].MoveAbs(KeyPos7[i]);
    //        //}
    //        ////求取相对前一位置的电机转角变化量的绝对值
    //        //double[] DeltaP8 = new double[4];
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    DeltaP8[i] = System.Math.Abs(KeyPos8[i] - KeyPos7[i]);
    //        //}

    //        ////获取变化量的绝对值的最大值
    //        //double MaxDeltaP8 = DeltaP8.Max();

    //        ////设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    profileParameters = motor.ampObj[i].ProfileSettings;
    //        //    profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP8[i] / MaxDeltaP8;    //单位为°/s
    //        //    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //        //    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //        //    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //        //    motor.ampObj[i].ProfileSettings = profileParameters;
    //        //    motor.ampObj[i].MoveAbs(KeyPos8[i]);
    //        //}
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP9 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP9[i] = System.Math.Abs(KeyPos9[i] - KeyPos5[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP9 = DeltaP9.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP9[i] / MaxDeltaP9;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos9[i]);
    //        }
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        double[] DeltaP10 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP10[i] = System.Math.Abs(KeyPos10[i] - KeyPos9[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP10 = DeltaP10.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP10[i] / MaxDeltaP10;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos10[i]);
    //        }
    //        //求取相对前一位置的电机转角变化量的绝对值
    //        //double[] DeltaP11 = new double[4];
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    DeltaP11[i] = System.Math.Abs(KeyPos11[i] - KeyPos10[i]);
    //        //}

    //        ////获取变化量的绝对值的最大值
    //        //double MaxDeltaP11 = DeltaP11.Max();

    //        ////设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        //for (int i = 0; i < motor.motor_num; i++)
    //        //{
    //        //    profileParameters = motor.ampObj[i].ProfileSettings;
    //        //    profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP11[i] / MaxDeltaP11;    //单位为°/s
    //        //    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //        //    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //        //    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //        //    motor.ampObj[i].ProfileSettings = profileParameters;
    //        //    motor.ampObj[i].MoveAbs(KeyPos11[i]);
    //        //}
    //        //////////////////////////////////////////// ///////////////////////////


    //        double[] DeltaP12 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP12[i] = System.Math.Abs(KeyPos12[i] - KeyPos10[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP12 = DeltaP12.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP12[i] / MaxDeltaP12;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos12[i]);
    //        }

    //        //
    //        double[] DeltaP13 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP13[i] = System.Math.Abs(KeyPos13[i] - KeyPos12[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP13 = DeltaP13.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP13[i] / MaxDeltaP13;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos13[i]);
    //        }
    //        //
    //        double[] DeltaP14 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP14[i] = System.Math.Abs(KeyPos14[i] - KeyPos13[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP14 = DeltaP14.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP14[i] / MaxDeltaP14;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos14[i]);
    //        }

    //        //
    //        double[] DeltaP15 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP15[i] = System.Math.Abs(KeyPos15[i] - KeyPos14[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP15 = DeltaP15.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP15[i] / MaxDeltaP15;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos15[i]);
    //        }

    //        //
    //        double[] DeltaP16 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP16[i] = System.Math.Abs(KeyPos16[i] - KeyPos15[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP16 = DeltaP16.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP16[i] / MaxDeltaP16;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos16[i]);
    //        }

    //        //
    //        double[] DeltaP17 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP17[i] = System.Math.Abs(KeyPos17[i] - KeyPos16[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP17 = DeltaP17.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP17[i] / MaxDeltaP17;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos17[i]);
    //        }
    //        Task task1 = Task.Run(() =>
    //        {
    //            Thread.Sleep(200);

    //        });

    //        task1.Wait();
    //        //
    //        double[] DeltaP18 = new double[4];
    //        for (int i = 0; i < motor.motor_num; i++)
    //        {
    //            DeltaP18[i] = System.Math.Abs(KeyPos18[i] - KeyPos17[i]);
    //        }

    //        //获取变化量的绝对值的最大值
    //        double MaxDeltaP18 = DeltaP18.Max();

    //        //设置各电机运动参数,并在位置模式下运动到第左脚抬升位置
    //        for (int i = 1; i < motor.motor_num - 1; i++)
    //        {
    //            profileParameters = motor.ampObj[i].ProfileSettings;
    //            profileParameters.ProfileVel = MotorVelocity * 6400 * 4 * 160 / 360 * DeltaP18[i] / MaxDeltaP18;    //单位为°/s
    //            profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
    //            profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
    //            profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
    //            motor.ampObj[i].ProfileSettings = profileParameters;
    //            motor.ampObj[i].MoveAbs(KeyPos18[i]);
    //        }

    //    }
    //}
    #endregion

    #region
    class Standup2
    {
        // 站立动作细节：
        // 第一列与坐下的最后一行相同
        // 膝髋关节先增大后减小（如下2-3行）
        // 膝关节直立前，髋关节保持前倾25°（如下倒数第2行）
        // 最后髋关节再伸直
        double[,] KeyPos = { { 100, -90, 90, -100 },
                             {102,-151,151,-102},
                             { 102,-183,183,-102},
                             { 94,-135,135,-94},
                             { 71,-90,90,-71 },
                             {56,-84,84,-56 },
                             { 43,-61,61,-43},
                             {37,-52,52,-37 },
                             {29,-42,42,-29 },
                             {0,-25,25,-0 },
                             { 0,0,0,0,},}; // 站立动作的位置点(4列)（单位：°）
        double[,] KeyPos_s = new double[11, 4]; // 对KeyPos角度值的编码值

        //double[,] KeyPos = { { 100, -90, 90, -100 },
        //                     {102,-115,115,-102},
        //                     { 102,-122,122,-102},
        //                     { 94,-117,117,-94},
        //                     { 71,-90,90,-71 },
        //                     {56,-84,84,-56 },
        //                     { 43,-61,61,-43},
        //                     {37,-52,52,-37 },
        //                     {29,-42,42,-29 },
        //                     {0,-25,25,-0 },
        //                     { 0,0,0,0,},}; // 站立动作的位置点(4列)（单位：°）

        //double[,] KeyPos_s = new double[11, 4]; // 对KeyPos角度值的编码值


        AngleCoderTrans angletocoder = new AngleCoderTrans();
        ProfileSettingsObj profileParameters = new ProfileSettingsObj();    //用于设置电机参数

        //double MotorVelocity = 450; // 电机速度(越大越快)
        //double MotorAcceleration = 200; // 电机加速度(越大越快)
        //double MotorDeceleration = 200; // 电机减速度(越大越快)

        double MotorVelocity = 700; // 电机速度(越大越快)
        double MotorAcceleration = 450; // 电机加速度(越大越快)
        double MotorDeceleration = 450; // 电机减速度(越大越快)
        // 求取从初始位置到第一个位置的电机转角变化量的绝对值

        public void start_Standup2(Motors motor) // 站立，原理同坐下
        {
            for (int s = 0; s < 11; s++)
            {
                angletocoder.AngleToCoder((KeyPos[s, 0].ToString()), (KeyPos[s, 1].ToString()), (KeyPos[s, 2].ToString()), (KeyPos[s, 3].ToString()), ref KeyPos_s[s, 0], ref KeyPos_s[s, 1], ref KeyPos_s[s, 2], ref KeyPos_s[s, 3]);

                double[,] DeltaP1 = new double[11, 4];
                double MaxDeltaP = DeltaP1[s, 0];
                for (int i = 0; i < motor.motor_num; i++)
                {
                    if (s == 0)
                    {
                        DeltaP1[s, i] = System.Math.Abs(KeyPos_s[s, i] - 0);
                        if (MaxDeltaP < DeltaP1[s, i])
                        {
                            MaxDeltaP = DeltaP1[s, i];
                        }
                    }
                    else
                    {
                        DeltaP1[s, i] = System.Math.Abs(KeyPos_s[s, i] - KeyPos_s[s - 1, i]);
                        if (MaxDeltaP < DeltaP1[s, i])
                        {
                            MaxDeltaP = DeltaP1[s, i];
                        }
                    }
                }

                //设置各电机运动参数，并在位置模式下运动到第一个下蹲位置
                for (int i = 0; i < motor.motor_num; i++)
                {
                    //profileParameters = motor.ampObj[i].ProfileSettings;
                    profileParameters.ProfileVel = MotorVelocity * 640 * 2 * 160 / 360;//* DeltaP1[s, i] / MaxDeltaP;    //单位为°/s
                    profileParameters.ProfileAccel = MotorAcceleration * 6400 * 4 * 160 / 360;    //单位为°/s2
                    profileParameters.ProfileDecel = MotorDeceleration * 6400 * 4 * 160 / 360;    //单位为°/s
                    profileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                    motor.ampObj[i].ProfileSettings = profileParameters;
                    motor.ampObj[i].MoveAbs(KeyPos_s[s, i]);
                }

                if (s == 3 || s == 9) // 起立时有两个停顿的地方
                {
                    motor.ampObj[0].WaitMoveDone(50000);
                    motor.ampObj[1].WaitMoveDone(50000);
                    motor.ampObj[2].WaitMoveDone(50000);
                    motor.ampObj[3].WaitMoveDone(50000);
                    Task tasks = Task.Run(() =>
                    {
                        Thread.Sleep(300);

                    });
                    tasks.Wait();
                }
            }
            motor.ampObj[0].WaitMoveDone(50000);
            motor.ampObj[1].WaitMoveDone(50000);
            motor.ampObj[2].WaitMoveDone(50000);
            motor.ampObj[3].WaitMoveDone(50000);
        }
    }
    #endregion
}

