using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontPipedriveIntegrationProject
{
    class Tag
    {
        /*
             * Tags and their corresponding ids
             * [ce]      :   tag_2qf6t
             * [ce do]   :   tag_2qf79
             * [pi]      :   tag_2zbt1
             * [fail]    :   tag_2zbsl
             * [contact] :   tag_2zbs5
             */

        /*! important: readable name can be identified from the name
             */
        public decimal tagCreationDate;
        public string tagId;
        public string readableTagName;
        //? public string convId;
        //? public string convEmail;


        public Tag(dynamic e)
        {
            tagCreationDate = (decimal)e["emitted_at"];
            // tagCreationDate = (decimal)res["created_at"];           
            tagId = (string)(e["target"]["data"]["id"]);
            readableTagName = (string)(e["target"]["data"]["name"]);
            //? convId = (string)(res["conversation"]["id"]);
            //? convEmail = (string)(res["conversation"]["recipient"]["handle"]);
            //readableTagName = (string)e["name"];
            ; //? BREAKPOINT. PLEASE REMOVE

                    /*Could store in a dictionary before batch updating but would be too much hassle*/
            
        }

        /*

        public Tag(dynamic res)
        {
            
            tagCreationDate = (decimal)res["emitted_at"];
            //! NEED TO REMOVE, ONLY FOR DEBUGGING
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((double)tagCreationDate).ToLocalTime();
            tagId = (string)(res["target"]["data"]["id"]);
            //? convId = (string)(res["conversation"]["id"]);
            //? convEmail = (string)(res["conversation"]["recipient"]["handle"]);
            //todo change this to use the "name" instead
            switch (tagId)
            {
                case "tag_2qf6t":
                    readableTagName = "CE";
                    break;
                case "tag_2qf79":
                    readableTagName = "CE DO";
                    break;
                case "tag_2zbt1":
                    readableTagName = "PI";
                    break;
                case "tag_2zbsl":
                    readableTagName = "FAIL";
                    break;
                case "tag_2zbs5":
                    readableTagName = "CONTACT";
                    break;

                    // Could store in a dictionary before batch updating but would be too much hassle
            }
        }
    */
    }
}
