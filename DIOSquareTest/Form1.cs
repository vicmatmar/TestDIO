using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

using FTD2XX_NET;

using DIO;

namespace DIOSquareTest
{
    public partial class Form1 : Form
    {
        DIO.FT232HDIO _ft232hdio;
        DIO.FT232HDIO.DIO_BUS _ftdi_bus = FT232HDIO.DIO_BUS.AC_BUS;
        public string FTDI_BUS { get { return _ftdi_bus.ToString(); } }

        int _ftdi_dev_index = -1;

        public Form1()
        {
            InitializeComponent();

            _ft232hdio = new FT232HDIO();
            _ftdi_dev_index = _ft232hdio.GetFirstDevIndex();
            if (_ftdi_dev_index < 0)
            {
                throw new Exception("Unable to find an F232H device");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte data = 0x00;
            //byte[] buffer = new byte[]{0x82, data, 0xFF};
            //uint n = 0;

            _ft232hdio.Open((uint)_ftdi_dev_index);
            try
            {
                bool value = false;
                for (int i = 0; i < 10; i++)
                {
                    FTDI.FT_STATUS status = _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN1, value);
                    value = !value;

                    //FTDI.FT_STATUS status = _ft232hdio.Write(buffer, 3, ref n);
                    //buffer[1] ^= 0x01;

                    if (status != FTDI.FT_STATUS.FT_OK)
                    {
                        throw new Exception(string.Format("Bad status after write: {0}", status));
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception) { }
            finally
            {

                _ft232hdio.Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _ft232hdio.Open((uint)_ftdi_dev_index);
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    bool value = _ft232hdio.ReadPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0);
                    value = _ft232hdio.ReadPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0);
                    value = _ft232hdio.ReadPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0);
                }
            }
            finally
            {

                _ft232hdio.Close();
            }

        }
    }
}
