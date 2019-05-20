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
        public Int32 id;
        public string title;

        public decimal lastOpenContactDate;
        public decimal lastPiDate;
        public decimal lastOpenCeDate;
        public decimal lastFailedCeDate;
        public decimal lastCeDoDate;

        public decimal autoUpdateDate;          // Last date(timestamp) when the fields were updated

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


        

        public Deal(dynamic data)
        {
            id = (Int32)data["id"];
            title = (string)data["title"];
            if (data["a54bb91d20b88a895343586b7628d487dfdabfb6"] != null) { //Update only if the there was an auto-update ever. Else keep it to default 0 which indicates the last update was at epoch time (Jan 1, 1970) 
                autoUpdateDate = data["a54bb91d20b88a895343586b7628d487dfdabfb6"];
            }
        }

        public void AddFieldToUpdate(string fieldId, string value)
        {

        }
    }
}


// Example juan.beck @belamiecommerce.com
