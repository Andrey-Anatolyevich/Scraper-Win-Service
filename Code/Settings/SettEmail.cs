using System;

namespace ParserCore
{
    public class SettEmail
    {
        public string SmtpHostName { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string TargetEmailFrom { get; set; }
        public string TargetEmail { get; set; }
        public string EmailTitle { get; set; }

        /// <summary>
        /// Throw exception if one of fields is not valid
        /// </summary>
        public void SettingsAreValid()
        {
            if (string.IsNullOrEmpty(this.SmtpHostName))
                throw new ArgumentException("this.SmtpHostName is null or empty");
            if (string.IsNullOrEmpty(Login))
                throw new ArgumentException("Login is null or empty");
            if (string.IsNullOrEmpty(Password))
                throw new ArgumentException("Password is null or empty");
            if (string.IsNullOrEmpty(TargetEmail))
                throw new ArgumentException("TargetEmail is null or empty");
            if (string.IsNullOrEmpty(TargetEmailFrom))
                throw new ArgumentException("TargetEmailFrom is null or empty");
            if (string.IsNullOrEmpty(EmailTitle))
                throw new ArgumentException("EmailTitle is null or empty");
            if (SmtpPort <= 0)
                throw new ArgumentException("SmtpPort <= 0");
        }
    }
}