using System;
using System.Net.Mail;
using System.Text;

namespace ParserCore
{
    /// <summary>
    /// Класс для отправки писем на почту
    /// </summary>
    public class Mailer
    {
        private string SmtpHost;
        private int SmtpPort;
        private bool SslEnabled;
        private string Login;
        private string Password;
        private string SenderEmail;
        private string TargetEmail;
        private string MessageTitle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="smtpHost">      SMTP address</param>
        /// <param name="smtpPort">      SMTP port</param>
        /// <param name="sslEnabled">    SSL is enabled</param>
        /// <param name="senderEmail">   Address, message is sent from</param>
        /// <param name="senderPassword">Password of the account</param>
        /// <param name="targetEmail">   Where to send message</param>
        /// <param name="messageTitle">  Title of the message</param>
        public Mailer(string smtpHost, int smtpPort, bool sslEnabled, string senderEmail,
            string senderPassword, string targetEmail, string messageTitle)
        {
            this.SmtpHost = smtpHost;
            this.SmtpPort = smtpPort;
            this.SslEnabled = sslEnabled;
            this.Login = senderEmail;
            this.Password = senderPassword;
            this.SenderEmail = senderEmail;
            this.TargetEmail = targetEmail;
            this.MessageTitle = messageTitle;
        }

        // send a message to an e-mail
        public void SendString(string message, int totalUnits)
        {
            SmtpClient client = new SmtpClient();
            client.Host = this.SmtpHost;
            client.Port = this.SmtpPort;
            client.EnableSsl = this.SslEnabled;
            client.Timeout = 5000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(Login, Password);

            string Title = this.MessageTitle + " " + totalUnits + " шт. " + string.Format("{0:yyyy.MM.dd HH:mm}", DateTime.Now, Consts.Settings.DTFormat);
            MailMessage mm = new MailMessage(this.SenderEmail, this.TargetEmail, Title, message);
            mm.IsBodyHtml = true;
            mm.BodyEncoding = Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
    }
}