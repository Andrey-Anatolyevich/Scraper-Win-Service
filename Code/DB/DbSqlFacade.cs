using CoreElements;
using ParserCore.DB.Sql;
using ParserCore.Factories;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace ParserCore
{
    /// <summary>
    /// Implementation for sql db
    /// </summary>
    internal class DbSqlFacade : IStorage
    {
        private ParserSqlDataContext GetNewParserContext()
        {
            return this.Factory.GetDbContext();
        }

        /// <summary>
        /// The factory for new classes
        /// </summary>
        private ClassesFactory Factory;

        public DbSqlFacade(ClassesFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            this.Factory = factory;
        }

        public List<Detail> GetUnitsDetails(int from, int to)
        {
            if (from < 0)
                throw new ArgumentException("from < 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (to < from)
                throw new ArgumentException("to < from");


            int toTake = to - from;
            List<Detail> result = new List<Detail>();

            ParserSqlDataContext context = this.GetNewParserContext();
            List<Add> takenAdds = context
                .Adds
                .Where(x => x.Title.Length > 0)
                .Skip(from)
                .Take(toTake)
                .ToList();

            foreach (Add add in takenAdds)
            {
                Detail nextDetail = this.GetUnitDetailExtendedFromAdd(add);
                result.Add(nextDetail);
            }

            return result;
        }

        public List<Detail> GetShippedItems()
        {
            List<Detail> result = new List<Detail>();

            List<Add> mailedAdds = this.GetNewParserContext().Adds.Where(x => x.IsMailed == true).ToList();
            foreach (Add add in mailedAdds)
            {
                Detail nextDetail = this.GetUnitDetailExtendedFromAdd(add);

                result.Add(nextDetail);
            }

            return result;
        }

        public List<DetailShort> GetUnparsedListUnits(int priceMin, int priceMax)
        {
            List<DetailShort> result = new List<DetailShort>();

            List<Add> unparsed = this.GetNewParserContext().Adds
                .Where(x =>
                    x.IsProcessed != true
                    && x.Price >= priceMin
                    && x.Price <= priceMax)
                .ToList();

            foreach (Add add in unparsed)
                result.Add(this.Factory.GetDetailShort(add));

            return result;
        }

        public List<Detail> GetUnshipped(int priceMin, int priceMax, string urlOriginal, List<string> constraints, List<string> keyWords, bool onlyWithPictures)
        {
            ParserSqlDataContext context = this.GetNewParserContext();

            ISingleResult<GetFilteredUnshippedAddsResult> procResponse = context.GetFilteredUnshippedAdds(priceMin, priceMax
                , urlOriginal, string.Join(",", constraints), string.Join(",", keyWords), onlyWithPictures);

            List<Detail> result = new List<Detail>();
            foreach (GetFilteredUnshippedAddsResult unit in procResponse)
            {
                Detail item = new Detail();
                item.WebID = unit.AddID;
                item.Content = unit.Text;
                item.Price = unit.Price == null ? 0 : (int)unit.Price;
                item.PublishDT = unit.InsertDt == null ? DateTime.MinValue : (DateTime)unit.InsertDt;
                item.Title = unit.Title;
                item.Url = unit.Url;
                item.UrlParent = unit.UrlParent;
                item.PictureUrls = new List<string>();
                IEnumerable<Image> possiblePictures = context.Images.Where(x => x.AddID == unit.ID);
                foreach (Image img in possiblePictures)
                    item.PictureUrls.Add(img.Url);

                result.Add(item);
            }
            return result;
        }

        public void SaveUnitsDetailsList(List<Detail> input)
        {
            if (input == null)
                throw new ArgumentException("input == null");

            ParserSqlDataContext context = this.GetNewParserContext();

            List<Add> addsToInsert = new List<Add>();
            foreach (Detail unit in input)
            {
                // take existing one
                IEnumerable<Add> possibles = context.Adds.Where(x => x.Url == unit.Url);

                if (!possibles.Any())
                {
                    // insert new one
                    Add newAdd = new Add();
                    newAdd.AddID = unit.WebID;
                    newAdd.IsProcessed = true;
                    newAdd.Price = unit.Price;
                    newAdd.Text = unit.Content;
                    newAdd.Title = unit.Title;
                    newAdd.InsertDt = DateTime.Now;
                    newAdd.UpdateDt = DateTime.Now;
                    newAdd.Url = unit.Url;
                    newAdd.UrlParent = unit.UrlParent;

                    context.Adds.InsertOnSubmit(newAdd);
                }
                else
                {
                    // take first and change it
                    Add found = possibles.First();
                    found.IsProcessed = true;
                    found.Price = unit.Price;
                    found.Text = unit.Content;
                    found.Title = unit.Title;
                    found.UpdateDt = DateTime.Now;
                }

                context.SubmitChanges();

                IEnumerable<Add> unitForImagesID = context.Adds.Where(x => x.Url == unit.Url);
                if (!unitForImagesID.Any())
                    throw new Exception("After found unit is saved, there is no such unit in the db");

                Add ourAdd = unitForImagesID.First();

                // save pictures
                foreach (string s in unit.PictureUrls)
                {
                    // try get pictures
                    IEnumerable<Image> foundImages = context.Images.Where(x => x.AddID == ourAdd.ID && x.Url == s);
                    if (!foundImages.Any())
                    {
                        Image imageToInsert = new Image()
                        {
                            AddID = ourAdd.ID
                            ,
                            Url = s
                            ,
                            InsertDt = DateTime.Now
                            ,
                            UpdateDt = DateTime.Now
                        };

                        context.Images.InsertOnSubmit(imageToInsert);
                    }
                }
                context.SubmitChanges();
            }
        }

        public void SaveUnitsList(List<DetailShort> input)
        {
            if (input == null)
                throw new ArgumentException("input == null");


            ParserSqlDataContext context = this.GetNewParserContext();

            List<Add> addsToInsert = new List<Add>();
            foreach (DetailShort unit in input)
                addsToInsert.Add(this.Factory.GetAdd(unit));

            // keep only new ones
            addsToInsert = addsToInsert.Where(f => !context.Adds.Where(x => x.Url == f.Url).Any())
                .ToList();

            ParserSqlDataContext currentContext = context;
            currentContext.Adds.InsertAllOnSubmit(addsToInsert);
            currentContext.SubmitChanges();
        }

        public void SetShipped(Interest intake, string urlOriginal)
        {
            ParserSqlDataContext context = this.GetNewParserContext();

            List<Detail> Result = this.GetUnshipped(intake.PriceMin,
                    intake.PriceMax, urlOriginal, intake.ConstraintsList, intake.KeyWords, intake.OnlyWithPictures);

            foreach (Detail detail in Result)
            {
                context.Adds
                    .Where(x => x.AddID == detail.WebID)
                    .ToList()
                    .ForEach(n => n.IsMailed = true);
            }
            context.SubmitChanges();
        }

        private Detail GetUnitDetailExtendedFromAdd(Add add)
        {
            if (add == null)
                throw new ArgumentException("add == null");

            Detail result = new Detail();
            result.WebID = add.AddID;
            result.Content = add.Text;
            result.IsMailed = add.IsMailed != null ? (bool)add.IsMailed : false;
            result.Price = add.Price != null ? (double)add.Price : 0D;
            result.PublishDT = add.InsertDt != DateTime.MinValue ? (DateTime)add.InsertDt : DateTime.MinValue;
            result.Title = add.Title;
            result.Url = add.Url;
            result.UrlParent = add.UrlParent;

            IEnumerable<Image> possiblePictures = this.GetNewParserContext().Images.Where(x => x.AddID == add.ID);
            foreach (Image img in possiblePictures)
                result.PictureUrls.Add(img.Url);

            return result;
        }

        /// <summary>
        /// Save mail setting into the DB
        /// </summary>
        public void AddMailSetting(SettEmail emailSetting, string encryptedPass, string salt)
        {
            if (emailSetting == null)
                throw new ArgumentNullException("emailSetting");
            if (string.IsNullOrEmpty(encryptedPass))
                throw new ArgumentException("encryptedPass is null or empty");
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("salt is null or empty");

            this.GetNewParserContext().AddEmailSetting(emailSetting.SmtpHostName, emailSetting.SmtpPort, emailSetting.EnableSSL
                , emailSetting.Login, encryptedPass, salt, emailSetting.TargetEmailFrom, emailSetting.TargetEmail, emailSetting.EmailTitle);
        }

        /// <summary>
        /// Get email setting by it's id
        /// </summary>
        public SettEmail GetMailSettingByID(int id)
        {
            if (id <= 0)
                throw new ArgumentException("id <= 0");


            ISingleResult<GetMailSettingByIDResult> response = this.GetNewParserContext().GetMailSettingByID(id);
            if (response == null || response.Count() == 0)
                throw new Exception("Not found setting by id");

            GetMailSettingByIDResult first = response.First();

            SettEmail result = new SettEmail();
            result.EmailTitle = first.EmailTitle;
            result.EnableSSL = first.SSLEnabled;
            result.Login = first.AccountLogin;
            result.Password = Crypto.Decrypt(first.AccountPassCrypted, first.AccountPassSalt);
            result.SmtpHostName = first.SmtpHostName;
            result.SmtpPort = first.SmtpPort;
            result.TargetEmail = first.TargetEmailTo;
            result.TargetEmailFrom = first.TargetEmailFrom;

            result.SettingsAreValid();
            return result;
        }

        public List<SettEmail> GetMailSettings(int from, int to)
        {
            if (from <= 0)
                throw new ArgumentException("from <= 0");
            if (to <= 0)
                throw new ArgumentException("to <= 0");
            if (from > to)
                throw new ArgumentException("from > to");

            ISingleResult<GetMailSettingsResult> response = this.GetNewParserContext().GetMailSettings(from, to);
            List<SettEmail> result = new List<SettEmail>();
            foreach (GetMailSettingsResult setting in response)
            {
                SettEmail item = new SettEmail();
                item.EmailTitle = setting.EmailTitle;
                item.EnableSSL = setting.SSLEnabled;
                item.Login = setting.AccountLogin;
                item.Password = Crypto.Decrypt(setting.AccountPassCrypted, setting.AccountPassSalt);
                item.SmtpHostName = setting.SmtpHostName;
                item.SmtpPort = setting.SmtpPort;
                item.TargetEmail = setting.TargetEmailTo;
                item.TargetEmailFrom = setting.TargetEmailFrom;

                item.SettingsAreValid();
                result.Add(item);
            }
            return result;
        }
    }
}