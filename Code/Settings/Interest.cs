using System;
using System.Collections.Generic;

namespace ParserCore
{
    public class Interest
    {
        public string Name { get; set; }
        public int PriceMin { get; set; }
        public int PriceMax { get; set; }
        public bool OnlyWithPictures { get; set; }
        public bool IsEnabled { get; set; }
        public List<string> ConstraintsList { get; set; }
        public List<string> KeyWords { get; set; }

        public Interest()
        {
            this.ConstraintsList = new List<string>();
            this.KeyWords = new List<string>();
        }

        public Interest(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            this.Name = name;
            this.ConstraintsList = new List<string>();
            this.KeyWords = new List<string>();
        }
    }
}