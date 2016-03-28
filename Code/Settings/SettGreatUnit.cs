using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ParserCore
{
    /// <summary>
    /// Main settings unit
    /// </summary>
    public class SettGreatUnit
    {
        private string SettingsFileName;

        public SettEmail EmailSettings { get; set; }

        public List<Section> SectionsList { get; set; }

        /// <summary>
        /// Constructor for deserializer
        /// </summary>
        public SettGreatUnit()
        {
            this.EmailSettings = new SettEmail();
            this.SectionsList = new List<Section>();
        }

        public SettGreatUnit(string settingFileName)
        {
            if (string.IsNullOrWhiteSpace(settingFileName))
                throw new ArgumentNullException("settingFileName");

            this.SettingsFileName = settingFileName;

            this.EmailSettings = new SettEmail();
            this.SectionsList = new List<Section>();
        }

        /// <summary>
        /// Save settings on disk
        /// </summary>
        public void SaveSettings()
        {
            this.FixSettings();

            // encrypt password
            if (this.EmailSettings != null && !string.IsNullOrWhiteSpace(this.EmailSettings.Password))
            {
                this.EmailSettings.Password = Crypto.Encrypt(this.EmailSettings.Password, Consts.Settings.CryptoKey);
            }

            XmlSerializer Ser = new XmlSerializer(typeof(SettGreatUnit));
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(SettingsFileName);
                Ser.Serialize(writer, this);
                writer.Close();
            }
            finally
            {
                if (writer != null)
                    writer.Dispose();
            }
        }

        /// <summary>
        /// Fix some data in sections or throw exception
        /// </summary>
        private void FixSettings()
        {
            // check if exists setting where sleep delay is not set
            IEnumerable<Section> badSections = this.SectionsList
                .Where(x => x.SleepDelaySeconds == 0);
            string errorMessage = string.Empty;
            if (badSections.Any())
            {
                foreach (Section s in badSections)
                {
                    s.SleepDelaySeconds = Consts.Settings.SleepDelaySecondsDefault;
                }
            }
            // fix empty keys and constraints
            foreach (Section sect in SectionsList)
            {
                foreach (Interest inter in sect.InterestList)
                {
                    inter.KeyWords = inter.KeyWords
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                    inter.ConstraintsList = inter.ConstraintsList
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Get sett section by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Section GetSectionByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Section result = null;
            IEnumerable<Section> secs = this.SectionsList
                .Where(x => x.Name == name);
            if (secs.Any())
                result = secs.First();
            return result;
        }

        /// <summary>
        /// Get list of names of all sections
        /// </summary>
        /// <returns></returns>
        public List<string> GetSectinsNames()
        {
            List<string> result = new List<string>();
            if (this.SectionsList == null)
                this.SectionsList = new List<Section>();

            result = this.SectionsList.Select(x => x.Name).ToList();
            return result;
        }

        /// <summary>
        /// save sample settings into file
        /// </summary>
        public void SaveSettingsTest()
        {
            EmailSettings.EmailTitle = "Some title for emails";
            EmailSettings.EnableSSL = true;
            EmailSettings.Login = "your login for and e-mail you want to use for sending mail";
            EmailSettings.Password = "password";
            EmailSettings.SmtpHostName = "gmtp.gmail.com";
            EmailSettings.SmtpPort = 587;
            EmailSettings.TargetEmail = "And email you want to send results to";
            EmailSettings.TargetEmailFrom = "Your login & password email address";

            Section Section = new Section();
            Section.Name = "Test name";
            Section.FrequencyEmailSeconds = 900;
            Section.FrequencyWebRequestMSeconds = 2000;
            Section.SleepDelaySeconds = 300;
            Section.Url = "https://www.avito.ru/moskva/noutbuki?user=1";
            Section.IsEnabled = true;
            Section.InterestList.Add(new Interest("sample")
            {
                PriceMin = 20000,
                PriceMax = 45000,
                OnlyWithPictures = true,
                IsEnabled = true,
                ConstraintsList = new List<string>() {
                    "apple",
                    "macbook"
                },
                KeyWords = new List<string>() {
                    "words your require to be in add",
                    "one",
                    "per",
                    "line"
                }
            });
            SectionsList.Add(Section);
            SaveSettings();
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public void LoadSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SettGreatUnit));
            FileStream fStream = new FileStream(this.SettingsFileName, FileMode.Open);
            SettGreatUnit result = (SettGreatUnit)serializer.Deserialize(fStream);
            // Вот тут и восстанавливаем настройки
            this.EmailSettings = result.EmailSettings;
            this.SectionsList = result.SectionsList;
            FixSettings();

            // decrypt password
            if (this.EmailSettings != null
                && !string.IsNullOrWhiteSpace(this.EmailSettings.Password))
            {
                this.EmailSettings.Password = Crypto.Decrypt(this.EmailSettings.Password, Consts.Settings.CryptoKey);
            }
        }

        /// <summary>
        /// save section by it's name
        /// </summary>
        /// <param name="changed"></param>
        public void SaveSection(Section changed)
        {
            if (changed == null)
                throw new ArgumentNullException("changed");
            if (string.IsNullOrEmpty(changed.Name))
                throw new ArgumentException("changed.Name == null or empty");

            IEnumerable<Section> sections = this.SectionsList
                .Where(x => x.Name == changed.Name);

            if (sections == null || !sections.Any())
                throw new Exception(string.Format("Section with name [{0}] is not found in list of sections", changed.Name));

            Section found = sections.First();
            found.FrequencyEmailSeconds = changed.FrequencyEmailSeconds;
            found.FrequencyWebRequestMSeconds = changed.FrequencyWebRequestMSeconds;
            found.InterestList = changed.InterestList;
            found.Url = changed.Url;
            found.IsEnabled = changed.IsEnabled;
        }

        /// <summary>
        /// get enabled sections
        /// </summary>
        /// <returns></returns>
        public List<Section> GetEnabledSections()
        {
            if (this.SectionsList == null || !this.SectionsList.Any())
                return new List<Section>();

            return this.SectionsList.Where(x => x.IsEnabled == true).ToList();
        }

        /// <summary>
        /// get sections which are valid for request
        /// </summary>
        /// <returns></returns>
        public List<Section> GetSectionsValidForRequest()
        {
            if (this.SectionsList == null || !this.SectionsList.Any())
                return new List<Section>();

            List<Section> result = this.SectionsList
                .Where(x => x.IsEnabled == true)
                .Where(x => x.LastSectionUrlRequest.Add(TimeSpan.FromSeconds(x.SleepDelaySeconds)) < DateTime.Now)
                .ToList();

            return result;
        }

        /// <summary>
        /// Добавить новую секцию
        /// </summary>
        /// <param name="name"></param>
        public void AddNewSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name is null or whitespace");

            if (this.SectionsList == null)
                this.SectionsList = new List<Section>();

            var matches = this.SectionsList.Where(x => x.Name == name);
            if (!matches.Any())
            {
                this.SectionsList.Add(new Section(name));
            }
        }

        /// <summary>
        /// Delete section by name
        /// </summary>
        /// <param name="name"></param>
        public void DeleteSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name is null or whitespace");

            if (this.SectionsList == null)
                this.SectionsList = new List<Section>();

            var matches = this.SectionsList.Where(x => x.Name == name);
            if (!matches.Any())
                return;

            foreach (Section match in matches)
            {
                this.SectionsList.Remove(match);
            }
        }

        /// <summary>
        /// Get next dt any enabled section is valid for request
        /// </summary>
        /// <returns></returns>
        public DateTime GetNextRequestMinDt()
        {
            DateTime result = DateTime.Now;
            // getenabled sections
            IEnumerable<Section> enabledSecs = this.SectionsList
                .Where(x => x.IsEnabled == true);
            if (enabledSecs != null && enabledSecs.Any())
            {
                result = enabledSecs
                    .Select(x => x.LastSectionUrlRequest.Add(TimeSpan.FromSeconds(x.SleepDelaySeconds)))
                    .Min();
            }

            return result;
        }
    }
}