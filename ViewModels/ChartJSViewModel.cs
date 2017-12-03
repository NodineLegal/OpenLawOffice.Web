using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels
{
    public class ChartJSViewModel
    {
        public class Data
        {
            public List<string> labels { get; set; }
            public List<Dataset> datasets { get; set; }

            public Data()
            {
                labels = new List<string>();
                datasets = new List<Dataset>();
            }
        }

        public class Dataset
        {
            public string label { get; set; }
            public List<double> data { get; set; }
            public List<string> backgroundColor { get; set; }
            public List<string> borderColor { get; set; }
            public string borderWidth { get; set; }

            public Dataset()
            {
                data = new List<double>();
                backgroundColor = new List<string>();
                borderColor = new List<string>();
            }
        }

        public class Options
        {
            public class Scales
            {
                public class Axes
                {
                    public class Ticks
                    {
                        public bool beginAtZero { get; set; }
                    }

                    public Ticks ticks { get; set; }
                }

                public List<Axes> yAxes { get; set; }
            }

            public Scales scales { get; set; }
        }

        public string type { get; set; }
        public Data data { get; set; }
        public Options options { get; set; }

        public ChartJSViewModel()
        {
            type = null;
            data = new Data();
            options = new Options();
        }
    }
}