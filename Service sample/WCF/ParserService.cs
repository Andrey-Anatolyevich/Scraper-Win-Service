using CoreElements;
using ParserCore;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Service
{
    /// <summary>
    /// Realisation of ParserService
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true
        , InstanceContextMode = InstanceContextMode.Single)]
    public class ParserService : IParserService, IDisposable
    {
        private ParserFacade FacadeForParser;

        /// <summary>
        /// Implementation of IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Construstor
        /// </summary>
        public ParserService()
        {
            this.FacadeForParser = new ParserFacade();
        }

        public List<Detail> GetAllTitles(int from, int to)
        {
            if (from < 0)
                throw new ArgumentException("from < 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (to < from)
                throw new ArgumentException("to < from");

            return this.FacadeForParser.GetAllTitles(from, to);
        }

        public SettGreatUnit GetSettings()
        {
            return this.FacadeForParser.GetSettings();
        }

        public bool ParserIsRunning()
        {
            if (this.FacadeForParser == null)
                return false;

            return this.FacadeForParser.ParserIsRunning();
        }

        public void SaveSettings(SettGreatUnit settings)
        {
            this.FacadeForParser.SaveSettings(settings);
        }

        public void Start()
        {
            if (FacadeForParser == null)
                this.FacadeForParser = new ParserFacade();

            this.FacadeForParser.Start();
        }

        public void Stop()
        {
            if (this.FacadeForParser != null)
                this.FacadeForParser.Stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.FacadeForParser != null)
                {
                    // free managed resources
                    if (this.FacadeForParser != null)
                    {
                        this.FacadeForParser.Dispose();
                        this.FacadeForParser = null;
                    }
                }
            }
        }

        /// <summary>
        /// Add new mail setting
        /// </summary>
        /// <param name="setting"></param>
        public void AddMailSetting(SettEmail setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");


            setting.SettingsAreValid();
            this.FacadeForParser.AddMailSetting(setting);
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

            SettEmail result = this.FacadeForParser.GetMailSettingByID(id);
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

            List<SettEmail> result = this.FacadeForParser.GetMailSettings(from, to);
            return result;
        }
    }
}