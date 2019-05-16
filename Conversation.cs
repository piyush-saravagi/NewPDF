using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontPipedriveIntegrationProject
{
    class Conversation
    {
        public List<dynamic> listOfMessages;
        public HashSet<string> setOfEmails;
        public string primaryEmail;
        public string id;
        public Dictionary<string, Tag> dictOfTags = new Dictionary<string, Tag>();
        public Dictionary<string, Deal> PDDealsAffectedByConversation;
        public string subject;
        public decimal createdAt;
        public int CEOpenWindowDays = 10;   // Number of days to resolve CE before the CE goes stale

        public Conversation(dynamic conv)
        {
            // Conversations extracted from "events" DO NOT contain full information (eg. list of messages)
            // Hence, we may need to get the entire conversation using the conversation ID
            this.id = conv["id"];
            this.subject = conv["subject"];
            this.primaryEmail = conv["recipient"]["handle"];
            this.createdAt = conv["created_at"];

            var fullConversationData = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}", id), ApiAccessHelper.FRONT_API_KEY);

            this.listOfMessages = new List<dynamic>();
            // Returns an array of messages
            var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}/messages", id), ApiAccessHelper.FRONT_API_KEY)["_results"];
            foreach (var msg in response)
            {
                this.listOfMessages.Add(msg);   // Convert to list
            }
            this.setOfEmails = GetEmailsFromMessageList(listOfMessages);
            this.PDDealsAffectedByConversation = GetListOfDealsToBeUpdated(setOfEmails);

            Console.WriteLine();

        }

        private Dictionary<string, Deal> GetListOfDealsToBeUpdated(HashSet<string> setOfEmails)
        {
            Dictionary<string, Deal> dealsToUpdate = new Dictionary<string, Deal>();
            //Using the email ids to get the deals to be updated
            foreach (var emailId in setOfEmails)
            {
                //get person ID from pipedrive using email id
                var PdPersonAccounts = ApiAccessHelper.GetResponseFromPipedriveApi(String.Format("/persons/find?term={0}", emailId), ApiAccessHelper.PD_API_KEY, urlParameters: true);

                if (PdPersonAccounts != null)   // Handling the possibility that no PD account exists for the email ID
                {
                    foreach (var person in PdPersonAccounts)    // Multiple persons possible with the same email
                    {
                        var pid = person["id"];
                        //? Console.WriteLine("Person ID: " + pid);
                        var allDealsForGivenPerson = ApiAccessHelper.GetResponseFromPipedriveApi(String.Format("/persons/{0}/deals", pid), ApiAccessHelper.PD_API_KEY);
                        if (allDealsForGivenPerson != null)     // ensuring that the person actually has deals associated with his account
                        {
                            foreach (var deal in allDealsForGivenPerson)
                            {
                                //todo need to filter to OPEN deals
                                // Try getting the deal  from the local list
                                Deal tempDeal;
                                
                                if(!dealsToUpdate.ContainsKey(deal["id"].ToString())){ // If deal object not present, create one
                                    tempDeal = new Deal(deal);
                                    dealsToUpdate.Add(deal["id"] + "", tempDeal);
                                }
                            }
                        }
                    }
                }
            }
            return dealsToUpdate;
        }

        private HashSet<string> GetEmailsFromMessageList(List<dynamic> listOfMessages)
        {
            HashSet<string> emailIdsInConv = new HashSet<string>();

            foreach (var message in listOfMessages)
            {
                foreach (var recipient in message["recipients"])
                {
                    if (!(recipient["handle"].Contains("leansentry.com") || recipient["handle"].Contains("leanserver.com") || recipient["handle"].Contains("pipedrivemail")))
                    {
                        //Filtered, deal specific email
                        emailIdsInConv.Add(recipient["handle"]);
                    }
                }
            }
            return emailIdsInConv;
        }

        public void AddTagFromEvent(dynamic tagEvent)
        {
            Tag t = new Tag(tagEvent);
            if(!dictOfTags.ContainsKey(t.tagId)){   //If this conversation hasn't been tagged with this tag yet, create new tag and add that 
                dictOfTags.Add(t.tagId, t);
            }
        }

        public List<string> GetAllTagNames()
        {
            List<string> result = new List<string>();
            foreach (Tag t in dictOfTags.Values)
            {
                result.Add(t.readableTagName);
            }
            return result;
        }
    }
}
