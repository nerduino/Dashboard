using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO;
using System.Collections;
using System.Diagnostics;
using MFCommon.Hardware;
using MFCommon.Network;

using VikingErik.NetMF.MicroLinq;

namespace Dashboard
{
    public class Program
    {

        private static string IP_ADDR = "ADDRESS_GOES_HERE";

        private static string[] SERVERS = new string[] { "000", "211", "212", "221", "222", "231", "232", "241", "242", "251", "252" };
        private static string OHAI = "OHAI";
        private static string KTHXBAI = "KTHXBAI";
       
        private int POLL_PERIOD = 3000; 
        private int SLEEP_PERIOD = 25;
        private static int DECAY_FACTOR = 10;

        private Hashtable outputs;
        private static Led led= new Led(Pins.ONBOARD_LED, false, 20);

        public static void Main()
        {
            new Program().Start();
        }

        private void Init()
        {
            outputs = new Hashtable();

            AD5206[] ad5206 = new AD5206[]{new AD5206(Pins.GPIO_PIN_D9), new AD5206(Pins.GPIO_PIN_D10)};

            byte device = 0;
            byte channel = 0;
            
            foreach (string server in SERVERS)
            {
                outputs.Add(server, new Indicator(new AD5206_Channel(ad5206[device], channel++), DECAY_FACTOR));
                if (channel > 5)
                {
                    channel = 0;
                    device++;
                }
            }
        }


        public void Start()
        {
            Init();

            Stopwatch stopWatch = new Stopwatch();
            while (true)
            {
                led.Flash(3);
                FetchReadings();

                stopWatch.Start();                
                while (stopWatch.ElapsedMilliseconds < POLL_PERIOD)
                {
                    DoOutputDecay();
                    led.Flash();
                    Thread.Sleep(SLEEP_PERIOD);
                }
                stopWatch.Reset();


                Debug.Print("Memory: " + Debug.GC(false));
            }
        }


        private void FetchReadings()
        {
            try
            {
                led.Flash(2);

                using (StreamReader streamReader = NetworkUtils.Get(IP_ADDR, 9999, null))
                {
                    String line = streamReader.ReadLine();
                    if (line.Equals(OHAI))
                    {
                        Begin(line);
                    }
                    else if (line.Equals(KTHXBAI))
                    {
                        End(line);
                    }
                    else
                    {
                        Data(line);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.Message);
            }
        }
        
        private void Begin(string line)
        {

        }
        private void End(string line)
        {

        }
        private void Data(string line)
        {
            string server;
            int percent;
            int position;

            string[] fields = line.Split(':');
            server = fields[0];
            percent = int.Parse(fields[1]);
            position = (percent * 255) / 100;

            Debug.Print("Server: " + server + " Percent: " + percent + " Position: " + position);

            Indicator indicator = (Indicator)outputs[server];
            indicator.TargetValue = position;
        }


        private void DoOutputDecay()
        {
            foreach (Indicator indicator in outputs.Values)
            {
                try
                {
                    indicator.Update();
                }
                catch
                { 
                }
            }
        }

    }

    class Indicator
    {
        private AD5206_Channel channel;
        private int currentValue;

        public int CurrentValue { get { return CurrentValue; } }
        public int TargetValue { get; set; }
        private int DecayFactor { get; set; }

        public Indicator(AD5206_Channel channel, int decayFactor)
        {
            this.channel = channel;
            currentValue = 0;
            TargetValue = 0;
            DecayFactor = decayFactor;
        }

        public void Update()
        {
            if (TargetValue > CurrentValue)
            {
                currentValue = TargetValue;
                channel.Wiper = (byte)TargetValue;
            }
            else if(TargetValue < CurrentValue)
            {
                int delta = (((currentValue - TargetValue) * DecayFactor) / 100);
                currentValue = currentValue + delta;
            }

            channel.Wiper = (byte)currentValue;
        }

    }
}
