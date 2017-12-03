using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Leads
{
    public class DashboardGraphDataViewModel
    {
        public List<ChartItem> LeadsBySource { get; set; }

        public DashboardGraphDataViewModel()
        {
            LeadsBySource = new List<ChartItem>();
        }

        public class ChartItem
        {
            public double value { get; set; }
            public string color { get; set; }
            public string highlight { get; set; }
            public string label { get; set; }
        }
    }
}