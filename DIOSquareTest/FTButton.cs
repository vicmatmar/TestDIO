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
    /// The button default state is enabled but as soon as a click is detected it gets disabled and the
    /// click event fires to avoid double clicks and triggering unwanted events.
    /// This class uses a while loop and thread.sleep to data pool the pin where the switch is connected to.
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

        public bool Enabled { get { return _enable; } set { _enable = value; } }

        bool _enable = false;
        int _sampling_ms = 150;

        Task _task_monitor;
        CancellationTokenSource _cancel_token = new CancellationTokenSource();

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

            Enabled = true;

            _task_monitor = new Task(() => monitor(_cancel_token.Token), _cancel_token.Token);
            //_task_monitor.ContinueWith(pretest_done_handler, TaskContinuationOptions.OnlyOnRanToCompletion);
            _task_monitor.ContinueWith(monitor_exception_handler, TaskContinuationOptions.OnlyOnFaulted);
            _task_monitor.Start();

            
        }

        /// <summary>
        /// Loop used to monitor pin state
        /// When pin is low the sampling is disabled and the click event fires
        /// </summary>
        void monitor(CancellationToken cancel)
        {
            while (true)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }

                if (Enabled)
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
                Thread.Sleep(_sampling_ms);
            }
        }

        void monitor_exception_handler(Task task)
        {
            var exception = task.Exception;
            string errmsg = exception.InnerException.Message;
            throw new Exception(errmsg);
        }


        /// <summary>
        /// Gets pin value
        /// </summary>
        /// <returns></returns>
        private bool get_button_state()
        {
            bool value = _dio.ReadPin(_bus, _pin);
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
