using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SIIRobot
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            IFormatProvider culture = new CultureInfo("es-ES", true);
            string sMode = ConfigurationManager.AppSettings["Mode"];                              // execution mode: win/mac
            string sDebug = ConfigurationManager.AppSettings["Debug"];                            // Debug mode: on/off
            string sLanguage = ConfigurationManager.AppSettings["Language"];                      // Language: eng/spa
            string sCleanFolders = ConfigurationManager.AppSettings["CleanFolders"];              // flag for clean folders: on/off

            int range = Convert.ToInt32(ConfigurationManager.AppSettings["range"]);               // 365 days
            string iniDate = DateTime.Now.AddDays(-range).ToString("yyyy/MM/dd");                 // initial search date
            iniDate = iniDate.Replace("/", "-");
            string endDate = DateTime.Now.ToString("yyyy/MM/dd");                                 // search end date
            endDate = endDate.Replace("/", "-");

            Console.WriteLine("****************************************");
            Console.WriteLine(" LEGAL BOT - SERVICIO DE IMPUESTOS INTERNOS ");
            Console.WriteLine(" Version 1.0.0  07-01-2021");
            Console.WriteLine(" Modo de ejecucion: " + sMode);
            Console.WriteLine(" inicio de ejecucion: " + DateTime.Now);
            Console.WriteLine(" Rango desde:" + iniDate + " hasta:" + endDate);
            Console.WriteLine("****************************************");
            log.Info("inicio de ejecucion DIRTRAB");

            string PDFPath = "";
            string TIFFPath = "";
            string TXTPath = "";

            if (sMode == "win")
            {
                PDFPath = ConfigurationManager.AppSettings["PDFPath"];                  // Path win to save PDF
                TIFFPath = ConfigurationManager.AppSettings["TIFFPath"];                // Path win to save PDF
                TXTPath = ConfigurationManager.AppSettings["TXTPath"];                  // Path win to save PDF
            }
            else
            {
                PDFPath = ConfigurationManager.AppSettings["PDFPathMac"];               // Path MacOs to save PDF
                TIFFPath = ConfigurationManager.AppSettings["TIFFPathMac"];             // Path MacOs to save PDF
                TXTPath = ConfigurationManager.AppSettings["TXTPathMac"];               // Path MacOs to save PDF
            }

            string jResult = "-";                                                       // string for save JSON 
            string sResult = "";                                                        // string for print messages  
            int iCountCycle = 0;                                                        // records saved in the current cycle
            int iGeneralCount = 0;                                                      // number of records saved
            int iMainCount = 0;                                                         // total number of records analyzed
            int iNotSaved = 0;                                                          // total number of records not saved
            int iCountJur = 0;                                                          // count register for JUR_ADMIN
            int iCountNoNewsJur = 0;                                                    // unsaved record count for JUR_ADMIN
            Ocr miOCR = new Ocr();                                                      // new instance of OCR Class

        }
    }
}
