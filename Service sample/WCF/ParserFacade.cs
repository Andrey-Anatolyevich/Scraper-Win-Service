using CoreElements;
using ParserCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class ParserFacade : IDisposable
    {
        /// <summary>
        /// Instance of the parser
        /// </summary>
        private ParserService ParserWCFService;

        /// <summary>
        /// Cancellation token source
        /// </summary>
        private CancellationTokenSource CancelTokenSource;

        /// <summary>
        /// Parser process
        /// </summary>
        private ParserProcess ParserCoreProcess;

        /// <summary>
        /// task where parser core process runs
        /// </summary>
        private Task ParserTask;

        /// <summary>
        /// launch parser
        /// </summary>
        internal void Start()
        {
            // init parser process
            CancellationToken theToken = this.GetNewCancelToken();
            this.ParserCoreProcess = new ParserProcess(theToken);

            this.ParserWCFService = new ParserService();

            this.ParserTask = new Task(this.ParserCoreProcess.Launch);
            this.ParserTask.Start();
        }

        /// <summary>
        /// returns all the titles from parsed adds
        /// </summary>
        /// <returns></returns>
        internal List<Detail> GetAllTitles(int from, int to)
        {
            if (from < 0)
                throw new ArgumentException("from < 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (to < from)
                throw new ArgumentException("to < from");

            return this.ParserCoreProcess.GetAllTitles(from, to);
        }

        internal bool ParserIsRunning()
        {
            if (this.ParserTask == null)
                return false;

            return this.ParserTask.Status == TaskStatus.Running;
        }

        /// <summary>
        /// stop parser
        /// </summary>
        internal void Stop()
        {
            if (this.ParserTask == null)
                return;

            this.CancelTokenSource.Cancel();

            // wait for 10 serconds untill the process is done
            DateTime maxWaitUntill = DateTime.Now.AddSeconds(10);
            while (!ParserTask.IsCompleted && DateTime.Now < maxWaitUntill)
            {
                Thread.Sleep(500);
            }

            // task should be finished.
        }

        /// <summary>
        /// Save settings
        /// </summary>
        /// <param name="settings"></param>
        internal void SaveSettings(SettGreatUnit settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.ParserCoreProcess.SaveSettings(settings);
        }

        public SettGreatUnit GetSettings()
        {
            return this.ParserCoreProcess.GetSettings();
        }

        #region HELP METHODS

        /// <summary>
        /// Gets new cancellationToken
        /// </summary>
        /// <returns></returns>
        private CancellationToken GetNewCancelToken()
        {
            //создали токен
            this.CancelTokenSource = new CancellationTokenSource();
            return this.CancelTokenSource.Token;
        }

        /// <summary>
        /// Implementation of IDesposable
        /// </summary>
        public void Dispose()
        {
            if (this.ParserTask != null)
                this.ParserTask.Dispose();

            if (this.CancelTokenSource != null)
                this.CancelTokenSource.Dispose();
        }

        /// <summary>
        /// Save new mail settings
        /// </summary>
        /// <param name="setting"></param>
        public void AddMailSetting(SettEmail setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");
            setting.SettingsAreValid();

            this.ParserCoreProcess.AddMailSetting(setting);
        }

        /// <summary>
        /// Get email setting by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SettEmail GetMailSettingByID(int id)
        {
            if (id <= 0)
                throw new ArgumentException("id <= 0");

            SettEmail result = this.ParserCoreProcess.GetMailSettingByID(id);
            result.SettingsAreValid();
            return result;
        }


        /// <summary>
        /// Get mail settings from and to
        /// </summary>
        public List<SettEmail> GetMailSettings(int from, int to)
        {
            if (from <= 0)
                throw new ArgumentException("from <= 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (from > to)
                throw new ArgumentException("from > to");

            List<SettEmail> result = this.ParserCoreProcess.GetMailSettings(from, to);
            return result;
        }

        #endregion HELP METHODS
    }
}