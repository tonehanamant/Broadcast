using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Enums
{
    //Order is important because jobs are prioritized by the order of the queues
    public enum QueueEnum
    {
        [Display(Name = "campaignaggregation")]
        CampaignAggregation,
        [Display(Name = "default")]
        Default
    }
}
