using CoreElements;
using ParserCore;
using System.Collections.Generic;
using System.ServiceModel;

namespace Service
{
    // Define a service contract.
    [ServiceContract(Namespace = "http://anatolyevichPC/ParserOnline")]
    public interface IParserService
    {
        [OperationContract]
        void Start();

        [OperationContract]
        void Stop();

        [OperationContract]
        bool ParserIsRunning();

        [OperationContract]
        List<Detail> GetAllTitles(int from, int to);

        [OperationContract]
        void SaveSettings(SettGreatUnit settings);

        [OperationContract]
        SettGreatUnit GetSettings();

        [OperationContract]
        void AddMailSetting(SettEmail setting);

        [OperationContract]
        SettEmail GetMailSettingByID(int id);

        [OperationContract]
        List<SettEmail> GetMailSettings(int from, int to);
    }
}