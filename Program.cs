using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;


namespace FrontPipedriveIntegrationProject
{
    class Program
    {
        //todo move to Helper class safely
        public const string PD_API_KEY = "0b9f8a7f360f41c3264ab14ed5d2a760ecaf39f3";
        public const string FRONT_API_KEY = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY29wZXMiOlsic2hhcmVkOioiXSwiaWF0IjoxNTU2MzEzNjI3LCJpc3MiOiJmcm9udCIsInN1YiI6ImxlYW5zZXJ2ZXIiLCJqdGkiOiI5MDZkYTc3NjA2NWVkOTA5In0.b28IHdaeo0YXwq4dy-xEbzG54RkHnXcOwrbMpbJ5LyY";
        ApiAccessHelper apiHelper = new ApiAccessHelper();
        static Dictionary<string, Conversation> listOfConversations = new Dictionary<string, Conversation>();

        static void Main(string[] args)
        {

            //var response = GetResponseFromPipedriveApi("/deals", PD_API_KEY);
            //var response = apiHelper.GetResponseFromFrontApi("/tags", FRONT_API_KEY);
            //ScanFrontEmailsAndUpdatePD();
            //UpdateTEST();
            ScanFrontEmails();
            //ProcessConversations();
            Console.WriteLine("==============================");
            Console.ReadKey();

        }

        //? DEBUGGING METHOD. PLEASE DELETE
        public static void PrintListConversations()
        {
            foreach (Conversation c in listOfConversations.Values) {
                Console.WriteLine("Conv Subject: []");
                Console.Write("Conv Tags: [");
                foreach (Tag t in c.dictOfTags.Values) {
                    Console.Write(t.readableTagName + ", ");
                }
                Console.WriteLine("]");
            }

        }

        private static void ScanFrontEmails()
        {
            Int32 currTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Int32 timeStampOneYearAgo = currTimestamp - 50 * 86400;     //todo last 50 days, need to change to 365 days

            //!0. PAGINATION

            var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations?q[after]={0}&limit=100", timeStampOneYearAgo), FRONT_API_KEY);
            bool hasNextPage = true;
            int count = 0;
            while (hasNextPage) {
                var allConvInOneYear = response["_results"];

                foreach (var conversation in allConvInOneYear)
                {
                    string convId = conversation["id"];
                    Conversation c;
                    if (!listOfConversations.TryGetValue(convId, out c))
                    {
                        //conversation not present in listOfConversations. Need to create and add a new one
                        c = new Conversation(conversation);
                        // Adding the conversation to our dictionary 
                        listOfConversations.Add(c.id, c);
                    }
                }

                if (response["_pagination"]["next"] == null)
                {
                    Console.WriteLine("NO NEXT PAGE");
                    hasNextPage = false;
                }
                else {
                    string pageToken = response["_pagination"]["next"];
                    pageToken= pageToken.Replace("https://api2.frontapp.com", ""); //Stripping the api url as we need the relative url only
                    Console.WriteLine("HAS NEXT PAGE");
                    response = ApiAccessHelper.GetResponseFromFrontApi(pageToken, FRONT_API_KEY);
                    count++;
                }
            }



            do {
                
            }
            while (response["next"] != null);

            
            PrintListConversations();
            ; //? BREAKPOINT > PLEASE DELETE



           
            
            /*
            foreach (var tagEvent in tagEvents)
            {
                string tagEventId = tagEvent["id"];
                string convId = tagEvent["conversation"]["id"];
                Conversation c;

                if (!listOfConversations.TryGetValue(convId, out c))
                {
                    //conversation not present in listOfConversations. Need to create and add a new one
                    c = new Conversation(tagEvent["conversation"]);
                    // Adding the conversation to our dictionary 
                    listOfConversations.Add(c.id, c);
                }

                // Add tag details to the conversation
                c.AddTagFromEvent(tagEvent);
                Console.WriteLine("Scanned email thread: " + tagEvent["conversation"]["subject"]);
            } */

            //todo Get conversations CREATED in the last 30 days
            Console.WriteLine("");
        }


        private static void ProcessConversations() {
            //Process each conversation and update the Deal fields
            foreach (Conversation conversation in listOfConversations.Values) {
                if (conversation.dictOfTags.ContainsKey("tag_2zbt1"))
                {
                    //! IMPORTANT: Contains PI tag
                    var piTagDate = conversation.dictOfTags["tag_2zbt1"].tagCreationDate;
                    // Updating latest PI field for all deals affected by this conversation
                    foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                    {
                        if (deal.lastPiDate == default(decimal) || deal.lastPiDate < piTagDate)
                        {
                            // todo Update only if the tag is still present
                            deal.lastPiDate = piTagDate;
                            Console.WriteLine("Updating Last PI for " + deal.title + " to " + TimestampToLocalTime(deal.lastPiDate) + " due to thread with subject: " + conversation.subject);
                        }
                    }
                    Console.WriteLine();
                }
                else if (conversation.dictOfTags.ContainsKey("tag_2zbsl"))
                {
                    //! IMPORTANT: Contains FAIL tag
                    var failTagDate = conversation.dictOfTags["tag_2zbsl"].tagCreationDate;
                    // Updating last failed CE field for all deals affected by this conversation
                    foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                    {
                        if (deal.lastFailedCeDate == default(decimal) || deal.lastFailedCeDate < failTagDate)
                        {
                            // todo Update only if the tag is still present
                            // toda Update only if the tag has not been stale
                            deal.lastFailedCeDate = failTagDate;
                            Console.WriteLine("Updating Last failed CE date for " + deal.title + " to " + TimestampToLocalTime(deal.lastFailedCeDate) + " due to thread with subject: " + conversation.subject);
                        }
                    }
                }
                else if (conversation.dictOfTags.ContainsKey("tag_2qf6t")) {
                    //! IMPORTANT: Contains CE tag
                    /*
                        * It could be successful CE, open CE or stale CE
                        * 1. successful CE if 'CE DO' tagged within x days of 'CE' tag
                        * 2. stale CE if no 'CE DO' or 'fail' tags and today's date > x days after it was marked as CE
                        * 3. open CE if no 'CE DO' or 'fail' tags AND today's date < x days of it being marked CE
                        */
                    decimal ceTagDate = conversation.dictOfTags["tag_2qf6t"].tagCreationDate;
                    if (conversation.dictOfTags.ContainsKey("tag_2qf79"))
                    {
                        //! IMPORTANT: Contains 'CE DO' tag. Could be a success or stale

                        decimal ceDoTagDate = conversation.dictOfTags["tag_2qf79"].tagCreationDate;

                        if (ceDoTagDate - ceTagDate <= conversation.CEOpenWindowDays * 86400)
                        { // Assuming emails are tagged as 'CE DO' only after 'CE'
                            // Great! 'CE DO' was tagged within x days. We have a successful and timely CE DO
                            // Update the lastCeDoDate for all deals
                            foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                            {
                                if (deal.lastCeDoDate == default(decimal) || deal.lastCeDoDate < ceDoTagDate)
                                {
                                    // todo Update only if the tag is still present
                                    // todo Update only if the tag has not been stale
                                    deal.lastCeDoDate = ceDoTagDate;
                                    Console.WriteLine("Updating Last successful CE date for " + deal.title + " to " + TimestampToLocalTime(deal.lastCeDoDate) + " due to thread with subject: " + conversation.subject);
                                    Console.WriteLine("Update was made because CE was tagged on " + TimestampToLocalTime(ceTagDate) + " and CE DO was tagged on " + TimestampToLocalTime(ceDoTagDate) + " which is within " + conversation.CEOpenWindowDays + " days");
                                }
                            }
                        }
                        else
                        {
                            // stale CE DO as the conversation was marked 'CE DO' after x days of tagging it as CE
                            //todo Need to work on stale CE list
                            foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                            {
                                Console.WriteLine("Stale CE for " + conversation.subject + " because CE was tagged on " + TimestampToLocalTime(ceTagDate) + " and CE DO was tagged on " + TimestampToLocalTime(ceDoTagDate) + " which is AFTER " + conversation.CEOpenWindowDays + " days");
                            }

                        }
                    }
                    else {
                        //! IMPORTANT: Does not contain a 'CE DO' tag (neither fail tag)
                        //! IMPORTANT: Could be stale or open CE
                        decimal currTimestamp = (decimal)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        if (currTimestamp - ceTagDate <= conversation.CEOpenWindowDays * 8400)
                        {
                            //Open CE identified
                            foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values) {
                                // Update each deal
                                if (deal.lastOpenCeDate == default(decimal) || deal.lastOpenCeDate < currTimestamp)
                                {
                                    // todo Update only if the tag is still present
                                    deal.lastOpenCeDate = currTimestamp;
                                    Console.WriteLine(String.Format("Reason: marked CE on {0} and today's date is {1} which is WITHIN {2} days", TimestampToLocalTime(ceTagDate), TimestampToLocalTime(currTimestamp), conversation.CEOpenWindowDays));
                                    Console.WriteLine("Updating Last OPEN CE (UNRESOLVED) for " + deal.title + " to " + TimestampToLocalTime(currTimestamp) + " due to thread with subject: " + conversation.subject);
                                }
                            }
                        }
                        else {
                            //Stale CE
                            foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                            {
                                // Update each deal
                                Console.WriteLine(String.Format("Reason: marked CE on {0} and today's date is {1} which is OUTSIDE {2} days", TimestampToLocalTime(ceTagDate), TimestampToLocalTime(currTimestamp), conversation.CEOpenWindowDays));
                                Console.WriteLine("STALE CE (UNRESOLVED) for " + deal.title + " to " + TimestampToLocalTime(currTimestamp) + " due to thread with subject: " + conversation.subject);
                            }
                        }
                    }

                }
                Console.WriteLine();
            }
        }

        private static string TimestampToLocalTime(decimal timestamp)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the timestamp (number of seconds since the Epoch) to be converted
            dateTime = dateTime.AddSeconds((double)timestamp).ToLocalTime();
            return dateTime.ToString();
        }


        /*! Delete this if the other function works well
        private static dynamic ScanFrontEmailsAndUpdatePD()
        {
            Dictionary<Int32, Deal> dealsToUpdate = new Dictionary<Int32, Deal>();
            Int32 currTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            //0. PAGINATION

            //1. Get deals tagged in the last 30 days
            Int32 timeStamp30daysAgo = currTimestamp - 30 * 86400;
            var taggedConversations = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/events?q[types][]=tag&q[after]={0}", timeStamp30daysAgo), FRONT_API_KEY);

            //For each tag event, identify the date created, tag id, conv id and the email id associated with that conversation
            foreach (var res in taggedConversations["_results"])
            {

                HashSet<string> emailIdsInConv = new HashSet<string>();

                Tag convTag = new Tag(res);

                Console.WriteLine("Tag created at: " + convTag.tagCreationDate);
                Console.WriteLine("Tag id: " + convTag.tagId);
                Console.WriteLine("Conversation id: " + convTag.convId);
                Console.WriteLine("Conversation email: " + convTag.convEmail);


                //Getting a list of messages in the conversation and storing the email ids 
                var messagesInConv = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}/messages", convTag.convId), FRONT_API_KEY)["_results"];
                foreach (var message in messagesInConv)
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
                //Using the email ids to get the deals to be updated
                foreach (var emailId in emailIdsInConv)
                {
                    //get person ID from pipedrive using email id
                    var PdPersonAccounts = ApiAccessHelper.GetResponseFromPipedriveApi(String.Format("/persons/find?term={0}", emailId), PD_API_KEY, urlParameters: true);

                    if (PdPersonAccounts != null)   // Handling the possibility that no PD account exists for the email ID
                    {
                        foreach (var person in PdPersonAccounts)    // Multiple persons possible with the same email
                        {
                            var pid = person["id"];
                            Console.WriteLine("Person ID: " + pid);
                            var allDealsForGivenPerson = ApiAccessHelper.GetResponseFromPipedriveApi(String.Format("/persons/{0}/deals", pid), PD_API_KEY);
                            if (allDealsForGivenPerson != null)     // ensuring that the person actually has deals associated with his account
                            {
                                foreach (var deal in allDealsForGivenPerson)
                                {
                                    // Try getting the deal  from the local list
                                    Deal tempDeal;
                                    if (!dealsToUpdate.TryGetValue(deal["id"], out tempDeal))   // If deal object not present, create one
                                    {
                                        tempDeal = new Deal(deal);
                                        dealsToUpdate.Add(deal["id"], tempDeal);
                                    }
                                    CalculateFieldValues(tempDeal, convTag);
                                }
                            }
                        }
                    }


                    // get person ID from pipedrive using email id
                    //var PdPersonAccount = GetResponseFromPipedriveApi(String.Format("/persons/find?term={0}", res["conversation"]["recipient"]["handle"]), PD_API_KEY, urlParameters: true);
                    //foreach (var person in PdPersonAccount)
                    //{
                    //    var pid = person["id"];
                    //    Console.WriteLine("Person ID: " + pid);
                    //    UpdatePDFields(currTimestamp, res["emitted_at"], res["target"]["data"]["id"], pid, res["conversation"]["recipient"]["handle"]);
                    //}

                    // todo: Clear PD fields



                    //4. Get email ID from conversation

                    //6. get deals associated with that person

                    // 7. update the field accordingly

                    // 1. Get deals corresponding to each tag
                    // var tagEvents = apiHelper.GetResponseFromFrontApi("/tags/tag_2zbt1/conversations", FRONT_API_KEY);


                    //2. Clear all fields

                    //3. Get tag added date

                    //If within 30 days, 

                    //3. Get email ID for each of these

                    //4. Find out the person id on PD

                    //5. Use person id to update field by 1


                }
                Console.WriteLine(string.Join("", emailIdsInConv));

                //Console.WriteLine("Deals to be updated by this conversation:");
                //foreach (Deal deal in dealsToUpdate.Values) {
                //    Console.WriteLine(deal.title);
                //}

                Console.WriteLine("--------");
            }
            return null;
        }
        /*
        //! Delete this after testing
        private static void UpdateTEST()
        {
            var data = new Dictionary<string, string>();
            data.Add("f7cf37886fc1fdf3a5acad99de357616f568b668", "04/05/2019");
            ApiAccessHelper.PostPipedriveJson("/deals/1510", data, "PUT");
        }


        private static string TimestampToLocalTime(decimal timestamp)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the timestamp (number of seconds since the Epoch) to be converted
            dateTime = dateTime.AddSeconds((double)timestamp).ToLocalTime();
            return dateTime.ToString();
        }

        private static void CalculateFieldValues(Deal deal, Tag tag)
        {

            if (tag.readableTagName == "PI")
            {
                if (deal.lastPIDate == default(decimal) || deal.lastPIDate < tag.tagCreationDate)
                {
                    //update if value is null or PI date is later than current lastPIdate

                    // todo stale PI need to be excluded
                    deal.lastPIDate = tag.tagCreationDate;
                    Console.WriteLine("PI tag was added to " + deal.title + "on " + TimestampToLocalTime(deal.lastPIDate));
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("E:\\pi_data.txt", true))
                    {
                        file.WriteLine("PI tag was added to " + deal.title + "on " + TimestampToLocalTime(deal.lastPIDate));
                    }
                }
            }
            else if (tag.readableTagName == "CE")
            {
                // It could be an open CE, failed CE, successful CE or stale CE

                // Getting all the tags for this particular conversation
                var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/conversations/{0}", tag.convId), FRONT_API_KEY);
                List<string> listOfTagsInConv = new List<string>();


                foreach (var t in response["tags"])
                {
                    listOfTagsInConv.Add(t["name"]);
                }

                foreach (var t in response["tags"])
                {
                    if (t["name"] == "fail")
                    {
                        // 1. Failed CE
                    }
                    else
                    {

                        // Checking if it is not stale
                        //2. Successful CE

                    }
                }
                // 1. Open CE: if no "CE DO" or "fail" tag and within 10 days of CE
                Console.Write("");
            }
        }

        /*
        private static void UpdatePDFields(double currTimestamp, double tagTimestamp, string tagId, int personId, string email)
        {

            switch (tagId)
            {
                case "tag_2qf6t":
                    Console.WriteLine("CE DETECTED for user " + email);
                    break;
                case "tag_2qf79":
                    Console.WriteLine("CE DO DETECTED for user " + email);
                    break;
                case "tag_2zbt1":
                    Console.WriteLine("PI DETECTED for user " + email);

                    //last PI date field ID = 12628
                    //last PI date field key = f7cf37886fc1fdf3a5acad99de357616f568b668

                    //Get all deals for that user
                    var allDealsForGivenPerson = GetResponseFromPipedriveApi(String.Format("/persons/{0}/deals", personId), PD_API_KEY);
                    if (allDealsForGivenPerson != null)     // ensuring that the person actually has deals associated with his account
                    {
                        foreach (var deal in allDealsForGivenPerson)
                        {

                            //Todo: Need to check last PI date before updating


                            //Update the last PI date for this deal
                            var data = new Dictionary<string, string>();
                            data.Add("f7cf37886fc1fdf3a5acad99de357616f568b668", TimestampToLocalTime(tagTimestamp));


                            PostPipedriveJson(String.Format("/deals/{0}", deal["id"]), data, "PUT");
                            Console.WriteLine("Updated last PI date field for deal [" + deal["title"] + "] with the value [" + TimestampToLocalTime(tagTimestamp) + "]");
                        }
                    }
                    break;
                case "tag_2zbsl":
                    Console.WriteLine("FAIL DETECTED for user " + email);
                    break;
                case "tag_2zbs5":
                    Console.WriteLine("CONTACT DETECTED for user " + email);
                    break;

                    //Could store in a dictionary before batch updating but would be too much hassle
            }

            Console.WriteLine("");
        }
        */
    }
}


