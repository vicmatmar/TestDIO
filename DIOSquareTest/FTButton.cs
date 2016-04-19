using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using FTD2XX_NET;
using DIO;

namespace FTDIO_Object
{

    /// <summary>
    /// Class use to monitor a hardware push button attached to FT232H board.
    /// It is assume a pull-up resistor is at the FT232H pin and the switch connects it to ground.
    /// The button default state is enabled but as soon as a click is detected it is disabled and the
    /// click event fired.  this is to avoid double clicks and triggering unwanted events.  T
    /// </summary>
    class FTButton
    {
        FT232HDIO _dio;
        FT232HDIO.DIO_BUS _bus = FT232HDIO.DIO_BUS.AC_BUS;
        FT232HDIO.PIN _pin = FT232HDIO.PIN.PIN0;

        public FT232HDIO DIO { get { return _dio; } }
        public FT232HDIO.DIO_BUS BUS { get { return _bus; } }
        public FT232HDIO.PIN PIN { get { return _pin; } }

        public delegate void ClickHandler(object sender);
        public event ClickHandler Click_Event;

        public bool Enabled { get { return _timer.Enabled; } set { this._timer.Enabled = value; } }

        // Timer use to sample the state of the pin attached to the button
        System.Timers.Timer _timer; 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dio"></param>
        /// <param name="bus"></param>
        /// <param name="pin"></param>
        public FTButton(FT232HDIO dio, FT232HDIO.DIO_BUS bus, FT232HDIO.PIN pin)
        {
            _dio = dio;
            _bus = bus;
            _pin = pin;

            _timer = new System.Timers.Timer(200.00);  // So the button most be held for 200ms max to detect a click
            _timer.Elapsed += _timer_Elapsed;
            Enabled = true;
        }

        /// <summary>
        /// Timer event used to monitor pin state
        /// When pin is low the timer is disabled and the click event fires
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool value = get_button_state();
            // Assume state is always high (pull up resistor) and
            // button press sets it low (GND)
            if (!value)
            {
                Enabled = false;
                fire_click();
            }
        }

        /// <summary>
        /// Gets pin value
        /// </summary>
        /// <returns></returns>
        private bool get_button_state()
        {
            bool value = _dio.ReadPin(FT232HDIO.DIO_BUS.AC_BUS, FT232HDIO.PIN.PIN0);
            return value;
        }

        /// <summary>
        /// Fire the event
        /// </summary>
        void fire_click()
        {
            if (Click_Event != null)
            {
                Click_Event(this);
            }
        }

    }
}
