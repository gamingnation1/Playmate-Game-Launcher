using LibreHardwareMonitor.Hardware;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Computer = LibreHardwareMonitor.Hardware.Computer;

namespace Game_Launcher_V2.Scripts
{
    internal class GetSensor
    {

        public static void openSensor()
        {
            thisPC.Open();
            thisPC.Accept(new UpdateVisitor());
        }

        public static Computer thisPC = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        public static float getCPUInfo(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                            {
                                value = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public static float getAMDGPU(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                            {
                                value = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public static float getNVGPU(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuNvidia)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                            {
                                value = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public static float getBattery(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Battery)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                            {
                                value = (float)sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public static float getRAMInfo(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Memory)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if(sensorType == SensorType.Load)
                            {
                                if (sensor.SensorType == sensorType && !sensor.Name.Contains(sensorName))
                                {
                                    value = sensor.Value.GetValueOrDefault();
                                }
                            }
                            {
                                if (!sensor.Name.Contains("Virtual"))
                                {
                                    if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                                    {
                                        value = sensor.Value.GetValueOrDefault();
                                    }
                                }
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }
    }
}

