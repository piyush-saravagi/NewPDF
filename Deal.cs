using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontPipedriveIntegrationProject
{
    class Deal
    {
        //todo ReMoVe UNecEssAry FielDSSSSS
        //todo change from decimal to integers
        public Int32 id;
        public string title;

        public decimal lastOpenContactDate; // Something that we can work on aka unresolved
        public decimal lastPiDate;
        public decimal lastOpenCeDate;
        public decimal lastFailedCeDate;
        public decimal lastCeDoDate;

        public decimal autoUpdateDateTimestamp;          // Last date(timestamp) when the fields were updated

        public decimal totalPI30Days = 0;
        public decimal totalOpportunities30Days = 0;
        public decimal totalStaleOpportunities30Days = 0;
        public decimal totalStaleCE30Days = 0;
        public decimal totalCE30Days = 0;
        public decimal successfulResolvedCe30Days = 0;
        public decimal staleResolvedCe30Days = 0;
        public decimal failedResolvedCe30Days = 0;
        public decimal openUnresolvedCe30Days = 0;
        public decimal staleUnresolvedCe30Days = 0;
        public decimal totalFailedOpportunities30Days = 0;
        public decimal openUnresolvedOpportunity = 0;
        public decimal staleUnresolvedOpportunity30Days = 0;
        public decimal staleResolvedOpportunity30Days = 0;
        public decimal successfulResolvedOpportunity30Days = 0;
        public decimal openUnresolvedOpportunities30Days = 0;

        public decimal totalOpportunitiesYearly = 0;
        public decimal totalPiYearly = 0;
        public decimal totalCeYearly = 0;
        public decimal totalCeDoYearly = 0;



        public string[] contactHistoryStringArray = "0 0 0 0 0 0 0 0 0 0 0 0".Split();
        public string[] piHistoryStringArray = "0 0 0 0 0 0 0 0 0 0 0 0".Split();
        public string[] ceHistoryStringArray = "0 0 0 0 0 0 0 0 0 0 0 0".Split();
        public string[] ceDoHistoryStringArray = "0 0 0 0 0 0 0 0 0 0 0 0".Split();


        public static Dictionary<string, string> pdFieldKeys = new Dictionary<string, string>() {
            {"AUTO_UPDATE_FIELD", "a54bb91d20b88a895343586b7628d487dfdabfb6"},
            {"PI_HISTORY_FIELD", "62721168b30501206ba71f90ccc3365be658d224"},
            {"CONTACT_HISTORY_FIELD", "d7d9f36af37efdccc9219d2fc45446036baa9092" },
            {"CE_HISTORY_FIELD", "d1ef6a939ad18362781c8bb6a87c43ef22dc93f9"},
            {"CE_DO_HISTORY_FIELD", "3105ff80bb3fc252d5141d2213279cc331d27632"},
            {"LAST_PI_DATE_FIELD", "f7cf37886fc1fdf3a5acad99de357616f568b668"},
            {"LAST_OPEN_CONTACT_DATE_FIELD", "038616ffe14bdbc19ce244b78c6c4d98358d3863"},
            {"LAST_OPEN_CE_DATE_FIELD", "8154b743118c30a40de437f958f5191eba4b5c0e"},
            {"LAST_FAILED_CE_DATE_FIELD", "bd553b711011cb8226cad4ac72feb0fc73f4f7fe"},
            {"LAST_CE_DO_DATE_FIELD", "7f76304cff7cf24f0efaddc492402b5aafa0b9df"},


            { "TOTAL_OPPORTUNITIES_30_DAYS_FIELD", "71a4bcb6a537f816ddb780f2fcc25c5da2b76fb9"},
            { "TOTAL_PI_30_DAYS", "50d088d07814367c8f889f85177ccb89567f8d1a"},
            { "TOTAL_FAILED_OPPORTUNITIES_30_DAYS", "c8ab13c8d2cf295c2508a5288600a3c8606d6519"},
            { "TOTAL_OPPORTUNITIES_1_YEAR", "9f5d4acfe26d08732d9735881fa13eb6f6850eff"},
            { "TOTAL_PI_1_YEAR", "b20b15c918ebfcc2a80a280731f2806295de2efb"},
            { "TOTAL_FAILED_OPPORTUNITIES_1_YEAR", "c927211a25ba2b669eeab2b6f67a0adfe2d57645"},




        };

        public static Dictionary<string, string> pdFieldNames = new Dictionary<string, string>() {
            {"a54bb91d20b88a895343586b7628d487dfdabfb6", "AUTO_UPDATE_FIELD"},
            {"62721168b30501206ba71f90ccc3365be658d224", "PI_HISTORY_FIELD"},
            {"d7d9f36af37efdccc9219d2fc45446036baa9092", "CONTACT_HISTORY_FIELD" },
            {"d1ef6a939ad18362781c8bb6a87c43ef22dc93f9", "CE_HISTORY_FIELD"},
            {"3105ff80bb3fc252d5141d2213279cc331d27632", "CE_DO_HISTORY_FIELD"},
            {"f7cf37886fc1fdf3a5acad99de357616f568b668", "LAST_PI_DATE_FIELD"},
            {"038616ffe14bdbc19ce244b78c6c4d98358d3863", "LAST_OPEN_CONTACT_DATE_FIELD"},
            {"8154b743118c30a40de437f958f5191eba4b5c0e", "LAST_OPEN_CE_DATE_FIELD"},
            {"bd553b711011cb8226cad4ac72feb0fc73f4f7fe", "LAST_FAILED_CE_DATE_FIELD"},
            {"7f76304cff7cf24f0efaddc492402b5aafa0b9df", "LAST_CE_DO_DATE_FIELD"},

            {"71a4bcb6a537f816ddb780f2fcc25c5da2b76fb9", "TOTAL_OPPORTUNITIES_30_DAYS_FIELD"},
            {"50d088d07814367c8f889f85177ccb89567f8d1a", "TOTAL_PI_30_DAYS"},
            {"c8ab13c8d2cf295c2508a5288600a3c8606d6519", "TOTAL_FAILED_OPPORTUNITIES_30_DAYS"},
            {"9f5d4acfe26d08732d9735881fa13eb6f6850eff", "TOTAL_OPPORTUNITIES_1_YEAR"},
            {"b20b15c918ebfcc2a80a280731f2806295de2efb", "TOTAL_PI_1_YEAR"},
            {"c927211a25ba2b669eeab2b6f67a0adfe2d57645", "TOTAL_FAILED_OPPORTUNITIES_1_YEAR"},
        };


        


        public Deal(dynamic data)
        {
            //todo remove these fields and use the dictionary instead
            string AUTO_UPDATE_DATE_FIELD_ID = "a54bb91d20b88a895343586b7628d487dfdabfb6";
            string CONTACT_HISTORY_FIELD_ID = "d7d9f36af37efdccc9219d2fc45446036baa9092";
            string PI_HISTORY_FIELD_ID = "62721168b30501206ba71f90ccc3365be658d224";
            string CE_HISTORY_FIELD_ID = "d1ef6a939ad18362781c8bb6a87c43ef22dc93f9";
            string CE_DO_HISTORY_FIELD_ID = "3105ff80bb3fc252d5141d2213279cc331d27632";



            id = (Int32)data["id"];
            title = (string)data["title"];
            if (data[AUTO_UPDATE_DATE_FIELD_ID] != null && !data[PI_HISTORY_FIELD_ID].Equals(""))
            { //Update only if the there was an auto-update ever. Else keep it to default 0 which indicates the last update was at epoch time (Jan 1, 1970) 
                autoUpdateDateTimestamp = data[AUTO_UPDATE_DATE_FIELD_ID];
            }

            if (data[CONTACT_HISTORY_FIELD_ID] != null && !data[PI_HISTORY_FIELD_ID].Equals(""))
            { //Update only if the contact history field has some value already
                contactHistoryStringArray = data[CONTACT_HISTORY_FIELD_ID].Split();
            }

            if (data[PI_HISTORY_FIELD_ID] != null && !data[PI_HISTORY_FIELD_ID].Equals(""))
            { //Update only if the contact history field has some value already
                piHistoryStringArray = data[PI_HISTORY_FIELD_ID].Split();
            }

            if (data[CE_HISTORY_FIELD_ID] != null && !data[PI_HISTORY_FIELD_ID].Equals(""))
            { //Update only if the contact history field has some value already
                ceHistoryStringArray = data[CE_HISTORY_FIELD_ID].Split();
            }

            if (data[CE_DO_HISTORY_FIELD_ID] != null && !data[PI_HISTORY_FIELD_ID].Equals(""))
            { //Update only if the contact history field has some value already
                ceDoHistoryStringArray = data[CE_DO_HISTORY_FIELD_ID].Split();
            }
        }


        public Dictionary<string, string> GetPostableData()
        {


            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add(pdFieldKeys["AUTO_UPDATE_FIELD"], autoUpdateDateTimestamp.ToString());

            result.Add(pdFieldKeys["PI_HISTORY_FIELD"], String.Join(" ", piHistoryStringArray));
            result.Add(pdFieldKeys["CONTACT_HISTORY_FIELD"], String.Join(" ", contactHistoryStringArray));
            result.Add(pdFieldKeys["CE_HISTORY_FIELD"], String.Join(" ", ceHistoryStringArray));
            result.Add(pdFieldKeys["CE_DO_HISTORY_FIELD"], String.Join(" ", ceDoHistoryStringArray));
            
            result.Add(pdFieldKeys["LAST_PI_DATE_FIELD"], Program.TimestampToLocalTime(lastPiDate).ToString());
            result.Add(pdFieldKeys["LAST_OPEN_CONTACT_DATE_FIELD"], Program.TimestampToLocalTime(lastOpenContactDate).ToString());
            result.Add(pdFieldKeys["LAST_OPEN_CE_DATE_FIELD"], Program.TimestampToLocalTime(lastOpenCeDate).ToString());
            result.Add(pdFieldKeys["LAST_FAILED_CE_DATE_FIELD"], Program.TimestampToLocalTime(lastFailedCeDate).ToString());
            result.Add(pdFieldKeys["LAST_CE_DO_DATE_FIELD"], Program.TimestampToLocalTime(lastCeDoDate).ToString());

            result.Add(pdFieldKeys["TOTAL_OPPORTUNITIES_30_DAYS_FIELD"], totalOpportunities30Days.ToString());
            result.Add(pdFieldKeys["TOTAL_PI_30_DAYS"], totalPI30Days.ToString());
            result.Add(pdFieldKeys["TOTAL_FAILED_OPPORTUNITIES_30_DAYS"], totalFailedOpportunities30Days.ToString());
            result.Add(pdFieldKeys["TOTAL_OPPORTUNITIES_1_YEAR"], totalOpportunitiesYearly.ToString());
            result.Add(pdFieldKeys["TOTAL_PI_1_YEAR"], totalPiYearly.ToString());

            return result;
        }


        //?todo DELETE
        public void AddFieldToUpdate(string fieldId, string value)
        {

        }
    }
}


// Example juan.beck @belamiecommerce.com
