using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using FTD2XX_NET;

namespace DIO
{
    public class FT232HDIO
    {
        FTDI _ftdi = new FTDI();

        /// <summary>
        /// Track the state of the output pins
        /// </summary>
        byte _cha_state = 0x00;
        byte _chb_state = 0x00;

        /// <summary>
        /// AD_BUS = D[0:7], AC_BUS = C[0:7]
        /// </summary>
        public enum DIO_BUS { AD_BUS, AC_BUS };
        /// <summary>
        /// The pin numbers
        /// </summary>
        public enum PIN { PIN0 = 0, PIN1 = 1, PIN2 = 2, PIN3 = 3, PIN4 = 4, PIN5 = 5, PIN6 = 6, PIN7 = 7 };

        FTD2XX_NET.FTDI.FT_DEVICE _dev_type;
        public FTD2XX_NET.FTDI.FT_DEVICE DeviceType { get { return _dev_type; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <see cref="http://www.ftdichip.com/Support/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf"/>
        /// <param name="dev_index"></param>
        /// <returns></returns>
        public FTDI.FT_STATUS ResetDevice()
        {
            FTDI.FT_STATUS status = _ftdi.ResetDevice();
            return status;
        }


        /// <summary>
        /// Open the FTDI device
        /// </summary>
        /// <param name="index"></param>
        public void Open(uint index)
        {
            if (_ftdi.IsOpen)
            {
                //_ftdi.Close();
                throw new Exception(string.Format("Device at index {0} already opened", index));
            }

            FTDI.FT_STATUS status = _ftdi.OpenByIndex(index);
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new Exception(string.Format("Problem opening FTDI device at index {0}", index));

            // Reset pins
            status = _ftdi.SetBitMode(0xFF, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new Exception(string.Format("Unable to set bit mode reset to device at index {0}", index));
            status = _ftdi.SetBitMode(0xFF, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new Exception(string.Format("Unable to set bit mode MPSEE to device at index {0}", index));
        }

        /// <summary>
        /// Returns the index of the first available FTDI 232H device found in the system
        /// </summary>
        /// <returns></returns>
        public int GetFirstDevIndex()
        {
            int count = 10;

            FTDI.FT_DEVICE_INFO_NODE[] devlist = new FTDI.FT_DEVICE_INFO_NODE[count];
            FTDI.FT_STATUS status = _ftdi.GetDeviceList(devlist);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK, "Problem getting FTDI device list");

            int index = -1;
            for (int i = 0; i < count; i++)
            {
                FTDI.FT_DEVICE_INFO_NODE devinfo = devlist[i];
                if (devinfo != null)
                {
                    if (devinfo.Type == FTD2XX_NET.FTDI.FT_DEVICE.FT_DEVICE_232H)
                    {
                        index = i;
                        _dev_type = devinfo.Type;
                        break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Gets the address for each "write" data bus
        /// </summary>
        /// <param name="bus"></param>
        /// <returns></returns>
        byte get_bus_write_address(DIO_BUS bus)
        {
            byte addr = 0x80;
            if (bus == DIO_BUS.AC_BUS)
                addr = 0x82;
            return addr;
        }

        /// <summary>
        /// Gets the address for each "read" data bus
        /// </summary>
        /// <param name="bus"></param>
        /// <returns></returns>
        byte get_bus_read_address(DIO_BUS bus)
        {
            byte addr = 0x81;
            if (bus == DIO_BUS.AC_BUS)
                addr = 0x83;
            return addr;
        }

        /// <summary>
        /// Gets the stored state of a the pins for a particular bus
        /// </summary>
        /// <param name="bus"></param>
        /// <returns></returns>
        byte get_stored_state(DIO_BUS bus)
        {
            byte state = _cha_state;
            if (bus == DIO_BUS.AC_BUS)
                state = _chb_state;
            return state;
        }

        /// <summary>
        /// Set the bus pins and stores the state
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        FTDI.FT_STATUS set_state(DIO_BUS bus, byte state)
        {
            if (bus == DIO_BUS.AD_BUS)
                _cha_state = state;
            else if (bus == DIO_BUS.AC_BUS)
                _chb_state = state;

            byte addr = get_bus_write_address(bus);
            byte[] buffer = new byte[] { addr, state, 0xFF };
            uint n = 0;
            FTDI.FT_STATUS status = _ftdi.Write(buffer, buffer.Length, ref n);

            return status;

        }

        /// <summary>
        /// Low lever Write wrapper
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="numBytes"></param>
        /// <param name="numBytesWritten"></param>
        /// <returns></returns>
        public FTDI.FT_STATUS Write(byte[] dataBuffer, int numBytes, ref uint numBytesWritten)
        {
            FTDI.FT_STATUS status = _ftdi.Write(dataBuffer, numBytes, ref numBytesWritten);
            return status;
        }

        /// <summary>
        /// Set the state of the pin
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FTDI.FT_STATUS SetPin(DIO_BUS bus, uint pin, bool value)
        {
            Debug.Assert(pin < 8 && pin >= 0, "Pin number must be between 0 and 7");

            byte pin_num = Convert.ToByte(pin);

            byte state_current = get_stored_state(bus);
            //byte state_current2 = GetBusData(bus);
            //if ((state_current & 0x0F) != (state_current2 & 0x0F))
            //{
            //    // If we never see this it means we can get rid of get_bus_state and
            //    // keeping track of the state
            //    throw new Exception("Unexpected FT232DIO bus state.  Was it close and re-opened?");
            //}

            byte state_new = state_current;
            if (value)
            {
                state_new |= (byte)(1 << pin_num);
            }
            else
            {
                state_new &= (byte)(~(1 << pin_num) & 0xFF);
            }

            FTDI.FT_STATUS status = set_state(bus, state_new);

            return status;
        }

        /// <summary>
        /// Returns the stored (may not be actual) state of the pin
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool GetPin(DIO_BUS bus, uint pin)
        {
            byte data = get_stored_state(bus);
            byte pin_num = Convert.ToByte(pin);
            int b = (byte)(1 << pin_num);
            bool value = Convert.ToBoolean((data & b));

            return value;

        }

        public bool ReadPin(DIO_BUS bus, PIN pin)
        {
            byte pin_num = Convert.ToByte(pin);
            bool value = ReadPin(bus, pin_num);
            return value;
        }

        /// <summary>
        /// Reads the state of the pin
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool ReadPin(DIO_BUS bus, uint pin)
        {
            byte data = ReadBus(bus);
            byte pin_num = Convert.ToByte(pin);
            int b = (byte)(1 << pin_num);
            bool value = Convert.ToBoolean((data & b));

            return value;
        }

        /// <summary>
        /// Reads one byte from the specified bus
        /// </summary>
        /// <param name="bus"></param>
        /// <returns></returns>
        public byte ReadBus(DIO_BUS bus)
        {
            byte addr = get_bus_read_address(bus);
            byte[] data = new byte[] { addr };
            uint n = 0;
            FTDI.FT_STATUS status = _ftdi.Write(data, data.Length, ref n);
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new Exception(string.Format("Problem writing read command to bus {0}", bus.ToString()));

            status = _ftdi.Read(data, (uint)data.Length, ref n);
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new Exception(string.Format("Problem writing read command to bus {0}", bus.ToString()));

            return data[0];
        }

        /// <summary>
        /// Set the state of the pin
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FTDI.FT_STATUS SetPin(DIO_BUS bus, PIN pin, bool value)
        {
            byte pin_num = Convert.ToByte(pin);
            FTDI.FT_STATUS status = SetPin(bus, pin_num, value);
            return status;
        }

        /// <summary>
        /// Reset the port
        /// </summary>
        /// <returns></returns>
        public FTDI.FT_STATUS ResetPort()
        {
            FTDI.FT_STATUS status = _ftdi.ResetPort();
            return status;
        }

        /// <summary>
        /// Close the port
        /// </summary>
        /// <returns></returns>
        public FTDI.FT_STATUS Close()
        {
            FTDI.FT_STATUS status = _ftdi.Close();
            return status;
        }

        /// <summary>
        /// Close the port
        /// </summary>
        /// <returns></returns>
        public FTDI.FT_STATUS Rescan()
        {
            FTDI.FT_STATUS status = _ftdi.Rescan();
            return status;
        }
    }
}
