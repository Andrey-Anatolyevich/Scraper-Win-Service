namespace ParserCore
{
    public class Consts
    {
        public class Settings
        {
            /// <summary>
            /// Key for encrypting password
            /// </summary>
            internal static string CryptoKey = "2(@#kdIdmt&kkdd]/294t(*@$(**Ye8B(*&%#Y@(&b83(#b&$(*";

            /// <summary>
            /// [Settings.xml]
            /// </summary>
            public static string SettingsFileName = "Settings.xml";

            /// <summary>
            /// [2000] Default miliseconds between web-requests
            /// </summary>
            public static int FreqWebReqDefault = 2000;

            /// <summary>
            /// [900] Default delay in seconds between emails
            /// </summary>
            public static int FreqEmailsDefault = 900;

            /// <summary>
            /// [300] Default delay between section url requests
            /// </summary>
            public static int SleepDelaySecondsDefault = 300;

            /// <summary>
            /// [yyyy.MM.dd HH:mm:ss] DateTime format
            /// </summary>
            public const string DTFormat = "yyyy.MM.dd HH:mm:ss";
        }
    }
}