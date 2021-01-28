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

            string sResult = "";                                                        // string for print messages  
            int iCountCycle = 0;                                                        // records saved in the current cycle
            int iGeneralCount = 0;                                                      // number of records saved
            int iMainCount = 0;                                                         // total number of records analyzed
            int iNotSaved = 0;                                                          // total number of records not saved
            int iCountJur = 0;                                                          // count register for JUR_ADMIN
            int iCountNoNewsJur = 0;                                                    // unsaved record count for JUR_ADMIN
            Ocr miOCR = new Ocr();                                                      // new instance of OCR Class
            SII miSII = new SII();
            string slinkID = "";
            string sTitle = "";
            string sAbstract = "";
            int iPosTemp = 0;
            DateTime dSentenceDate;
            //-----------------------------------------------------------------------------------------------------------------------
            // get info from website SUSESO
            //-----------------------------------------------------------------------------------------------------------------------
            string URL = "https://www.sii.cl//normativa_legislacion/jurisprudencia_administrativa/ley_impuesto_ventas/2018/ley_impuesto_ventas_jadm2018.htm";

            //-----------------------------------------------------------------------------------------------------------------------
            // we get all the data
            // we create a class that maps the structure of the json obtained from the suseso website --> suseso.cs
            //-----------------------------------------------------------------------------------------------------------------------
            string siteBase = SII.extractWeb(URL);
            bool bElement = false;
            (siteBase, iPosTemp, bElement) = SII.sExtractTag(iPosTemp, siteBase, "<tbody>", "</tbody>");
            do
            {
                iPosTemp = 0;
                //Link
                (slinkID, iPosTemp, bElement) = SII.sExtractTag(iPosTemp, siteBase, "<a href='", ".htm'");

                if (slinkID.Length > 0)
                {
                    string sAid = slinkID;
                    string slink = "https://www.sii.cl//normativa_legislacion/jurisprudencia_administrativa/ley_impuesto_ventas/2018/" + slinkID + ".htm";

                    //Text
                    string sTextWeb = SII.extractWeb(slink);
                    sTextWeb = SII.StripHTML(sTextWeb);

                    //Title
                    (sTitle, iPosTemp, bElement) = SII.sExtractTag(iPosTemp, siteBase, "rel='modal'>", "</a>");
                    sTitle = SII.StripHTML(sTitle);

                    //Abstract
                    (sAbstract, iPosTemp, bElement) = SII.sExtractTag(iPosTemp, siteBase, "<p style='margin-top:0px;margin-bottom:5px;text-align:justify;'>", "</p>");
                    sAbstract = SII.StripHTML(sAbstract).Trim();

                    //Ord
                    string sOrd = "";
                    string sDate = "";
                    int iTemp = 0;
                    bool bTemp = false;

                    (sOrd, iTemp, bTemp) = SII.sExtractTag(0, sTitle, "(Ord.", ")");

                    sDate = sOrd.Substring(sOrd.LastIndexOf("de ") + 3);
                    sOrd = sOrd.Substring(0, sOrd.LastIndexOf(","));
                    //sDate
                    dSentenceDate = Convert.ToDateTime(sDate);

                    SII mySII = new SII();
                    mySII.sAid = sAid;
                    mySII.sTitle = sTitle;
                    mySII.sSentenceText = sTextWeb;
                    mySII.sAbstrac = sAbstract;
                   
                    mySII.sRol = sOrd;
                    mySII.dtSentenceDate = dSentenceDate;
                    mySII.sLink = slink;

                    //-----------------------------------------------------------------------------------------------------------------------
                    // get records from SQLite database SII 
                    //-----------------------------------------------------------------------------------------------------------------------
                    bool bExistAID = mySII.validateAID();

                    if (bExistAID)
                    {
                        sResult = "El registro  \"{0}\" ya fue ingresado anteriormente." + mySII.sAid;
                    }
                    else
                    {
                        sResult = mySII.addElement();
                        if (sResult == "ok")
                        {
                            iGeneralCount++;
                            iCountCycle++;
                        }
                        else
                        {
                            iNotSaved++;
                        }
                    }
                    iMainCount++;
                    siteBase = siteBase.Substring(iPosTemp);
                    Console.WriteLine("Registro {0} revisado", mySII.sAid);
                }
            } while (bElement);

            #region COMMENTS
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-- PASO 1                                                        ");
            Console.WriteLine("-- FIN DE LA OBTENCION DE DATOS                                  ");
            Console.WriteLine("-- A LAS " + System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + iMainCount);
            Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS PARA VALIDAR:" + iGeneralCount);
            Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iNotSaved);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-- PASO 2                                                        ");
            Console.WriteLine("-- SE CRUZAN LOS DATOS ENTRE LA BD LOCAL Y CENTRAL               ");
            Console.WriteLine("-- SE GUARDAN LOS ARCHIVOS PDF                                   ");
            Console.WriteLine("-- SE GUARDA EN TXT EL CONTENIDO                                 ");
            Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS:" + iCountJur);
            Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iCountNoNewsJur);
            Console.WriteLine("-----------------------------------------------------------------");

            Email miEmail = new Email();
            miEmail.sendEmail(iMainCount, 0, iCountJur);
            Console.WriteLine("-- FIN DE LA EJECUCION " + DateTime.Now + "----");


            #endregion
            if (sDebug == "on")
            {
                Console.ReadLine();
            }
        }
    }
}
