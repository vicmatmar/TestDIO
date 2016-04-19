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

        bool _toggle = false;

        public Form1()
        {
            InitializeComponent();

            _ft232hdio = new FT232HDIO();
            _ftdi_dev_index = _ft232hdio.GetFirstDevIndex();
            if (_ftdi_dev_index < 0)
            {
                throw new Exception("Unable to find an F232H device");
            }

            _ft232hdio.Open((uint)_ftdi_dev_index);

        }

        private void button1_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            for (int i = 0; i < 100; i++)
            {
                bool value = _ft232hdio.ReadPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0);
                this.radioButton1.Checked = value;
                Thread.Sleep(250);
            }
            this.button2.Enabled = true;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            _toggle = !_toggle;
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN2, _toggle);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ft232hdio.Close();

        }
    }
}
