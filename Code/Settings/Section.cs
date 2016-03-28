using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserCore
{
    public class Section
    {
        /// <summary>
        /// datetime of last request to sectin url
        /// </summary>
        public DateTime LastSectionUrlRequest;

        public Section()
        {
            this.InterestList = new List<Interest>();
            this.LastEmailDT = new DateTime();
        }

        public Section(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name");

            this.InterestList = new List<Interest>();
            this.LastEmailDT = new DateTime();
            this.Name = name;
            this.FrequencyEmailSeconds = Consts.Settings.FreqEmailsDefault;
            this.FrequencyWebRequestMSeconds = Consts.Settings.FreqWebReqDefault;
        }

        public int FrequencyEmailSeconds { get; set; }
        public int FrequencyWebRequestMSeconds { get; set; }
        public List<Interest> InterestList { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime LastEmailDT { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// сколько секунд спать между запросами основного адреса
        /// </summary>
        public int SleepDelaySeconds { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Add new Interest if possible
        /// </summary>
        /// <param name="name"></param>
        public void AddNewInterest(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (InterestList == null)
                InterestList = new List<Interest>();

            var foundItems = this.InterestList.Where(x => x.Name == name);
            if (foundItems.Any())
                return;

            this.InterestList.Add(new Interest(name));
        }

        /// <summary>
        /// Delete interest by it's name
        /// </summary>
        /// <param name="name"></param>
        public void DeleteInterest(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (InterestList == null)
                InterestList = new List<Interest>();

            var foundItems = this.InterestList.Where(x => x.Name == name);
            if (!foundItems.Any())
                return;

            foreach (Interest match in foundItems)
                this.InterestList.Remove(match);
        }

        /// <summary>
        /// get enabled interests
        /// </summary>
        /// <returns></returns>
        public List<Interest> GetEnabledInterests()
        {
            if (this.InterestList == null || !this.InterestList.Any())
                return new List<Interest>();
            return this.InterestList.Where(x => x.IsEnabled == true).ToList();
        }

        /// <summary>
        /// Get Interest by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Interest GetInterestByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            var inters = InterestList.Where(x => x.Name == name);
            if (!inters.Any())
                return null;

            return inters.First();
        }

        /// <summary>
        /// save interest if it exists in section
        /// </summary>
        /// <param name="changed"></param>
        public void SaveInterest(Interest changed)
        {
            if (changed == null)
                throw new ArgumentNullException("changed");
            if (string.IsNullOrWhiteSpace(changed.Name))
                throw new ArgumentException("changed.Name is null or empty");

            List<Interest> inters = this.InterestList
                .Where(x => x.Name == changed.Name)
                .ToList();

            if (inters == null || !inters.Any())
                return;

            Interest toChange = inters.First();
            toChange.ConstraintsList = changed.ConstraintsList
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            toChange.KeyWords = changed.KeyWords
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            toChange.PriceMax = changed.PriceMax;
            toChange.PriceMin = changed.PriceMin;
            toChange.OnlyWithPictures = changed.OnlyWithPictures;
            toChange.IsEnabled = changed.IsEnabled;
        }
    }
}