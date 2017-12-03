using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Opportunities
{
    public class CreateOpportunityViewModel
    {
        public OpportunityViewModel Opportunity { get; set; }
        public Matters.MatterViewModel Matter { get; set; }
        public List<Matters.CourtTypeViewModel> CourtTypes { get; set; }
        public List<Matters.CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }
        public List<Matters.CourtSittingInCityViewModel> CourtSittingInCities { get; set; }

        public Matters.MatterContactViewModel Contact1 { get; set; }
        public Matters.MatterContactViewModel Contact2 { get; set; }
        public Matters.MatterContactViewModel Contact3 { get; set; }
        public Matters.MatterContactViewModel Contact4 { get; set; }
        public Matters.MatterContactViewModel Contact5 { get; set; }
        public Matters.MatterContactViewModel Contact6 { get; set; }
        public Matters.MatterContactViewModel Contact7 { get; set; }
        public Matters.MatterContactViewModel Contact8 { get; set; }
        public Matters.MatterContactViewModel Contact9 { get; set; }
        public Matters.MatterContactViewModel Contact10 { get; set; }

        public CreateOpportunityViewModel()
        {
            CourtTypes = new List<Matters.CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<Matters.CourtGeographicalJurisdictionViewModel>();
            CourtSittingInCities = new List<Matters.CourtSittingInCityViewModel>();

            Contact1 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact2 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact3 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact4 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact5 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact6 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact7 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact8 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact9 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact10 = new Matters.MatterContactViewModel() { Matter = new Matters.MatterViewModel(), Contact = new Contacts.ContactViewModel() };
        }
    }
}