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



        public Deal(dynamic data)
        {

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

            Dictionary<string, string> pdFieldKeys = new Dictionary<string, string>();
            pdFieldKeys.Add("LAST_PI_DATE_FIELD", "f7cf37886fc1fdf3a5acad99de357616f568b668");
            pdFieldKeys.Add("PI_HISTORY_FIELD", "62721168b30501206ba71f90ccc3365be658d224");
            pdFieldKeys.Add("AUTO_UPDATE_FIELD", "a54bb91d20b88a895343586b7628d487dfdabfb6");


            //string LAST_OPEN_CONTACT_DATE_FIELD = 12610; // Something that we can work on aka unresolved
            string LAST_PI_DATE_FIELD = "f7cf37886fc1fdf3a5acad99de357616f568b668";
            //string LAST_OPEN_CE_FIELD_KEY = 12611;
            //string LAST_FAILED_CE_FIELD_KEY = 12629;
            //string LAST_CE_DO_FIELD_KEY = 12630;



            string PI_HISTORY_FIELD = "62721168b30501206ba71f90ccc3365be658d224";
            string AUTO_UPDATE_FIELD = "a54bb91d20b88a895343586b7628d487dfdabfb6";

            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add(pdFieldKeys["LAST_PI_DATE_FIELD"], Program.TimestampToLocalTime(lastPiDate).ToString());
            result.Add(pdFieldKeys["PI_HISTORY_FIELD"], String.Join(" ", piHistoryStringArray));
            result.Add(pdFieldKeys["AUTO_UPDATE_FIELD"], autoUpdateDateTimestamp.ToString());
            return result;
        }


        //?todo DELETE
        public void AddFieldToUpdate(string fieldId, string value)
        {

        }
    }
}


// Example juan.beck @belamiecommerce.com
