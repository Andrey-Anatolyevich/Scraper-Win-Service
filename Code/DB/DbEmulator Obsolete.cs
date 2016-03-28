namespace ParserCore
{
    /*
    class DBEmulator : IStorage
    {
        private List<DetailShort> Units;
        private List<Detail> UnitDetails;

        public DBEmulator()
        {
            this.Units = new List<DetailShort>();
            this.UnitDetails = new List<Detail>();
        }
        public void SaveUnitsList(List<DetailShort> input)
        {
            bool added = false;
            string parentUrl = "";
            foreach (DetailShort dShort in input)
            {
                if (!Units.Any(x => x.Url == dShort.Url))
                {
                    Units.Add(dShort);
                    added = true;
                    parentUrl = dShort.UrlParent;
                }
            }
            // обрежем лишние элементы, чтобы освободить память.
            if (added == true && Units.Count > 300)
            {
                Units = Units
                    .Where(x => x.UrlParent == parentUrl)
                    .OrderByDescending(x => x.PublishDT)
                    .Take(3000)
                    .ToList();
            }
        }
        public void SaveUnitsDetailsList(List<Detail> input)
        {
            bool Added = false;
            string ParentUrl = "";
            foreach (Detail U in input)
            {
                Units.Where(w => w.IsParsed == false
                    && UnitDetails.Any(x => x.WebID == w.WebID))
                    .ToList()
                    .ForEach(i => i.IsParsed = true);
                if (!UnitDetails.Any(x => x.Url == U.Url))
                {
                    UnitDetails.Add(U);
                    Added = true;
                    ParentUrl = U.UrlParent;
                }
            }
            // обрежем лишние элементы, чтобы освободить память.
            if (input != null)
            {
                // если по нужному нам parentURL получено больше 500 элементов, то отрежем лишнее.
                if (Added == true && UnitDetails.Where(x => x.UrlParent == input.FirstOrDefault().UrlParent).ToList().Count > 500)
                {
                    UnitDetails = UnitDetails
                        .Where(x => x.UrlParent == ParentUrl)
                        .OrderByDescending(x => x.PublishDT)
                        .Take(500)
                        .ToList();
                }
            }
        }
        public List<DetailShort> GetUnparsedListUnits(int priceMin, int priceMax)
        {
            List<DetailShort> Result = Units.Where(x => x.IsParsed == false
                && x.Price >= priceMin).ToList();
            if (priceMax != 0)
            {
                Result = Result.Where(x => x.Price <= priceMax).ToList();
            }
            return Result;
        }
        public List<Detail> GetUnshipped(int priceMin, int priceMax, string urlOriginal,
            List<string> constraints, List<string> keyWords, bool OnlyWithPictures)
        {
            IEnumerable<Detail> unitsCurrentSectionUnshipped = UnitDetails
                // Не реализован IsMailed для текущего класса. переписать если будет необходимо
                //.Where(x => x.IsMailed == false
                && x.Price >= priceMin
                && x.UrlParent == urlOriginal);

            if (priceMax > 0)
            {
                unitsCurrentSectionUnshipped = unitsCurrentSectionUnshipped
                    .Where(x => x.Price <= priceMax);
            }

            if (constraints.Count > 0)
            {
                IEnumerable<Detail> unitsConstr = UnitDetails
                    .Where(x => constraints.Any(y =>
                        (x.Title + " " + x.Content)
                        .ToLower()
                        .IndexOf(y.ToLower()) >= 0)
                    && x.UrlParent == urlOriginal);

                unitsCurrentSectionUnshipped = unitsCurrentSectionUnshipped.Except(unitsConstr);
            }
            if (keyWords.Count > 0)
            {
                unitsCurrentSectionUnshipped = unitsCurrentSectionUnshipped
                    .Where(x => keyWords.Any(y =>
                    (x.Title + " " + x.Content)
                    .ToLower()
                    .IndexOf(y.ToLower()) >= 0)
                    && x.UrlParent == urlOriginal
                    ).ToList();
            }
            if (OnlyWithPictures)
            {
                unitsCurrentSectionUnshipped = unitsCurrentSectionUnshipped
                    .Where(x => x.PictureUrls.Count > 0);
            }
            return unitsCurrentSectionUnshipped.ToList();
        }
        public void SetShipped(Interest intake, string urlOriginal)
        {
            List<Detail> Result = GetUnshipped(intake.PriceMin,
                    intake.PriceMax, urlOriginal, intake.ConstraintsList, intake.KeyWords, intake.OnlyWithPictures);
            foreach (Detail detail in Result)
            {
                UnitDetails.Where(x => x.WebID == detail.WebID).ToList().ForEach(n => n.EMailed = true);
            }
        }
        public List<Detail> GetShippedTotal()
        {
            List<Detail> Result = UnitDetails.Where(x =>
                x.EMailed == true).ToList();
            return Result;
        }
        public List<Detail> GetAllUnitDetails()
        {
            int HowMuchToTake = 3000;
            if (UnitDetails.Count < 3000)
                HowMuchToTake = UnitDetails.Count;

            return UnitDetails.OrderByDescending(x => x.PublishDT).Take(HowMuchToTake).ToList();
        }
    }
    */
}