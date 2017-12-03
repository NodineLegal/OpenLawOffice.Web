// -----------------------------------------------------------------------
// <copyright file="OpportunitiesController.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AutoMapper;
    using System.Data;
    using System.Linq;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class OpportunitiesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            int colorKeeper = -1;
            string accountFilter;
            int? probabilityFilter = null, stageFilter = null;
            bool? closedFilter = null;
            ViewModels.Opportunities.DashboardViewModel viewModel = new ViewModels.Opportunities.DashboardViewModel();
            System.Drawing.Color[] availableColors = new System.Drawing.Color[]
            {
                System.Drawing.Color.FromArgb(228,26,28),
                System.Drawing.Color.FromArgb(55,126,184),
                System.Drawing.Color.FromArgb(77,175,74),
                System.Drawing.Color.FromArgb(152,78,163),
                System.Drawing.Color.FromArgb(255,127,0),
                System.Drawing.Color.FromArgb(166,86,40),
                System.Drawing.Color.FromArgb(247,129,191),
            };
            ViewModels.ChartJSViewModel chartOppSourceVM, chartOppConversionVM;

            accountFilter = Request["accountFilter"];

            if (!string.IsNullOrEmpty(Request["probabilityFilter"]))
            {
                int i;
                if (int.TryParse(Request["probabilityFilter"], out i))
                    probabilityFilter = i;
            }

            if (!string.IsNullOrEmpty(Request["stageFilter"]))
            {
                int i;
                if (int.TryParse(Request["stageFilter"], out i))
                    stageFilter = i;
            }

            if (!string.IsNullOrEmpty(Request["closedFilter"]))
            {
                if (Request["closedFilter"] != "Both")
                {
                    bool i;
                    if (bool.TryParse(Request["closedFilter"], out i))
                        closedFilter = i;
                }
            }
            else
                closedFilter = false;

            chartOppSourceVM = new ViewModels.ChartJSViewModel()
            {
                type = "pie"
            };
            chartOppSourceVM.data.datasets.Add(new ViewModels.ChartJSViewModel.Dataset());

            chartOppConversionVM = new ViewModels.ChartJSViewModel()
            {
                type = "bar"
            };

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                viewModel.Opportunities = new List<ViewModels.Opportunities.OpportunityViewModel>();
                viewModel.StageTypes = new List<ViewModels.Opportunities.OpportunityStageViewModel>();

                Data.Opportunities.OpportunityStage.List(conn, false).ForEach(x =>
                {
                    viewModel.StageTypes.Add(Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(x));
                });

                Data.Opportunities.Opportunity.List(accountFilter, probabilityFilter, stageFilter, closedFilter, conn, false).ForEach(x =>
                {
                    ViewModels.Opportunities.OpportunityViewModel ovm = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(x);
                    ovm.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(x.Account.Id.Value));
                    ovm.Stage = Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(Data.Opportunities.OpportunityStage.Get(x.Stage.Id.Value));
                    viewModel.Opportunities.Add(ovm);
                });

                Data.Opportunities.Opportunity.OpportunitiesBySource(DateTime.UtcNow.AddDays(-30), conn, false).ForEach(x =>
                {
                    colorKeeper++;
                    if (colorKeeper >= availableColors.Length)
                        colorKeeper = 0;

                    chartOppSourceVM.data.labels.Add(x.Title);
                    chartOppSourceVM.data.datasets[0].backgroundColor.Add("rgba(" + availableColors[colorKeeper].R + ", " + availableColors[colorKeeper].G + ", " + availableColors[colorKeeper].B + ", 1)");
                    chartOppSourceVM.data.datasets[0].data.Add(x.Count.Value);
                    //chartVM.data.datasets[0].borderWidth = "1";
                });

                colorKeeper = -1;
                ViewModels.ChartJSViewModel.Dataset ds = new ViewModels.ChartJSViewModel.Dataset();
                Data.Opportunities.Opportunity.ListMatterConversionsGroupingByMonth(DateTime.UtcNow.AddYears(-1), conn, false).ForEach(x =>
                {
                    colorKeeper++;
                    if (colorKeeper >= availableColors.Length)
                        colorKeeper = 0;

                    chartOppConversionVM.data.labels.Add(x.Month.Value.ToString("MMM"));
                    ds.backgroundColor.Add("rgba(" + availableColors[colorKeeper].R + ", " + availableColors[colorKeeper].G + ", " + availableColors[colorKeeper].B + ", 1)");
                    //ds.label = x.Month.Value.ToString("MMM");
                    ds.data.Add(x.Count.Value);

                });
                chartOppConversionVM.data.datasets.Add(ds);
                chartOppConversionVM.options.scales = new ViewModels.ChartJSViewModel.Options.Scales();
                chartOppConversionVM.options.scales.yAxes = new List<ViewModels.ChartJSViewModel.Options.Scales.Axes>();
                chartOppConversionVM.options.scales.yAxes.Add(new ViewModels.ChartJSViewModel.Options.Scales.Axes()
                {
                    ticks = new ViewModels.ChartJSViewModel.Options.Scales.Axes.Ticks()
                    {
                        beginAtZero = true
                    }
                });
            }

            ViewBag.OpportunitySourceGraphData = Newtonsoft.Json.JsonConvert.SerializeObject(chartOppSourceVM,
                Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            ViewBag.OpportunityConversionGraphData = Newtonsoft.Json.JsonConvert.SerializeObject(chartOppConversionVM,
                Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(long id, bool preventRedirect = false)
        {
            ViewModels.Opportunities.OpportunityViewModel viewModel;
            Common.Models.Opportunities.Opportunity model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Opportunities.Opportunity.Get(id, conn, false);                

                if (model.Matter != null && model.Matter.Id.HasValue)
                {
                    if (!preventRedirect)
                        return RedirectToAction("Details", "Matters", new { id = model.Matter.Id.Value });
                    else
                        model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);
                }

                model.Lead = Data.Leads.Lead.Get(model.Lead.Id.Value, conn, false);
                if (model.Lead.Fee != null && model.Lead.Fee.Id.HasValue)
                {
                    model.Lead.Fee = Data.Leads.LeadFee.Get(model.Lead.Fee.Id.Value, conn, false);
                    if (model.Lead.Fee.To != null && model.Lead.Fee.To.Id.HasValue)
                        model.Lead.Fee.To = Data.Contacts.Contact.Get(model.Lead.Fee.To.Id.Value, conn, false);
                }
                model.Lead.Contact = Data.Contacts.Contact.Get(model.Lead.Contact.Id.Value, conn, false);
                model.Lead.Source = Data.Leads.LeadSource.Get(model.Lead.Source.Id.Value, conn, false);
                if (model.Lead.Source.Type != null && model.Lead.Source.Type.Id.HasValue)
                    model.Lead.Source.Type = Data.Leads.LeadSourceType.Get(model.Lead.Source.Type.Id.Value, conn, false);
                model.Lead.Status = Data.Leads.LeadStatus.Get(model.Lead.Status.Id.Value, conn, false);
                model.Account = Data.Contacts.Contact.Get(model.Account.Id.Value, conn, false);
                model.Stage = Data.Opportunities.OpportunityStage.Get(model.Stage.Id.Value, conn, false);
                if (model.Matter != null && model.Matter.Id.HasValue)
                    model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);

                // Tweaks
                // Probability is stored as a decimal (<1) need to change representation to percent
                model.Probability *= 100;

                viewModel = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(model);
                viewModel.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(model.Lead);
                if (model.Lead.Fee != null)
                {
                    viewModel.Lead.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(model.Lead.Fee);
                    if (model.Lead.Fee.To != null)
                        viewModel.Lead.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Lead.Fee.To);
                }
                viewModel.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Lead.Contact);
                viewModel.Lead.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(model.Lead.Source);
                if (model.Lead.Source.Type != null)
                    viewModel.Lead.Source.Type = Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(model.Lead.Source.Type);
                viewModel.Lead.Status = Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(model.Lead.Status);
                viewModel.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Account);
                viewModel.Stage = Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(model.Stage);
                if (model.Matter != null)
                    viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);

                viewModel.Contacts = new List<ViewModels.Contacts.ContactViewModel>();
                Data.Opportunities.OpportunityContact.ListForOpportunity(id, conn, false).ForEach(x =>
                {
                    viewModel.Contacts.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(x.Contact.Id.Value)));
                });

                viewModel.Activities = new List<ViewModels.Activities.ActivityBaseViewModel>();
                viewModel.InactiveActivities = new List<ViewModels.Activities.ActivityBaseViewModel>();
                Data.Opportunities.Opportunity.GetActivities(id, conn, false).ForEach(x =>
                {
                    Common.Models.Activities.ActivityStatusReason statusReason = Data.Activities.ActivityStatusReason.Get(x.StatusReason.Id.Value, conn, false);
                    Common.Models.Activities.ActivityType type = Data.Activities.ActivityType.Get(x.Type.Id.Value);
                    ViewModels.Activities.ActivityBaseViewModel vm = Mapper.Map<ViewModels.Activities.ActivityBaseViewModel>(x);
                    vm.Type = Mapper.Map<ViewModels.Activities.ActivityTypeViewModel>(type);
                    vm.StatusReason = Mapper.Map<ViewModels.Activities.ActivityStatusReasonViewModel>(statusReason);

                    if (x.State.HasValue && x.State.Value)
                        viewModel.Activities.Add(vm);
                    else
                        viewModel.InactiveActivities.Add(vm);
                });
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id)
        {
            List<ViewModels.Leads.LeadStatusViewModel> leadStatusList = new List<ViewModels.Leads.LeadStatusViewModel>();
            List<ViewModels.Leads.LeadSourceViewModel> leadSourceList = new List<ViewModels.Leads.LeadSourceViewModel>();
            List<ViewModels.Opportunities.OpportunityStageViewModel> opportunityStageList = new List<ViewModels.Opportunities.OpportunityStageViewModel>();
            ViewModels.Opportunities.OpportunityViewModel viewModel = new ViewModels.Opportunities.OpportunityViewModel();            
            Common.Models.Opportunities.Opportunity model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Opportunities.Opportunity.Get(id, conn, false);
                model.Lead = Data.Leads.Lead.Get(model.Lead.Id.Value, conn, false);
                if (model.Lead.Fee != null && model.Lead.Fee.Id.HasValue)
                {
                    model.Lead.Fee = Data.Leads.LeadFee.Get(model.Lead.Fee.Id.Value, conn, false);
                    if (model.Lead.Fee.To != null && model.Lead.Fee.To.Id.HasValue)
                        model.Lead.Fee.To = Data.Contacts.Contact.Get(model.Lead.Fee.To.Id.Value, conn, false);
                }
                model.Lead.Contact = Data.Contacts.Contact.Get(model.Lead.Contact.Id.Value, conn, false);
                model.Lead.Source = Data.Leads.LeadSource.Get(model.Lead.Source.Id.Value, conn, false);
                if (model.Lead.Source.Type != null && model.Lead.Source.Type.Id.HasValue)
                    model.Lead.Source.Type = Data.Leads.LeadSourceType.Get(model.Lead.Source.Type.Id.Value, conn, false);
                model.Lead.Status = Data.Leads.LeadStatus.Get(model.Lead.Status.Id.Value, conn, false);
                model.Account = Data.Contacts.Contact.Get(model.Account.Id.Value, conn, false);
                model.Stage = Data.Opportunities.OpportunityStage.Get(model.Stage.Id.Value, conn, false);
                if (model.Matter != null && model.Matter.Id.HasValue)
                    model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);

                // Tweaks
                // Probability is stored as a decimal (<1) need to change representation to percent
                model.Probability *= 100;

                viewModel = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(model);
                viewModel.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(model.Lead);
                if (model.Lead.Fee != null)
                {
                    viewModel.Lead.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(model.Lead.Fee);
                    if (model.Lead.Fee.To != null)
                        viewModel.Lead.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Lead.Fee.To);
                    else
                        viewModel.Lead.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
                else
                {
                    viewModel.Lead.Fee = new ViewModels.Leads.LeadFeeViewModel();
                    viewModel.Lead.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
                viewModel.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Lead.Contact);
                viewModel.Lead.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(model.Lead.Source);
                if (model.Lead.Source.Type != null)
                    viewModel.Lead.Source.Type = Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(model.Lead.Source.Type);
                viewModel.Lead.Status = Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(model.Lead.Status);
                viewModel.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Account);
                viewModel.Stage = Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(model.Stage);
                if (model.Matter != null)
                    viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);

                viewModel.Contacts = new List<ViewModels.Contacts.ContactViewModel>();
                Data.Opportunities.OpportunityContact.ListForOpportunity(id, conn, false).ForEach(x =>
                {
                    viewModel.Contacts.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(x.Contact.Id.Value)));
                });
                for (int i=0; i<(20 - viewModel.Contacts.Count); i++)
                {
                    viewModel.Contacts.Add(new ViewModels.Contacts.ContactViewModel());
                }

                Data.Leads.LeadStatus.List(conn, false).ForEach(x =>
                {
                    leadStatusList.Add(Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(x));
                });

                Data.Leads.LeadSource.List(conn, false).ForEach(x =>
                {
                    leadSourceList.Add(Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(x));
                });

                Data.Opportunities.OpportunityStage.List(conn, false).ForEach(x =>
                {
                    opportunityStageList.Add(Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(x));
                });
            }

            ViewBag.LeadStatusList = leadStatusList;
            ViewBag.LeadSourceList = leadSourceList;
            ViewBag.StageList = opportunityStageList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id, ViewModels.Opportunities.OpportunityViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Opportunities.Opportunity model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    Common.Models.Leads.Lead lead;
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    Common.Models.Leads.Lead oldLead = Data.Leads.Lead.Get(trans, viewModel.Lead.Id.Value);

                    lead = Mapper.Map<Common.Models.Leads.Lead>(viewModel.Lead);
                    lead.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(viewModel.Lead.Fee);

                    if (oldLead.Fee != null)
                        lead.Fee = Data.Leads.LeadFee.Edit(trans, lead.Fee, currentUser);
                    else
                        lead.Fee = Data.Leads.LeadFee.Create(trans, lead.Fee, currentUser);
                    
                    lead = Data.Leads.Lead.Edit(trans, lead, currentUser);

                    model = Mapper.Map<Common.Models.Opportunities.Opportunity>(viewModel);

                    // Check the probability, is it percent or decimal?
                    if (model.Probability.HasValue && model.Probability.Value > 1)
                        model.Probability = model.Probability.Value / 100;

                    model = Data.Opportunities.Opportunity.Edit(trans, model, currentUser);

                    // Add new contacts
                    viewModel.Contacts.ForEach(x =>
                    {
                        if (x.Id.HasValue)
                        {
                            Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                            {
                                Contact = Mapper.Map<Common.Models.Contacts.Contact>(x),
                                Opportunity = model
                            };
                            oc = Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                        }
                    });

                    // Handle deletions
                    Data.Opportunities.OpportunityContact.ListForOpportunity(trans, model.Id.Value).ForEach(x =>
                    {
                        if (!viewModel.Contacts.Any(vm => 
                            {
                                if (vm.Id.HasValue)
                                    return vm.Id.Value == x.Contact.Id.Value;
                                return false;
                            }))
                            Data.Opportunities.OpportunityContact.Disable(trans, x, currentUser);
                    });

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = id });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Convert(long id)
        {
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
            List<ViewModels.Billing.BillingGroupViewModel> billingGroupList;
            List<ViewModels.Matters.MatterTypeViewModel> matterTypeList;
            List<ViewModels.Matters.CourtTypeViewModel> courtTypeList;
            List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel> courtGeographicalJurisdictionList;
            List<ViewModels.Matters.CourtSittingInCityViewModel> courtSittingInCityList;
            List<ViewModels.Leads.LeadStatusViewModel> leadStatusList = new List<ViewModels.Leads.LeadStatusViewModel>();
            List<ViewModels.Leads.LeadSourceViewModel> leadSourceList = new List<ViewModels.Leads.LeadSourceViewModel>();
            List<ViewModels.Opportunities.OpportunityStageViewModel> opportunityStageList = new List<ViewModels.Opportunities.OpportunityStageViewModel>();
            ViewModels.Opportunities.CreateOpportunityViewModel viewModel = new ViewModels.Opportunities.CreateOpportunityViewModel();
            Common.Models.Opportunities.Opportunity opportunity;

            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();
            billingGroupList = new List<ViewModels.Billing.BillingGroupViewModel>();
            matterTypeList = new List<ViewModels.Matters.MatterTypeViewModel>();
            courtTypeList = new List<ViewModels.Matters.CourtTypeViewModel>();
            courtGeographicalJurisdictionList = new List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>();
            courtSittingInCityList = new List<ViewModels.Matters.CourtSittingInCityViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                opportunity = Data.Opportunities.Opportunity.Get(id, conn, false);
                opportunity.Lead = Data.Leads.Lead.Get(opportunity.Lead.Id.Value, conn, false);
                if (opportunity.Lead.Fee != null && opportunity.Lead.Fee.Id.HasValue)
                {
                    opportunity.Lead.Fee = Data.Leads.LeadFee.Get(opportunity.Lead.Fee.Id.Value, conn, false);
                    if (opportunity.Lead.Fee.To != null && opportunity.Lead.Fee.To.Id.HasValue)
                        opportunity.Lead.Fee.To = Data.Contacts.Contact.Get(opportunity.Lead.Fee.To.Id.Value, conn, false);
                }
                opportunity.Lead.Contact = Data.Contacts.Contact.Get(opportunity.Lead.Contact.Id.Value, conn, false);
                opportunity.Lead.Source = Data.Leads.LeadSource.Get(opportunity.Lead.Source.Id.Value, conn, false);
                if (opportunity.Lead.Source.Type != null && opportunity.Lead.Source.Type.Id.HasValue)
                    opportunity.Lead.Source.Type = Data.Leads.LeadSourceType.Get(opportunity.Lead.Source.Type.Id.Value, conn, false);
                opportunity.Lead.Status = Data.Leads.LeadStatus.Get(opportunity.Lead.Status.Id.Value, conn, false);
                opportunity.Account = Data.Contacts.Contact.Get(opportunity.Account.Id.Value, conn, false);
                opportunity.Stage = Data.Opportunities.OpportunityStage.Get(opportunity.Stage.Id.Value, conn, false);
                if (opportunity.Matter != null && opportunity.Matter.Id.HasValue)
                    opportunity.Matter = Data.Matters.Matter.Get(opportunity.Matter.Id.Value, conn, false);

                // Tweaks
                // Probability is stored as a decimal (<1) need to change representation to percent
                opportunity.Probability *= 100;

                viewModel.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(opportunity);
                viewModel.Opportunity.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(opportunity.Lead);
                if (opportunity.Lead.Fee != null)
                {
                    viewModel.Opportunity.Lead.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(opportunity.Lead.Fee);
                    if (opportunity.Lead.Fee.To != null)
                        viewModel.Opportunity.Lead.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opportunity.Lead.Fee.To);
                    else
                        viewModel.Opportunity.Lead.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
                else
                {
                    viewModel.Opportunity.Lead.Fee = new ViewModels.Leads.LeadFeeViewModel();
                    viewModel.Opportunity.Lead.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
                viewModel.Opportunity.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opportunity.Lead.Contact);
                viewModel.Opportunity.Lead.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(opportunity.Lead.Source);
                if (opportunity.Lead.Source.Type != null)
                    viewModel.Opportunity.Lead.Source.Type = Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(opportunity.Lead.Source.Type);
                viewModel.Opportunity.Lead.Status = Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(opportunity.Lead.Status);
                viewModel.Opportunity.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opportunity.Account);
                viewModel.Opportunity.Stage = Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(opportunity.Stage);
                if (opportunity.Matter != null)
                {
                    return RedirectToAction("Details", "Matters", new { id = opportunity.Matter.Id.Value });
                }

                viewModel.Opportunity.Contacts = new List<ViewModels.Contacts.ContactViewModel>();
                Data.Opportunities.OpportunityContact.ListForOpportunity(id, conn, false).ForEach(x =>
                {
                    viewModel.Opportunity.Contacts.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(x.Contact.Id.Value)));
                });
                for (int i = 0; i < (20 - viewModel.Opportunity.Contacts.Count); i++)
                {
                    viewModel.Opportunity.Contacts.Add(new ViewModels.Contacts.ContactViewModel());
                }



                // Matter
                viewModel.Matter = new ViewModels.Matters.MatterViewModel()
                {
                    Active = true,
                    OverrideMatterRateWithEmployeeRate = true
                };
                
                
                Data.Leads.LeadStatus.List(conn, false).ForEach(x =>
                {
                    leadStatusList.Add(Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(x));
                });

                Data.Leads.LeadSource.List(conn, false).ForEach(x =>
                {
                    leadSourceList.Add(Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(x));
                });

                Data.Opportunities.OpportunityStage.List(conn, false).ForEach(x =>
                {
                    opportunityStageList.Add(Mapper.Map<ViewModels.Opportunities.OpportunityStageViewModel>(x));
                });

                Data.Billing.BillingRate.List(conn, false).ForEach(x =>
                {
                    ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                    vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                    billingRateList.Add(vm);
                });

                Data.Billing.BillingGroup.List(conn, false).ForEach(x =>
                {
                    ViewModels.Billing.BillingGroupViewModel vm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(x);
                    vm.Title += " (" + vm.Amount.ToString("C") + ")";
                    billingGroupList.Add(vm);
                });

                Data.Matters.MatterType.List(conn, false).ForEach(x =>
                {
                    ViewModels.Matters.MatterTypeViewModel vm = Mapper.Map<ViewModels.Matters.MatterTypeViewModel>(x);
                    matterTypeList.Add(vm);
                });

                Data.Matters.CourtType.List(conn, false).ForEach(x =>
                {
                    ViewModels.Matters.CourtTypeViewModel vm = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(x);
                    courtTypeList.Add(vm);
                });

                Data.Matters.CourtGeographicalJurisdiction.List(conn, false).ForEach(x =>
                {
                    ViewModels.Matters.CourtGeographicalJurisdictionViewModel vm = Mapper.Map<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>(x);
                    courtGeographicalJurisdictionList.Add(vm);
                });

                Data.Matters.CourtSittingInCity.List(conn, false).ForEach(x =>
                {
                    ViewModels.Matters.CourtSittingInCityViewModel vm = Mapper.Map<ViewModels.Matters.CourtSittingInCityViewModel>(x);
                    courtSittingInCityList.Add(vm);
                });
            }

            ViewBag.BillingRateList = billingRateList;
            ViewBag.BillingGroupList = billingGroupList;
            ViewBag.MatterTypeList = matterTypeList;
            ViewBag.CourtTypeList = courtTypeList;
            ViewBag.CourtGeographicalJurisdictionList = courtGeographicalJurisdictionList;
            ViewBag.CourtSittingInCityList = courtSittingInCityList;
            ViewBag.LeadStatusList = leadStatusList;
            ViewBag.LeadSourceList = leadSourceList;
            ViewBag.StageList = opportunityStageList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Convert(long id, ViewModels.Opportunities.CreateOpportunityViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Opportunities.Opportunity opportunity;
            Common.Models.Matters.Matter matter;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    Common.Models.Leads.Lead lead;
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    Common.Models.Leads.Lead oldLead = Data.Leads.Lead.Get(trans, viewModel.Opportunity.Lead.Id.Value);

                    lead = Mapper.Map<Common.Models.Leads.Lead>(viewModel.Opportunity.Lead);
                    lead.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(viewModel.Opportunity.Lead.Fee);

                    if (oldLead.Fee != null)
                        lead.Fee = Data.Leads.LeadFee.Edit(trans, lead.Fee, currentUser);
                    else
                        lead.Fee = Data.Leads.LeadFee.Create(trans, lead.Fee, currentUser);

                    lead = Data.Leads.Lead.Edit(trans, lead, currentUser);


                    matter = Mapper.Map<Common.Models.Matters.Matter>(viewModel.Matter);
                    matter = Data.Matters.Matter.Create(trans, matter, currentUser);

                    viewModel.Opportunity.Id = id;
                    viewModel.Opportunity.Matter = new ViewModels.Matters.MatterViewModel() { Id = matter.Id };
                    opportunity = Mapper.Map<Common.Models.Opportunities.Opportunity>(viewModel.Opportunity);

                    // Check the probability, is it percent or decimal?
                    if (opportunity.Probability.HasValue && opportunity.Probability.Value > 1)
                        opportunity.Probability = opportunity.Probability.Value / 100;

                    opportunity = Data.Opportunities.Opportunity.Edit(trans, opportunity, currentUser);


                    // Assign Contacts

                    if (viewModel.Contact1 != null && viewModel.Contact1.Contact != null
                        && viewModel.Contact1.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact1);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact2 != null && viewModel.Contact2.Contact != null
                        && viewModel.Contact2.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact2);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact3 != null && viewModel.Contact3.Contact != null
                        && viewModel.Contact3.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact3);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact4 != null && viewModel.Contact4.Contact != null
                        && viewModel.Contact4.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact4);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact5 != null && viewModel.Contact5.Contact != null
                        && viewModel.Contact5.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact5);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact6 != null && viewModel.Contact6.Contact != null
                        && viewModel.Contact6.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact6);
                        mc.Matter = matter;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }


                    trans.Commit();

                    return RedirectToAction("Details", "Matters", new { Id = matter.Id });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Close(long id)
        {
            return Details(id);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Close(long id, ViewModels.Opportunities.OpportunityViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Opportunities.Opportunity model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    model = Mapper.Map<Common.Models.Opportunities.Opportunity>(viewModel);                    
                    model = Data.Opportunities.Opportunity.Close(trans, model, currentUser);
                    trans.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }
    }
}