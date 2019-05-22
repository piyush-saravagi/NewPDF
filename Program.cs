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
using System.ComponentModel;

namespace FrontPipedriveIntegrationProject
{
    class Program
    {
        //todo ignore tier4
        //todo move to Helper class safely
        public const string PD_API_KEY = "0b9f8a7f360f41c3264ab14ed5d2a760ecaf39f3";
        public const string FRONT_API_KEY = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY29wZXMiOlsic2hhcmVkOioiXSwiaWF0IjoxNTU2MzEzNjI3LCJpc3MiOiJmcm9udCIsInN1YiI6ImxlYW5zZXJ2ZXIiLCJqdGkiOiI5MDZkYTc3NjA2NWVkOTA5In0.b28IHdaeo0YXwq4dy-xEbzG54RkHnXcOwrbMpbJ5LyY";
        ApiAccessHelper apiHelper = new ApiAccessHelper();
        static Dictionary<string, Conversation> listOfConversations = new Dictionary<string, Conversation>();

        static Int32 currTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        static string LOG_FILE_NAME = currTimestamp.ToString() + ".txt";

        
        //todo rename to timeStamp30daysAgo
        static Int32 timeStampOneYearAgo = currTimestamp - 30 * 86400;     //todo last 30 days, need to change to 365 days
        
        static void Main(string[] args)
        {

            ScanFrontEmails();
            ProcessConversations(currTimestamp);
            Console.WriteLine("==============================");

            //todo ADD FUNCTIONALITY TO AUTOMATICALLY DELETE HISTORY EVERY YEAR OR ASK JILL TO DO THAT 


            //? Debugging loop. Please remove
            foreach (Conversation c in listOfConversations.Values)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    ;
                }
            }

            Console.ReadKey();

        }

        //? DEBUGGING METHOD. PLEASE DELETE
        public static void PrintListConversations()
        {
            foreach (Conversation c in listOfConversations.Values)
            {
                Console.WriteLine("Conv Subject: [" + c.subject + "]");
                Console.Write("Conv Tags: [");
                foreach (Tag t in c.dictOfTags.Values)
                {
                    Console.Write(t.readableTagName + ", ");
                }
                Console.WriteLine("]");
                Console.Write("PD Deals: [");
                if (c.PDDealsAffectedByConversation != null)
                {
                    foreach (Deal d in c.PDDealsAffectedByConversation.Values)
                    {
                        Console.Write(d.title + ",");
                    }
                    Console.Write("]");
                }
                else
                    Console.Write("NO DEALS LINKED WITH THIS EMAIL]" + c.primaryEmail);
                Console.WriteLine("\n");
            }

        }

        private static void ScanFrontEmails()
        {
            /*
              * 9_ToReadLater: "inb_b8z9"
              * 0_Tier1: "inb_600h"
              * "0_PRIORITY": "inb_6061"
              * "0_Tier2": "inb_g84l"
              * "4_Events": "inb_24it"
              * "0_INCOMING": "inb_2mnh"
              */

            
            List<string> idsOfinboxesThatAffectPdFields = new List<string>(new string[]{ "inb_6061", "inb_600h", "inb_g84l"});

            foreach (string inboxId in idsOfinboxesThatAffectPdFields) {
                //!0. PAGINATION
                Console.WriteLine("Conversations from " + inboxId + "\n___________________");
                var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/inboxes/{0}/conversations", inboxId), FRONT_API_KEY);
                bool hasNextPage = true;
                int count = 0;
                while (hasNextPage)
                {
                    var allConvInOneYear = response["_results"];

                    foreach (var conversation in allConvInOneYear)
                    {


                        string convId = conversation["id"];
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //todo FILTER BY INBOXES =========================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================

                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                        //? ==============================================================================
                       
                       

                        

                        
                        Console.WriteLine("\nScanned conv: " + conversation["subject"]);
                        Console.WriteLine("Last message on " + TimestampToLocalTime(conversation["last_message"]["created_at"]));
                        
                        
                            Conversation c;
                            if (!listOfConversations.TryGetValue(convId, out c))
                            {

                                //conversation not present in listOfConversations. Need to create and add a new one
                                c = new Conversation(conversation);
                                // Adding the conversation to our dictionary 

                                listOfConversations.Add(c.id, c);
                            }

                        
                      

                        //Get the last event for this conversation
                        string eventsRelativeUrl = conversation["_links"]["related"]["events"].Replace("https://api2.frontapp.com", "");
                        var latestEventDate = ApiAccessHelper.GetResponseFromFrontApi(eventsRelativeUrl, FRONT_API_KEY)["_results"][0]["emitted_at"];

                        //if (c.lastMessage < timeStampOneYearAgo)
                        //    return;
                        if (latestEventDate < timeStampOneYearAgo)
                        {
                            //! IMPORTANT - Do not delete: Needed to break out of the outer loop as well
                            hasNextPage = false;
                            break;
                        }
                    }

                    if (response["_pagination"]["next"] == null)
                    {
                        Console.WriteLine("NO NEXT PAGE");
                        hasNextPage = false;
                    }
                    else
                    {
                        string pageToken = response["_pagination"]["next"];
                        pageToken = pageToken.Replace("https://api2.frontapp.com", ""); //Stripping the api url as we need the relative url only
                        Console.WriteLine("HAS NEXT PAGE");
                        response = ApiAccessHelper.GetResponseFromFrontApi(pageToken, FRONT_API_KEY);
                        count++;
                    }
                }

            }




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
            //Console.WriteLine("");
        }


        private static void ProcessConversations(decimal currTimestamp)
        {
            string CE_TAG_ID = "tag_2qf6t";
            string CE_DO_TAG_ID = "tag_2qf79";
            string PI_TAG_ID = "tag_2zbt1";
            string FAIL_TAG_ID = "tag_2zbsl";

            string logReason;

            //Process each conversation and update the Deal fields
            foreach (Conversation conversation in listOfConversations.Values)
            {
                Logger(LOG_FILE_NAME, "");
                Logger(LOG_FILE_NAME, String.Format("Conversation subject: {0}", conversation.subject));
                Logger(LOG_FILE_NAME, String.Format("Conversation id: {0}", conversation.id));
                Logger(LOG_FILE_NAME, String.Format("Email IDs: {0}", String.Join(", ", conversation.setOfEmails)));
                //Logging PD Fields
                List<string> pdTitles = new List<string>();
                //? could have used linq as well
                foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                {
                    pdTitles.Add(deal.title);
                }
                Logger(LOG_FILE_NAME, String.Format("Pipedrive deals affected: {0}", String.Join(", ", pdTitles)));
                pdTitles = null;


                // Every conversation is an opportunity
                foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                {
                    d.totalOpportunities30Days++;
                    if (conversation.createdAt > d.autoUpdateDate)
                    {    // Conversation created after last update, need to update history
                        // We have a new opportunity (CE/Non-CE doesn't matter)
                        //Update in history
                        int monthToUpdate = TimestampToLocalTime(conversation.createdAt).Month - 1;
                        Int32 temp = Int32.Parse(d.contactHistoryStringArray[monthToUpdate]) + 1;
                        //Update it back to the deal
                        d.contactHistoryStringArray[monthToUpdate] = temp.ToString();
                        Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of {4}", d.title, "totalOpportunities30Days", (temp - 1), temp, "new conversation: " + conversation.subject))
                        ;
                    }
                }

                if (conversation.dictOfTags.ContainsKey(CE_TAG_ID))
                {
                    // CE opportunity
                    Tag ce = conversation.dictOfTags[CE_TAG_ID];
                    foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                    {
                        d.totalCE30Days++;  //CE resolved/ unresolved/ failed etc are all still considered CEs
                        Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of - {4}", d.title, "totalCE30Days", d.totalCE30Days - 1, d.totalCE30Days, "marked CE on " + TimestampToLocalTime(ce.tagCreationDate)));

                        if (ce.tagCreationDate > d.autoUpdateDate)
                        {    // Conversation marked CE after last update, need to update history
                             // We have a new CE (Resolved/Unresolved doesn't matter)
                             //Update in history
                            int monthToUpdate = TimestampToLocalTime(ce.tagCreationDate).Month - 1;
                            Int32 temp = Int32.Parse(d.contactHistoryStringArray[monthToUpdate]) + 1;
                            //Update it back to the deal
                            d.contactHistoryStringArray[monthToUpdate] = temp.ToString();
                            //todo add log to history if needed 
                        }
                    }

                    if (conversation.dictOfTags.ContainsKey(CE_DO_TAG_ID))
                    {

                        Tag ce_do = conversation.dictOfTags[CE_DO_TAG_ID];
                        if (ce_do.tagCreationDate - ce.tagCreationDate < conversation.CEOpenWindowDays * 86400)
                        {
                            //! SUCCESSFUL RESOLVED CE 
                            //! CONTAINS CE TAG AND WAS MARKED CE-DO ON TIME
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                d.successfulResolvedCe30Days++;
                                Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of - {4}", d.title, "successFullyResolvedCe30Days", d.successfulResolvedCe30Days - 1, d.successfulResolvedCe30Days, "marked CE on " + TimestampToLocalTime(ce.tagCreationDate) + " and CE DO marked on " + TimestampToLocalTime(ce_do.tagCreationDate) + " which is within " + conversation.CEOpenWindowDays + " days"));

                                if (d.lastCeDoDate < ce_do.tagCreationDate)
                                {
                                    Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of - {4}", d.title, "lastCeDoDate", TimestampToLocalTime(d.lastCeDoDate), TimestampToLocalTime(ce_do.tagCreationDate), ""));
                                    d.lastCeDoDate = ce_do.tagCreationDate;
                                }

                                if (ce_do.tagCreationDate > d.autoUpdateDate)
                                {    // Conversation tagged CE-DO after the last update, need to update history
                                     //Update in history
                                    int monthToUpdate = TimestampToLocalTime(ce_do.tagCreationDate).Month - 1;
                                    Int32 temp = Int32.Parse(d.ceDoHistoryStringArray[monthToUpdate]) + 1;
                                    //Update it back to the deal
                                    d.ceDoHistoryStringArray[monthToUpdate] = temp.ToString();
                                    //todo store history in log if needed
                                }
                            }
                        }
                        else
                        {
                            //! STALE RESOLVED CE
                            //! CONTAINS CE AND CE DO TAGS BUT WAS NOT MARKED CE-DO ON TIME 
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                d.staleResolvedCe30Days++;
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleResolvedCe30Days", d.staleResolvedCe30Days - 1, d.staleResolvedCe30Days, "marked CE on " + TimestampToLocalTime(ce.tagCreationDate) + " and CE DO on " + TimestampToLocalTime(ce_do.tagCreationDate) + " which is outside the window of " + conversation.CEOpenWindowDays + " days"));
                            }
                        }

                    }
                    else if (conversation.dictOfTags.ContainsKey(FAIL_TAG_ID))
                    {
                        //! FAILED RESOLVED CE
                        //! CONTAINS CE TAG AND FAIL TAG
                        //todo UPDATE FIELDS

                        Tag fail = conversation.dictOfTags[FAIL_TAG_ID];
                        foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                        {
                            d.failedResolvedCe30Days++;
                            if (d.lastFailedCeDate < fail.tagCreationDate)
                            {
                                d.lastFailedCeDate = fail.tagCreationDate;
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastFailedCeDate", TimestampToLocalTime(d.lastFailedCeDate), TimestampToLocalTime(fail.tagCreationDate), "marked CE on " + TimestampToLocalTime(ce.tagCreationDate) + " and fail on " + TimestampToLocalTime(fail.tagCreationDate)));
                            }
                        }
                    }
                    else
                    {
                        // Unresolved CE, not marked with CE-DO or FAIL tags

                        if (currTimestamp - ce.tagCreationDate < conversation.CEOpenWindowDays * 86400)
                        {
                            //!  OPEN UNRESOLVED CE 
                            //! CONTAINS CE TAG, WAS NOT MARKED CE-DO OR FAIL AND IS WITHIN THE TIME WINDOW FOR OPEN CE
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                d.openUnresolvedCe30Days++;
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "openUnresolvedCe30Days", d.openUnresolvedCe30Days - 1, d.openUnresolvedCe30Days, "marked CE on " + TimestampToLocalTime(ce.tagCreationDate) + " and today is " + TimestampToLocalTime(currTimestamp) + " which is outside the window of " + conversation.CEOpenWindowDays + " days"));

                                //? NEED TO ADD LOGS FOR THE REST OF THE TAGS BELOW THIS LINE
                                //? ---------------------------------------------------------

                                if (d.lastOpenContactDate < conversation.createdAt)
                                {
                                    d.lastOpenContactDate = conversation.createdAt;
                                }
                                if (d.lastOpenCeDate < ce.tagCreationDate)
                                {
                                    d.lastOpenCeDate = ce.tagCreationDate;
                                }
                            }
                        }
                        else
                        {
                            //! STALE UNRESOLVED CE
                            //! CONTAINS CE AND WAS NOT MARKED CE-DO OR FAIL ON TIME
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleUnresolvedCe30Days", d.staleUnresolvedCe30Days, d.staleUnresolvedCe30Days+1, "marked CE on " + ce.tagCreationDate + " and today's date is " + TimestampToLocalTime(currTimestamp) + " which is outside the window of " + conversation.CEOpenWindowDays + " days"));
                                d.staleUnresolvedCe30Days++;


                                if (d.lastOpenContactDate < conversation.createdAt)
                                {
                                    Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastOpenContactDate", TimestampToLocalTime(d.lastOpenContactDate), TimestampToLocalTime(conversation.createdAt), " new conversation: " + conversation.subject));
                                    d.lastOpenContactDate = conversation.createdAt;
                                }
                            }
                        }
                    }
                }
                else
                {

                    // Non CE opportunity
                    if (conversation.dictOfTags.ContainsKey(PI_TAG_ID))
                    {
                        Tag pi = conversation.dictOfTags[PI_TAG_ID];
                        if (pi.tagCreationDate - conversation.createdAt < conversation.OpportunityOpenWindowDays * 86400 )
                        {
                            //! SUCCESSFUL RESOLVED OPPORTUNITY
                            //! CONTAINS A PI TAG, BUT NO CE TAG
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "successfulResolvedOpportunity30Days", d.successfulResolvedOpportunity30Days, d.successfulResolvedOpportunity30Days + 1, "conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and marked PI on " + TimestampToLocalTime(pi.tagCreationDate)));

                                d.successfulResolvedOpportunity30Days++;
                                if (d.lastPiDate < pi.tagCreationDate)
                                {
                                    logReason = "marked PI on " + pi.tagCreationDate + " and previous PI was on " + d.lastPiDate; Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastPiDate", TimestampToLocalTime(d.lastPiDate), TimestampToLocalTime(pi.tagCreationDate), logReason));
                                    d.lastPiDate = pi.tagCreationDate;
                                }

                                if (pi.tagCreationDate > d.autoUpdateDate)
                                {    // Conversation tagged PI after last update, need to update history
                                     // We have a new PI (not stale)
                                     //Update in history
                                    int monthToUpdate = TimestampToLocalTime(pi.tagCreationDate).Month - 1;
                                    Int32 temp = Int32.Parse(d.piHistoryStringArray[monthToUpdate]) + 1;
                                    //Update it back to the deal
                                    d.piHistoryStringArray[monthToUpdate] = temp.ToString();
                                    //todo update log if needed
                                }
                            }
                        }
                        else
                        {
                            //! STALE RESOLVED OPPORTUNITY
                            //! CONTAINS A PI TAG BUT OUTSIDE THE WINDOW
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                logReason = "conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and maked PI on " + TimestampToLocalTime(pi.tagCreationDate) + " which is outside the " + conversation.OpportunityOpenWindowDays + " days window";
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleResolvedOpportunity30Days", d.staleResolvedOpportunity30Days, d.staleResolvedOpportunity30Days + 1, logReason));
                                d.staleResolvedOpportunity30Days++;
                            }
                        }
                    }
                    else if (conversation.dictOfTags.ContainsKey(FAIL_TAG_ID))
                    {
                        //! FAILED OPPORTUNITY
                        //! CONTAINS A FAIL TAG, BUT NO CE
                        //todo UPDATE FIELDS
                        Tag fail = conversation.dictOfTags[FAIL_TAG_ID];
                        foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                        {
                            logReason = "created on " + TimestampToLocalTime(conversation.createdAt) + " and marked fail on " + TimestampToLocalTime(fail.tagCreationDate);
                            Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "failedResolvedCe30Days", d.failedResolvedCe30Days, d.failedResolvedCe30Days + 1, logReason));
                            d.failedResolvedCe30Days++;
                        }
                    }
                    else
                    {
                        //Unresolved Opportunity
                        if (currTimestamp - conversation.createdAt < conversation.OpportunityOpenWindowDays * 86400)
                        {
                            //! OPEN UNRESOLVED OPPORTUNITY
                            //! DOES NOT CONTAIN A CE, FAIL, PI TAG BUT IS WITHIN THE OPEN WINDOW
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                logReason = "created on " + TimestampToLocalTime(conversation.createdAt) + " and today is " + TimestampToLocalTime(currTimestamp) + " which is within " + conversation.OpportunityOpenWindowDays + " days window";
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "openUnresolvedOpportunities30Days", d.openUnresolvedOpportunities30Days, d.openUnresolvedOpportunities30Days + 1, logReason));
                                d.openUnresolvedOpportunities30Days++;
                                if (d.lastOpenContactDate < conversation.createdAt)
                                {
                                    logReason = "new conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and previous conversation was on" + TimestampToLocalTime(d.lastOpenContactDate);
                                    Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastOpenContactDate", d.lastOpenContactDate, conversation.createdAt, logReason));
                                    d.lastOpenContactDate = conversation.createdAt;
                                }
                            }

                        }
                        else
                        {
                            //! STALE UNRESOLVED OPPORTUNITY
                            //! DOES NOT CONTAIN A CE, FAIL, PI TAG BUT IS WITHIN THE OPEN WINDOW
                            //todo UPDATE FIELDS
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                logReason = "created on " + TimestampToLocalTime(conversation.createdAt) + " and today is " + TimestampToLocalTime(currTimestamp) + " which is outside " + conversation.OpportunityOpenWindowDays + " days window";
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleUnresolvedOpportunity30Days", d.staleUnresolvedOpportunity30Days, d.staleUnresolvedOpportunity30Days + 1, logReason));
                                d.staleUnresolvedOpportunity30Days++;

                                if (d.lastOpenContactDate < conversation.createdAt)
                                {
                                    logReason = "new conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and previous conversation was on" + TimestampToLocalTime(d.lastOpenContactDate);
                                    Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastOpenContactDate", d.lastOpenContactDate, conversation.createdAt, logReason));
                                    d.lastOpenContactDate = conversation.createdAt;
                                }
                            }
                        }
                    }
                }
            }

            //Update the auto-update date. This is done separately from the rest of the updates because we need to update the other fields that are dependent on this BEFORE making changes to this field
            foreach (Conversation conversation in listOfConversations.Values)
            {
                foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                {
                    d.autoUpdateDate = currTimestamp;
                }
            }

            UpdateYearlyFields();
        }

        private static void UpdateYearlyFields()
        {
            foreach (Conversation c in listOfConversations.Values)
            {
                foreach (Deal d in c.PDDealsAffectedByConversation.Values)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        d.totalOpportunitiesYearly += Int32.Parse(d.contactHistoryStringArray[i]);
                        d.totalPiYearly += Int32.Parse(d.piHistoryStringArray[i]);
                        d.totalCeYearly += Int32.Parse(d.ceHistoryStringArray[i]);
                        d.totalCeDoYearly += Int32.Parse(d.ceDoHistoryStringArray[i]);
                    }
                }
            }
        }


        //? can be removed if the other processConversation is working longer needed
        private static void OLD_ProcessConversations()
        {
            //Process each conversation and update the Deal fields
            foreach (Conversation conversation in listOfConversations.Values)
            {

                foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                {
                    /*
                        * Increase the total number of opportunities irrespective of the autoUpdateDate since total number of opportunites start from 0
                        */
                    d.totalOpportunities30Days++;
                }

                if (conversation.dictOfTags.ContainsKey("tag_2zbt1"))
                {
                    //! IMPORTANT: Contains PI tag
                    var piTagDate = conversation.dictOfTags["tag_2zbt1"].tagCreationDate;
                    // Updating latest PI field for all deals affected by this conversation
                    foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                    {
                        /*
                            * Increase the total number of PIs irrespective of the autoUpdateDate since total number of PIs start from 0
                            */
                        deal.totalPI30Days++;
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
                else if (conversation.dictOfTags.ContainsKey("tag_2qf6t"))
                {
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
                                deal.totalStaleCE30Days++;
                                Console.WriteLine("Stale CE for " + conversation.subject + " because CE was tagged on " + TimestampToLocalTime(ceTagDate) + " and CE DO was tagged on " + TimestampToLocalTime(ceDoTagDate) + " which is AFTER " + conversation.CEOpenWindowDays + " days");
                            }

                        }
                    }
                    else
                    {
                        //! IMPORTANT: Does not contain a 'CE DO' tag (neither fail tag)
                        //! IMPORTANT: Could be stale or open CE
                        decimal currTimestamp = (decimal)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        if (currTimestamp - ceTagDate <= conversation.CEOpenWindowDays * 86400)
                        {
                            //Open CE identified
                            foreach (Deal deal in conversation.PDDealsAffectedByConversation.Values)
                            {
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
                        else
                        {
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

        private static DateTime TimestampToLocalTime(decimal timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the timestamp (number of seconds since the Epoch) to be converted
            dateTime = dateTime.AddSeconds((double)timestamp).ToLocalTime();
            return dateTime;
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

        public static void Logger(string filename, string lines)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(lines);
            }
        }
    }
}


