using System;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string conStringSQLite = ConfigurationManager.ConnectionStrings["conStringSQLite"].ConnectionString;
        public string  sAid { get; set; }
        public string sTitle { get; set; }
        public string sSentenceText { get; set; }
        public string sAbstrac { get; set; }
        public string sRol { get; set; }
        public string sLink { get; set; }
        public DateTime dtSentenceDate { get; set; }
       

        private DataManager _myDataManager;
        private DataManager myDataManager
        {
            get => _myDataManager;
            set => _myDataManager = new DataManager(conStringSQLite);
        }

        /// <summary>
        /// extract text from "SII" Web Site
        /// </summary>
        /// <param name="URL">URL SII</param>
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
        /// <summary>
        /// find text between patterns
        /// </summary>
        /// <param name="iPosition"></param>
        /// <param name="sText"></param>
        /// <param name="sInitPattern"></param>
        /// <param name="sEndPattern2"></param>
        /// <returns></returns>
        public static (string sSubText,int iPosition,bool bElement) sExtractTag(int iPosition, string sText, string sInitPattern,string sEndPattern2)
        {
            bool bContent = false;
            int iInitialPatternPosition = sText.IndexOf(sInitPattern, iPosition);
            if (iInitialPatternPosition > 0)
            {
                int iInitialPosition = iInitialPatternPosition + sInitPattern.Length;

                int iLength = sText.IndexOf(sEndPattern2, iInitialPosition) - iInitialPosition;
                int iEndPosition = sText.IndexOf(sEndPattern2, iPosition) + sEndPattern2.Length;
                string sSubText = sText.Substring(iInitialPosition, iLength);
                if (sSubText.Length > 0)
                {
                    bContent = true;
                }
                return (sSubText, iEndPosition, bContent);
            }
            else return ("", sText.Length, false);
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
            txt = txt.Replace(@"'", "\"");
            return txt;
        }

        /// <summary>
        ///  validate AID
        ///  if the record exists, then it returns true
        /// </summary>
        /// <returns></returns>
        public bool validateAID()
        {
            DataTable dtTemp;
            this.myDataManager = new DataManager(this.conStringSQLite);

            string sSQL = "select AID from SII where AID='" + this.sAid + "' LIMIT 1;";
            dtTemp = this.myDataManager.getData(sSQL);
            if (dtTemp.Rows.Count > 0)
            {
                if (dtTemp.Rows[0][0].ToString() != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        /// <summary>
        /// insert instance of suseso to SQLite Database.
        /// </summary>
        /// <returns> OK/Error </returns>
        public string addElement()
        {
            try
            {
                this.sTitle = ReplaceHexadecimalSymbols(this.sTitle);
                this.sAbstrac = ReplaceHexadecimalSymbols(this.sAbstrac);
                this.sSentenceText = ReplaceHexadecimalSymbols(this.sSentenceText);

                this.myDataManager = new DataManager(this.conStringSQLite);
                string SQL = "INSERT INTO  'SII' ('aid', 'title', 'abstract', 'sentenceText', 'insertDate', 'status', 'sentenceDate','rol','link') VALUES (" +
                            "'" + this.sAid + "'," +
                            "'" + this.sTitle + "'," +
                            "'" + this.sAbstrac + "'," +
                            "'" + this.sSentenceText + "'," +
                            "'" + System.DateTime.Now + "'," +
                            "0," +
                            "'" + this.dtSentenceDate.ToString("yyyy/MM/dd") + "'," +
                            "'" + this.sRol + "'," +
                            "'"+ this.sLink+ "');";
                
                string sMsg = myDataManager.setData(SQL);
                if (sMsg == "ok")
                {
                    Console.WriteLine("El registro  \"{0}\" ingresado correctamente.", this.sAid);
                    return "ok";
                }
                else
                {
                    return "error en el ingreso";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                log.Error("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);

                Console.WriteLine("........Fail");
                return "error";
            }
        }



    }
}
