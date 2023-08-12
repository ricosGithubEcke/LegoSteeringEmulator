using System;
using System.Threading;
using MonoBrick.EV3;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;

class Program
{

    //beans


    #region variables
    static bool isRunning = true;
    static int updateRateMs = 50;
    static int currentGear = 1;
    static int previousGear = 1;
    static int gearShiftSleepTime = 100;
    static bool doLogInfo = true;
    static bool enableKeyboardControls = true;

    static double steeringWheelRotation;
    static double gasPedalRotation;
    static double brakePedalRotation;
    static bool isClutching;
    static bool wasClutchPressed = false;
    static bool changeViewButton = false;

    static double emulSteer;
    static double emulBrake;
    static double emulGas;

    static int maxSteerRotation = 540;
    static int maxGasPedalRotation = 70;  
    static int maxBrakePedalRotation = 50;
    static int minGasPedalRotation = 10;
    static int minBrakePedalRotation = 10;

    static int lastGear = -1;

    static Motor steeringWheelServo;
    static Motor gasPedalServo;
    static Motor brakePedalServo;
    static TouchSensor clutch;
    static IRSensor clutchXsensor;
    static ColorSensor clutchYsensor;

    static ViGEmClient vigemClient;
    static IDualShock4Controller ds4Controller;
    static IXbox360Controller xControlller;

    // Bindings

    //Xbox
    static Xbox360Axis BSteering = Xbox360Axis.LeftThumbX;
    static Xbox360Slider BGas = Xbox360Slider.RightTrigger;
    static Xbox360Slider BBrake = Xbox360Slider.LeftTrigger;

    static Xbox360Button BClutch = Xbox360Button.LeftShoulder;
    static Xbox360Button BGearUp = Xbox360Button.A;
    static Xbox360Button BGearDown = Xbox360Button.X;

    static bool doEmulateClutchPedal = false;


    #endregion

    #region Main
    static void Main()
    {
        var ev3 = new Brick<Sensor, Sensor, Sensor, Sensor>("usb");
        ev3.Connection.Open();

        Console.WriteLine("Initialized Brick");

        steeringWheelServo = ev3.MotorA;
        steeringWheelServo.ResetTacho();

        gasPedalServo = ev3.MotorB;
        gasPedalServo.ResetTacho();

        brakePedalServo = ev3.MotorC;
        brakePedalServo.ResetTacho();

        Console.WriteLine("Initialized Motors");

        ev3.Sensor1 = new TouchSensor();
        clutch = (TouchSensor)ev3.Sensor1;

        ev3.Sensor3 = new ColorSensor(ColorMode.Reflection);
        clutchYsensor = (ColorSensor)ev3.Sensor3;

        ev3.Sensor4 = new IRSensor(IRMode.Proximity);
        clutchXsensor = (IRSensor)ev3.Sensor4;

        

        Console.WriteLine("Initialized Sensors");

        vigemClient = new ViGEmClient();

        //.......................................................................DS4

        //ds4Controller = vigemClient.CreateDualShock4Controller();
        //ds4Controller.Connect();

        //.......................................................................XBOX360

        xControlller = vigemClient.CreateXbox360Controller();
        xControlller.Connect();
        

        Console.WriteLine("Connected emulated Controller");


        Console.WriteLine("REcalibrate Input?  y/n");
        string doCalibrate = Console.ReadLine();
        if(doCalibrate == "y")
        {
            CalibrateInput();
        }
        EmulateXGearUp();

        if (enableKeyboardControls)
        {
            EnableKeyboardControls();
        }


        Console.WriteLine("Press B for binding Mode");

        previousGear = GetGear();
        ClearConsole();
        ConsoleKeyInfo keyInfo;
        

        while (isRunning)
        {
            
            steeringWheelRotation = GetSteer();
            gasPedalRotation = GetGas();
            brakePedalRotation = GetBrake();
            isClutching = GetClutch();//.....................................................Clutch disabled

            emulSteer = GetEmulSteer();
            emulGas = GetEmulGas();
            emulBrake = GetEmulBrake();


            ClutchReleaseListener();


            if (doLogInfo)
            {
                LogInfo();
            }
            if(Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                if(keyInfo.Key == ConsoleKey.B)
                {
                    BindingMode();
                }
            }
            
            //LogSteeringInfo();

            //UpdateDualShock4Controller();.............................................DS4
            UpdateXCOntroller();

            Thread.Sleep(updateRateMs); // Control the update rate.
        }

        ClearConsole();

        //ds4Controller.Disconnect();//.......................................................DS4
        vigemClient.Dispose();

        ev3.Connection.Close();
    }
    static void CalibrateInput()
    {
        Console.WriteLine("Enter Update Rate, 0:Current = 50ms");
        int newUpdateRate = int.Parse(Console.ReadLine());
        if (newUpdateRate != 0)
        {
            updateRateMs = newUpdateRate;
        }

        Console.WriteLine("Enter Max Steer Rate, 0:Current = 135 degrees each dir");
        int newSteerRate = int.Parse(Console.ReadLine());
        if (newSteerRate != 0)
        {
            maxSteerRotation = newSteerRate;
        }

        Console.WriteLine("Now push down gas pedal!");
        while(true)
        {
            if(GetGas() > 15)
            {
                Thread.Sleep(1000);
                maxGasPedalRotation = Convert.ToInt32(GetGas());

                Console.WriteLine($"Let go now, maxGasVal has been set to {maxGasPedalRotation}");
                break;
            }
        }

        Console.WriteLine("Now push down brake pedal!");
        while (true)
        {
            if (GetBrake() > 15)
            {
                Thread.Sleep(1000);
                maxBrakePedalRotation = Convert.ToInt32(GetBrake());

                Console.WriteLine($"Let go now, maxBrakeVal has been set to {maxBrakePedalRotation}");
                break;
            }
        }

        Console.WriteLine("Input last gear emulation (1 or 6 usually)");
        lastGear = int.Parse(Console.ReadLine());
       

        Console.WriteLine("Want to log Info  y/n");
        string doLog = Console.ReadLine();
        if (doLog == "y")
        {
            doLogInfo = true;
        }

        Console.WriteLine("Want to Enable Keyboard extended controls?  y/n");
        string doKeyCtl = Console.ReadLine();
        if (doKeyCtl == "y")
        {
            enableKeyboardControls = true;
        }
    }

    static void BindingMode()
    {
        bool isRunning = true;
        ClearConsole();
        Console.WriteLine("Gas > y > gear Up");
        Console.WriteLine("Brake > a > gear Down");
        Console.WriteLine("Press K to exit");
        ConsoleKeyInfo keyInfo;
        while (isRunning)
        {
            gasPedalRotation = GetGas();
            brakePedalRotation = GetBrake();

            xControlller.SetButtonState(BGearUp, gasPedalRotation > minGasPedalRotation);
            xControlller.SetButtonState(BGearDown, brakePedalRotation > minBrakePedalRotation);


            if(Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.K)
                {
                    isRunning = false;
                    // Auch Chuck Norris hat als Kind Sandburgen gebaut..... Heute kennen wir sie als die Pyramiden
                }
            }
            

            Thread.Sleep(updateRateMs);
            
        }
    }

    #endregion

    #region Logging
    static void ClearConsole()
    {
        // Reset the console after exiting the loop
        Console.SetCursorPosition(0, 1);
        Console.WriteLine("                                        ");
    }
    static void LogInfo()
    {
        // Update the console with the latest rotation values
        Console.SetCursorPosition(0, 1);
        Console.WriteLine($"Steering Wheel Rotation: {steeringWheelRotation,7} degrees");
        Console.WriteLine($"Gas Pedal Rotation: {gasPedalRotation,15} degrees");
        Console.WriteLine($"Brake Pedal Rotation: {brakePedalRotation,13} degrees");
        Console.WriteLine($"Clutching: {isClutching,20}");
        Console.WriteLine($"Current Gear: {currentGear,20}");
        Console.WriteLine($"ClutchX: {clutchXsensor.ReadAsString(),20}");
        Console.WriteLine($"ClutchY: {clutchYsensor.ReadAsString(),20}");
    }
    static void LogSteeringInfo()
    {
        // Update the console with the latest steering-related values
        Console.SetCursorPosition(0, 1);
        Console.WriteLine($"Steering Wheel Rotation: {steeringWheelRotation,7} degrees");
        Console.WriteLine($"Emulated Steering: {emulSteer,15}");
        Console.WriteLine($"Max Steering Rotation: {maxSteerRotation,10}");
    }
    #endregion

    #region DataSourcing
    static double GetSteer()
    {
        return (double)steeringWheelServo.GetTachoCount();
    }

    static double GetGas()
    {
        return (double)gasPedalServo.GetTachoCount();
    }

    static double GetBrake()
    {
        return (double)brakePedalServo.GetTachoCount();
    }

    static bool GetClutch()
    {
        int result = clutch.Read();
        if(result == 2)
        {
            changeView();
            return false;
        }
        if(result == 1)
            return true;

       return false;
    }
    static bool isPressed = false;
    static void ClutchReleaseListener()
    {
        isPressed = GetClutch();

        // Detect clutch release event
        if (wasClutchPressed && !isPressed)
        {
            OnClutchRelease();
        }

        // Update the previous state of the touch sensor
        wasClutchPressed = isPressed;
    }
    static void EnableKeyboardControls()
    {
        Thread keyboardListenerThread = new Thread(() =>
        {
            while (isRunning)
            {
                // Listen for a key press
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                xControlller.SetButtonState(Xbox360Button.Right, keyInfo.Key == ConsoleKey.RightArrow);
                xControlller.SetButtonState(Xbox360Button.Left, keyInfo.Key == ConsoleKey.LeftArrow);
                xControlller.SetButtonState(Xbox360Button.Up, keyInfo.Key == ConsoleKey.UpArrow);
                xControlller.SetButtonState(Xbox360Button.Down, keyInfo.Key == ConsoleKey.DownArrow);

                xControlller.SetButtonState(Xbox360Button.X , keyInfo.Key == ConsoleKey.X);
                xControlller.SetButtonState(Xbox360Button.B , keyInfo.Key == ConsoleKey.B);

                xControlller.SetButtonState(Xbox360Button.RightShoulder , keyInfo.Key == ConsoleKey.V);

                xControlller.SubmitReport();
                Thread.Sleep(100);

                xControlller.SetButtonState(Xbox360Button.Right, false);
                xControlller.SetButtonState(Xbox360Button.Left, false);
                xControlller.SetButtonState(Xbox360Button.Up, false);
                xControlller.SetButtonState(Xbox360Button.Down, false);

                xControlller.SetButtonState(Xbox360Button.X, false);
                xControlller.SetButtonState(Xbox360Button.B, false);

                xControlller.SetButtonState(Xbox360Button.RightShoulder, false);

                xControlller.SubmitReport();
                Thread.Sleep(100);
            }
        });

        // Start the keyboard input listener thread
        keyboardListenerThread.Start();
    }
    #endregion

    #region Emulation
    static void OnClutchRelease()
    {

        currentGear = GetGear();

        // Calculate the gear difference
        int gearDifference = currentGear - previousGear;

        // Perform gear up or gear down actions based on the gear difference
        if (gearDifference > 0)
        {
            Thread gearUpThread = new Thread(() =>
            {
                for (int i = 0; i < gearDifference; i++)
                {
                    // Call a function to emulate the triangle button press on the emulated controller
                    //EmulateGearUp();.......................................................................................DS4
                    EmulateXGearUp();
                    Console.WriteLine("Gear Up!");
                }
            });
            gearUpThread.Start();
                // Perform gear up action (triangle button press) 'gearDifference' times
                
        }
        else if (gearDifference < 0)
        {
            Thread gearDownThread = new Thread(() =>
            {
                // Perform gear down action (X button press) 'abs(gearDifference)' times
                for (int i = 0; i < Math.Abs(gearDifference); i++)
                {
                    // Call a function to emulate the X button press on the emulated controller
                    //EmulateGearDown();
                    EmulateXGearDown();
                    Console.WriteLine("Gear Down!");
                }
            });
            gearDownThread.Start();
        }

        // Update the previous gear state to the current gear
        previousGear = currentGear;

        if (doEmulateClutchPedal)
        {

            Thread releaseClutch = new Thread(() =>
            {
                int sleepTime = gearShiftSleepTime * Math.Abs(gearDifference) * 2 + 50 + gearShiftSleepTime;
                Thread.Sleep(sleepTime);

                xControlller.SetButtonState(Xbox360Button.LeftShoulder, false);
            });
            releaseClutch.Start();
        }
    }

    static void EmulateGearUp()
    {

                Thread.Sleep(gearShiftSleepTime);

                ds4Controller.SetButtonState(DualShock4Button.Triangle, true);
                ds4Controller.SubmitReport();

                Thread.Sleep(gearShiftSleepTime);

                ds4Controller.SetButtonState(DualShock4Button.Triangle, false);
                ds4Controller.SubmitReport();
       

        
    }
    static void EmulateXGearUp()
    {
        
            Thread.Sleep(gearShiftSleepTime);

            xControlller.SetButtonState(BGearUp, true);
            xControlller.SubmitReport();

            Thread.Sleep(gearShiftSleepTime);

            xControlller.SetButtonState(BGearUp, false);
            xControlller.SubmitReport();
       

    }

    static void EmulateGearDown()
    {
            Thread.Sleep(gearShiftSleepTime);

            ds4Controller.SetButtonState(DualShock4Button.Cross, true);
            ds4Controller.SubmitReport();

            Thread.Sleep(gearShiftSleepTime);

            ds4Controller.SetButtonState(DualShock4Button.Cross, false);
            ds4Controller.SubmitReport();
        
    }
    static void EmulateXGearDown()
    {
            Thread.Sleep(gearShiftSleepTime);

            xControlller.SetButtonState(BGearDown, true);
            xControlller.SubmitReport();

            Thread.Sleep(gearShiftSleepTime);

            xControlller.SetButtonState(BGearDown, false);
            xControlller.SubmitReport();
        
    }

    static bool shiftGearUp = false;
    static bool shiftGearDown = false;

    static void changeView()
    {
        xControlller.SetButtonState(Xbox360Button.RightShoulder, true);
        xControlller.SubmitReport();
        Thread.Sleep(100);
        xControlller.SetButtonState(Xbox360Button.RightShoulder, false);
        xControlller.SubmitReport();
        Thread.Sleep(100);
    }
    

    static void UpdateDualShock4Controller()
    {
        ds4Controller.SetAxisValue(DualShock4Axis.LeftThumbX, (byte)(emulSteer * byte.MaxValue));

        byte gasValue = (byte)(emulGas * 255);
        ds4Controller.SetSliderValue(DualShock4Slider.RightTrigger, gasValue);

        byte brakeValue = (byte)(emulBrake * 255);
        ds4Controller.SetSliderValue(DualShock4Slider.LeftTrigger, brakeValue);

        ds4Controller.SetButtonState(DualShock4Button.ShoulderLeft, isClutching);

        // Update the virtual DualShock 4 controller with the new report
        ds4Controller.SubmitReport();
    }
    static void UpdateXCOntroller()
    {
        xControlller.SetAxisValue(BSteering, (short)(GetXEmulSteer() * short.MaxValue));

        byte gasValue = (byte)(emulGas * 255);
        xControlller.SetSliderValue(BGas, gasValue);

        byte brakeValue = (byte)(emulBrake * 255);
        xControlller.SetSliderValue(BBrake, brakeValue);

        if(isClutching && doEmulateClutchPedal) // Enables clutch on press, but doesnt disable
        {
            xControlller.SetButtonState(BClutch, true);
        }
    }
    #endregion

    #region Processing
    static double GetEmulSteer()
    {
        double emulSteer = steeringWheelRotation / maxSteerRotation + 0.5;

        emulSteer = Math.Max(Math.Min(emulSteer, 1), 0);

        return emulSteer;
    }
    static double GetXEmulSteer()
    {
        double emulSteer = steeringWheelRotation / maxSteerRotation;

        emulSteer = Math.Max(Math.Min(emulSteer, 1), -1);

        return emulSteer;
    }

    static double GetEmulGas()
    {
        if (gasPedalRotation < minGasPedalRotation) return 0;
        double emulGas = gasPedalRotation / maxGasPedalRotation;
        emulGas = Math.Max(Math.Min(emulGas, 1), 0);
        return emulGas;
    }

    static double GetEmulBrake()
    {
        if (brakePedalRotation < minBrakePedalRotation) return 0;
        double emulBrake = brakePedalRotation / maxBrakePedalRotation;
        emulBrake = Math.Max(Math.Min(emulBrake, 1), 0);
        return emulBrake;
    }

    static int GetGear()
    {
        int irSensorValue = clutchXsensor.Read();
        int colorSensorValue = clutchYsensor.Read();

        // Determine the gear based on IR sensor and color sensor values
        if (irSensorValue >= 11 && irSensorValue <= 15 && colorSensorValue > 25)
        {
            return 1;
        }
        else if (irSensorValue >= 11 && irSensorValue <= 15 && colorSensorValue < 25)
        {
            return 2;
        }
        else if (irSensorValue >= 8 && irSensorValue <= 10 && colorSensorValue > 25)
        {
            return 3;
        }
        else if (irSensorValue >= 8 && irSensorValue <= 10 && colorSensorValue < 25)
        {
            return 4;
        }
        else if (irSensorValue >= 1 && irSensorValue <= 7 && colorSensorValue > 25)
        {
            return 5;
        }
        else if (irSensorValue >= 1 && irSensorValue <= 7 && colorSensorValue < 25)
        {
            return lastGear;
        }
        
            // Default to neutral (you may adjust this based on your requirement)
            return 7;
    }
    #endregion
}
