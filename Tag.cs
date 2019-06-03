using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontPipedriveIntegrationProject
{
    /*
     * Tags and their corresponding ids
     * [ce]      :   tag_2qf6t
     * [ce do]   :   tag_2qf79
     * [pi]      :   tag_2zbt1
     * [fail]    :   tag_2zbsl
     * [contact] :   tag_2zbs5
     * [billing] :   tag_36oud
     */
    class Tag
    {
        


        public decimal tagCreationDate;
        public string tagId;
        public string readableTagName;


        public Tag(dynamic e)
        {
            tagCreationDate = (decimal)e["emitted_at"];        
            tagId = (string)(e["target"]["data"]["id"]);
            readableTagName = (string)(e["target"]["data"]["name"]);
        }
    }
}
