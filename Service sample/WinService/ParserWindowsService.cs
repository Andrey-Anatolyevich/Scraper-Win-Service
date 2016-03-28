using System.ServiceModel;
using System.ServiceProcess;

namespace Service
{
    /// <summary>
    /// Class for launching and stoping windows service
    /// </summary>
    public class ParserWindowsService : ServiceBase
    {
        #region LOCALS

        /// <summary>
        /// Type of service, this class will host
        /// </summary>
        private ServiceHost ServiceHost = null;

        #endregion LOCALS

        #region PROGRAM ENTRY POINT

        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main()
        {
            ServiceBase.Run(new ParserWindowsService());
        }

        #endregion PROGRAM ENTRY POINT

        public ParserWindowsService()
        {
            // Name the Windows Service
            this.ServiceName = Consts.WindowsServiceTitle;
        }

        #region WIN SERVICE ONSTART

        /// <summary>
        /// start windows service
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            if (this.ServiceHost != null)
                this.ServiceHost.Close();

            this.ServiceHost = new ServiceHost(typeof(ParserService));
            this.ServiceHost.Open();
        }

        #endregion WIN SERVICE ONSTART

        #region WIN SERVICE ONSTOP

        /// <summary>
        /// On stop of windows service
        /// </summary>
        protected override void OnStop()
        {
            if (ServiceHost != null)
            {
                ServiceHost.Close();
                ServiceHost = null;
            }
        }

        #endregion WIN SERVICE ONSTOP
    }
}