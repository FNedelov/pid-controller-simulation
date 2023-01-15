# pid-controller-simulation
The folders contain a 2018 project that simulates a PID (proportional–integral–derivative) controller circuit

Program operation:
- The fan speed is monitored by an opto-gate. From the output signal of the optical gate, the current RPM value can be calculated, which I try to equate with the pre-set setpoint with PID control.
- The Kc, Ki, Kd values of the PID controller and the required RPM can be set between 0-2500. The effect of the PID control, as well as the current PWM (pulse width modulation) of the signal and the rotation frequency of the fan can be monitored in real time from the LabView UI, Winforms app.
The PID control also allows that if pressure is applied to the fan, it rotates at a higher speed in order to maintain the required value.
- The speed of the fan is changed with PWM, using the output signal from the NI USB 6009 device.

Folders contain:
- Designined and built the circuit, using optocoupler sensor, PC fan as actuator and NI USB-4431 as signal generator.
- Created PID control algorithm in LabView and in C#.
- Finished project that allows the user to use a WinForms app, in which the PID terms and the fan RPM can be freely modified. By changing the parameters, one can track the effects in real time, by watching a graph or the speed of the PC fan.
- Videos of the finished project in 'Fan_Circuit' folder.
