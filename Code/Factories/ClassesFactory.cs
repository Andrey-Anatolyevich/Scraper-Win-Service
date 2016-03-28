using CoreElements;
using ParserCore.DB.Sql;
using ParserCore.Utilites;
using System;
using System.Security.Cryptography;

namespace ParserCore.Factories
{
    /// <summary>
    /// Factory of classes for common use
    /// </summary>
    internal class ClassesFactory
    {
        /// <summary>
        /// Get new generator of random numbers for salt
        /// </summary>
        /// <returns></returns>
        public RNGCryptoServiceProvider GetRandomNumberGenerator()
        {
            return new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Get new salt maker
        /// </summary>
        /// <returns></returns>
        public SaltMaker GetSaltMaker()
        {
            RNGCryptoServiceProvider generator = this.GetRandomNumberGenerator();
            return new SaltMaker(generator);
        }

        /// <summary>
        /// Get SQL Facade
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public DbSqlFacade GetSqlFacade(ClassesFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            DbSqlFacade result = new DbSqlFacade(factory);
            return result;
        }

        /// <summary>
        /// Get new WebHelper
        /// </summary>
        public WebHelper GetWebHelper()
        {
            return new WebHelper();
        }

        /// <summary>
        /// Get new Utils
        /// </summary>
        public Utils GetUtils()
        {
            return new Utils();
        }

        /// <summary>
        /// Get new dbContext
        /// </summary>
        /// <returns></returns>
        public ParserSqlDataContext GetDbContext()
        {
            return new ParserSqlDataContext();
        }

        /// <summary>
        /// Get DetailShort from Add
        /// </summary>
        /// <param name="add"></param>
        /// <returns></returns>
        public DetailShort GetDetailShort(Add add)
        {
            if (add == null)
                throw new ArgumentNullException("add");

            DetailShort result = new DetailShort();
            result.WebID = add.AddID;
            result.PublishDT = add.InsertDt != null ? (DateTime)add.InsertDt : DateTime.MinValue;
            result.Price = add.Price != null ? (int)add.Price : 0;
            result.Url = add.Url;
            result.UrlParent = add.UrlParent;

            return result;
        }

        /// <summary>
        /// Get Add from DetailShort
        /// </summary>
        public Add GetAdd(DetailShort dShort)
        {
            if (dShort == null)
                throw new ArgumentNullException("dShort");


            Add result = new Add();
            result.AddID = dShort.WebID;
            result.InsertDt = DateTime.Now;
            result.IsMailed = false;
            result.IsProcessed = false;
            result.Price = dShort.Price;
            result.Text = string.Empty;
            result.Title = dShort.Title;
            result.UpdateDt = DateTime.Now;
            result.Url = dShort.Url;
            result.UrlParent = dShort.UrlParent;

            return result;
        }
    }
}