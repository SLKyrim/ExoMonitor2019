using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExoGaitMonitorVer2
{
   public class AngleCoderTrans
    {
        double LXKmapan, LXHmapan, RXKmapan, RXHmapan; // 
        double ration = 12; double mapan = 2000; double sigang = 5;
        double Kc = 73.055; double Ka = 24; double Kb = 50; double KL1 = 29; double KL2 = 49.3; double theatK = 120;
        double Hc = 60.05; double Ha = 24; double Hb = 55; double HL1 = 23; double HL2 = 52; double theatH = 157;

        #region 第4代外骨骼专用
        public void AngleToCoder(string alk, string alh, string arh, string ark, ref double clk, ref double clh, ref double crh, ref double crk) // 电机角度值转为编码值
        {
            
            clk = -1 * double.Parse(alk) * 160 / 360 * 25600;
            clh = -1 * double.Parse(alh) * 160 / 360 * 25600;
            crh = -1 * double.Parse(arh) * 160 / 360 * 25600;
            crk = -1 * double.Parse(ark) * 160 / 360 * 25600;
        }

        public void CoderToAngle(double clk, double clh, double crh, double crk, ref double alk, ref double alh, ref double arh, ref double ark) // 电机编码值转为角度值
        {
            alk = 1 * 360 * clk / 25600 / 160;
            alh = -1 * 360 * clh / 25600 / 160;
            arh = 1 * 360 * crh / 25600 / 160;
            ark = -1 * 360 * crk / 25600 / 160;
        }

        public void GetVel(double clk, double clh, double crh, double crk, ref double alk, ref double alh, ref double arh, ref double ark) // 电机编码值转为角度值
        {
            alk = 0.1 * clk / 25600 * 2 * Math.PI / 160;
            alh = 0.1 * clh / 25600 * 2 * Math.PI / 160;
            arh = 0.1 * crh / 25600 * 2 * Math.PI / 160;
            ark = 0.1 * crk / 25600 * 2 * Math.PI / 160;
        }
        #endregion

        #region 第3代外骨骼专用
        //public void AngleToCoder(string alk, string alh, string arh, string ark, ref double clk, ref double clh, ref double crh, ref double crk) // 电机角度值转为编码值
        //{
        //    double aa= 1 * double.Parse(alk);
        //    double cc = -1 * double.Parse(alh);
        //    double dd = 1 * double.Parse(arh);
        //    double bb = -1 * double.Parse(ark);

        //    LXKmapan = Kc - KL1 * Math.Sin(( aa+ theatK) * Math.PI / 180) - Math.Sqrt(Math.Pow(KL2, 2) - Math.Pow((Ka - Kb - KL1 * Math.Cos((aa + theatK) * Math.PI / 180)), 2));
        //    RXKmapan = Kc - KL1 * Math.Sin((bb + theatK) * Math.PI / 180) - Math.Sqrt(Math.Pow(KL2, 2) - Math.Pow((Ka - Kb - KL1 * Math.Cos((bb + theatK) * Math.PI / 180)), 2));
        //    LXHmapan = Hc - HL1 * Math.Sin((cc + theatH) * Math.PI / 180) - Math.Sqrt(Math.Pow(HL2, 2) - Math.Pow((Ha - Hb - HL1 * Math.Cos((cc + theatH) * Math.PI / 180)), 2));
        //    RXHmapan = Hc - HL1 * Math.Sin((dd + theatH) * Math.PI / 180) - Math.Sqrt(Math.Pow(HL2, 2) - Math.Pow((Ha - Hb - HL1 * Math.Cos((dd + theatH) * Math.PI / 180)), 2));
        //    clk = -LXKmapan * mapan * ration / sigang;
        //    clh = -LXHmapan * mapan * ration / sigang;
        //    crh = -RXHmapan * mapan * ration / sigang;
        //    crk = -RXKmapan * mapan * ration / sigang;
        //}
        //public void CoderToAngle(double clk, double  clh, double  crh, double  crk, ref double alk, ref double alh, ref double arh, ref double ark) // 电机编码值转为角度值
        //{
        //    double aa = -1*sigang*clk/(mapan*ration);
        //    double bb = (Math.Pow(KL1,2)-Math.Pow(KL2,2)+Math.Pow((Kb-Ka),2)+Math.Pow((Kc-aa),2)) / (2*KL1*Math.Sqrt(Math.Pow(Kb-Ka,2)+Math.Pow(Kc-aa,2)));
        //    double cc = Math.Atan(Math.Max(Math.Min(((Kb-Ka)/(Kc-aa)),1),-1));    //sigang*(2*Math.PI*aa/sigang)
        //    alk = -1 * (Math.Asin(Math.Max(Math.Min((bb), 1), -1)) - cc) * 180 / Math.PI - theatK + 180;

        //    aa = -1*sigang * crk / (mapan * ration);
        //    bb = (Math.Pow(KL1, 2) - Math.Pow(KL2, 2) + Math.Pow((Kb - Ka), 2) + Math.Pow((Kc - aa), 2)) / (2 * KL1 * Math.Sqrt(Math.Pow(Kb - Ka, 2) + Math.Pow(Kc - aa, 2)));
        //    cc = Math.Atan(Math.Max(Math.Min(((Kb - Ka) / (Kc - aa)),1),-1));
        //    ark = -1 * (Math.Asin(Math.Max(Math.Min((bb), 1), -1) ) - cc) * 180 / Math.PI - theatK + 180;

        //    aa = -1*sigang * clh / (mapan * ration);
        //    bb = (Math.Pow(HL1, 2) - Math.Pow(HL2, 2) + Math.Pow((Hb - Ha), 2) + Math.Pow((Hc - aa), 2)) / (2 * HL1 * Math.Sqrt(Math.Pow(Hb - Ha, 2) + Math.Pow(Hc - aa, 2)));
        //    cc = Math.Atan(Math.Max(Math.Min(((Hb - Ha) / (Hc - aa)),1),-1));
        //    alh = -1 * (Math.Asin(Math.Max(Math.Min((bb), 1), -1)) - cc) * 180 / Math.PI - theatH + 180;

        //    aa = -1*sigang * crh / (mapan * ration);
        //    bb = (Math.Pow(HL1, 2) - Math.Pow(HL2, 2) + Math.Pow((Hb - Ha), 2) + Math.Pow((Hc - aa), 2)) / (2 * HL1 * Math.Sqrt(Math.Pow(Hb - Ha, 2) + Math.Pow(Hc - aa, 2)));
        //    cc = Math.Atan(Math.Max(Math.Min(((Hb - Ha) / (Hc - aa)),1),-1));
        //    arh = -1 * (Math.Asin(Math.Max(Math.Min((bb), 1), -1)) - cc) * 180 / Math.PI - theatH + 180;
        //}
        #endregion
    }
}
