using System;
using System.Threading;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO;
using System.Collections;
using MFCommon.Hardware;
using MFCommon.Network;
using Komodex.NETMF.MicroTweet.HTTP;


namespace Dashboard
{
    public class Program
    {

        //private static string URL = "ADDRESS_GOES_HERE";
        private static string URL = "http://10.82.115.238:999/";

        private static string[] SERVERS = new string[] { "000", "211", "212", "221", "222", "231", "232", "241", "242", "251", "252" };

       
        private int POLL_PERIOD = 3000; 
        private int SLEEP_PERIOD = 250;
        private static int DECAY_FACTOR = 10;

        private Hashtable outputs;
        private static Led led = new Led(Pins.ONBOARD_LED, false, 20);

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

                HttpRequest httpRequest = new HttpRequest(new HttpUri(URL));

                HttpResponse response = httpRequest.GetResponse();
                Debug.Print(response.ResponseBody);

                string[] lines = response.ResponseBody.Split('\n');
                for (int i = 0; i < lines.Length; i += 1)
                {
                    ProcessData(lines[i].Trim());
                }
            }

            catch (Exception e)
            {
                Debug.Print("Exception: " + e.Message);
            }
        }

        private void ProcessData(string line)
        {
            string server;
            int percent;
            int position;


            string[] fields = line.Split(',');
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
                    Debug.Print("Update");
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

        public int CurrentValue { get; private set; }
        public int TargetValue { get; set; }
        private int DecayFactor { get; set; }

        public Indicator(AD5206_Channel channel, int decayFactor)
        {
            this.channel = channel;
            CurrentValue = 0;
            TargetValue = 0;
            DecayFactor = decayFactor;
        }

        public void Update()
        {
            if (TargetValue > CurrentValue)
            {
                CurrentValue = TargetValue;
                channel.Wiper = (byte)TargetValue;
            }
            else if(TargetValue < CurrentValue)
            {
                int delta = (((CurrentValue - TargetValue) * DecayFactor) / 100);
                CurrentValue = CurrentValue + delta;
            }

            channel.Wiper = (byte)CurrentValue;
        }

    }
}
