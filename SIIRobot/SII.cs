﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.Net;
using System.IO;
using System.Web;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;

namespace SIIRobot
{
    class SII
    {
        /// <summary>
        /// extract text from "DIRECCION DEL TRABAJO" Web Site
        /// </summary>
        /// <param name="URL">URL DIR TRAB Web</param>
        /// <returns>string with the DirTrab dump</returns>
        public static string extractWeb(string URL)
        {
            try
            {
                string docImportSrc = string.Empty;
               
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";

                var webResponse = request.GetResponse();
                var webStream = webResponse.GetResponseStream();
                var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                responseReader.Close();

                return response;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                return "error";
            }
        }

        public static string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }

        public static (string sSubText,int iPosition,bool bElement) sExtractTag(int iPosition, string sText, string sInitPattern,string sEndPattern2)
        {
            bool bContent = false;
            int iInitialPosition = sText.IndexOf(sInitPattern, iPosition) + sInitPattern.Length;
            int iLength = sText.IndexOf(sEndPattern2, iInitialPosition) - iInitialPosition;
            int iEndPosition = sText.IndexOf(sEndPattern2, iPosition) + sEndPattern2.Length;
            string sSubText = sText.Substring(iInitialPosition, iLength);
            if (sSubText.Length > 0)
            {
                bContent = true;
            }

            return (sSubText,iEndPosition, bContent);
        }

        public static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26\x91\x92]";
            txt = Regex.Replace(txt, r, " ", RegexOptions.Compiled);
            txt = txt.Replace("<", "&lt;");
            txt = txt.Replace(">", "&gt;");
            txt = txt.Replace("\n\n", "*SALTO*");
            txt = txt.Replace("\n", "");
            txt = txt.Replace("*SALTO*", "\n\n");
            txt = txt.Replace(@"&ndash;", "|");

            return txt;
        }

    }
}
