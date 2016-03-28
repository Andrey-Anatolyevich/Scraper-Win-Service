using CoreElements;
using ParserCore.Factories;
using ParserCore.Utilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ParserCore
{
    // delegates for events
    public delegate void NewDatetime(DateTime dt);

    public delegate void NewDatetimeNumber(DateTime dt, int number);

    public delegate void NewUnshippedUnits(int number, int totalShipped);

    public delegate void NewAllDetails(IEnumerable<Detail> listOfUnits);

    public delegate void NewNoArguments();

    public delegate void NewErrorHandler(Exception ex);

    public class ParserProcess
    {
        // список найденных плагинов
        private List<IParserPlugin> LoadedPlugins;

        // dic of domain to plugin matches
        private Dictionary<string, IParserPlugin> DomainToPluginMatches;

        /// <summary>
        /// БД
        /// </summary>
        private IStorage DB;

        /// <summary>
        /// Current settings
        /// </summary>
        private SettGreatUnit MainSettings;

        /// <summary>
        /// EMail worker
        /// </summary>
        private Mailer MailWorker;

        /// <summary>
        /// DT of last request
        /// </summary>
        private DateTime LastRequest;

        // Delivers us HTML of URL
        private WebHelper WebHelp;

        // our Cancellation token
        private CancellationToken ParserCancelationToken;

        /// <summary>
        /// Flag that cancellation is requested from somewhere
        /// </summary>
        private bool StopIsRequested;

        // marker, that settings are to be updated
        private bool NeedToUpdateSettings;

        /// <summary>
        /// The factory for creation of new classes
        /// </summary>
        private ClassesFactory Factory { get { return this.FactoryLazy.Value; } }
        private Lazy<ClassesFactory> FactoryLazy = new Lazy<ClassesFactory>();

        /// <summary>
        /// </summary>
        /// <param name="cancelToken">    </param>
        /// <param name="pluginsLocation"></param>
        /// <exception cref="Exception">Throw exception if couldn't load settings.</exception>
        public ParserProcess(CancellationToken cancelToken, string pluginsLocation = "Plugins")
        {
            if (cancelToken == null)
                throw new ArgumentNullException("cancelToken");
            if (string.IsNullOrEmpty(pluginsLocation))
                throw new ArgumentException("pluginsLocation is null or empty");


            Utils util = this.Factory.GetUtils();
            string currentAssemblyDir = util.GetAssemblyDirectory();
            string pluginsFullPath = Path.Combine(currentAssemblyDir, pluginsLocation);
            string settingsFullPath = Path.Combine(currentAssemblyDir, Consts.Settings.SettingsFileName);

            // задали токен
            this.ParserCancelationToken = cancelToken;
            this.ParserCancelationToken.Register(() =>
            {
                FireNewCancelledByToken();
            });

            // init dic
            this.DomainToPluginMatches = new Dictionary<string, IParserPlugin>();
            // инициализация словарика
            this.LoadedPlugins = UniversalPluginLoader<IParserPlugin>.LoadPlugins(pluginsFullPath);

            // загрузили настройки из файла
            this.MainSettings = new SettGreatUnit(settingsFullPath);
            try
            {
                this.MainSettings.LoadSettings();
            }
            catch
            {
                this.MainSettings.SaveSettingsTest();
                this.FireNewSampleSettingsCreated();
                throw new Exception("Failed to load settings. Sample settings created.");
            }
            SettEmail mailSettings = this.MainSettings.EmailSettings;
            // Initialize Sender guy
            this.MailWorker = new Mailer(mailSettings.SmtpHostName, mailSettings.SmtpPort, mailSettings.EnableSSL,
                mailSettings.Login, mailSettings.Password, mailSettings.TargetEmail, mailSettings.EmailTitle);
            this.LastRequest = new DateTime();
            // DB
            this.DB = this.Factory.GetSqlFacade(this.Factory);
            this.WebHelp = this.Factory.GetWebHelper();
        }

        public void Launch()
        {
            try
            {
                while (!this.ParserCancelationToken.IsCancellationRequested && !this.StopIsRequested)
                {
                    // parsing & monitoring ops
                    IEnumerable<Section> validSectins = this.MainSettings.GetSectionsValidForRequest();

                    if (validSectins != null && validSectins.Any())
                        this.ProcessSections(validSectins);

                    this.ProcessMailing();

                    if (this.ParserCancelationToken.IsCancellationRequested
                        || this.StopIsRequested)
                        break;

                    // запустим ивент обновления списка детализированных юнитов
                    this.FireAllUnitsUpdate(DB.GetUnitsDetails(1, 30));
                    // Wait for some time get min time from enabled sections
                    DateTime sleepUntil = this.MainSettings.GetNextRequestMinDt();
                    this.FireNewIdleEvent(sleepUntil);

                    if (this.NeedToUpdateSettings)
                        this.UpdateSettings();

                    this.SleepUntil(sleepUntil);
                }
            }
            catch (Exception ex)
            {
                FireNewException(ex);
            }
        }

        /// <summary>
        /// Спать пока не наступит вермя..
        /// </summary>
        /// <param name="sleepUntil">Время, до которого будем спать</param>
        private void SleepUntil(DateTime sleepUntil)
        {
            while (DateTime.Now < sleepUntil
                        && !this.NeedToUpdateSettings
                        && !this.ParserCancelationToken.IsCancellationRequested
                        && !this.StopIsRequested)
            {
                Thread.Sleep(1500);
            }
        }

        /// <summary>
        /// Отправить письма если необходимо
        /// </summary>
        private void ProcessMailing()
        {
            // email ops
            StringBuilder emailMessage = new StringBuilder();
            int TotalInMail = 0;
            bool haveSomethingToSend = false;

            foreach (Section currentSection in this.MainSettings.GetEnabledSections())
            {
                List<Detail> forMailing = new List<Detail>();
                foreach (Interest CInter in currentSection.GetEnabledInterests())
                {
                    // получим неотправленные
                    forMailing.AddRange(this.DB.GetUnshipped(CInter.PriceMin,
                        CInter.PriceMax, currentSection.Url, CInter.ConstraintsList, CInter.KeyWords, CInter.OnlyWithPictures));
                }
                forMailing = forMailing.Distinct().ToList();
                TotalInMail += forMailing.Count;

                DateTime nextMailMinTime = currentSection.LastEmailDT + new TimeSpan(0, 0, currentSection.FrequencyEmailSeconds);

                if (forMailing.Count > 0
                    && DateTime.Now > nextMailMinTime)
                {
                    // compose message string
                    emailMessage.Append(MailHelper.ComposeEmailString(forMailing));
                    //Mail the message
                    foreach (Interest CInter in currentSection.GetEnabledInterests())
                    {
                        DB.SetShipped(CInter, currentSection.Url);
                        currentSection.LastEmailDT = DateTime.Now;
                    }
                    haveSomethingToSend = true;
                }
                // вывод неотправленного кол-ва элементов получим неотправленное кол-во
                List<Detail> unshipped = new List<Detail>();
                foreach (Interest currentInterest in currentSection.GetEnabledInterests())
                {
                    List<Detail> innerUnshipped = DB.GetUnshipped(currentInterest.PriceMin, currentInterest.PriceMax, currentSection.Url,
                        currentInterest.ConstraintsList, currentInterest.KeyWords, currentInterest.OnlyWithPictures).ToList();
                    unshipped.AddRange(innerUnshipped);
                    unshipped = unshipped.Distinct().ToList();
                }
                int Shipped = DB.GetShippedItems().Count;
                // запустим событие
                FireNewUnshipped(unshipped.Count, Shipped);
            }
            if (haveSomethingToSend)
            {
                this.MailWorker.SendString(emailMessage.ToString(), TotalInMail);
                // fire new e-mail event
                FireNewEMail(TotalInMail);
                haveSomethingToSend = false;
            }
        }

        /// <summary>
        /// Обработать секции
        /// </summary>
        /// <param name="validSectins"></param>
        private void ProcessSections(IEnumerable<Section> validSectins)
        {
            if (validSectins == null)
                throw new ArgumentNullException("validSectins");

            foreach (Section currentSection in validSectins)
            {
                if (this.ParserCancelationToken.IsCancellationRequested || this.StopIsRequested)
                    break;

                // получаем html
                string htmlWithListOfItems = "";
                // if we got good response
                this.LastRequest = DateTime.Now;
                this.FireNewHttpRequest();
                if (!WebHelp.GetHtmlFromUrl(currentSection.Url, out htmlWithListOfItems))
                    continue;

                currentSection.LastSectionUrlRequest = DateTime.Now;

                // получим список элементов на странице ссылок get current plugin
                IParserPlugin currentPlugin = this.GetPluginForUrl(currentSection.Url);

                if (currentPlugin != null)
                {
                    List<DetailShort> ListUnits = currentPlugin.GetLinksFromListPage(htmlWithListOfItems, currentSection.Url).ToList();
                    // Положим список в "базу"
                    this.DB.SaveUnitsList(ListUnits);
                    // Возьмём не запарсенный список
                    List<DetailShort> notParsed = new List<DetailShort>();
                    List<Interest> validInterests = currentSection.InterestList
                        .Where(x => x.IsEnabled == true)
                        .ToList();
                    foreach (var CurInter in validInterests)
                    {
                        notParsed.AddRange(
                            DB.GetUnparsedListUnits(CurInter.PriceMin, CurInter.PriceMax));
                    }
                    notParsed = notParsed.Distinct().ToList();
                    // перемешаем элементы
                    var rnd = new Random();
                    var result = notParsed.OrderBy(item => rnd.Next());
                    // скормим его парсеру объявлений
                    List<Detail> ParsedUnits = this.ProcessUnitsList(notParsed, this, ref LastRequest,
                        new TimeSpan(0, 0, 0, 0, currentSection.FrequencyWebRequestMSeconds), currentPlugin, ParserCancelationToken);
                    // Сохраним в "базу"
                    DB.SaveUnitsDetailsList(ParsedUnits);
                }
            }
        }

        /// <summary>
        /// Парсит страницы с деталями объявления
        /// </summary>
        private List<Detail> ProcessUnitsList(List<DetailShort> input, ParserProcess proc,
            ref DateTime lastRequest, TimeSpan requestFrequency, IParserPlugin plugin, CancellationToken token)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (proc == null)
                throw new ArgumentNullException("proc");
            if (plugin == null)
                throw new ArgumentNullException("plugin");


            List<Detail> result = new List<Detail>();
            WebHelper www = new WebHelper();
            foreach (DetailShort dShort in input)
            {
                DateTime waitTill = lastRequest.Add(requestFrequency);
                while (DateTime.Now < waitTill && !token.IsCancellationRequested)
                    Thread.Sleep(500);

                if (token.IsCancellationRequested)
                    break;

                lastRequest = DateTime.Now;

                Detail currentDetail = null;
                try
                {
                    // получим код страницы
                    string html = "";
                    proc.FireNewHttpRequest();
                    if (www.GetHtmlFromUrl(dShort.Url, out html))
                    {
                        // скормим его парсеру
                        currentDetail = plugin.GetUnitDetailsFromHtml(dShort, html);
                    }
                }
                catch (WebException ex)
                {
                    currentDetail = new Detail()
                    {
                        PublishDT = DateTime.Now,
                        WebID = dShort.WebID,
                        Url = dShort.Url,
                        UrlParent = dShort.UrlParent,
                        Title = ex.Message
                    };
                }

                if (currentDetail != null)
                    result.Add(currentDetail);
            }
            return result;
        }

        /// <summary>
        /// get plugin which can process url
        /// </summary>
        private IParserPlugin GetPluginForUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("url is null or empty");
            if (this.DomainToPluginMatches == null)
                throw new Exception("Dictionary with plugins is not initialized.");

            // get domain
            Uri parsed = null;
            Uri.TryCreate(url, UriKind.Absolute, out parsed);
            if (parsed == null)
                return null;

            string key = parsed.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            if (string.IsNullOrEmpty(key))
                return null;

            if (this.DomainToPluginMatches.ContainsKey(key))
                return this.DomainToPluginMatches[key];

            // not found in linked dic. need to check plugins
            foreach (IParserPlugin plugin in this.LoadedPlugins)
            {
                if (plugin.CanProcessDomain(url))
                {
                    this.DomainToPluginMatches.Add(key, plugin);
                    return plugin;
                }
            }
            return null;
        }

        #region Regarding events and FireAnEventMethods

        // событие нового запроса на сайт
        public event NewDatetime NewRequestEvent;

        // событие нового e-mail
        public event NewDatetimeNumber NewEMailEvent;

        // new delay event
        public event NewDatetime NewIdleEvent;

        // событие кол-ва неотправленных элементов
        public event NewUnshippedUnits NewUnshippedUnits;

        public event NewAllDetails NewAllUnitsUpdate;

        // парсер вновь запущен
        public event NewNoArguments NewResumed;

        // sample settings file created
        public event NewNoArguments NewSampleSettingsCreated;

        // парсер остановлен токеном
        public event NewNoArguments NewCancelledByToken;

        // настройки загружены
        public event NewNoArguments NewSettingsLoaded;

        // ивент ошибки в приложении
        public event NewErrorHandler NewException;

        internal void FireNewHttpRequest()
        {
            if (NewRequestEvent != null)
                NewRequestEvent(DateTime.Now);
        }

        private void FireNewEMail(int emailed)
        {
            if (NewEMailEvent != null)
                NewEMailEvent(DateTime.Now, emailed);
        }

        private void FireNewIdleEvent(DateTime SleepUntil)
        {
            if (NewIdleEvent != null)
            {
                NewIdleEvent(SleepUntil);
            }
        }

        private void FireAllUnitsUpdate(IEnumerable<Detail> intake)
        {
            if (intake == null)
                throw new ArgumentException("intake == null");

            if (NewAllUnitsUpdate != null)
                NewAllUnitsUpdate(intake);
        }

        private void FireNewUnshipped(int number, int shipped)
        {
            if (NewUnshippedUnits != null)
                NewUnshippedUnits(number, shipped);
        }

        private void FireNewPaused()
        {
            if (NewResumed != null)
            {
                NewResumed();
            }
        }

        private void FireNewCancelledByToken()
        {
            if (NewCancelledByToken != null)
            {
                NewCancelledByToken();
            }
        }

        private void FireNewSettingsLoaded()
        {
            if (NewSettingsLoaded != null)
            {
                NewSettingsLoaded();
            }
        }

        private void FireNewException(Exception ex)
        {
            if (NewException != null)
            {
                NewException(ex);
            }
        }

        private void FireNewSampleSettingsCreated()
        {
            if (NewSampleSettingsCreated != null)
                NewSampleSettingsCreated();
        }

        /// <summary>
        /// sets flag Indicating to update settings
        /// </summary>
        public void RequestUpdateSettings()
        {
            this.NeedToUpdateSettings = true;
        }

        /// <summary>
        /// processor of updating settings
        /// </summary>
        private void UpdateSettings()
        {
            this.MainSettings.LoadSettings();
            this.FireNewSettingsLoaded();
            // reset the trigger
            this.NeedToUpdateSettings = false;
        }

        /// <summary>
        /// Method to request stop the process
        /// </summary>
        public void StopParser()
        {
            this.StopIsRequested = true;
        }

        /// <summary>
        /// returns current settings to caller
        /// </summary>
        /// <returns></returns>
        public SettGreatUnit GetSettings()
        {
            return MainSettings;
        }

        public void SaveSettings(SettGreatUnit settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.MainSettings = settings;
            this.MainSettings.SaveSettings();
        }

        /// <summary>
        /// Get all titles of all parsed adds
        /// </summary>
        /// <returns></returns>
        public List<Detail> GetAllTitles(int from, int to)
        {
            if (from < 0)
                throw new ArgumentException("from < 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (to < from)
                throw new ArgumentException("to < from");


            return this.DB.GetUnitsDetails(from, to)
                .ToList();
        }

        /// <summary>
        /// Save email setting in storage
        /// </summary>
        public void AddMailSetting(SettEmail emailSetting)
        {
            if (emailSetting == null)
                throw new ArgumentNullException("emailSetting");


            emailSetting.SettingsAreValid();

            SaltMaker sm = this.Factory.GetSaltMaker();
            string salt = sm.GetNewSalt(150);
            string encryptedPass = Crypto.Encrypt(emailSetting.Password, salt);
            this.DB.AddMailSetting(emailSetting, encryptedPass, salt);
        }

        /// <summary>
        /// Get mail setting by it's id
        /// </summary>
        public SettEmail GetMailSettingByID(int id)
        {
            if (id <= 0)
                throw new ArgumentException("id <= 0");

            SettEmail result = this.DB.GetMailSettingByID(id);
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

            List<SettEmail> result = this.DB.GetMailSettings(from, to);
            return result;
        }

            #endregion Regarding events and FireAnEventMethods
        }
}