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

using FTDIO_Object;

namespace DIOSquareTest
{
    public partial class Form1 : Form
    {
        DIO.FT232HDIO _ft232hdio;
        DIO.FT232HDIO.DIO_BUS _ftdi_bus = FT232HDIO.DIO_BUS.AC_BUS;
        public string FTDI_BUS { get { return _ftdi_bus.ToString(); } }

        delegate void setCallback();

        int _ftdi_dev_index = -1;

        bool _toggle = false;

        FTButton ftbutton;

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
            ftbutton = new FTButton(_ft232hdio, FT232HDIO.DIO_BUS.AD_BUS, FT232HDIO.PIN.PIN0);
            ftbutton.Click_Event += ftbutton_Click_Event;
            updategui();

        }

        void ftbutton_Click_Event(object sender)
        {
            updategui();

            Thread.Sleep(5000);
            ftbutton.Enabled = true;

            updategui();
        }

        void updategui()
        {
            if (InvokeRequired)
            {
                setCallback d = new setCallback(updategui);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (ftbutton.Enabled)
                {
                    label1.Text = "Watting on click.\r\nButton is enabled.\r\nLED is on";

                    Random ran = new Random();
                    bool pass_fail = Convert.ToBoolean(ran.Next(2));
                    turn_leds_pass_fail(pass_fail);
                    radioButton1.Checked = true;
                }
                else
                {
                    label1.Text = "Something running.\r\nButton is disabled\r\nLED is off";

                    turn_leds(false);
                    radioButton1.Checked = false;
                }
            }
        }

        void turn_leds_pass_fail(bool pass_fail)
        {
            if (pass_fail)
            {
                _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0, true);
                _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN2, true);
            }
            else
            {
                _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN1, true);
                _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN3, true);
            }
        }

        void turn_leds(bool state)
        {
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0, state);
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN1, state);
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN2, state);
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN3, state);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool value = false;
            for (int i = 0; i < 10; i++)
            {
                FTDI.FT_STATUS status = _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN1, value);
                value = !value;
                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    throw new Exception(string.Format("Bad status after write: {0}", status));
                }
                Thread.Sleep(10);
            }

        }


        private void toggleLED_Click(object sender, EventArgs e)
        {
            _toggle = !_toggle;
            _ft232hdio.SetPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN1, _toggle);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ft232hdio.Close();

        }
    }
}
