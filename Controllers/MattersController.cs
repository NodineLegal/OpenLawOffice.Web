// -----------------------------------------------------------------------
// <copyright file="MattersController.cs" company="Nodine Legal, LLC">
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
    using System.Web.Profile;
    using System.Web.Security;
    using System.Linq;
    using AutoMapper.Internal;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class MattersController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            ViewModels.Matters.MatterIndexViewModel viewModel = new ViewModels.Matters.MatterIndexViewModel();
            bool? active;
            string activeStr, contactFilter, titleFilter, caseNumberFilter;
            int? courtTypeFilter = null, courtGeographicalJurisdictionFilter = null;

            switch (activeStr = Request["active"])
            {
                case "inactive":
                    active = false;
                    break;
                case "both":
                    active = null;
                    break;
                default:
                    active = true;
                    break;
            }

            contactFilter = Request["contactFilter"];
            titleFilter = Request["titleFilter"];
            caseNumberFilter = Request["caseNumberFilter"];

            if (!string.IsNullOrEmpty(Request["courtTypeFilter"]))
            {
                int i;
                if (int.TryParse(Request["courtTypeFilter"], out i))
                    courtTypeFilter = i;
            }

            if (!string.IsNullOrEmpty(Request["courtGeographicalJurisdictionFilter"]))
            {
                int i;
                if (int.TryParse(Request["courtGeographicalJurisdictionFilter"], out i))
                    courtGeographicalJurisdictionFilter = i;
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Matters.CourtType.List(conn, false).ForEach(x =>
                {
                    viewModel.CourtTypes.Add(Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(x));
                });

                Data.Matters.CourtGeographicalJurisdiction.List(conn, false).ForEach(x =>
                {
                    viewModel.CourtGeographicalJurisdictions.Add(Mapper.Map<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>(x));
                });

                Data.Matters.Matter.List(active, contactFilter,
                    titleFilter, caseNumberFilter, courtTypeFilter, courtGeographicalJurisdictionFilter,
                    conn, false).ForEach(x =>
                {
                    viewModel.Matters.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
                });
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult ListTitleOnly()
        {
            string term;
            List<Common.Models.Matters.Matter> list;

            term = Request["term"];

            list = Data.Matters.Matter.ListTitlesOnly(term.Trim());

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult ListCaseNumberOnly()
        {
            string term;
            List<Common.Models.Matters.Matter> list;

            term = Request["term"];

            list = Data.Matters.Matter.ListCaseNumbersOnly(term.Trim());

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //[Authorize(Roles = "Login, User")]
        //public ActionResult ListChildrenJqGrid(Guid? id)
        //{
        //    ViewModels.JqGridObject jqObject;

        //    jqObject = ListChildrenJqGridObject(id, x => GetChildrenList(x));

        //    return Json(jqObject, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        //[Authorize(Roles = "Login, User")]
        //public ActionResult ListChildrenForContactJqGrid(Guid? id)
        //{
        //    ViewModels.JqGridObject jqObject;
        //    int contactId;

        //    contactId = int.Parse(Request["ContactId"]);

        //    jqObject = ListChildrenJqGridObject(id, x => GetChildrenListForContact(x, contactId));

        //    return Json(jqObject, JsonRequestBehavior.AllowGet);
        //}

        //private ViewModels.JqGridObject ListChildrenJqGridObject(Guid? id, Func<Guid?, List<ViewModels.Matters.MatterViewModel>> act)
        //{
        //    List<ViewModels.Matters.MatterViewModel> modelList;
        //    List<object> anonList;
        //    int level = 0;

        //    if (id == null)
        //    {
        //        // jqGrid uses nodeid by default
        //        if (!string.IsNullOrEmpty(Request["nodeid"]))
        //            id = Guid.Parse(Request["nodeid"]);
        //    }

        //    modelList = act(id);
        //    //modelList = GetChildrenList(id);
        //    anonList = new List<object>();

        //    if (!string.IsNullOrEmpty(Request["n_level"]))
        //        level = int.Parse(Request["n_level"]) + 1;

        //    modelList.ForEach(x =>
        //    {
        //        if (x.Parent == null)
        //            anonList.Add(new
        //            {
        //                Id = x.Id,
        //                Title = x.Title,
        //                Synopsis = x.Synopsis,
        //                level = level,
        //                isLeaf = false,
        //                expanded = false
        //            });
        //        else
        //            anonList.Add(new
        //            {
        //                Id = x.Id,
        //                parent = x.Parent.Id,
        //                Title = x.Title,
        //                Synopsis = x.Synopsis,
        //                level = level,
        //                isLeaf = false,
        //                expanded = false
        //            });
        //    });

        //    return new ViewModels.JqGridObject()
        //    {
        //        TotalPages = 1,
        //        CurrentPage = 1,
        //        TotalRecords = modelList.Count,
        //        Rows = anonList.ToArray()
        //    };
        //}

        //private List<ViewModels.Matters.MatterViewModel> GetChildrenList(Guid? id)
        //{
        //    List<ViewModels.Matters.MatterViewModel> viewModelList;

        //    viewModelList = new List<ViewModels.Matters.MatterViewModel>();

        //    using (IDbConnection conn = Data.Database.Instance.GetConnection())
        //    {
        //        Data.Matters.Matter.ListChildren(id, conn, false).ForEach(x =>
        //        {
        //            viewModelList.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
        //        });
        //    }

        //    return viewModelList;
        //}

        //private List<ViewModels.Matters.MatterViewModel> GetChildrenListForContact(Guid? id, int contactId)
        //{
        //    List<ViewModels.Matters.MatterViewModel> viewModelList;

        //    viewModelList = new List<ViewModels.Matters.MatterViewModel>();

        //    using (IDbConnection conn = Data.Database.Instance.GetConnection())
        //    {
        //        Data.Matters.Matter.ListChildrenForContact(id, contactId, conn, false).ForEach(x =>
        //        {
        //            viewModelList.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
        //        });
        //    }

        //    return viewModelList;
        //}

        [Authorize(Roles = "Login, User")]
        public ActionResult Close(Guid id)
        {
            return Close(id, null);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Close(Guid id, ViewModels.Matters.MatterViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    model = Data.Matters.Matter.Get(id);
                    model.Active = false;

                    model = Data.Matters.Matter.Edit(model, currentUser);
                    trans.Commit();
                    return RedirectToAction("Details", new { Id = id });
                }
                catch
                {
                    trans.Rollback();
                    return Close(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            List<string> neededRoles;
            string alertText = "";
            ViewModels.Matters.MatterViewModel viewModel;
            Common.Models.Matters.Matter model;
            List<Common.Models.Billing.InvoiceExpense> billedExpenseList;
            List<Common.Models.Billing.Expense> unbilledExpenseList;
            List<Common.Models.Billing.InvoiceFee> billedFeeList;
            List<Common.Models.Billing.Fee> unbilledFeeList;
            List<Common.Models.Billing.InvoiceTime> billedTimeList;
            List<Common.Models.Timing.Time> unbilledTimeList;
            decimal feesBilled = 0, feesUnbilled = 0,
                expensesBilled = 0, expensesUnbilled = 0,
                timeBilledDollars = 0,
                totalValue = 0;
            TimeSpan timeBilledSpan = new TimeSpan(),
                timeUnbilledSpan = new TimeSpan(),
                nonBillableTimeSpan = new TimeSpan();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.Matter.Get(id, conn, false);

                if (model.BillTo != null)
                    model.BillTo = Data.Contacts.Contact.Get(model.BillTo.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.MatterViewModel>(model);
                if (viewModel.MatterType != null)
                    viewModel.MatterType = Mapper.Map<ViewModels.Matters.MatterTypeViewModel>(
                        Data.Matters.MatterType.Get(viewModel.MatterType.Id.Value, conn, false));
                if (viewModel.CourtType != null)
                    viewModel.CourtType = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(
                        Data.Matters.CourtType.Get(viewModel.CourtType.Id.Value, conn, false));
                if (viewModel.CourtGeographicalJurisdiction != null)
                    viewModel.CourtGeographicalJurisdiction = Mapper.Map<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>(
                        Data.Matters.CourtGeographicalJurisdiction.Get(viewModel.CourtGeographicalJurisdiction.Id.Value, conn, false));
                if (viewModel.CourtSittingInCity != null)
                    viewModel.CourtSittingInCity = Mapper.Map<ViewModels.Matters.CourtSittingInCityViewModel>(
                        Data.Matters.CourtSittingInCity.Get(viewModel.CourtSittingInCity.Id.Value, conn, false));
                viewModel.Clients = new List<ViewModels.Contacts.ContactViewModel>();
                viewModel.Tasks = TasksController.GetListForMatter(id, true, conn);
                viewModel.BillTo = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.BillTo);
                if (viewModel.DefaultBillingRate != null)
                    viewModel.DefaultBillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(
                        Data.Billing.BillingRate.Get(viewModel.DefaultBillingRate.Id.Value, conn, false));
                if (viewModel.BillingGroup != null)
                    viewModel.BillingGroup = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(
                        Data.Billing.BillingGroup.Get(viewModel.BillingGroup.Id.Value, conn, false));

                // -- Financial Info
                billedExpenseList = Data.Billing.InvoiceExpense.ListForMatter(model.Id.Value, conn, false);
                unbilledExpenseList = Data.Billing.Expense.ListUnbilledExpensesForMatter(model.Id.Value, null, null, conn, false);
                billedFeeList = Data.Billing.InvoiceFee.ListForMatter(model.Id.Value, conn, false);
                unbilledFeeList = Data.Billing.Fee.ListUnbilledFeesForMatter(model.Id.Value, null, null, conn, false);
                billedTimeList = Data.Billing.InvoiceTime.ListForMatter(model.Id.Value, conn, false);
                unbilledTimeList = Data.Timing.Time.ListUnbilledTimeForMatter(model.Id.Value, conn, false);

                billedFeeList.ForEach(x =>
                {
                    feesBilled += x.Amount;
                });
                unbilledFeeList.ForEach(x =>
                {
                    feesUnbilled += x.Amount;
                });

                billedExpenseList.ForEach(x =>
                {
                    expensesBilled += x.Amount;
                });
                unbilledExpenseList.ForEach(x =>
                {
                    expensesUnbilled += x.Amount;
                });

                billedTimeList.ForEach(x =>
                {
                    timeBilledSpan = timeBilledSpan.Add(x.Duration);
                    timeBilledDollars += ((decimal)x.Duration.TotalHours * x.PricePerHour);
                });
                unbilledTimeList.ForEach(x =>
                {
                    if (x.Billable)
                    {
                        if (x.Stop.HasValue)
                            timeUnbilledSpan = timeUnbilledSpan.Add(x.Stop.Value - x.Start);
                    }
                    else
                        if (x.Stop.HasValue)
                        nonBillableTimeSpan = nonBillableTimeSpan.Add(x.Stop.Value - x.Start);
                });

                totalValue = feesBilled + feesUnbilled + expensesBilled + expensesUnbilled + timeBilledDollars;

                ViewBag.FeesBilled = feesBilled;
                ViewBag.FeesUnbilled = feesUnbilled;
                ViewBag.Fees = feesBilled + feesUnbilled;
                ViewBag.ExpensesBilled = expensesBilled;
                ViewBag.ExpensesUnbilled = expensesUnbilled;
                ViewBag.Expenses = expensesBilled + expensesUnbilled;
                ViewBag.TimeBilledDollars = timeBilledDollars;
                ViewBag.TimeBilledSpan = timeBilledSpan;
                ViewBag.TimeUnbilledSpan = timeUnbilledSpan;
                ViewBag.TimeBilled = timeBilledDollars.ToString("C") + " (" + Helpers.TimeSpanHelper.TimeSpan(timeBilledSpan, false) + ")";
                ViewBag.TimeUnbilled = "(" + Helpers.TimeSpanHelper.TimeSpan(timeUnbilledSpan, false) + ")";
                ViewBag.TotalValue = totalValue.ToString("C") + " (Unbilled Time: " + Helpers.TimeSpanHelper.TimeSpan(timeUnbilledSpan, false) + ")";
                ViewBag.NonBillableTime = nonBillableTimeSpan;
                ViewBag.EffHourlyRate = string.Format("{0:C}", (((double)timeBilledDollars + (double)feesBilled) / timeBilledSpan.TotalHours));

                // CRM
                Common.Models.Opportunities.Opportunity opp = Data.Opportunities.Opportunity.GetForMatter(id, conn, false);

                if (opp != null)
                {
                    viewModel.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(opp);

                    viewModel.Activities = new List<ViewModels.Activities.ActivityBaseViewModel>();
                    Data.Opportunities.Opportunity.GetActivities(opp.Id.Value, conn, false).ForEach(x =>
                    {
                        viewModel.Activities.Add(Mapper.Map<ViewModels.Activities.ActivityBaseViewModel>(x));
                    });

                    opp.Lead = Data.Leads.Lead.Get(opp.Lead.Id.Value, conn, false);
                    
                    if (opp.Account != null && opp.Account.Id.HasValue)
                    {
                        opp.Account = Data.Contacts.Contact.Get(opp.Account.Id.Value, conn, false);
                        viewModel.Opportunity.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opp.Account);
                    }

                    if (opp.Lead != null)
                    {
                        viewModel.Opportunity.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(opp.Lead);

                        if (opp.Lead.Contact != null && opp.Lead.Contact.Id.HasValue)
                        {
                            opp.Lead.Contact = Data.Contacts.Contact.Get(opp.Lead.Contact.Id.Value, conn, false);
                            viewModel.Opportunity.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opp.Lead.Contact);
                        }

                        if (opp.Lead.Source != null && opp.Lead.Source.Id.HasValue)
                        {
                            opp.Lead.Source = Data.Leads.LeadSource.Get(opp.Lead.Source.Id.Value, conn, false);
                            viewModel.Opportunity.Lead.Source = Mapper.Map<ViewModels.Leads.LeadSourceViewModel>(opp.Lead.Source);

                            if (opp.Lead.Source.Type != null && opp.Lead.Source.Type.Id.HasValue)
                            {
                                opp.Lead.Source.Type = Data.Leads.LeadSourceType.Get(opp.Lead.Source.Type.Id.Value, conn, false);
                                viewModel.Opportunity.Lead.Source.Type = Mapper.Map<ViewModels.Leads.LeadSourceTypeViewModel>(opp.Lead.Source.Type);
                            }
                            if (opp.Lead.Source.Contact != null && opp.Lead.Source.Contact.Id.HasValue)
                            {
                                opp.Lead.Source.Contact = Data.Contacts.Contact.Get(opp.Lead.Source.Contact.Id.Value, conn, false);
                                viewModel.Opportunity.Lead.Source.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opp.Lead.Source.Contact);
                            }
                        }

                        if (opp.Lead.Fee != null && opp.Lead.Fee.Id.HasValue)
                        {
                            opp.Lead.Fee = Data.Leads.LeadFee.Get(opp.Lead.Fee.Id.Value, conn, false);
                            viewModel.Opportunity.Lead.Fee = Mapper.Map<ViewModels.Leads.LeadFeeViewModel>(opp.Lead.Fee);

                            if (opp.Lead.Fee.To != null && opp.Lead.Fee.To.Id.HasValue)
                            {
                                opp.Lead.Fee.To = Data.Contacts.Contact.Get(opp.Lead.Fee.To.Id.Value, conn, false);
                                viewModel.Opportunity.Lead.Fee.To = Mapper.Map<ViewModels.Contacts.ContactViewModel>(opp.Lead.Fee.To);
                            }
                        }
                    }
                }



                viewModel.Notes = new List<ViewModels.Notes.NoteViewModel>();
                Data.Notes.NoteMatter.ListForMatter(id, conn, false).ForEach(x =>
                {
                    viewModel.Notes.Add(Mapper.Map<ViewModels.Notes.NoteViewModel>(x));
                });

                viewModel.TaskNotes = new List<ViewModels.Notes.NoteTaskViewModel>();
                Data.Matters.Matter.ListAllTaskNotes(model, conn, false).ForEach(x =>
                {
                    ViewModels.Notes.NoteTaskViewModel ntvm = Mapper.Map<ViewModels.Notes.NoteTaskViewModel>(x);
                    ntvm.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(Data.Tasks.Task.Get(x.Task.Id.Value, conn, false));
                    ntvm.Note = Mapper.Map<ViewModels.Notes.NoteViewModel>(Data.Notes.Note.Get(x.Note.Id.Value, conn, false));
                    viewModel.TaskNotes.Add(ntvm);
                });

                viewModel.Assets = new List<ViewModels.Assets.AssetViewModel>();
                Data.Assets.Asset.ListForMatter(id, conn, false).ForEach(x =>
                {
                    ViewModels.Assets.AssetViewModel avm = Mapper.Map<ViewModels.Assets.AssetViewModel>(x);
                    avm.Tags = new List<ViewModels.Assets.TagViewModel>();
                    Data.Assets.Tag.ListForAsset(avm.Id.Value, conn, false).ForEach(y =>
                    {
                        avm.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(y));
                    });
                    viewModel.Assets.Add(avm);
                });

                PopulateCoreDetails(viewModel, conn);

                neededRoles = new List<string>();
                neededRoles.Add("Client");

                if (!string.IsNullOrEmpty(model.CaseNumber))
                {
                    // If a case has been filed
                    neededRoles.Add("Judge");
                    neededRoles.Add("Party");
                }

                Data.Matters.MatterContact.ListForMatter(model.Id.Value, conn, false).ForEach(x =>
                {
                    if (x.IsClient)
                    {
                        neededRoles.Remove("Client");
                        Common.Models.Contacts.Contact contactModel = Data.Contacts.Contact.Get(x.Contact.Id.Value, conn, false);
                        viewModel.Clients.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(contactModel));
                    }

                    if (x.IsJudge && neededRoles.Contains("Judge"))
                        neededRoles.Remove("Judge");

                    if (x.IsParty && neededRoles.Contains("Party"))
                        neededRoles.Remove("Party");
                });
            }

            if (neededRoles.Contains("Judge"))
                alertText += "<li>Missing role 'Judge'.</li>";
            if (neededRoles.Contains("Party"))
                alertText += "<li>Missing role 'Party'.</li>";
            if (neededRoles.Contains("Client"))
                alertText += "<li>Missing role 'Client'.</li>";

            if (alertText.Length > 0)
                ViewBag.AlertText = "<ul>" + alertText + "</ul>";

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            List<ViewModels.Account.UsersViewModel> userList;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
            List<ViewModels.Billing.BillingGroupViewModel> billingGroupList;
            List<ViewModels.Matters.MatterTypeViewModel> matterTypeList;
            List<ViewModels.Matters.CourtTypeViewModel> courtTypeList;
            List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel> courtGeographicalJurisdictionList;
            List<ViewModels.Matters.CourtSittingInCityViewModel> courtSittingInCityList;

            userList = new List<ViewModels.Account.UsersViewModel>();
            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();
            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();
            billingGroupList = new List<ViewModels.Billing.BillingGroupViewModel>();
            matterTypeList = new List<ViewModels.Matters.MatterTypeViewModel>();
            courtTypeList = new List<ViewModels.Matters.CourtTypeViewModel>();
            courtGeographicalJurisdictionList = new List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>();
            courtSittingInCityList = new List<ViewModels.Matters.CourtSittingInCityViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Account.Users.List(conn, false).ForEach(x =>
                {
                    userList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
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

            ViewBag.UserList = userList;
            ViewBag.EmployeeContactList = employeeContactList;
            ViewBag.BillingRateList = billingRateList;
            ViewBag.BillingGroupList = billingGroupList;
            ViewBag.MatterTypeList = matterTypeList;
            ViewBag.CourtTypeList = courtTypeList;
            ViewBag.CourtGeographicalJurisdictionList = courtGeographicalJurisdictionList;
            ViewBag.CourtSittingInCityList = courtSittingInCityList;

            return View(new ViewModels.Matters.CreateMatterViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Matters.CreateMatterViewModel viewModel)
        {
            string errorListString = "";
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter model;
            List<Common.Models.Matters.Matter> possibleDuplicateList;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.Matter>(viewModel.Matter);

                    if (Request["OverrideConflict"] != "True")
                    {
                        possibleDuplicateList = Data.Matters.Matter.ListPossibleDuplicates(trans, model);

                        if (possibleDuplicateList.Count > 0)
                        {
                            List<ViewModels.Account.UsersViewModel> userList;
                            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
                            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
                            List<ViewModels.Billing.BillingGroupViewModel> billingGroupList;
                            List<ViewModels.Matters.MatterTypeViewModel> matterTypeList;
                            List<ViewModels.Matters.CourtTypeViewModel> courtTypeList;
                            List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel> courtGeographicalJurisdictionList;
                            List<ViewModels.Matters.CourtSittingInCityViewModel> courtSittingInCityList;

                            possibleDuplicateList.ForEach(x =>
                            {
                                errorListString += "<li><a href=\"/Matters/Details/" + x.Id.Value + "\">" + x.Title + "</a> [<a href=\"/Matters/Edit/" + x.Id.Value + "\">edit</a>]</li>";
                            });

                            userList = new List<ViewModels.Account.UsersViewModel>();
                            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();
                            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();
                            billingGroupList = new List<ViewModels.Billing.BillingGroupViewModel>();
                            matterTypeList = new List<ViewModels.Matters.MatterTypeViewModel>();
                            courtTypeList = new List<ViewModels.Matters.CourtTypeViewModel>();
                            courtGeographicalJurisdictionList = new List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>();
                            courtSittingInCityList = new List<ViewModels.Matters.CourtSittingInCityViewModel>();

                            Data.Account.Users.List(trans).ForEach(x =>
                            {
                                userList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                            });

                            Data.Contacts.Contact.ListEmployeesOnly(trans).ForEach(x =>
                            {
                                employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                            });

                            Data.Billing.BillingRate.List(trans).ForEach(x =>
                            {
                                ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                                vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                                billingRateList.Add(vm);
                            });

                            Data.Billing.BillingGroup.List(trans).ForEach(x =>
                            {
                                ViewModels.Billing.BillingGroupViewModel vm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(x);
                                vm.Title += " (" + vm.Amount.ToString("C") + ")";
                                billingGroupList.Add(vm);
                            });

                            Data.Matters.MatterType.List(trans).ForEach(x =>
                            {
                                ViewModels.Matters.MatterTypeViewModel vm = Mapper.Map<ViewModels.Matters.MatterTypeViewModel>(x);
                                matterTypeList.Add(vm);
                            });

                            Data.Matters.CourtType.List(trans).ForEach(x =>
                            {
                                ViewModels.Matters.CourtTypeViewModel vm = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(x);
                                courtTypeList.Add(vm);
                            });

                            Data.Matters.CourtGeographicalJurisdiction.List(trans).ForEach(x =>
                            {
                                ViewModels.Matters.CourtGeographicalJurisdictionViewModel vm = Mapper.Map<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>(x);
                                courtGeographicalJurisdictionList.Add(vm);
                            });

                            Data.Matters.CourtSittingInCity.List(trans).ForEach(x =>
                            {
                                ViewModels.Matters.CourtSittingInCityViewModel vm = Mapper.Map<ViewModels.Matters.CourtSittingInCityViewModel>(x);
                                courtSittingInCityList.Add(vm);
                            });

                            ViewBag.ErrorMessage = "Matter possibly conflicts with the following existing matters:<ul>" + errorListString + "</ul>Click Save again to create the matter anyway.";
                            ViewBag.OverrideConflict = "True";

                            ViewBag.UserList = userList;
                            ViewBag.EmployeeContactList = employeeContactList;
                            ViewBag.BillingRateList = billingRateList;
                            ViewBag.BillingGroupList = billingGroupList;
                            ViewBag.MatterTypeList = matterTypeList;

                            return View(viewModel);
                        }
                    }

                    // Lead Attorney is created within this method
                    model = Data.Matters.Matter.Create(trans, model, currentUser);

                    // Assign Contacts

                    if (viewModel.Contact1 != null && viewModel.Contact1.Contact != null
                        && viewModel.Contact1.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact1);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact2 != null && viewModel.Contact2.Contact != null
                        && viewModel.Contact2.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact2);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact3 != null && viewModel.Contact3.Contact != null
                        && viewModel.Contact3.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact3);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact4 != null && viewModel.Contact4.Contact != null
                        && viewModel.Contact4.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact4);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact5 != null && viewModel.Contact5.Contact != null
                        && viewModel.Contact5.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact5);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact6 != null && viewModel.Contact6.Contact != null
                        && viewModel.Contact6.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact6);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact7 != null && viewModel.Contact7.Contact != null
                        && viewModel.Contact7.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact7);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact8 != null && viewModel.Contact8.Contact != null
                        && viewModel.Contact8.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact8);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact9 != null && viewModel.Contact9.Contact != null
                        && viewModel.Contact9.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact9);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }
                    if (viewModel.Contact10 != null && viewModel.Contact10.Contact != null
                        && viewModel.Contact10.Contact.Id.HasValue)
                    {
                        Common.Models.Matters.MatterContact mc = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel.Contact10);
                        mc.Matter = model;
                        Data.Matters.MatterContact.Create(trans, mc, currentUser);
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Details", new { Id = model.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
            List<ViewModels.Billing.BillingGroupViewModel> billingGroupList;
            List<ViewModels.Matters.MatterTypeViewModel> matterTypeList;
            List<ViewModels.Matters.CourtTypeViewModel> courtTypeList;
            List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel> courtGeographicalJurisdictionList;
            List<ViewModels.Matters.CourtSittingInCityViewModel> courtSittingInCityList;
            ViewModels.Matters.EditMatterViewModel viewModel;
            Common.Models.Matters.Matter model;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();
            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();
            billingGroupList = new List<ViewModels.Billing.BillingGroupViewModel>();
            matterTypeList = new List<ViewModels.Matters.MatterTypeViewModel>();
            courtTypeList = new List<ViewModels.Matters.CourtTypeViewModel>();
            courtGeographicalJurisdictionList = new List<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>();
            courtSittingInCityList = new List<ViewModels.Matters.CourtSittingInCityViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.Matter.Get(id, conn, false);

                if (model.BillTo != null)
                    model.BillTo = Data.Contacts.Contact.Get(model.BillTo.Id.Value, conn, false);

                if (model.DefaultBillingRate != null)
                    model.DefaultBillingRate = Data.Billing.BillingRate.Get(model.DefaultBillingRate.Id.Value, conn, false);

                if (model.BillingGroup != null)
                    model.BillingGroup = Data.Billing.BillingGroup.Get(model.BillingGroup.Id.Value, conn, false);

                if (model.MatterType != null)
                    model.MatterType = Data.Matters.MatterType.Get(model.MatterType.Id.Value, conn, false);

                if (model.CourtType != null)
                    model.CourtType = Data.Matters.CourtType.Get(model.CourtType.Id.Value, conn, false);

                if (model.CourtGeographicalJurisdiction != null)
                    model.CourtGeographicalJurisdiction = Data.Matters.CourtGeographicalJurisdiction.Get(model.CourtGeographicalJurisdiction.Id.Value, conn, false);

                if (model.CourtSittingInCity != null)
                    model.CourtSittingInCity = Data.Matters.CourtSittingInCity.Get(model.CourtSittingInCity.Id.Value, conn, false);

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
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

            viewModel = new ViewModels.Matters.EditMatterViewModel();
            viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model);
            viewModel.Matter.BillTo = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.BillTo);
            viewModel.Matter.DefaultBillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(model.DefaultBillingRate);
            viewModel.Matter.BillingGroup = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(model.BillingGroup);
            viewModel.Matter.CourtType = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(model.CourtType);
            viewModel.Matter.CourtGeographicalJurisdiction = Mapper.Map<ViewModels.Matters.CourtGeographicalJurisdictionViewModel>(model.CourtGeographicalJurisdiction);
            viewModel.Matter.CourtSittingInCity = Mapper.Map<ViewModels.Matters.CourtSittingInCityViewModel>(model.CourtSittingInCity);

            viewModel.EmployeeContactList = employeeContactList;
            viewModel.BillingRateList = billingRateList;
            viewModel.BillingGroupList = billingGroupList;
            viewModel.MatterTypeList = matterTypeList;
            viewModel.CourtTypes = courtTypeList;
            viewModel.CourtGeographicalJurisdictions = courtGeographicalJurisdictionList;
            viewModel.CourtSittingInCities = courtSittingInCityList;

            ViewBag.Matter = model.Title;
            ViewBag.MatterId = model.Id;

            if (viewModel.Matter.MatterType != null && viewModel.Matter.MatterType.Id.HasValue)
                viewModel.MatterTypeId = viewModel.Matter.MatterType.Id.Value;
            if (viewModel.Matter.DefaultBillingRate != null && viewModel.Matter.DefaultBillingRate.Id.HasValue)
                viewModel.DefaultBillingRateId = viewModel.Matter.DefaultBillingRate.Id.Value;
            if (viewModel.Matter.BillingGroup != null && viewModel.Matter.BillingGroup.Id.HasValue)
                viewModel.BillingGroupId = viewModel.Matter.BillingGroup.Id.Value;
            if (viewModel.Matter.CourtType != null && viewModel.Matter.CourtType.Id.HasValue)
                viewModel.CourtTypeId = viewModel.Matter.CourtType.Id.Value;
            if (viewModel.Matter.CourtGeographicalJurisdiction != null && viewModel.Matter.CourtGeographicalJurisdiction.Id.HasValue)
                viewModel.CourtGeographicalJurisdictionId = viewModel.Matter.CourtGeographicalJurisdiction.Id.Value;
            if (viewModel.Matter.CourtSittingInCity != null && viewModel.Matter.CourtSittingInCity.Id.HasValue)
                viewModel.CourtSittingInCityId = viewModel.Matter.CourtSittingInCity.Id.Value;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Matters.EditMatterViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter model;

            viewModel.Matter.Id = id;
            if (viewModel.MatterTypeId.HasValue)
                viewModel.Matter.MatterType = new ViewModels.Matters.MatterTypeViewModel() { Id = viewModel.MatterTypeId };
            if (viewModel.DefaultBillingRateId.HasValue)
                viewModel.Matter.DefaultBillingRate = new ViewModels.Billing.BillingRateViewModel() { Id = viewModel.DefaultBillingRateId };
            if (viewModel.BillingGroupId.HasValue)
                viewModel.Matter.BillingGroup = new ViewModels.Billing.BillingGroupViewModel() { Id = viewModel.BillingGroupId };
            if (viewModel.CourtTypeId.HasValue)
                viewModel.Matter.CourtType = new ViewModels.Matters.CourtTypeViewModel() { Id = viewModel.CourtTypeId };
            if (viewModel.CourtGeographicalJurisdictionId.HasValue)
                viewModel.Matter.CourtGeographicalJurisdiction = new ViewModels.Matters.CourtGeographicalJurisdictionViewModel() { Id = viewModel.CourtGeographicalJurisdictionId };
            if (viewModel.CourtSittingInCityId.HasValue)
                viewModel.Matter.CourtSittingInCity = new ViewModels.Matters.CourtSittingInCityViewModel() { Id = viewModel.CourtSittingInCityId };

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (viewModel.Matter.BillTo == null || !viewModel.Matter.BillTo.Id.HasValue)
                    {
                        List<ViewModels.Contacts.ContactViewModel> employeeContactList;

                        model = Data.Matters.Matter.Get(trans, id);

                        employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

                        Data.Contacts.Contact.ListEmployeesOnly(trans).ForEach(x =>
                        {
                            employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                        });

                        ModelState.AddModelError("BillTo", "Bill To is required");

                        ViewBag.EmployeeContactList = employeeContactList;
                        ViewBag.Matter = model.Title;
                        ViewBag.MatterId = model.Id;
                        return View(viewModel);
                    }

                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.Matter>(viewModel.Matter);

                    model = Data.Matters.Matter.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", new { id = model.Id });
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        // A note on delete - https://github.com/NodineLegal/OpenLawOffice/wiki/Design-of-Disabling-a-Matter

        [Authorize(Roles = "Login, User")]
        public ActionResult Tags(Guid id)
        {
            Common.Models.Matters.Matter matter;
            List<ViewModels.Matters.MatterTagViewModel> viewModelList;

            viewModelList = new List<ViewModels.Matters.MatterTagViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Matters.MatterTag.ListForMatter(id, conn, false).ForEach(x =>
                {
                    viewModelList.Add(Mapper.Map<ViewModels.Matters.MatterTagViewModel>(x));
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }
            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Contacts(Guid id)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Contacts.Contact contact;
            ViewModels.Matters.MatterContactViewModel viewModel;
            List<ViewModels.Matters.MatterContactViewModel> viewModelList;

            viewModelList = new List<ViewModels.Matters.MatterContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Matters.MatterContact.ListForMatter(id, conn, false).ForEach(x =>
                {
                    contact = OpenLawOffice.Data.Contacts.Contact.Get(x.Contact.Id.Value, conn, false);

                    viewModel = Mapper.Map<ViewModels.Matters.MatterContactViewModel>(x);
                    viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                    viewModelList.Add(viewModel);
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Taskless()
        {
            List<ViewModels.Matters.MatterViewModel> tasklessMatters = new List<ViewModels.Matters.MatterViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Matters.Matter.ListMattersWithoutActiveTasks(null, conn, false).ForEach(x =>
                {
                    tasklessMatters.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
                });
            }

            return View(tasklessMatters);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Tasks(Guid id)
        {
            Common.Models.Matters.Matter matter;

            bool? active;
            string activeStr = Request["active"];
            switch (activeStr)
            {
                case "inactive":
                    active = false;
                    break;
                case "both":
                    active = null;
                    break;
                default:
                    active = true;
                    break;
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);
                ViewBag.Matter = matter;

                return View(TasksController.GetListForMatter(id, active, conn));
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult FormFields(Guid id)
        {
            List<ViewModels.Forms.FormFieldMatterViewModel> vmList = new List<ViewModels.Forms.FormFieldMatterViewModel>();
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);
                ViewBag.Matter = matter.Title;
                ViewBag.MatterId = matter.Id;

                Data.Forms.FormFieldMatter.ListForMatter(id, conn, false).ForEach(x =>
                {
                    ViewModels.Forms.FormFieldMatterViewModel vm = new ViewModels.Forms.FormFieldMatterViewModel();
                    x.FormField = Data.Forms.FormField.Get(x.FormField.Id.Value, conn, false);
                    if (x.Matter != null)
                        vm = Mapper.Map<ViewModels.Forms.FormFieldMatterViewModel>(x);
                    else
                        vm = new ViewModels.Forms.FormFieldMatterViewModel();
                    vm.FormField = Mapper.Map<ViewModels.Forms.FormFieldViewModel>(x.FormField);
                    vmList.Add(vm);
                });
            }

            return View(vmList);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult FormFields(Guid id, IEnumerable<ViewModels.Forms.FormFieldMatterViewModel> viewModel)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Account.Users currentUser;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    matter = Data.Matters.Matter.Get(trans, id);

                    viewModel.Each(x =>
                    {
                        Common.Models.Forms.FormFieldMatter model =
                            Data.Forms.FormFieldMatter.Get(trans, matter.Id.Value, x.FormField.Id.Value);

                        if (model != null)
                        {
                            //model = Data.Forms.FormFieldMatter.Get(x.Id.Value);
                            // Edit
                            if (model.Value != x.Value)
                            {
                                model.Value = x.Value;
                                model = Data.Forms.FormFieldMatter.Edit(trans, model, currentUser);
                            }
                        }
                        else
                        {
                            // Create
                            model = new Common.Models.Forms.FormFieldMatter();
                            model.FormField = new Common.Models.Forms.FormField();
                            model.FormField.Id = x.FormField.Id;
                            model.Matter = matter;
                            model.Value = x.Value;
                            model = Data.Forms.FormFieldMatter.Create(trans, model, currentUser);
                        }

                    });

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = id });
                }
                catch
                {
                    trans.Rollback();
                    return FormFields(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Invoices(Guid id)
        {
            Common.Models.Matters.Matter matter;
            List<ViewModels.Billing.InvoiceViewModel> list = new List<ViewModels.Billing.InvoiceViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Billing.Invoice.ListInvoicesForMatter(id, conn, false).ForEach(x =>
                {
                    list.Add(Mapper.Map<ViewModels.Billing.InvoiceViewModel>(x));
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;

            return View(list);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Notes(Guid id)
        {
            Common.Models.Matters.Matter matter;
            List<ViewModels.Notes.NoteTaskViewModel> viewModelList;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                viewModelList = new List<ViewModels.Notes.NoteTaskViewModel>();
                Data.Matters.Matter.ListAllNotes(matter).ForEach(x =>
                {
                    ViewModels.Notes.NoteTaskViewModel ntvm = Mapper.Map<ViewModels.Notes.NoteTaskViewModel>(x);
                    if (x.Task != null && x.Task.Id.HasValue)
                        ntvm.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                            Data.Tasks.Task.Get(x.Task.Id.Value, conn, false));
                    ntvm.Note = Mapper.Map<ViewModels.Notes.NoteViewModel>(
                        Data.Notes.Note.Get(x.Note.Id.Value, conn, false));
                    viewModelList.Add(ntvm);
                });
            }

            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Expenses(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Billing.ExpenseViewModel viewModel;
            List<ViewModels.Billing.ExpenseViewModel> viewModelList;

            viewModelList = new List<ViewModels.Billing.ExpenseViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Billing.Expense.ListForMatter(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Billing.ExpenseViewModel>(x);

                    viewModelList.Add(viewModel);
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Fees(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Billing.FeeViewModel viewModel;
            List<ViewModels.Billing.FeeViewModel> viewModelList;

            viewModelList = new List<ViewModels.Billing.FeeViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Billing.Fee.ListForMatter(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Billing.FeeViewModel>(x);

                    viewModelList.Add(viewModel);
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Documents(Guid id)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Documents.Version version;
            ViewModels.Documents.SelectableDocumentViewModel viewModel;
            List<ViewModels.Documents.SelectableDocumentViewModel> viewModelList;

            viewModelList = new List<ViewModels.Documents.SelectableDocumentViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Documents.Document.ListForMatter(id, conn, false).ForEach(x =>
                {
                    version = Data.Documents.Document.GetCurrentVersion(x.Id.Value, conn, false);

                    viewModel = Mapper.Map<ViewModels.Documents.SelectableDocumentViewModel>(x);

                    viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Documents.Document.GetTask(x.Id.Value, conn, false));
                    viewModel.Extension = version.Extension;

                    viewModelList.Add(viewModel);
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(viewModelList);
        }

        //[HttpPost]
        //[Authorize(Roles = "Login, User")]
        //public ActionResult Documents(Guid id, List<ViewModels.Documents.SelectableDocumentViewModel> viewModelListNull)
        //{
        //    //string zipTitle;
        //    //int inc = 0;
        //    //string path;
        //    //Guid temp;
        //    //List<Common.Models.Documents.Document> documents;
        //    //Common.Models.Matters.Matter matter;
        //    //Common.Models.Documents.Version version;
        //    //ViewModels.Documents.SelectableDocumentViewModel viewModel;
        //    //List<ViewModels.Documents.SelectableDocumentViewModel> viewModelList;

        //    //viewModelList = new List<ViewModels.Documents.SelectableDocumentViewModel>();

        //    //matter = Data.Matters.Matter.Get(id);

        //    //documents = Data.Documents.Document.ListForMatter(id);

        //    //using (ZipFile zip = new ZipFile())
        //    //{
        //    //    //zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
        //    //    documents.ForEach(x =>
        //    //    {
        //    //        version = Data.Documents.Document.GetCurrentVersion(x.Id.Value);

        //    //        viewModel = Mapper.Map<ViewModels.Documents.SelectableDocumentViewModel>(x);

        //    //        viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(Data.Documents.Document.GetTask(x.Id.Value));

        //    //        viewModelList.Add(viewModel);

        //    //        if (Request["CB_" + x.Id.Value.ToString()] == "on")
        //    //        {

        //    //            path = Common.Settings.Manager.Instance.FileStorage.GetCurrentVersionFilepathFor(version.Id.Value + "." + version.Extension);

        //    //            zipTitle = x.Title + "." + version.Extension;

        //    //            if (zip[zipTitle] != null)
        //    //            {
        //    //                inc = 1;
        //    //                zipTitle = x.Title + " (" + inc.ToString() + ")" + "." + version.Extension;
        //    //                while (zip[zipTitle] != null)
        //    //                {
        //    //                    inc++;
        //    //                    zipTitle = x.Title + " (" + inc.ToString() + ")" + "." + version.Extension;
        //    //                }
        //    //            }

        //    //            ZipEntry ze = zip.AddFile(path, "");
        //    //            ze.FileName = zipTitle;
        //    //        }
        //    //    });

        //    //    if (!System.IO.Directory.Exists(Common.Settings.Manager.Instance.FileStorage.TempPath))
        //    //        System.IO.Directory.CreateDirectory(Common.Settings.Manager.Instance.FileStorage.TempPath);

        //    //    temp = Guid.NewGuid();

        //    //    if (zip.Count > 0)
        //    //        zip.Save(Common.Settings.Manager.Instance.FileStorage.TempPath + temp.ToString() + ".zip");
        //    //    else
        //    //        return View(viewModelList);
        //    //}


        //    //return new DeletingFileResult(Common.Settings.Manager.Instance.FileStorage.TempPath + temp.ToString() + ".zip", matter.Title + ".zip");
        //    return null;
        //}

        [Authorize(Roles = "Login, User")]
        public ActionResult Time(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Matters.MatterTimeViewModel viewModel;
            List<Common.Models.Timing.Time> times;
            ViewModels.Matters.MatterTimeViewModel.Task task;
            ViewModels.Timing.TimeViewModel timeViewModel;
            Common.Models.Contacts.Contact contact;

            viewModel = new ViewModels.Matters.MatterTimeViewModel();
            viewModel.Tasks = new List<ViewModels.Matters.MatterTimeViewModel.Task>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Tasks.Task.ListForMatter(id, null, conn, false).ForEach(x =>
                {
                    times = Data.Timing.Time.ListForTask(x.Id.Value, conn, false);

                    if (times != null && times.Count > 0)
                    {
                        task = new ViewModels.Matters.MatterTimeViewModel.Task()
                        {
                            Id = x.Id.Value,
                            Title = x.Title
                        };

                        task.Times = new List<ViewModels.Timing.TimeViewModel>();

                        times.ForEach(y =>
                        {
                            timeViewModel = Mapper.Map<ViewModels.Timing.TimeViewModel>(y);

                            contact = Data.Contacts.Contact.Get(timeViewModel.Worker.Id.Value, conn, false);

                            timeViewModel.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);
                            timeViewModel.WorkerDisplayName = timeViewModel.Worker.DisplayName;

                            task.Times.Add(timeViewModel);
                        });

                        viewModel.Tasks.Add(task);
                    }
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheet(Guid id)
        {
            int contactId;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            ViewModels.Timing.DayViewModel viewModel = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            dynamic profile = ProfileBase.Create(Membership.GetUser().UserName);
            if (profile.ContactId != null && !string.IsNullOrEmpty(profile.ContactId))
                contactId = int.Parse(profile.ContactId);
            else
                throw new ArgumentNullException("Must have a ContactId set in profile.");

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                Data.Contacts.Contact.Get(contactId, conn, false));

                Data.Timing.Time.ListForMatterWithinRange(id, null, null, conn, false).ForEach(x =>
                {
                    ViewModels.Timing.DayViewModel.Item dayVMItem;

                    dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                    dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(dayVMItem.Time.Id.Value, conn, false));

                    dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(dayVMItem.Task.Id.Value, conn, false));

                    viewModel.Items.Add(dayVMItem);
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult Timesheet(ViewModels.Home.DashboardViewModel currentDVM)
        {
            Guid id;
            int contactId;
            Common.Models.Matters.Matter matter;
            DateTime? from = null, to = null;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            ViewModels.Timing.DayViewModel viewModel = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            id = Guid.Parse((string)RouteData.Values["Id"]);
            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    matter = Data.Matters.Matter.Get(trans, id);

                    if (currentDVM.Employee != null && currentDVM.Employee.Id.HasValue)
                    {
                        contactId = currentDVM.Employee.Id.Value;
                    }
                    else
                    {
                        dynamic profile = ProfileBase.Create(Membership.GetUser().UserName);
                        if (profile.ContactId != null && !string.IsNullOrEmpty(profile.ContactId))
                            contactId = int.Parse(profile.ContactId);
                        else
                            throw new ArgumentNullException("Must supply an Id or have a ContactId set in profile.");
                    }
                    viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(trans, contactId));

                    Data.Timing.Time.ListForMatterWithinRange(trans, matter.Id.Value, from, to).ForEach(x =>
                    {
                        ViewModels.Timing.DayViewModel.Item dayVMItem;

                        dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                        dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                        dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                            Data.Timing.Time.GetRelatedTask(trans, dayVMItem.Time.Id.Value));

                        dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                            Data.Tasks.Task.GetRelatedMatter(trans, dayVMItem.Task.Id.Value));

                        viewModel.Items.Add(dayVMItem);
                    });

                    Data.Contacts.Contact.ListEmployeesOnly(trans).ForEach(x =>
                    {
                        employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                    });
                }
                catch
                {
                    trans.Rollback();
                    return Timesheet(id);
                }
            }

            if (from.HasValue)
                ViewBag.From = from.Value;
            if (to.HasValue)
                ViewBag.To = to.Value;

            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheet_PrintInternal(ViewModels.Home.DashboardViewModel currentDVM)
        {
            Guid id;
            int contactId = 0;
            Common.Models.Matters.Matter matter;
            DateTime? from = null, to = null;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            List<Common.Models.Timing.Time> timeList = null;
            ViewModels.Timing.DayViewModel viewModel = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            id = Guid.Parse((string)RouteData.Values["Id"]);
            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                if (currentDVM.Employee != null && currentDVM.Employee.Id.HasValue)
                {
                    contactId = currentDVM.Employee.Id.Value;
                }
                else if (!string.IsNullOrEmpty(Request["empid"]))
                {
                    contactId = int.Parse(Request["empid"]);
                }


                if (contactId > 0)
                {
                    viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(contactId, conn, false));
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, contactId, from, to, conn, false);
                }
                else
                {
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, from, to, conn, false);
                }


                timeList.ForEach(x =>
                {
                    ViewModels.Timing.DayViewModel.Item dayVMItem;

                    dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                    dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(dayVMItem.Time.Id.Value, conn, false));

                    dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(dayVMItem.Task.Id.Value, conn, false));

                    viewModel.Items.Add(dayVMItem);
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }


            if (from.HasValue)
                ViewBag.From = from.Value;
            if (to.HasValue)
                ViewBag.To = to.Value;

            ViewBag.Matter = matter.Title;
            ViewBag.CourtType = matter.CourtType;
            ViewBag.CourtType = matter.CourtGeographicalJurisdiction;
            ViewBag.CourtSittingInCity = matter.CourtSittingInCity;
            ViewBag.CaseNumber = matter.CaseNumber;
            ViewBag.MatterId = matter.Id;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheet_PrintClient(ViewModels.Home.DashboardViewModel currentDVM)
        {
            Guid id;
            int contactId = 0;
            Common.Models.Matters.Matter matter;
            DateTime? from = null, to = null;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            List<Common.Models.Timing.Time> timeList = null;
            ViewModels.Timing.DayViewModel viewModel = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            id = Guid.Parse((string)RouteData.Values["Id"]);
            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                if (currentDVM.Employee != null && currentDVM.Employee.Id.HasValue)
                {
                    contactId = currentDVM.Employee.Id.Value;
                }
                else if (!string.IsNullOrEmpty(Request["empid"]))
                {
                    contactId = int.Parse(Request["empid"]);
                }


                if (contactId > 0)
                {
                    viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(contactId, conn, false));
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, contactId, from, to, conn, false);
                }
                else
                {
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, from, to, conn, false);
                }


                timeList.ForEach(x =>
                {
                    ViewModels.Timing.DayViewModel.Item dayVMItem;

                    dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                    dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(dayVMItem.Time.Id.Value, conn, false));

                    dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(dayVMItem.Task.Id.Value, conn, false));

                    viewModel.Items.Add(dayVMItem);
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }


            if (from.HasValue)
                ViewBag.From = from.Value;
            if (to.HasValue)
                ViewBag.To = to.Value;

            ViewBag.Matter = matter.Title;
            ViewBag.CourtType = matter.CourtType;
            ViewBag.CourtType = matter.CourtGeographicalJurisdiction;
            ViewBag.CourtSittingInCity = matter.CourtSittingInCity;
            ViewBag.CaseNumber = matter.CaseNumber;
            ViewBag.MatterId = matter.Id;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheet_Print3rdParty(ViewModels.Home.DashboardViewModel currentDVM)
        {
            Guid id;
            int contactId = 0;
            Common.Models.Matters.Matter matter;
            DateTime? from = null, to = null;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            List<Common.Models.Timing.Time> timeList = null;
            ViewModels.Timing.DayViewModel viewModel = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            id = Guid.Parse((string)RouteData.Values["Id"]);
            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                if (currentDVM.Employee != null && currentDVM.Employee.Id.HasValue)
                {
                    contactId = currentDVM.Employee.Id.Value;
                }
                else if (!string.IsNullOrEmpty(Request["empid"]))
                {
                    contactId = int.Parse(Request["empid"]);
                }

                if (contactId > 0)
                {
                    viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(contactId, conn, false));
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, contactId, from, to, conn, false);
                }
                else
                {
                    timeList = Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, from, to, conn, false);
                }

                timeList.ForEach(x =>
                {
                    ViewModels.Timing.DayViewModel.Item dayVMItem;

                    dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                    dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(dayVMItem.Time.Id.Value, conn, false));

                    dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(dayVMItem.Task.Id.Value, conn, false));

                    viewModel.Items.Add(dayVMItem);
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }


            if (from.HasValue)
                ViewBag.From = from.Value;
            if (to.HasValue)
                ViewBag.To = to.Value;

            ViewBag.Matter = matter.Title;
            ViewBag.CourtType = matter.CourtType;
            ViewBag.CourtType = matter.CourtGeographicalJurisdiction;
            ViewBag.CourtSittingInCity = matter.CourtSittingInCity;
            ViewBag.CaseNumber = matter.CaseNumber;
            ViewBag.MatterId = matter.Id;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Events(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Events.EventViewModel viewModel;
            List<ViewModels.Events.EventViewModel> viewModelList;

            viewModelList = new List<ViewModels.Events.EventViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Events.Event.ListForMatter(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Events.EventViewModel>(x);

                    viewModelList.Add(viewModel);
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Assets(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Assets.AssetsOfMatterViewModel viewModel = new ViewModels.Assets.AssetsOfMatterViewModel();
            viewModel.Assets = new List<ViewModels.Assets.SelectableAssetViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                Data.Assets.Asset.ListForMatter(id, conn, false).ForEach(x =>
                {
                    ViewModels.Assets.SelectableAssetViewModel vm = Mapper.Map<ViewModels.Assets.SelectableAssetViewModel>(x);

                    vm.Tags = new List<ViewModels.Assets.TagViewModel>();
                    Data.Assets.Tag.ListForAsset(x.Id.Value, conn, false).ForEach(tag =>
                    {
                        vm.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                    });

                    viewModel.Assets.Add(vm);
                });
            }

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult Assets(Guid id, ViewModels.Assets.AssetsOfMatterViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter matter;
            ViewModels.Assets.AssetsOfMatterViewModel newViewModel = new ViewModels.Assets.AssetsOfMatterViewModel();
            newViewModel.Assets = new List<ViewModels.Assets.SelectableAssetViewModel>();

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    matter = Data.Matters.Matter.Get(trans, id);

                    Data.Assets.Asset.ListForMatter(trans, id, viewModel.DateFromFilter, viewModel.DateToFilter, 
                        viewModel.FlagFilter, viewModel.TagFilter, viewModel.TitleFilter).ForEach(x =>
                    {
                        ViewModels.Assets.SelectableAssetViewModel a = null;
                        if (viewModel.Assets != null)
                        {
                            IEnumerable<ViewModels.Assets.SelectableAssetViewModel> hits = viewModel.Assets.Where(y => y.Id == x.Id);
                            if (hits.Count() > 1)
                                a = hits.First();
                            else
                                a = hits.SingleOrDefault();
                        }
                        if ((a != null && a.IsSelected) ||
                            string.IsNullOrEmpty(viewModel.BulkFunction))
                        {
                            ViewModels.Assets.SelectableAssetViewModel vm = Mapper.Map<ViewModels.Assets.SelectableAssetViewModel>(x);

                            vm.Tags = new List<ViewModels.Assets.TagViewModel>();
                            Data.Assets.Tag.ListForAsset(trans, x.Id.Value).ForEach(tag =>
                            {
                                vm.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                            });

                            vm.Files = new List<ViewModels.Assets.FileViewModel>();
                            Data.Assets.Asset.ListFilesForMostRecentVersion(trans, x.Id.Value).ForEach(file =>
                            {
                                vm.Files.Add(Mapper.Map<ViewModels.Assets.FileViewModel>(file));
                            });

                            newViewModel.Assets.Add(vm);

                            if (viewModel.BulkFunction == "checkout")
                            {
                                Data.Assets.Asset.Checkout(trans, x.Id.Value, currentUser);
                            }
                        }
                    });

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

            newViewModel.BulkFunction = viewModel.BulkFunction;
            newViewModel.DateFromFilter = viewModel.DateFromFilter;
            newViewModel.DateToFilter = viewModel.DateToFilter;
            newViewModel.FlagFilter = viewModel.FlagFilter;
            newViewModel.TagFilter = viewModel.TagFilter;
            newViewModel.TitleFilter = viewModel.TitleFilter;

            ViewBag.Matter = matter;

            return View(newViewModel);
        }
    }
}