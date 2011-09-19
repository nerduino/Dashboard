using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using MFCommon.Hardware;
using MFCommon.Network;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace Dashboard
{
    public class Outputs
    {
        public const Cpu.Pin RED1 = Pins.GPIO_PIN_D2;
        public const Cpu.Pin AMBER1 = Pins.GPIO_PIN_D3;
        public const Cpu.Pin GREEN1 = Pins.GPIO_PIN_D4;
        public const Cpu.Pin BLUE1 = Pins.GPIO_PIN_D7;
        public const Cpu.Pin RED2 = Pins.GPIO_PIN_D8;
        public const Cpu.Pin GREEN2 = Pins.GPIO_PIN_D9;
        
        public const Cpu.Pin AWESBUTTON1 = Pins.GPIO_PIN_D10;
        public const Cpu.Pin AWESBUTTON2 = Pins.GPIO_PIN_D6;

        public const Cpu.Pin CS1 = Pins.GPIO_PIN_D0;
        public const Cpu.Pin CS2 = Pins.GPIO_PIN_D1;

        public const Cpu.Pin AWESMETER1 = Pins.GPIO_PIN_D5;
        
        private const Cpu.Pin MOSI = Pins.GPIO_PIN_D11;
        private const Cpu.Pin MISO= Pins.GPIO_PIN_D12;
        private const Cpu.Pin SPCK = Pins.GPIO_PIN_D13;
        
    }

    public class Program
    {
        /*
         *  Static config
         */
        private static string URL = "http://10.82.115.238:999/";

        private static string[] SERVERS = new string[] { "000", "211", "212", "221", "222", "231", "232", "241", "242", "251", "252" };

        private const int POLL_PERIOD = 3000;
        private const int DECAY_FACTOR = 5;
        

        /*
         * Hardware config
         * 
         */
        private Led onboardLed = new Led(Pins.ONBOARD_LED, false, 50);

        private Led[] lamps;
        private Awesometer awesomeMeter;

        /*
         * Other fields
         */
        private Hashtable outputs;
        private MFCommon.Hardware.Button resetAwesomeness;
        private MFCommon.Hardware.Button increaseAwesomeness;

        public static void Main()
        {
           
            Microsoft.SPOT.Hardware.Utility.SetLocalTime(NTP.NTPTime(8));
            Debug.Print("Clock Set");

            new Program().Start();
        }

        public void Start()
        {
            InitIndicators();
            InitLamps();
            InitMeters();

            Debug.Print("Init Complete.");
            Debug.Print("Memory: " + Debug.GC(false));

            DoPOST();
            Debug.Print("POST Complete.");

            while (true)
            {
                onboardLed.Flash(3);
                FetchReadings();
                Thread.Sleep(POLL_PERIOD);

                Debug.Print("Memory: " + Debug.GC(false));
            }
        }

        private void DoPOST()
        {

            onboardLed.Flash(5);

            foreach(Led lamp in lamps) {
                lamp.State = true;
            }
            foreach (Indicator indicator in outputs.Values)
            {
                indicator.Value = 100;
            }
            awesomeMeter.Awesomeness = 100;
            awesomeMeter.Decay = 0;
            awesomeMeter.Jitter = 0;

            Thread.Sleep(2000);
            
            foreach (Led lamp in lamps)
            {
                lamp.State = false;
            }
            foreach (Indicator indicator in outputs.Values)
            {
                indicator.Value = 0;
            }

            awesomeMeter.Awesomeness = 0;
            awesomeMeter.Jitter = 10;
            awesomeMeter.Decay = 5;

            onboardLed.Flash(5);
        }

     
        private void FetchReadings()
        {
            try
            {
                onboardLed.Flash(2);

                using (HttpWebRequest request = HttpWebRequest.Create(URL) as HttpWebRequest)
                {
                    request.KeepAlive = true;
                    request.Method = "GET";
                    request.Timeout = 10000;
                    request.ReadWriteTimeout = 15000;

                    using (WebResponse response = request.GetResponse())
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseBody = streamReader.ReadToEnd();

                        Debug.Print(responseBody);

                        string[] lines = responseBody.Split('\n');
                        for (int i = 0; i < lines.Length; i += 1)
                        {
                            ProcessData(lines[i].Trim());
                        }
                        streamReader.Close();
                    }
                }
            }

            catch (Exception e)
            {
                Debug.Print("Exception: "+e.Message);
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
            indicator.Value = position;
        }


        private void InitIndicators()
        {
            outputs = new Hashtable();

            AD5206[] ad5206 = new AD5206[] { new AD5206(Outputs.CS1), new AD5206(Outputs.CS2) };

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

        private void InitLamps()
        {
                Led red1 = new Led(Outputs.RED1, false);
                Led amber1 = new Led(Outputs.AMBER1, false);
                Led green1 = new Led(Outputs.GREEN1, false);
                Led blue1 = new Led(Outputs.BLUE1, false);
                Led red2 = new Led(Outputs.RED2, false);
                Led green2 = new Led(Outputs.GREEN2, false);

            lamps = new Led[6] { red1, amber1, green1, blue1, red2, green2};
                  
        }

        private void InitMeters()
        {
            awesomeMeter = new Awesometer(Outputs.AWESMETER1);
            awesomeMeter.Jitter = 10;
            awesomeMeter.Decay = 5;

            increaseAwesomeness = new MFCommon.Hardware.Button(Outputs.AWESBUTTON1);
            resetAwesomeness = new MFCommon.Hardware.Button(Outputs.AWESBUTTON2);

            increaseAwesomeness.Pressed += new NoParamEventHandler(increaseAwesomeness_Pressed);
            resetAwesomeness.Pressed += new NoParamEventHandler(resetAwesomeness_Pressed);

        }


        void resetAwesomeness_Pressed()
        {
            if (increaseAwesomeness.IsPressed)
            {
                BothButtonsPressed();
            }
            else
            {
                awesomeMeter.Reset();
            }
        }

        void increaseAwesomeness_Pressed()
        {
            if (resetAwesomeness.IsPressed)
            {
                BothButtonsPressed();
            }
            else
            {
                awesomeMeter.Awesomeness += 5;
            }
        }

        private void BothButtonsPressed()
        {
            PowerState.RebootDevice(true);
        }

    }


    class Indicator
    {
        private AD5206_Channel channel;

        public int CurrentValue { get; private set; }
        public int Value { get; set; }
        private int DecayFactor { get; set; }

        public bool Stopped { get; set; }

        public Indicator(AD5206_Channel channel, int decayFactor)
        {
            this.channel = channel;
            CurrentValue = 0;
            Value = 0;
            DecayFactor = decayFactor;
            Stopped = false;
            new Thread(new ThreadStart(Loop)).Start(); ;
        }

        void Loop()
        {
            while (!Stopped)
            {
                Update();
                Thread.Sleep(250);
            }
        }

        public void Update()
        {
            if (Value > CurrentValue)
            {
                CurrentValue = Value;
                channel.Wiper = (byte)percentToRange(CurrentValue);
                Debug.Print("Update wiper to "+channel.Wiper+" channel "+ channel.Channel);

            }
            else if (Value < CurrentValue)
            {

                CurrentValue = CurrentValue - DecayFactor;
                if (CurrentValue < 0)
                {
                    CurrentValue = 0;
                }
                channel.Wiper = (byte)percentToRange(CurrentValue);
                Debug.Print("Decay wiper to " + channel.Wiper + " channel " + channel.Channel);
            }
            else
            {
                //do nothing.
            }
        }

        private int percentToRange(int percent)
        {
            return (255 * percent) / 100;
        }
    }


    class Awesometer
    {
        PWM pwm;
        Thread thread;
        Random random;

        public int Awesomeness { get; set; }
        public int Jitter { get; set; }
        public int Decay { get; set; }

        bool Stopped { get; set; }
        int Tick { get; set; }

        public Awesometer(Cpu.Pin pin)
        {
            pwm = new PWM(pin);
            random = new Random();
            Start();
        }

        public void Start()
        {
            Stopped = false;
            thread = new Thread(new ThreadStart(Loop));
        }

        public void Stop()
        {
            Stopped = true;
        }

        public void Reset()
        {
            Tick = 0;
            Awesomeness = 0;
        }

        private void Loop()
        {
            while (!Stopped)
            {
                Tick++;
                Update();
                Thread.Sleep(50);
            }
        }

        private void Update()
        {
            int dutyCycle = Awesomeness + random.Next(Jitter) - (Jitter / 2);
            if (dutyCycle < 0) dutyCycle = 0;
            if (dutyCycle > 100) dutyCycle = 100;

            pwm.SetDutyCycle((uint)dutyCycle);
            if (Tick % 10 == 0)
            {
                Awesomeness = Awesomeness - Decay;
            }
        }
    }
}