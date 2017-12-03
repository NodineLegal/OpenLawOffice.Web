// -----------------------------------------------------------------------
// <copyright file="LeadsController.cs" company="Nodine Legal, LLC">
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

    [HandleError(View = "Errors/Index", Order = 10)]
    public class LeadsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            int colorKeeper = -1;
            string contactFilter, sourceFilter;
            int? sourceTypeFilter = null, statusTypeFilter = null;
            bool? closedFilter = null;
            ViewModels.Leads.DashboardViewModel viewModel = new ViewModels.Leads.DashboardViewModel();
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
            ViewModels.ChartJSViewModel chartLeadSourceVM, chartLeadConversionVM;

            contactFilter = Request["contactFilter"];
            sourceFilter = Request["sourceFilter"];

            if (!string.IsNullOrEmpty(Request["sourceTypeFilter"]))
            {
                int i;
                if (int.TryParse(Request["sourceTypeFilter"], out i))
                    sourceTypeFilter = i;
            }

            if (!string.IsNullOrEmpty(Request["statusTypeFilter"]))
            {
                int i;
                if (int.TryParse(Request["statusTypeFilter"], out i))
                    statusTypeFilter = i;
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

            chartLeadSourceVM = new ViewModels.ChartJSViewModel()
            {
                type = "pie"
            };
            chartLeadSourceVM.data.datasets.Add(new ViewModels.ChartJSViewModel.Dataset());

            chartLeadConversionVM = new ViewModels.ChartJSViewModel()
            {
                type = "bar"
            };

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                viewModel.Leads = new List<ViewModels.Leads.LeadViewModel>();
                viewModel.StatusTypes = new List<ViewModels.Leads.LeadStatusViewModel>();
                viewModel.SourceTypes = new List<ViewModels.Leads.LeadSourceTypeViewModel>();

                Data.Leads.LeadSourceType.List(conn, false).ForEach(x =>
                {
                    viewModel.SourceTypes.Add(Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(x));
                });

                Data.Leads.LeadStatus.List(conn, false).ForEach(x =>
                {
                    viewModel.StatusTypes.Add(Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(x));
                });

                Data.Leads.Lead.List(contactFilter, sourceFilter, sourceTypeFilter, statusTypeFilter, closedFilter, conn, false).ForEach(x =>
                {
                    ViewModels.Leads.LeadViewModel lvm = Mapper.Map<ViewModels.Leads.LeadViewModel>(x);
                    lvm.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(x.Contact.Id.Value));
                    lvm.Status = Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(Data.Leads.LeadStatus.Get(x.Status.Id.Value));
                    lvm.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(Data.Leads.LeadSource.Get(x.Source.Id.Value));
                    viewModel.Leads.Add(lvm);
                });

                Data.Leads.Lead.LeadsBySource(DateTime.UtcNow.AddDays(-30), conn, false).ForEach(x =>
                {
                    colorKeeper++;
                    if (colorKeeper >= availableColors.Length)
                        colorKeeper = 0;

                    chartLeadSourceVM.data.labels.Add(x.Title);
                    chartLeadSourceVM.data.datasets[0].backgroundColor.Add("rgba(" + availableColors[colorKeeper].R + ", " + availableColors[colorKeeper].G + ", " + availableColors[colorKeeper].B + ", 1)");
                    chartLeadSourceVM.data.datasets[0].data.Add(x.Count.Value);
                    //chartVM.data.datasets[0].borderWidth = "1";
                });

                colorKeeper = -1;
                ViewModels.ChartJSViewModel.Dataset ds = new ViewModels.ChartJSViewModel.Dataset();
                Data.Opportunities.Opportunity.ListSinceGroupingByMonth(DateTime.UtcNow.AddYears(-1), conn, false).ForEach(x =>
                {
                    colorKeeper++;
                    if (colorKeeper >= availableColors.Length)
                        colorKeeper = 0;

                    chartLeadConversionVM.data.labels.Add(x.Month.Value.ToString("MMM"));
                    ds.backgroundColor.Add("rgba(" + availableColors[colorKeeper].R + ", " + availableColors[colorKeeper].G + ", " + availableColors[colorKeeper].B + ", 1)");
                    //ds.label = x.Month.Value.ToString("MMM");
                    ds.data.Add(x.Count.Value);

                });
                chartLeadConversionVM.data.datasets.Add(ds);
                chartLeadConversionVM.options.scales = new ViewModels.ChartJSViewModel.Options.Scales();
                chartLeadConversionVM.options.scales.yAxes = new List<ViewModels.ChartJSViewModel.Options.Scales.Axes>();
                chartLeadConversionVM.options.scales.yAxes.Add(new ViewModels.ChartJSViewModel.Options.Scales.Axes()
                {
                    ticks = new ViewModels.ChartJSViewModel.Options.Scales.Axes.Ticks()
                    {
                        beginAtZero = true
                    }
                });
            }

            ViewBag.LeadSourceGraphData = Newtonsoft.Json.JsonConvert.SerializeObject(chartLeadSourceVM,
                Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            ViewBag.LeadConversionGraphData = Newtonsoft.Json.JsonConvert.SerializeObject(chartLeadConversionVM, 
                Newtonsoft.Json.Formatting.Indented, 
                new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(long id, bool preventRedirect = false)
        {
            ViewModels.Leads.LeadViewModel viewModel;
            Common.Models.Leads.Lead model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Leads.Lead.Get(id, conn, false);

                // Has this already been made an opportunity?  If so, redirect
                Common.Models.Opportunities.Opportunity op = Data.Opportunities.Opportunity.GetForLead(model.Id.Value);
                if (op != null && !preventRedirect)
                    return new RedirectResult("/Opportunities/Details/" + op.Id.Value.ToString());

                viewModel = Mapper.Map<ViewModels.Leads.LeadViewModel>(model);

                if (op != null)
                    viewModel.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(op);

                if (model.Status != null && model.Status.Id.HasValue)
                {
                    model.Status = Data.Leads.LeadStatus.Get(model.Status.Id.Value, conn, false);
                    viewModel.Status = Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(model.Status);
                }

                if (model.Contact != null && model.Contact.Id.HasValue)
                {
                    model.Contact = Data.Contacts.Contact.Get(model.Contact.Id.Value, conn, false);
                    viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);
                }

                if (model.Source != null && model.Source.Id.HasValue)
                {
                    model.Source = Data.Leads.LeadSource.Get(model.Source.Id.Value, conn, false);
                    viewModel.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(model.Source);

                    if (model.Source.Contact != null && model.Source.Contact.Id.HasValue)
                    {
                        model.Source.Contact = Data.Contacts.Contact.Get(model.Source.Contact.Id.Value, conn, false);
                        viewModel.Source.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Source.Contact);
                    }
                    if (model.Source.Type != null && model.Source.Type.Id.HasValue)
                    {
                        model.Source.Type = Data.Leads.LeadSourceType.Get(model.Source.Type.Id.Value, conn, false);
                        viewModel.Source.Type = Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(model.Source.Type);
                    }
                }

                if (model.Fee != null && model.Fee.Id.HasValue)
                {
                    model.Fee = Data.Leads.LeadFee.Get(model.Fee.Id.Value, conn, false);
                    viewModel.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(model.Fee);

                    if (model.Fee.To != null && model.Fee.To.Id.HasValue)
                    {
                        model.Fee.To = Data.Contacts.Contact.Get(model.Fee.To.Id.Value, conn, false);
                        viewModel.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Fee.To);
                    }
                }

                viewModel.Activities = new List<ViewModels.Activities.ActivityBaseViewModel>();
                viewModel.InactiveActivities = new List<ViewModels.Activities.ActivityBaseViewModel>();
                Data.Leads.Lead.GetActivities(id, conn, false).ForEach(x =>
                {
                    Common.Models.Activities.ActivityStatusReason statusReason = Data.Activities.ActivityStatusReason.Get(x.StatusReason.Id.Value, conn, false);
                    Common.Models.Activities.ActivityType type = Data.Activities.ActivityType.Get(x.Type.Id.Value, conn, false);
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
            ViewModels.Leads.LeadViewModel viewModel = new ViewModels.Leads.LeadViewModel();
            
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Common.Models.Leads.Lead model = Mapper.Map<Common.Models.Leads.Lead>(Data.Leads.Lead.Get(id, conn, false));

                // Has this already been made an opportunity?  If so, redirect
                Common.Models.Opportunities.Opportunity op = Data.Opportunities.Opportunity.GetForLead(model.Id.Value);
                if (op != null)
                    return new RedirectResult("/Opportunities/Edit/" + op.Id.Value.ToString());

                model.Contact = Mapper.Map<Common.Models.Contacts.Contact>(Data.Contacts.Contact.Get(model.Contact.Id.Value));
                if (model.Fee != null)
                {
                    model.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(Data.Leads.LeadFee.Get(model.Fee.Id.Value, conn, false));
                    if (model.Fee.To != null)
                        model.Fee.To = Mapper.Map<Common.Models.Contacts.Contact>(Data.Contacts.Contact.Get(model.Fee.To.Id.Value));
                }

                Data.Leads.LeadStatus.List(conn, false).ForEach(x =>
                {
                    leadStatusList.Add(Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(x));
                });

                Data.Leads.LeadSource.List(conn, false).ForEach(x =>
                {
                    leadSourceList.Add(Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(x));
                });

                viewModel = Mapper.Map<ViewModels.Leads.LeadViewModel>(model);
                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);
                viewModel.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(model.Fee);
                if (model.Fee != null)
                    viewModel.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Fee.To);
                else
                {
                    viewModel.Fee = new ViewModels.Leads.LeadFeeViewModel();
                    viewModel.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
            }

            ViewBag.LeadStatusList = leadStatusList;
            ViewBag.LeadSourceList = leadSourceList;

            return View(viewModel);
        }
        
        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id, ViewModels.Leads.LeadViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Leads.Lead model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    Common.Models.Leads.Lead oldLead = Data.Leads.Lead.Get(trans, id);

                    model = Mapper.Map<Common.Models.Leads.Lead>(viewModel);
                    //model.Source = Mapper.Map<Common.Models.Leads.LeadSource>(viewModel.Source);
                    model.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(viewModel.Fee);

                    if (oldLead.Fee != null)
                    {

                        model.Fee = Data.Leads.LeadFee.Edit(trans, model.Fee, currentUser);
                    }
                    else
                        model.Fee = Data.Leads.LeadFee.Create(trans, model.Fee, currentUser);

                    //model.Source = Data.Leads.LeadSource.Edit(trans, model.Source, currentUser);
                    model = Data.Leads.Lead.Edit(trans, model, currentUser);

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
        public ActionResult Create()
        {
            ViewModels.Leads.CreateLeadViewModel viewModel = new ViewModels.Leads.CreateLeadViewModel();
            List<ViewModels.Leads.LeadStatusViewModel> leadStatusList = new List<ViewModels.Leads.LeadStatusViewModel>();
            List<ViewModels.Leads.LeadSourceViewModel> leadSourceList = new List<ViewModels.Leads.LeadSourceViewModel>();
            List<ViewModels.Leads.LeadSourceTypeViewModel> leadSourceTypeList = new List<ViewModels.Leads.LeadSourceTypeViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                // Leads are external to organization
                viewModel.Contact = new ViewModels.Contacts.ContactViewModel() { IsOurEmployee = false };

                Data.Leads.LeadStatus.List(conn, false).ForEach(x =>
                {
                    leadStatusList.Add(Mapper.Map<ViewModels.Leads.LeadStatusViewModel>(x));
                });

                Data.Leads.LeadSource.List(conn, false).ForEach(x =>
                {
                    leadSourceList.Add(Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(x));
                });

                Data.Leads.LeadSourceType.List(conn, false).ForEach(x =>
                {
                    leadSourceTypeList.Add(Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(x));
                });
            }

            ViewBag.LeadStatusList = leadStatusList;
            ViewBag.LeadSourceList = leadSourceList;
            ViewBag.LeadSourceTypeList = leadSourceTypeList;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Leads.CreateLeadViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Leads.Lead model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    // Existing v. New checks
                    if (viewModel.ExistingContactId.HasValue)
                        viewModel.Contact.Id = viewModel.ExistingContactId;
                    if (viewModel.ExistingSourceId.HasValue)
                        viewModel.Source.Id = viewModel.ExistingSourceId;
                    if (viewModel.ExistingSourceContactId.HasValue)
                        viewModel.Source.Contact.Id = viewModel.ExistingSourceContactId;
                    if (viewModel.ExistingFeeToId.HasValue)
                        viewModel.Fee.To.Id = viewModel.ExistingFeeToId;
                    
                    model = Mapper.Map<Common.Models.Leads.Lead>((ViewModels.Leads.LeadViewModel)viewModel);

                    if (!viewModel.ExistingContactId.HasValue)
                    {
                        model.Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact);
                        model.Contact = Data.Contacts.Contact.Create(trans, model.Contact, currentUser);
                    }

                    if (!viewModel.ExistingSourceId.HasValue)
                    {
                        model.Source = Mapper.Map<Common.Models.Leads.LeadSource>(viewModel.Source);
                        model.Source = Data.Leads.LeadSource.Create(trans, model.Source, currentUser);
                    }

                    if (!viewModel.ExistingFeeToId.HasValue)
                    {
                        model.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(viewModel.Fee);
                        model.Fee = Data.Leads.LeadFee.Create(trans, model.Fee, currentUser);
                    }
                    
                    model = Data.Leads.Lead.Create(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = model.Id.Value });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return RedirectToAction("Create");
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Convert(long id)
        {
            List<ViewModels.Leads.LeadStatusViewModel> leadStatusList = new List<ViewModels.Leads.LeadStatusViewModel>();
            List<ViewModels.Leads.LeadSourceViewModel> leadSourceList = new List<ViewModels.Leads.LeadSourceViewModel>();
            List<ViewModels.Opportunities.OpportunityStageViewModel> opportunityStageList = new List<ViewModels.Opportunities.OpportunityStageViewModel>();
            ViewModels.Leads.LeadConvertViewModel viewModel = new ViewModels.Leads.LeadConvertViewModel();
            viewModel.Opportunity = new ViewModels.Opportunities.OpportunityViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Common.Models.Leads.Lead lead = Mapper.Map<Common.Models.Leads.Lead>(Data.Leads.Lead.Get(id, conn, false));

                // Has this already been made an opportunity?  If so, redirect
                Common.Models.Opportunities.Opportunity op = Data.Opportunities.Opportunity.GetForLead(lead.Id.Value);
                if (op != null)
                    return new RedirectResult("/Opportunities/Details/" + op.Id.Value.ToString());

                lead.Contact = Mapper.Map<Common.Models.Contacts.Contact>(Data.Contacts.Contact.Get(lead.Contact.Id.Value));
                if (lead.Fee != null)
                {
                    lead.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(Data.Leads.LeadFee.Get(lead.Fee.Id.Value, conn, false));
                    if (lead.Fee.To != null)
                        lead.Fee.To = Mapper.Map<Common.Models.Contacts.Contact>(Data.Contacts.Contact.Get(lead.Fee.To.Id.Value));
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

                viewModel.Opportunity.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(lead);
                viewModel.Opportunity.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(lead.Contact);
                viewModel.Opportunity.Account = viewModel.Opportunity.Lead.Contact;

                if (lead.Fee != null)
                {
                    viewModel.Opportunity.Lead.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(lead.Fee);

                    if (lead.Fee.To != null)
                        viewModel.Opportunity.Lead.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(lead.Fee.To);
                    else
                        viewModel.Opportunity.Lead.Fee.To = new ViewModels.Contacts.ContactViewModel();
                }
                else
                {
                    viewModel.Opportunity.Lead.Fee = new ViewModels.Leads.LeadFeeViewModel()
                    {
                        To = new ViewModels.Contacts.ContactViewModel()
                    };
                }
            }

            ViewBag.LeadStatusList = leadStatusList;
            ViewBag.LeadSourceList = leadSourceList;
            ViewBag.StageList = opportunityStageList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Convert(long id, ViewModels.Leads.LeadConvertViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Opportunities.Opportunity model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    Common.Models.Leads.Lead oldLead = Data.Leads.Lead.Get(trans, id);

                    model = Mapper.Map<Common.Models.Opportunities.Opportunity>(viewModel.Opportunity);
                    model.Lead = Mapper.Map<Common.Models.Leads.Lead>(viewModel.Opportunity.Lead);
                    //model.Lead.Source = Mapper.Map<Common.Models.Leads.LeadSource>(viewModel.Lead.Source);
                    model.Lead.Fee = Mapper.Map<Common.Models.Leads.LeadFee>(viewModel.Opportunity.Lead.Fee);


                    if (oldLead.Fee != null)
                        model.Lead.Fee = Data.Leads.LeadFee.Edit(trans, model.Lead.Fee, currentUser);
                    else
                        model.Lead.Fee = Data.Leads.LeadFee.Create(trans, model.Lead.Fee, currentUser);

                    //model.Lead.Source = Data.Leads.LeadSource.Edit(trans, model.Lead.Source, currentUser);
                    model.Lead = Data.Leads.Lead.Edit(trans, model.Lead, currentUser);

                    // Check the probability, is it percent or decimal?
                    if (model.Probability.HasValue && model.Probability.Value > 1)
                        model.Probability = model.Probability.Value / 100;

                    model = Data.Opportunities.Opportunity.Create(trans, model, currentUser);

                    // Contacts involved
                    if (viewModel.Contact1 != null & viewModel.Contact1.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact1)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact2 != null & viewModel.Contact2.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact2)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact3 != null & viewModel.Contact3.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact3)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact4 != null & viewModel.Contact4.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact4)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact5 != null & viewModel.Contact5.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact5)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact6 != null & viewModel.Contact6.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact6)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact7 != null & viewModel.Contact7.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact7)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact8 != null & viewModel.Contact8.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact8)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact9 != null & viewModel.Contact9.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact9)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }
                    if (viewModel.Contact10 != null & viewModel.Contact10.Id.HasValue)
                    {
                        Common.Models.Opportunities.OpportunityContact oc = new Common.Models.Opportunities.OpportunityContact()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity() { Id = model.Id.Value },
                            Contact = Mapper.Map<Common.Models.Contacts.Contact>(viewModel.Contact10)
                        };
                        Data.Opportunities.OpportunityContact.Create(trans, oc, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("Details", "Opportunities", new { Id = model.Id });
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
        public ActionResult Close(long id, ViewModels.Leads.LeadViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Leads.Lead model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    model = Mapper.Map<Common.Models.Leads.Lead>(viewModel);
                    Data.Leads.Lead.Close(trans, model, currentUser);
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