# LegoSteeringEmulator
Converts Lego Mindstorm Ev3 servo input to emulated XBox360 or DS4 controller input. Allows you to use your Ev3 as a racing wheel. VR-compatible

IMPORTANT: Separate installation of the VigemBus driver is necessary.

Description:
This repository contains a driving simulator controller emulator program that allows users to connect and control driving simulator games using physical input devices such as LEGO EV3 motors and sensors. The emulator supports both DualShock 4 and Xbox 360 controllers, enabling users to experience a realistic driving simulation with their preferred input devices. The program also offers Virtual Reality (VR) support, allowing users to immerse themselves in the driving experience.

How It Works:
The driving simulator controller emulator program leverages the LEGO EV3 hardware components, including motors for the steering wheel, gas pedal, and brake pedal, as well as touch and color sensors for clutch input. The user can physically interact with these hardware components to control the driving simulation.

    Hardware Setup: The program requires a LEGO EV3 brick connected to various motors and sensors. The steering wheel motor controls the virtual steering wheel, the gas pedal motor simulates acceleration, and the brake pedal motor simulates deceleration. The clutch input is detected through a touch sensor, while color and IR sensors provide additional data.

    Emulated Controllers: The program uses the VigemBusDriver library to create emulated controllers. It supports both the DualShock 4 and Xbox 360 controllers, allowing users to choose their preferred controller for the driving simulation.

    Calibration: The program offers a calibration menu to fine-tune various settings. Users can adjust the update rate, maximum steering rotation, and maximum values for gas and brake pedals to match their preferences and hardware setup.

    Real-time Data Processing: The program continuously reads the input from LEGO EV3 motors and sensors, processes the data, and updates the emulated controller accordingly. This real-time data processing ensures a smooth and responsive driving experience.

    Clutch Control: The clutch is controlled by a touch sensor. The program intelligently releases the clutch after the gear shifting is completed, ensuring realistic gear changes during gameplay.

    Virtual Reality Support: Users can enhance their driving experience by using VR headsets with the emulator. The program synchronizes the VR input with the physical input from the LEGO EV3 hardware, creating an immersive and engaging VR driving simulation.

    Logging and Debugging: The program includes logging features to display real-time information about the steering wheel rotation, gas pedal position, brake pedal position, current gear, and more. This data can be helpful for debugging and performance optimization.

Note:
This driving simulator controller emulator provides an innovative and unique way to control driving simulator games, offering users an immersive and realistic experience. The repository includes the source code, setup instructions, and usage guidelines to help users set up and enjoy the emulator with their LEGO EV3 hardware and favorite driving simulator games. Contributions and feedback from the open-source community are welcome to further enhance the program's features and compatibility with different driving simulation platforms. Enjoy the ride! 🚗🏎️🚦
