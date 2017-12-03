using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Leads
{
    public class CreateLeadViewModel : LeadViewModel
    {
        public int? ExistingContactId { get; set; }
        public string ExistingContactDisplayName { get; set; }
        public int? ExistingSourceId { get; set; }
        public string ExisitingSourceTitle { get; set; }
        public int? ExistingSourceContactId { get; set; }
        public string ExistingSourceContactDisplayName { get; set; }
        public int? ExistingFeeToId { get; set; }
        public string ExistingFeeToDisplayName { get; set; }
    }
}