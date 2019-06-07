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
        //todo add get set

        public Int32 id;
        public string title;

        public decimal lastOpenContactDate; // Something that we can work on aka unresolved
        public decimal lastPiDate;
        public decimal lastOpenCeDate;
        public decimal lastFailedCeDate;
        public decimal lastCeDoDate;

        public decimal autoUpdateDateTimestamp;   // Last date(timestamp) when the fields were updated

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
            { "TOTAL_PI_30_DAYS_FIELD", "50d088d07814367c8f889f85177ccb89567f8d1a"},
            { "TOTAL_FAILED_OPPORTUNITIES_30_DAYS_FIELD", "c8ab13c8d2cf295c2508a5288600a3c8606d6519"},
            { "TOTAL_OPPORTUNITIES_1_YEAR_FIELD", "9f5d4acfe26d08732d9735881fa13eb6f6850eff"},
            { "TOTAL_PI_1_YEAR_FIELD", "b20b15c918ebfcc2a80a280731f2806295de2efb"},
            { "TOTAL_FAILED_OPPORTUNITIES_1_YEAR_FIELD", "c927211a25ba2b669eeab2b6f67a0adfe2d57645"},
            { "CURRENTLY_OPEN_OPPORTUNITIES_FIELD", "567d81397bb8ac264bb0a1efe132075c32570fde"},
            { "CURRENTLY STALE OPPORTUNITIES", "8b0f5960b1933dff23142cbf9a06816ced5a26b4"},

            { "TOTAL_CE_30_DAYS_FIELD", "1719116818456b1350f9b35589963160808c212f"},
            { "TOTAL_CE_DO_30_DAYS_FIELD", "e0d3dac72ab3c8759191b596125840437de53492"},
            { "TOTAL_FAILED_CE_30_DAYS_FIELD", "0f655f3888028ba246f7a4db8b06833474c591b0"},
            { "TOTAL_CE_1_YEAR_FIELD", "26f25d835fb11320f19d4cd7521aeb5a16ebe9c1"},
            { "TOTAL_CE_DO_1_YEAR_FIELD", "0d691c90d13f3a3f374209b1162629a51c3d8369"},
            //{ "TOTAL_FAILED_CE_1_YEAR_FIELD", "c8ab13c8d2cf295c2508a5288600a3c8606d6519"},

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
            {"50d088d07814367c8f889f85177ccb89567f8d1a", "TOTAL_PI_30_DAYS_FIELD"},
            {"c8ab13c8d2cf295c2508a5288600a3c8606d6519", "TOTAL_FAILED_OPPORTUNITIES_30_DAYS_FIELD"},
            {"9f5d4acfe26d08732d9735881fa13eb6f6850eff", "TOTAL_OPPORTUNITIES_1_YEAR_FIELD"},
            {"b20b15c918ebfcc2a80a280731f2806295de2efb", "TOTAL_PI_1_YEAR_FIELD"},
            {"c927211a25ba2b669eeab2b6f67a0adfe2d57645", "TOTAL_FAILED_OPPORTUNITIES_1_YEAR_FIELD"},
            { "567d81397bb8ac264bb0a1efe132075c32570fde", "CURRENTLY_OPEN_OPPORTUNITIES_FIELD"},
            { "8b0f5960b1933dff23142cbf9a06816ced5a26b4", "CURRENTLY STALE OPPORTUNITIES"},

            {"1719116818456b1350f9b35589963160808c212f", "TOTAL_CE_30_DAYS_FIELD"},
            {"e0d3dac72ab3c8759191b596125840437de53492", "TOTAL_CE_DO_30_DAYS_FIELD"},
            {"0f655f3888028ba246f7a4db8b06833474c591b0", "TOTAL_FAILED_CE_30_DAYS_FIELD"},
            {"26f25d835fb11320f19d4cd7521aeb5a16ebe9c1", "TOTAL_CE_1_YEAR_FIELD"},
            {"0d691c90d13f3a3f374209b1162629a51c3d8369", "TOTAL_CE_DO_1_YEAR_FIELD"},
            //{"c8ab13c8d2cf295c2508a5288600a3c8606d6519", "TOTAL_FAILED_CE_1_YEAR_FIELD"},

        };





        public Deal(dynamic data)
        {
            id = (Int32)data["id"];
            title = (string)data["title"];
            if (data[pdFieldKeys["AUTO_UPDATE_FIELD"]] != null && !data[pdFieldKeys["AUTO_UPDATE_FIELD"]].Equals(""))
            { //Update only if the there was an auto-update ever. Else keep it to default 0 which indicates the last update was at epoch time (Jan 1, 1970) 
                autoUpdateDateTimestamp = data[pdFieldKeys["AUTO_UPDATE_FIELD"]];
            }

            if (data[pdFieldKeys["CONTACT_HISTORY_FIELD"]] != null && !data[pdFieldKeys["CONTACT_HISTORY_FIELD"]].Equals(""))
            { //Update only if the contact history field has some value already
                contactHistoryStringArray = data[pdFieldKeys["CONTACT_HISTORY_FIELD"]].Split();
            }

            if (data[pdFieldKeys["PI_HISTORY_FIELD"]] != null && !data[pdFieldKeys["PI_HISTORY_FIELD"]].Equals(""))
            { //Update only if the contact history field has some value already
                piHistoryStringArray = data[pdFieldKeys["PI_HISTORY_FIELD"]].Split();
            }

            if (data[pdFieldKeys["CE_HISTORY_FIELD"]] != null && !data[pdFieldKeys["CE_HISTORY_FIELD"]].Equals(""))
            { //Update only if the contact history field has some value already
                ceHistoryStringArray = data[pdFieldKeys["CE_HISTORY_FIELD"]].Split();
            }

            if (data[pdFieldKeys["CE_DO_HISTORY_FIELD"]] != null && !data[pdFieldKeys["CE_DO_HISTORY_FIELD"]].Equals(""))
            { //Update only if the contact history field has some value already
                ceDoHistoryStringArray = data[pdFieldKeys["CE_DO_HISTORY_FIELD"]].Split();
            }
        }


        public Dictionary<string, string> GetPostableData()
        {

            Dictionary<string, string> result = new Dictionary<string, string>
            {
                { pdFieldKeys["AUTO_UPDATE_FIELD"], autoUpdateDateTimestamp.ToString() },

                { pdFieldKeys["PI_HISTORY_FIELD"], String.Join(" ", piHistoryStringArray) },
                { pdFieldKeys["CONTACT_HISTORY_FIELD"], String.Join(" ", contactHistoryStringArray) },
                { pdFieldKeys["CE_HISTORY_FIELD"], String.Join(" ", ceHistoryStringArray) },
                { pdFieldKeys["CE_DO_HISTORY_FIELD"], String.Join(" ", ceDoHistoryStringArray) },

                { pdFieldKeys["LAST_PI_DATE_FIELD"], Program.TimestampToLocalTime(lastPiDate).ToString() },
                { pdFieldKeys["LAST_OPEN_CONTACT_DATE_FIELD"], Program.TimestampToLocalTime(lastOpenContactDate).ToString() },
                { pdFieldKeys["LAST_OPEN_CE_DATE_FIELD"], Program.TimestampToLocalTime(lastOpenCeDate).ToString() },
                { pdFieldKeys["LAST_FAILED_CE_DATE_FIELD"], Program.TimestampToLocalTime(lastFailedCeDate).ToString() },
                { pdFieldKeys["LAST_CE_DO_DATE_FIELD"], Program.TimestampToLocalTime(lastCeDoDate).ToString() },

                { pdFieldKeys["TOTAL_OPPORTUNITIES_30_DAYS_FIELD"], totalOpportunities30Days.ToString() },
                { pdFieldKeys["TOTAL_PI_30_DAYS_FIELD"], totalPI30Days.ToString() },
                { pdFieldKeys["TOTAL_FAILED_OPPORTUNITIES_30_DAYS_FIELD"], totalFailedOpportunities30Days.ToString() },
                { pdFieldKeys["TOTAL_OPPORTUNITIES_1_YEAR_FIELD"], totalOpportunitiesYearly.ToString() },
                { pdFieldKeys["TOTAL_PI_1_YEAR_FIELD"], totalPiYearly.ToString() },

                { pdFieldKeys["TOTAL_CE_30_DAYS_FIELD"], totalCE30Days.ToString() },
                { pdFieldKeys["TOTAL_CE_DO_30_DAYS_FIELD"], successfulResolvedCe30Days.ToString() },
                { pdFieldKeys["TOTAL_FAILED_CE_30_DAYS_FIELD"], failedResolvedCe30Days.ToString() },
                { pdFieldKeys["TOTAL_CE_1_YEAR_FIELD"], totalCeYearly.ToString() },
                { pdFieldKeys["TOTAL_CE_DO_1_YEAR_FIELD"], totalCeDoYearly.ToString() },

                { pdFieldKeys["CURRENTLY_OPEN_OPPORTUNITIES_FIELD"], (openUnresolvedCe30Days + openUnresolvedOpportunities30Days).ToString()},
                { pdFieldKeys["CURRENTLY STALE OPPORTUNITIES"],  (staleUnresolvedCe30Days + staleUnresolvedOpportunity30Days).ToString()},
            };

            return result;
        }


        //?todo DELETE
        public void AddFieldToUpdate(string fieldId, string value)
        {

        }
    }
}


// Example juan.beck @belamiecommerce.com
