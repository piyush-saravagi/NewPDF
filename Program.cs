using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.ComponentModel;

//todo IMPROVE RUNTIME EFFICIENCY BY MERGING MAIL PROCESSING WITH PD PROCESSING 
namespace FrontPipedriveIntegrationProject
{
    class Program
    {
        public const Int32 DAYS_TO_SCAN =  30;
        static Dictionary<string, Conversation> listOfConversations = new Dictionary<string, Conversation>();
        
        static Int32 currTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        static readonly Int32 timestamp30daysAgo = currTimestamp - DAYS_TO_SCAN * 86400;

        static readonly string LOG_FILE_NAME = @"C:\Users\psaravagi\Desktop\NewPDF\bin\Debug\" + currTimestamp.ToString() + ".txt";

        static void Main(string[] args)
        {
            ApiAccessHelper.PD_API_KEY = "0b9f8a7f360f41c3264ab14ed5d2a760ecaf39f3";
            ApiAccessHelper.FRONT_API_KEY = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY29wZXMiOlsic2hhcmVkOioiXSwiaWF0IjoxNTU2MzEzNjI3LCJpc3MiOiJmcm9udCIsInN1YiI6ImxlYW5zZXJ2ZXIiLCJqdGkiOiI5MDZkYTc3NjA2NWVkOTA5In0.b28IHdaeo0YXwq4dy-xEbzG54RkHnXcOwrbMpbJ5LyY";

            ScanFrontEmails();
            ProcessConversations(currTimestamp);
            Console.WriteLine("==============================");
            UpdateDealFields();

            //todo call ClearHistoryFields() once every new year

            //Passing command line argument "send-email" sends out an email to the team inbox
            if (args.Length != 0 && args.Contains("send-email"))
            {
                EmailSender emailSender = new EmailSender();
                emailSender.mail.Subject += (TimestampToLocalTime(currTimestamp).ToString(" MM/dd"));
                GenerateEmailBody(emailSender);
                emailSender.SendMessage();
            }
            Console.ReadKey();
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


            List<string> idsOfinboxesThatAffectPdFields = new List<string>(new string[] { "inb_6061", "inb_600h", "inb_g84l" });

            foreach (string inboxId in idsOfinboxesThatAffectPdFields)
            {
                //!0. PAGINATION
                Console.WriteLine("Conversations from " + inboxId + "\n___________________");
                var response = ApiAccessHelper.GetResponseFromFrontApi(String.Format("/inboxes/{0}/conversations", inboxId));
                bool hasNextPage = true;
                int count = 0;
                while (hasNextPage)
                {
                    var allConvInOneYear = response["_results"];

                    foreach (var conversation in allConvInOneYear)
                    {


                        string convId = conversation["id"];

                        Console.WriteLine("\nScanned conv: " + conversation["subject"]);
                        Console.WriteLine("Last message on " + TimestampToLocalTime(conversation["last_message"]["created_at"]));

                        string eventsRelativeUrl = conversation["_links"]["related"]["events"].Replace("https://api2.frontapp.com", "");
                        var conversationEvents = ApiAccessHelper.GetResponseFromFrontApi(eventsRelativeUrl)["_results"];


                        if (!listOfConversations.TryGetValue(convId, out Conversation c))
                        {

                            //conversation not present in listOfConversations. Need to create and add a new one
                            c = new Conversation(conversation, conversationEvents);
                            // Adding the conversation to our dictionary 

                            listOfConversations.Add(c.id, c);
                        }

                        //Get the last event for this conversation

                        var latestEventDate = conversationEvents[0]["emitted_at"];

                        //if (c.lastMessage < timestamp30daysAgo)
                        //    return;
                        if (latestEventDate < timestamp30daysAgo)
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
                        response = ApiAccessHelper.GetResponseFromFrontApi(pageToken);
                        count++;
                    }
                }
            }
        }


        private static void ProcessConversations(decimal currTimestamp)
        {
           string logReason;

            //Process each conversation and update the Deal fields
            foreach (Conversation conversation in listOfConversations.Values)
            {

                Logger(LOG_FILE_NAME, "");
                Logger(LOG_FILE_NAME, String.Format("Conversation subject: {0}", conversation.subject));
                Logger(LOG_FILE_NAME, String.Format("Conversation id: {0}", conversation.id));
                Logger(LOG_FILE_NAME, String.Format("Email IDs: {0}", String.Join(", ", conversation.setOfEmails)));
                
                var linqQuery = from deal in conversation.PDDealsAffectedByConversation.Values select deal.title;
                Logger(LOG_FILE_NAME, String.Format("Pipedrive deals affected: {0}", String.Join(", ", linqQuery)));


                if (conversation.dictOfTags.ContainsKey(Tag.BILLING_TAG_ID)) {
                    //billing - not considered an opportunity
                    Logger(LOG_FILE_NAME, "Billing tag - not processing this conversation any further");
                    continue;
                }

                // Every conversation is an opportunity
                foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                {
                    Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of {4}", d.title, "totalOpportunities30Days", d.totalOpportunities30Days, d.totalOpportunities30Days + 1, "new conversation: " + conversation.subject));
                    d.totalOpportunities30Days++;
                    if (d.autoUpdateDateTimestamp == default(decimal) || conversation.createdAt > d.autoUpdateDateTimestamp)
                    {    // Conversation created after last update, need to update history
                        // We have a new opportunity (CE/Non-CE doesn't matter)
                        //Update in history
                        int monthToUpdate = TimestampToLocalTime(conversation.createdAt).Month - 1;
                        Int32 temp = Int32.Parse(d.contactHistoryStringArray[monthToUpdate]) + 1;
                        //Update it back to the deal
                        d.contactHistoryStringArray[monthToUpdate] = temp.ToString();

                    }
                }

                if (conversation.dictOfTags.ContainsKey(Tag.CE_TAG_ID))
                {
                    // CE opportunity
                    Tag ce = conversation.dictOfTags[Tag.CE_TAG_ID];
                    foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                    {
                        d.totalCE30Days++;  //CE resolved/ unresolved/ failed etc are all still considered CEs
                        Logger(LOG_FILE_NAME, String.Format("{0}: Updated {1} from {2} to {3} because of - {4}", d.title, "totalCE30Days", d.totalCE30Days - 1, d.totalCE30Days, "marked CE on " + TimestampToLocalTime(ce.tagCreationDate)));

                        if (d.autoUpdateDateTimestamp == default(decimal) || ce.tagCreationDate > d.autoUpdateDateTimestamp)
                        {    // Conversation marked CE after last update, need to update history
                             // We have a new CE (Resolved/Unresolved doesn't matter)
                             //Update in history
                            int monthToUpdate = TimestampToLocalTime(ce.tagCreationDate).Month - 1;
                            Int32 temp = Int32.Parse(d.ceHistoryStringArray[monthToUpdate]) + 1;
                            //Update it back to the deal
                            d.ceHistoryStringArray[monthToUpdate] = temp.ToString();
                            //todo add log to history if needed 
                        }
                    }

                    if (conversation.dictOfTags.ContainsKey(Tag.CE_DO_TAG_ID))
                    {

                        Tag ce_do = conversation.dictOfTags[Tag.CE_DO_TAG_ID];
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

                                if (d.autoUpdateDateTimestamp == default(decimal) || ce_do.tagCreationDate > d.autoUpdateDateTimestamp)
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
                    else if (conversation.dictOfTags.ContainsKey(Tag.FAIL_TAG_ID))
                    {
                        //! FAILED RESOLVED CE
                        //! CONTAINS CE TAG AND FAIL TAG
                        //todo UPDATE FIELDS

                        Tag fail = conversation.dictOfTags[Tag.FAIL_TAG_ID];
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
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleUnresolvedCe30Days", d.staleUnresolvedCe30Days, d.staleUnresolvedCe30Days + 1, "marked CE on " + ce.tagCreationDate + " and today's date is " + TimestampToLocalTime(currTimestamp) + " which is outside the window of " + conversation.CEOpenWindowDays + " days"));
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
                    if (conversation.dictOfTags.ContainsKey(Tag.PI_TAG_ID))
                    {
                        Tag pi = conversation.dictOfTags[Tag.PI_TAG_ID];
                        if (pi.tagCreationDate - conversation.createdAt < conversation.OpportunityOpenWindowDays * 86400)
                        {
                            //! SUCCESSFUL RESOLVED OPPORTUNITY
                            //! CONTAINS A PI TAG, BUT NO CE TAG

                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {

                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "totalPi30Days", d.totalPI30Days, d.totalPI30Days + 1, "marked PI on " + TimestampToLocalTime(pi.tagCreationDate)));
                                d.totalPI30Days++;
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "successfulResolvedOpportunity30Days", d.successfulResolvedOpportunity30Days, d.successfulResolvedOpportunity30Days + 1, "conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and marked PI on " + TimestampToLocalTime(pi.tagCreationDate)));

                                d.successfulResolvedOpportunity30Days++;
                                if (d.lastPiDate < pi.tagCreationDate)
                                {
                                    logReason = "marked PI on " + TimestampToLocalTime(pi.tagCreationDate) + " and previous PI was on " + TimestampToLocalTime(d.lastPiDate); Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "lastPiDate", TimestampToLocalTime(d.lastPiDate), TimestampToLocalTime(pi.tagCreationDate), logReason));
                                    d.lastPiDate = pi.tagCreationDate;
                                }

                                if (d.autoUpdateDateTimestamp == default(decimal) || pi.tagCreationDate > d.autoUpdateDateTimestamp)
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
                            foreach (Deal d in conversation.PDDealsAffectedByConversation.Values)
                            {
                                logReason = "conversation created on " + TimestampToLocalTime(conversation.createdAt) + " and maked PI on " + TimestampToLocalTime(pi.tagCreationDate) + " which is outside the " + conversation.OpportunityOpenWindowDays + " days window";
                                Logger(LOG_FILE_NAME, String.Format("{0}: Changed {1} from {2} to {3} because of - {4}", d.title, "staleResolvedOpportunity30Days", d.staleResolvedOpportunity30Days, d.staleResolvedOpportunity30Days + 1, logReason));
                                d.staleResolvedOpportunity30Days++;
                            }
                        }
                    }
                    else if (conversation.dictOfTags.ContainsKey(Tag.FAIL_TAG_ID))
                    {
                        //! FAILED OPPORTUNITY
                        //! CONTAINS A FAIL TAG, BUT NO CE
                        Tag fail = conversation.dictOfTags[Tag.FAIL_TAG_ID];
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
            foreach (Deal d in Conversation.listOfAllDeals.Values)
            {
                d.autoUpdateDateTimestamp = currTimestamp;
                for (int i = 0; i < 12; i++)
                {
                    d.totalOpportunitiesYearly += Int32.Parse(d.contactHistoryStringArray[i]);
                    d.totalPiYearly += Int32.Parse(d.piHistoryStringArray[i]);
                    d.totalCeYearly += Int32.Parse(d.ceHistoryStringArray[i]);
                    d.totalCeDoYearly += Int32.Parse(d.ceDoHistoryStringArray[i]);
                }
            }

        }


        

        
        //! Updates the PD fields for all deals
        public static void UpdateDealFields()
        {
            // Multiple conversations could bring up the same deals again and again
            // Making sure they are updated just once
           
            foreach (Deal d in Conversation.listOfAllDeals.Values)
            {
                var data = d.GetPostableData();

                var oldData = ApiAccessHelper.GetResponseFromPipedriveApi("/deals/" + d.id);

                Console.WriteLine("\n" + d.title);
                Logger(LOG_FILE_NAME, "\n" + d.title + "\n----------------------");
                foreach (var dataElement in data)
                {
                    var oldValue = "";
                    if (oldData[dataElement.Key] != null)
                        oldValue = oldData[dataElement.Key].ToString();
                    Console.WriteLine(String.Format("{0,-40}\t{1,-30}\t=>\t{2,15}", Deal.pdFieldNames[dataElement.Key], oldValue, dataElement.Value));

                    Logger(LOG_FILE_NAME, String.Format("{0,-40}\t{1,-30}\t=>\t{2,15}", Deal.pdFieldNames[dataElement.Key], oldValue, dataElement.Value));
                }
                ApiAccessHelper.PostPipedriveJson("/deals/" + d.id, data, "PUT");
            }
        }
        


        private static void AppendConversationsByAssigneeToEmailBody(EmailSender emailSender, string assignee, dynamic allConvByState) {
            

            int totalPisForAssignee = 0;
            foreach (Conversation c in allConvByState["successfulResolvedOpportunity"]) {
                if(c.assignee == assignee)
                    totalPisForAssignee++;
            }
            int totalCeDosForAssignee = 0;
            foreach (Conversation c in allConvByState["successfulResolvedCe"])
            {
                if (c.assignee == assignee)
                    totalCeDosForAssignee++;

            }
            int openConvforAssignee = 0;
            foreach (Conversation c in allConvByState["openUnresolvedCe"])
            {
                if (c.assignee == assignee)
                    openConvforAssignee++;
            }
            foreach (Conversation c in allConvByState["staleUnresolvedCe"])
            {
                if (c.assignee == assignee)
                    openConvforAssignee++;
            }foreach (Conversation c in allConvByState["openUnresolvedOpportunity"])
            {
                if (c.assignee == assignee)
                    openConvforAssignee++;
            }
            foreach (Conversation c in allConvByState["staleUnresolvedOpportunity"])
            {
                if (c.assignee == assignee)
                    openConvforAssignee++;
            }




            emailSender.AppendLineToEmailBody(String.Format("<b>{0}: {1} PIs, {2} CE-DOs and {3} open opportunties</b><hr>", assignee, totalPisForAssignee, totalCeDosForAssignee, openConvforAssignee));
            int counter = 1;
            foreach (Conversation c in allConvByState["openUnresolvedCe"])
            {
                if (c.assignee == assignee)
                    if (c.PDDealsAffectedByConversation.Count != 0)
                    {
                        string link = @"https://app.frontapp.com/open/" + c.id;
                        Tag ceTag = c.dictOfTags[Tag.CE_TAG_ID];
                        Int32 daysOpen = (Int32)(currTimestamp - ceTag.tagCreationDate)/86400; //converting seconds to days
                        emailSender.AppendLineToEmailBody(String.Format(counter+++". <a href={0}></b>COMPELLING EVENT: {1}</b></a> | Open CE since {2} ({3} days | {4})", link, c.subject, TimestampToLocalTime(ceTag.tagCreationDate).ToString("MM-dd-yyyy"), daysOpen, c.primaryEmail));
                        
                    }
            }
            foreach (Conversation c in allConvByState["staleUnresolvedCe"])
            {
                if (c.assignee == assignee)
                    if (c.PDDealsAffectedByConversation.Count != 0)
                    {
                        string link = @"https://app.frontapp.com/open/" + c.id;
                        Tag ceTag = c.dictOfTags[Tag.CE_TAG_ID];
                        Int32 daysOpen = (Int32)(currTimestamp - ceTag.tagCreationDate) / 86400; //converting seconds to days
                        emailSender.AppendLineToEmailBody(String.Format(counter++ + ". <b><a href={0}><font color=red>COMPELLING EVENT: {1}</font></a></b> | Open CE since {2} ({3} days) | {4}", link, c.subject, TimestampToLocalTime(ceTag.tagCreationDate).ToString("MM-dd-yyyy"), daysOpen, c.primaryEmail));
                        
                    }
            }
            foreach (Conversation c in allConvByState["staleUnresolvedOpportunity"])
            {
                if (c.assignee == assignee)
                    if (c.PDDealsAffectedByConversation.Count != 0)
                    {
                        string link = @"https://app.frontapp.com/open/" + c.id;
                        Int32 daysOpen = (Int32)(currTimestamp - c.createdAt) / 86400; //converting seconds to days
                        emailSender.AppendLineToEmailBody(String.Format(counter++ + ". <a href={0}><font color=red>{1}</font></a> | Open  since {2} ({3} days) | {4}", link, c.subject, TimestampToLocalTime(c.createdAt).ToString("MM-dd-yyyy"), daysOpen, c.primaryEmail));
                    }
            }
            foreach (Conversation c in allConvByState["openUnresolvedOpportunity"])
            {
                if (c.assignee == assignee)
                    if (c.PDDealsAffectedByConversation.Count != 0)
                    {
                        string link = @"https://app.frontapp.com/open/" + c.id;
                        Int32 daysOpen = (Int32)(currTimestamp - c.createdAt) / 86400; //converting seconds to days
                        emailSender.AppendLineToEmailBody(String.Format(counter++ + ". <a href={0}>{1}</a> | Open since {2} ({3} days) | {4}", link, c.subject, TimestampToLocalTime(c.createdAt).ToString("MM-dd-yyyy"), daysOpen, c.primaryEmail));
                    }
            }
            emailSender.AppendLineToEmailBody("");
        }


        private static void GenerateEmailBody(EmailSender emailSender)
        {
            //todo improve runtime by merging with ProcessConversations
            // Better option would have been to use hashsets, but to keep things simple, using lists
            List<Conversation> billing = new List<Conversation>();

            List<Conversation> successfulResolvedCe = new List<Conversation>();
            List<Conversation> staleSuccessfulResolvedCe = new List<Conversation>();
            List<Conversation> failedCe = new List<Conversation>();
            List<Conversation> openUnresolvedCe = new List<Conversation>();
            List<Conversation> staleUnresolvedCe = new List<Conversation>();

            List<Conversation> successfulResolvedOpportunity = new List<Conversation>();
            List<Conversation> staleSuccessfulResolvedOpportunities = new List<Conversation>();
            List<Conversation> failedOpportunity = new List<Conversation>();
            List<Conversation> openUnresolvedOpportunity = new List<Conversation>();
            List<Conversation> staleUnresolvedOpportunity = new List<Conversation>();

            Dictionary<string, List<Conversation>> allConvByState = new Dictionary<string, List<Conversation>>();
            

            // Categorizing deals into their states
            foreach (Conversation conversation in listOfConversations.Values) {
                if (conversation.dictOfTags.ContainsKey(Tag.BILLING_TAG_ID)) {
                    billing.Add(conversation);
                    continue;
                }

                if (conversation.dictOfTags.ContainsKey(Tag.CE_TAG_ID))
                {
                    // CE opportunity
                    Tag ce = conversation.dictOfTags[Tag.CE_TAG_ID];
                    
                    if (conversation.dictOfTags.ContainsKey(Tag.CE_DO_TAG_ID))
                    {

                        Tag ce_do = conversation.dictOfTags[Tag.CE_DO_TAG_ID];
                        if (ce_do.tagCreationDate - ce.tagCreationDate < conversation.CEOpenWindowDays * 86400)
                        {
                            //! SUCCESSFUL RESOLVED CE 
                            //! CONTAINS CE TAG AND WAS MARKED CE-DO ON TIME
                            if(conversation.PDDealsAffectedByConversation.Count != 0)
                            successfulResolvedCe.Add(conversation);
                        }
                        else
                        {
                            //! STALE RESOLVED CE
                            //! CONTAINS CE AND CE DO TAGS BUT WAS NOT MARKED CE-DO ON TIME 
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                staleSuccessfulResolvedCe.Add(conversation);
                        }
                    }
                    else if (conversation.dictOfTags.ContainsKey(Tag.FAIL_TAG_ID))
                    {
                        //! FAILED RESOLVED CE
                        //! CONTAINS CE TAG AND FAIL TAG
                        if (conversation.PDDealsAffectedByConversation.Count != 0)

                            failedCe.Add(conversation);
                    }
                    else
                    {
                        // Unresolved CE, not marked with CE-DO or FAIL tags
                        if (currTimestamp - ce.tagCreationDate < conversation.CEOpenWindowDays * 86400)
                        {
                            //!  OPEN UNRESOLVED CE 
                            //! CONTAINS CE TAG, WAS NOT MARKED CE-DO OR FAIL AND IS WITHIN THE TIME WINDOW FOR OPEN CE
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                openUnresolvedCe.Add(conversation);
                        }
                        else
                        {
                            //! STALE UNRESOLVED CE
                            //! CONTAINS CE AND WAS NOT MARKED CE-DO OR FAIL ON TIME
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                staleUnresolvedCe.Add(conversation);
                        }
                    }
                }
                else
                {
                    // Non CE opportunity
                    if (conversation.dictOfTags.ContainsKey(Tag.PI_TAG_ID))
                    {
                        Tag pi = conversation.dictOfTags[Tag.PI_TAG_ID];
                        if (pi.tagCreationDate - conversation.createdAt < conversation.OpportunityOpenWindowDays * 86400)
                        {
                            //! SUCCESSFUL RESOLVED OPPORTUNITY
                            //! CONTAINS A PI TAG, BUT NO CE TAG
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                successfulResolvedOpportunity.Add(conversation);
                        }
                        else
                        {
                            //! STALE RESOLVED OPPORTUNITY
                            //! CONTAINS A PI TAG BUT OUTSIDE THE WINDOW
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                staleSuccessfulResolvedOpportunities.Add(conversation);
                        }
                    }
                    else if (conversation.dictOfTags.ContainsKey(Tag.FAIL_TAG_ID))
                    {
                        //! FAILED OPPORTUNITY
                        //! CONTAINS A FAIL TAG, BUT NO CE
                        if (conversation.PDDealsAffectedByConversation.Count != 0)

                            failedOpportunity.Add(conversation);
                    }
                    else
                    {
                        //Unresolved Opportunity
                        if (currTimestamp - conversation.createdAt < conversation.OpportunityOpenWindowDays * 86400)
                        {
                            //! OPEN UNRESOLVED OPPORTUNITY
                            //! DOES NOT CONTAIN A CE, FAIL, PI TAG BUT IS WITHIN THE OPEN WINDOW
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                openUnresolvedOpportunity.Add(conversation);

                        }
                        else
                        {
                            //! STALE UNRESOLVED OPPORTUNITY
                            //! DOES NOT CONTAIN A CE, FAIL, PI TAG BUT IS WITHIN THE OPEN WINDOW
                            if (conversation.PDDealsAffectedByConversation.Count != 0)

                                staleUnresolvedOpportunity.Add(conversation);
                        }
                    }
                }
            }

            allConvByState.Add("successfulResolvedCe", successfulResolvedCe);
            allConvByState.Add("staleSuccessfulResolvedCe", staleSuccessfulResolvedCe);
            allConvByState.Add("failedCe", failedCe);
            allConvByState.Add("openUnresolvedCe", openUnresolvedCe);
            allConvByState.Add("staleUnresolvedCe", staleUnresolvedCe);
            allConvByState.Add("successfulResolvedOpportunity", successfulResolvedOpportunity);
            allConvByState.Add("staleSuccessfulResolvedOpportunities", staleSuccessfulResolvedOpportunities);
            allConvByState.Add("failedOpportunity", failedOpportunity);
            allConvByState.Add("openUnresolvedOpportunity", openUnresolvedOpportunity);
            allConvByState.Add("staleUnresolvedOpportunity", staleUnresolvedOpportunity);

            var convByAssignee = (successfulResolvedCe.Concat(successfulResolvedOpportunity)).GroupBy(x => x.assignee).Select(g => new { assignee = g.Key, conversations = g }).OrderByDescending(g => g.conversations.Count());

            List<string> assignees = new List<string>();

            foreach (var x in convByAssignee) {
                assignees.Add(x.assignee);
            }
            if (!assignees.Contains("Keegan")) assignees.Add("Keegan");
            if (!assignees.Contains("Jill")) assignees.Add("Jill");
            if (!assignees.Contains("Mike")) assignees.Add("Mike");
            if (!assignees.Contains("Piyush")) assignees.Add("Piyush");
           

            emailSender.AppendLineToEmailBody("Hey team,<p>Here is a summary of how we are performing with the opportunities we've had in the past " + DAYS_TO_SCAN +" days since " + TimestampToLocalTime(currTimestamp).ToString()+"<br>");
            var totalOp = successfulResolvedCe.Count + staleSuccessfulResolvedCe.Count + failedCe.Count + openUnresolvedCe.Count + staleUnresolvedCe.Count + successfulResolvedOpportunity.Count + staleSuccessfulResolvedOpportunities.Count + failedOpportunity.Count + openUnresolvedOpportunity.Count + staleUnresolvedOpportunity.Count;

            emailSender.AppendLineToEmailBody(String.Format(@"<ul><li><b>Total opportunities (CE + Non CE):</b> {0}</li><li><b>Unresolved CEs:</b> {1} stale and {2} not stale</li><li><b>Unresolved Opportunities (Non-CE):</b> {3} stale and {4} not stale</li><li><b>Recent:</b> {5} CE-DOs and {6} PIs</li></ul>", totalOp, staleUnresolvedCe.Count, openUnresolvedCe.Count, staleUnresolvedOpportunity.Count, openUnresolvedOpportunity.Count, successfulResolvedCe.Count, successfulResolvedOpportunity.Count));

            emailSender.AppendLineToEmailBody("These are the unresolved conversations grouped by the person they are assigned to. Red indicates a conversation that has gone stale, bold indicates a compelling event, and a bold in red indicates a stale compelling event <br>");

            foreach (var a in assignees)
            {
                AppendConversationsByAssigneeToEmailBody(emailSender, a, allConvByState);
            }
            

            //======================================================================
            emailSender.AppendLineToEmailBody("<br><br><br><br><br><br><br>Details<br>");
            //======================================================================
            
            emailSender.AppendLineToEmailBody("CE-DOs ON TIME. Good job on these!");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in successfulResolvedCe) {    
                emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id+")");
            }
            emailSender.AppendLineToEmailBody("");


            emailSender.AppendLineToEmailBody("CE-DOs AFTER GOING STALE. Good job but try improve the time taken to reach DO");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in staleSuccessfulResolvedCe)
            {
                emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Failed CEs. These should not happen often");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in failedCe)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Open unresolved CEs. Quick, get them to CE-DOs before they go stale!");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in openUnresolvedCe)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Unresolved CEs that have gone stale - Immediate action needed");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in staleUnresolvedCe)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            //====
            emailSender.AppendLineToEmailBody("PI's ON TIME. Good job on these!");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in successfulResolvedOpportunity)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");


            emailSender.AppendLineToEmailBody("PIs AFTER GOING STALE. Good job but try improve the time taken to reach DO");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in staleSuccessfulResolvedOpportunities)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Failed Opportunities. These should not happen often");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in failedOpportunity)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Open unresolved opportunities. Quick, get them to PI before they go stale!");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in openUnresolvedOpportunity)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Unresolved opportunities that have gone stale - Immediate action needed");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in staleUnresolvedOpportunity)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");

            emailSender.AppendLineToEmailBody("Billing - did not affect our states");
            emailSender.AppendLineToEmailBody("---------------------------------------------");
            foreach (Conversation c in billing)
            {
                if (c.PDDealsAffectedByConversation.Count != 0)
                {
                    emailSender.AppendLineToEmailBody(c.subject + @" (https://app.frontapp.com/open/" + c.id + ")");
                }
            }
            emailSender.AppendLineToEmailBody("");
        }


        public static DateTime TimestampToLocalTime(decimal timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the timestamp (number of seconds since the Epoch) to be converted
            dateTime = dateTime.AddSeconds((double)timestamp).ToLocalTime();
            return dateTime;
        }

        public static void Logger(string filename, string lines)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(lines);
            }
        }

        public static void ClearHistoryFields()
        {
            var allDeals = ApiAccessHelper.GetResponseFromPipedriveApi("/deals");
            foreach (var d in allDeals)
            {
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { Deal.pdFieldKeys["PI_HISTORY_FIELD"], "0 0 0 0 0 0 0 0 0 0 0 0" },
                    { Deal.pdFieldKeys["CONTACT_HISTORY_FIELD"], "0 0 0 0 0 0 0 0 0 0 0 0" },
                    { Deal.pdFieldKeys["CE_HISTORY_FIELD"], "0 0 0 0 0 0 0 0 0 0 0 0" },
                    { Deal.pdFieldKeys["CE_DO_HISTORY_FIELD"], "0 0 0 0 0 0 0 0 0 0 0 0" }
                };
                ApiAccessHelper.PostPipedriveJson("/deals/" + d["id"], data, "PUT");
            }
        }
    }
}


