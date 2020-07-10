using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;


namespace ExoGaitMonitorVer2
{
    class Motionstim8
    {
        public
            static SerialPort StimSerialPort;
            string name = "COM5";
            static uint Channel_Stim;
            static uint Channel_Lf;
            static float Main_Time;
            static float Group_Time;
            static uint N_Factor;
            static uint nc;
            int initialised;
            int[] index = { 0, 1, 2, 3, 4, 5, 6, 7 };

        //global parameter
        static uint Mode_Update; //calculate this from group and main time  

        byte[] Init_Buffer = new byte[6];
        byte[] Write_Buffer = new byte[25];
        static uint Number_Bytes;
        static uint[] Pulse_Width = new uint[8];
        static uint[] Pulse_Current = new uint[8];
        static uint[] Mode = new uint[8];



        public Motionstim8()
        {

            Channel_Stim = 0;
            Channel_Lf = 0;
            Main_Time = 0;
            Group_Time = 0;
            N_Factor = 0;
            nc = 0;
            initialised = 0;
        }
        ~Motionstim8() { }

        public int OpenSetup_serial()
        {
           
            StimSerialPort = new SerialPort(name, 115200, Parity.None, 8, StopBits.One);
            StimSerialPort.Handshake = Handshake.None;
            StimSerialPort.RtsEnable = false;

            // Set the read/write timeouts
            //StimSerialPort.ReadTimeout = 1500;
            //StimSerialPort.WriteTimeout = 1500;

            //if (StimSerialPort.IsOpen)
            //    StimSerialPort.Close();
            if (!StimSerialPort.IsOpen)
                StimSerialPort.Open();
            return 0;

        }




        /*!
        \fn int motionstim8::Send_Init_Param(unsigned int nc,
                        unsigned int Channel_Stim, 
                        unsigned int Channel_Lf,
                        unsigned int Main_Time, 
                        unsigned int Group_Time, 
                        unsigned int N_Factor)

        \param nc Number of channels in the channel list
        \param Channel_Stim The channel list coded in bits. LSB is channel 1, bit 7 is channel 8. 
        \param Channel_Lf The low frequency channel list. Then channels in this list will be
        stimulated with a lower frequency.
        \param Main_Time This time is the time between the main pulses. The coding is as
        following 1.0 + 0.5* Main_Time. 
        \param Group_Time This time is the time in between the doublets and triplets. The coding is as following:1.5 +0.5*Group_Time.
        \param N_Factor The N_Factor is deciding the frequency of the channels in the Channel_Lf list.

        This function codes the information and sends this information to the stimulator to initialise the
        "Channel List Mode". Before opening the channel list mode, the serial port must be opened 
        by using Open_serial(char *Portname). Channel_Stim is the list of channels to be set up in the 
        channel list. The channels are coded in the bits starting with LSB is channel 1 to MSB is channel 8. The corresponding channels 
        in Channel_Lf will be stimulated with a lower frequency defined by N_Factor.

        Returns code:
        \verbatim
        Error                  Explanation
        ------------------    ----------------------------------------------------
        0                      No error has occurred. Stimulator successfully initialised
        1                      Channel_Stim==0, no channels in the channel list
        2                      Group Time > Main_Time. 
        3                      Stimulator is not responding. Check your cable
        4                      The stimulator returns that initialisation is unsuccessfully.
        5                      Group time is less than nc * 1.5.

        \endverbatim
        */
        public int Send_Init_Param(int nc,
                int Channel_Stim,
                int Channel_Lf,
                int Main_Time,
                int Group_Time,
                int N_Factor)
        {
            int tmp;
            int Check;
            int Mode_Update = 0;
            double Dou_Main_Time;
            double Dou_Group_Time;
            int error = 0;
            int Receive;
            char[] Read_Buffer = new char[10];
            int norb;
            int nb = 6;
            /*
            this->Channel_Stim=Channel_Stim;
            this->Channel_Lf=Channel_Lf;
            this->Main_Time=Main_Time;
            this->Group_Time=Group_Time;
            this->N_Factor=N_Factor;
            this->nc=nc;
            */
            /*
              Defining error return
              0 Successfully
              1 Channel_Stim==0 No channels in the channel list
              2 Group Time > Main_Time Initialisation unsuccessfully
              3 did not receive any byte from stimulator
              4 did not receive confirmation from stimulator
              5 Group time < nc * 1.5

            */

            //debugginh
            /*
             printf("Main Time: %d\n",Main_Time);
             printf("Group Time: %d\n",Group_Time);
             printf("N_Factor: %d\n",N_Factor);
             printf("Channel_Lf: %d\n",Channel_Lf);
             printf("Channel_Stim: %d\n",Channel_Stim);
            */


            /* Testing if more than one channel is used */
            if (Channel_Stim == 0)
                return 1;
            /*
              Tesing Main time and Group Time to find Mode_update. d.h if it is
              possible to send doublets and triplets. 
            */
            if (Main_Time == 0)
                Dou_Main_Time = 0;
            else
                Dou_Main_Time = Main_Time * 0.5 + 1.0;
            Dou_Group_Time = Group_Time * 0.5 + 1.5;

            if (Main_Time != 0)
            {
                if (Dou_Main_Time >= Dou_Group_Time)
                {
                    Mode_Update = 0; // Only singles allowed
                    if (Dou_Main_Time >= 2 * Dou_Group_Time)
                        Mode_Update = 1; // Doublets allowed
                    if (Dou_Main_Time >= 3 * Dou_Group_Time)
                        Mode_Update = 2; // Triplets allowed
                    if (Dou_Group_Time < 1.5 * nc)
                        return 5;
                }
                else
                    return 2;
            }
            else
                Mode_Update = 2; /* Triples allowed, frequency decided by user */


            /*
              Mask out Channel_Lf to  only those channels in the Channel_Stim list
            */
            Channel_Lf &= Channel_Stim;

            /* If N_Factor==0 must the Channel_Lf list contain no channel */
            if (N_Factor == 0)
                Channel_Lf = 0;


            Check = (N_Factor & 0x07) + (Main_Time & 0x7ff) + (Group_Time & 0x1f) +
               (Channel_Lf & 0xff) + (Channel_Stim & 0xff) + (Mode_Update & 0x03);


            Check = Check & 0x07;  /* Modulo 32 */

            /***** Byte 0 *********/
            tmp = 0;
            tmp = N_Factor >> 1; /* Setting 2 MSB to first byte */
            tmp |= (char)0x80; /* set first bit 1 */
            tmp |= Check << 2;
            tmp &= 0x9F;


            Init_Buffer[0] = (byte)tmp;

            /***** Byte 1 *********/
            tmp = 0;
            tmp = N_Factor << 6;
            tmp |= Channel_Stim >> 2;
            tmp &= 0x7F;
            Init_Buffer[1] = (byte)tmp;
            /* byte 2  */
            tmp = 0;
            tmp = Channel_Stim << 5;
            tmp &= 0xE0;
            tmp &= 0x7F;
            tmp |= Channel_Lf >> 3;
            Init_Buffer[2] = (byte)tmp;
            /*  byte 3 */
            tmp = 0;
            tmp |= Channel_Lf << 4;
            tmp &= 0x7F;
            tmp |= Mode_Update << 2;
            tmp |= Group_Time >> 3;
            Init_Buffer[3] = (byte)tmp;
            /*  byte 4 */
            tmp = 0;
            tmp |= Group_Time << 4;
            tmp |= Main_Time >> 7;
            tmp &= 0x7F;

            Init_Buffer[4] = (byte)tmp;
            /*  byte 5 */
            tmp = 0;
            tmp = Main_Time;
            tmp &= 0x7F;
            Init_Buffer[5] = (byte)tmp;


            StimSerialPort.Write(Init_Buffer, 0, nb);
            //  norb = this->Sport.serial_readstring(Read_Buffer, 1);
            //norb = StimSerialPort.Read(Read_Buffer, 0, 1);
            //Console.Write("Init norb:");
            //Console.WriteLine(norb);

            //Receive = (0xFF & Read_Buffer[0]);
            //if (norb > 0)
            //{
            //    Console.Write("Received initialisation byte:");
            //    Console.WriteLine(Receive);
            //}
            //else
            //{
            //    Console.WriteLine("Did not receive any inititalisation byte:");
            //    return 3;
            //}

            //if (Receive == 0x01)
            //{
            //    Console.WriteLine("Initialisation successfully");
            //    error = 0;
            //}
            //else
            //{
            //    Console.WriteLine("Initialisation failed");
            //    error = 4;
            //}

            //if (error == 0)
            //    initialised = 1;

            return 0; //error;
        }




        /*!
\fn int motionstim8::Send_Single_Pulse(unsigned int Channel_Number,
                                      unsigned int Pulse_Width,
		                      unsigned int Pulse_Current)


\param Channel_Number The number of the channel to be activated
\param Pulse_Width Pulse Width
\param Pulse_Current Current


This function is for a single pulse. 

Error code:
\verbatim
Error                  Explanation
------------------    ----------------------------------------------------
0                      No error has occurred
1                      motionstim8 not initialised

\endverbatim
*/

        public int Send_Single_Pulse(int Channel_Number,
                      int Pulse_Width,
                      int Pulse_Current)
        {


            int nb = 4;
            int tmp;
            int norb;
            byte[] Read_Buffer = new byte[10];
            int Receive;
            int error;
            int Check;
            int Number_Bytes = nb;
            int i;


            /* B Y T E  0 */
            tmp = 0xE0; /* Setting bit B7 ,B6 and B5 */
            Check = (Pulse_Width + Pulse_Current + Channel_Number); /* Adding all current and 
					       pulse with */
            Console.Write("Chechsum:");
            Console.WriteLine(Check);

            Check &= 0x1F; /* Take the 5 first bits of checksum (modulo 32)*/
                           //  printf("Check sum: %X\n",Check);
            Console.Write("Chechsum:");
            Console.WriteLine(Check);

            tmp |= Check;
            Write_Buffer[0] = (byte)tmp;

            /* B Y T E 1 */
            tmp = 0;
            tmp = Channel_Number << 4;
            tmp |= Pulse_Width >> 7;
            Write_Buffer[1] = (byte)tmp;
            /* B Y T E 2 */
            tmp = 0;
            tmp |= (Pulse_Width & 0x7F); /* 7LSB of pulse width */
            Write_Buffer[2] = (byte)tmp;
            /* B Y T E 3 */
            tmp = 0;
            tmp |= (Pulse_Current & 0x7F);
            Write_Buffer[3] = (byte)tmp;



            StimSerialPort.Write(Write_Buffer, 0, Number_Bytes);

            Console.Write("Number Bytes:");
            Console.WriteLine(Number_Bytes);

            Console.WriteLine("Write Buffer: ");
            for (i = 0; i < Number_Bytes; i++)
                Console.WriteLine((uint)Write_Buffer[i]);
            //Console.Write("\n");

            //public int Read(char[] buffer, int offset, int count);

            //norb = StimSerialPort.Read(Read_Buffer, 0, 1);
            //Console.Write("Single pulse norb:");
            //Console.WriteLine(norb);

            //Receive = (0XFF & Read_Buffer[0]);

            //if (norb > 0 & (0xFF & Receive) == 0xC1)
            //{
            //    //     printf("Updating parameters succsessfull\n");
            //    error = 0;
            //}
            //else
            //{
            //    //  printf("Updating parameter failed\n");
            //    error = 1;
            //}
            return 0;// error;
        }






    }
}
