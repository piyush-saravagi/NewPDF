using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FrontPipedriveIntegrationProject
{
    class Conversation
    {
        public static Dictionary<string, Deal> listOfAllDeals = new Dictionary<string, Deal>();

        public List<dynamic> listOfMessages;
        public HashSet<string> setOfEmails = new HashSet<string>();
        public string primaryEmail;
        public string id;
        public Dictionary<string, Tag> dictOfTags = new Dictionary<string, Tag>();
        public Dictionary<string, Deal> PDDealsAffectedByConversation;
        public string subject;
        public decimal createdAt;
        public decimal lastMessage;
        public int CEOpenWindowDays = 10;   // Number of days to resolve CE before the CE goes stale
        public int OpportunityOpenWindowDays = 10;   // Number of days to resolve opportunity before the opportunity goes stale
        public string assignee;


        public Conversation(dynamic conv, dynamic events)
        {
            // Conversations extracted from "events" DO NOT contain full information (eg. list of messages)
            // Hence, we may need to get the entire conversation using the conversation ID
            this.id = conv["id"];
            this.subject = conv["subject"];
            this.primaryEmail = conv["recipient"]["handle"];
            this.assignee = conv["assignee"]["first_name"];
            this.createdAt = conv["created_at"];
            this.lastMessage = conv["last_message"]["created_at"];

            var fullConversationData = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}", id), ApiAccessHelper.FRONT_API_KEY);

            this.listOfMessages = new List<dynamic>();
            // Returns an array of messages
            var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}/messages", id), ApiAccessHelper.FRONT_API_KEY)["_results"];
            foreach (var msg in response)
            {
                this.listOfMessages.Add(msg);   // Convert to list
            }

            //! STEP TAKEN TO REDUCE SCOPE. ASSUMING THAT EMAIL ID IS LINKED TO A DEAL
            //! UNCOMMENT NEXT LINE AND COMMENT THE LINE AFTER THAT TO INcLUDED ALL EMAIL IDS FROM ALL MESSAGES. BUT THIS WILL INCUR HEAVY API USAGE
            this.setOfEmails = GetEmailsFromMessageList(listOfMessages);
            //this.setOfEmails.Add(primaryEmail);


            this.PDDealsAffectedByConversation = GetListOfDealsToBeUpdated(setOfEmails);


            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES
            //todo  FIX TAGS VALUES


            foreach (var e in events)
            {
                if (e["type"] == "tag")
                {
                    Tag t;
                    if (!dictOfTags.TryGetValue(e["target"]["data"]["id"], out t))
                    {
                        t = new Tag(e);
                        //Add the tag to the list
                        dictOfTags.Add(t.tagId, t);
                        ; //? BREAKPOINT> PLEASE REMOVE                                    
                    }
                }
            }



            //? WILL BE REMOVED IF THE ABOVE LOOP IS FIXED
            /*
            if (fullConversationData["tags"].Length != 0)
            {
                foreach (var tag in fullConversationData["tags"])
                {
                    //Create a new tag if it does not exists
                    Tag t;
                    if (!dictOfTags.TryGetValue(tag["id"], out t))
                    {
                        t = new Tag(tag);
                        //Add the tag to the list
                        dictOfTags.Add(t.tagId, t);
                        ; //? BREAKPOINT> PLEASE REMOVE                                    
                    }

                }


            }
            */
            //Adding the tags to the fields
            //foreach(var tag in fullConversationData["tags"]){
            //    if (tag != null && dictOfTags.ContainsKey(tag[""])) { //not empty tag list

            //    }
            //}
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

                                //? =======================================================================================================
                                //? NEED TO UPDATE SO THAT ONLY DEALS THAT HAVE NOT BEEN CONSIDERED YET ARE CREATED, ELSE SHARE THE OBJECT
                                //? =======================================================================================================
                                if (!dealsToUpdate.ContainsKey(deal["id"].ToString()))
                                { // Deal object not present inside dealsToUpdate, but could be present inside AllDeals

                                    if (listOfAllDeals.ContainsKey(deal["id"].ToString()))
                                    {
                                        //We already have a deal inside allDeals. No need to create a new one. Just link the deal to dealsToUpdate
                                        //? REMOVE DEBUGGING LINE BELOW
                                        Console.WriteLine("Deal already created by diff conversation");
                                        tempDeal = listOfAllDeals[deal["id"] + ""];
                                    }
                                    else
                                    {
                                        // Need to create a new deal
                                        tempDeal = new Deal(deal);
                                        // Add it to the allDeals
                                        listOfAllDeals.Add(tempDeal.id + "", tempDeal);
                                    }
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
            if (!dictOfTags.ContainsKey(t.tagId))
            {   //If this conversation hasn't been tagged with this tag yet, create new tag and add that 
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
