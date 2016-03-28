using CoreElements;
using System.Collections.Generic;

namespace ParserCore
{
    /// <summary>
    /// Interface for data storage implementation
    /// </summary>
    internal interface IStorage
    {
        void SaveUnitsList(List<DetailShort> input);

        void SaveUnitsDetailsList(List<Detail> input);

        List<DetailShort> GetUnparsedListUnits(int priceMin, int priceMax);

        List<Detail> GetUnshipped(int priceMin, int priceMax, string urlOriginal,
            List<string> constraints, List<string> keyWords, bool OnlyWithPictures);

        void SetShipped(Interest intake, string urlOriginal);

        List<Detail> GetShippedItems();

        List<Detail> GetUnitsDetails(int from, int to);

        void AddMailSetting(SettEmail emailSetting, string encryptedPass, string salt);

        SettEmail GetMailSettingByID(int id);

        List<SettEmail> GetMailSettings(int from, int to);
    }
}