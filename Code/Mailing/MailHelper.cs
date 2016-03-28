using CoreElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParserCore
{
    /// <summary>
    /// Help create email with report
    /// </summary>
    internal class MailHelper
    {
        /// <summary>
        /// Create email string from list of units
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public static string ComposeEmailString(List<Detail> units)
        {
            if (units == null)
                throw new ArgumentException("units == null");

            // result string composer
            StringBuilder Result = new StringBuilder();

            for (int i = 0; i < units.Count; i++)
            {
                // take unit to send
                Detail unit = units[i];
                // main block
                Result.Append("<div style=\"border-color:#e5e5e5;border-style:solid;border-width:1px;position:relative;");
                Result.Append("clear:both;margin-bottom:15px;padding:5px;overflow:auto;\">");
                Result.Append("<div style=\"background-color:rgb(224, 255, 255);height:auto;overflow:auto;\">");
                Result.Append("<span style=\"color:gray;float:left;font-size:25px;margin-left:20px;\">");
                Result.Append(string.Format("{0}/{1}</span>", (i + 1).ToString(), units.Count.ToString()));
                Result.Append("<span style=\"clear:both;font-decoration:none;cursor:pointer;font-size:25px;margin-left:20px;\">&nbsp;&nbsp;<a href=\"");
                Result.Append(string.Format("{0}\">", unit.Url));
                Result.Append(string.Format("<span>{0}</span></a>&nbsp;&nbsp;</span>", unit.Title));
                Result.Append("</div>");
                Result.Append("<hr style=\"width:98%;color:gray;\"/><p style=\"clear:both;margin-left:10px;margin-right:15px;\">");
                Result.Append(string.Format("{0}</p>", unit.Content));
                Result.Append("<span style=\"font-size:23px;font-weight:bold;float:left;margin-left:15px;\">");
                Result.Append(string.Format("{0}</span>", unit.Price));
                Result.Append("<span style=\"color:gray;margin-right:100px;font-size:23px;float:right;\">");
                Result.Append(unit.PublishDT.ToLongTimeString());
                Result.Append("</span><hr style=\"width:98%;color:gray;\"/>");

                // adding pictures in block
                foreach (string s in unit.PictureUrls)
                {
                    Result.Append("<img style=\"max-width:280px;max-height:280px;display:inline-block;margin:10px;float:left;\" ");
                    Result.Append(string.Format("src =\"{0}\" />", s));
                }
                // finish block
                Result.Append("<span style=\"clear:both;\">&nbsp;</span>");
                Result.Append("</div>");
            }
            return Result.ToString();
        }
    }
}