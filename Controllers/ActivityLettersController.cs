// -----------------------------------------------------------------------
// <copyright file="ActivityLettersController.cs" company="Nodine Legal, LLC">
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
    public class ActivityLettersController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(long id)
        {
            ViewModels.Activities.ActivityLetterViewModel viewModel = new ViewModels.Activities.ActivityLetterViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Common.Models.Activities.ActivityLetter model = Data.Activities.ActivityLetter.Get(id, conn, false);
                model.Direction = Data.Activities.ActivityDirection.Get(model.Direction.Id.Value, conn, false);
                model.Owner = Data.Contacts.Contact.Get(model.Owner.Id.Value, conn, false);
                model.Sender = Data.Contacts.Contact.Get(model.Sender.Id.Value, conn, false);
                model.Recipient = Data.Contacts.Contact.Get(model.Recipient.Id.Value, conn, false);
                model.Priority = Data.Activities.ActivityPriority.Get(model.Priority.Id.Value, conn, false);
                model.StatusReason = Data.Activities.ActivityStatusReason.Get(model.StatusReason.Id.Value, conn, false);
                model.Type = Data.Activities.ActivityType.Get(model.Type.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Activities.ActivityLetterViewModel>(model);
                viewModel.Direction = Mapper.Map<ViewModels.Activities.ActivityDirectionViewModel>(model.Direction);
                viewModel.Owner = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Owner);
                viewModel.Sender = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Sender);
                viewModel.Recipient = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Recipient);
                viewModel.Priority = Mapper.Map<ViewModels.Activities.ActivityPriorityViewModel>(model.Priority);
                viewModel.StatusReason = Mapper.Map<ViewModels.Activities.ActivityStatusReasonViewModel>(model.StatusReason);
                viewModel.Type = Mapper.Map<ViewModels.Activities.ActivityTypeViewModel>(model.Type);

                Common.Models.Activities.ActivityRegardingType regtype = Data.Activities.ActivityRegardingType.GetFromActivityId(model.Id.Value, conn, false);

                if (regtype.Title == "Lead")
                {
                    Common.Models.Activities.ActivityRegardingLead ar = Data.Activities.ActivityRegardingLead.GetFromActivityId(model.Id.Value, conn, false);
                    ar.Lead = Data.Leads.Lead.Get(ar.Lead.Id.Value, conn, false);
                    ar.Lead.Contact = Data.Contacts.Contact.Get(ar.Lead.Contact.Id.Value, conn, false);
                    viewModel.RegardingLead = Mapper.Map<ViewModels.Activities.ActivityRegardingLeadViewModel>(ar);
                    viewModel.RegardingLead.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(ar.Lead);
                    viewModel.RegardingLead.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(ar.Lead.Contact);
                    viewModel.RegardingLead.Type = Mapper.Map<ViewModels.Activities.ActivityRegardingTypeViewModel>(regtype);
                }
                else
                {
                    Common.Models.Activities.ActivityRegardingOpportunity ao = Data.Activities.ActivityRegardingOpportunity.GetFromActivityId(model.Id.Value, conn, false);
                    ao.Opportunity = Data.Opportunities.Opportunity.Get(ao.Opportunity.Id.Value, conn, false);
                    ao.Opportunity.Account = Data.Contacts.Contact.Get(ao.Opportunity.Account.Id.Value, conn, false);
                    viewModel.RegardingOpportunity = Mapper.Map<ViewModels.Activities.ActivityRegardingOpportunityViewModel>(ao);
                    viewModel.RegardingOpportunity.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(ao.Opportunity);
                    viewModel.RegardingOpportunity.Opportunity.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(ao.Opportunity.Account);
                    viewModel.RegardingOpportunity.Type = Mapper.Map<ViewModels.Activities.ActivityRegardingTypeViewModel>(regtype);
                }

                PopulateCoreDetails(viewModel, conn);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create(long? regleadid, long? regopportunityid)
        {
            Common.Models.Account.Users currentUser;
            ViewModels.Activities.ActivityLetterViewModel viewModel = new ViewModels.Activities.ActivityLetterViewModel();
            List<ViewModels.Activities.ActivityDirectionViewModel> directionList = new List<ViewModels.Activities.ActivityDirectionViewModel>();
            List<ViewModels.Activities.ActivityStatusReasonViewModel> statusReasonList = new List<ViewModels.Activities.ActivityStatusReasonViewModel>();
            List<ViewModels.Activities.ActivityPriorityViewModel> priorityList = new List<ViewModels.Activities.ActivityPriorityViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                currentUser = Data.Account.Users.Get(User.Identity.Name, conn, false);

                Data.Activities.ActivityDirection.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityDirectionViewModel vm = Mapper.Map<ViewModels.Activities.ActivityDirectionViewModel>(x);
                    directionList.Add(vm);
                    if (vm.Order == 1)
                        viewModel.Direction = vm;
                });

                Data.Activities.ActivityStatusReason.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityStatusReasonViewModel vm = Mapper.Map<ViewModels.Activities.ActivityStatusReasonViewModel>(x);
                    statusReasonList.Add(vm);
                    if (vm.Order == 1)
                        viewModel.StatusReason = vm;
                });

                Data.Activities.ActivityPriority.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityPriorityViewModel vm = Mapper.Map<ViewModels.Activities.ActivityPriorityViewModel>(x);
                    priorityList.Add(vm);
                    if (vm.Default.HasValue && vm.Default.Value)
                        viewModel.Priority = vm;
                });

                if (regleadid != null && regleadid.HasValue)
                {
                    viewModel.RegardingLead = new ViewModels.Activities.ActivityRegardingLeadViewModel()
                    {
                        Type = new ViewModels.Activities.ActivityRegardingTypeViewModel()
                        {
                            Id = 1
                        }
                    };

                    viewModel.RegardingLead.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(Data.Leads.Lead.Get(regleadid.Value, conn, false));
                    viewModel.RegardingLead.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(viewModel.RegardingLead.Lead.Contact.Id.Value, conn, false));
                }
                else if (regopportunityid != null && regopportunityid.HasValue)
                {
                    viewModel.RegardingOpportunity = new ViewModels.Activities.ActivityRegardingOpportunityViewModel()
                    {
                        Type = new ViewModels.Activities.ActivityRegardingTypeViewModel()
                        {
                            Id = 2
                        }                        
                    };

                    viewModel.RegardingOpportunity.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(Data.Opportunities.Opportunity.Get(regopportunityid.Value, conn, false));
                    viewModel.RegardingOpportunity.Opportunity.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(viewModel.RegardingOpportunity.Opportunity.Account.Id.Value, conn, false));
                }
                else
                    throw new ArgumentException("Must have regleadid or regopportunityid.");

                dynamic profile = System.Web.Profile.ProfileBase.Create(currentUser.Username);
                viewModel.Owner = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(int.Parse(profile.ContactId), conn, false));
                viewModel.Type = Mapper.Map<ViewModels.Activities.ActivityTypeViewModel>(Data.Activities.ActivityType.Get("Letter", conn, false));
            }

            ViewBag.DirectionList = directionList;
            ViewBag.StatusReasonList = statusReasonList;
            ViewBag.PriorityList = priorityList;

            viewModel.Body = string.Empty;
            viewModel.Due = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 0, 0, DateTimeKind.Local);
            viewModel.Duration = 30;
            viewModel.IsCampaignResponse = false;
            viewModel.State = true;
            viewModel.Subject = string.Empty;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Activities.ActivityLetterViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Activities.ActivityLetter model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    
                    model = Mapper.Map<Common.Models.Activities.ActivityLetter>(viewModel);
                    model.Type = Data.Activities.ActivityType.Get(trans, "Letter");

                    if (!model.IsCampaignResponse.HasValue)
                        model.IsCampaignResponse = false;                    

                    model = Data.Activities.ActivityLetter.Create(trans, model, currentUser);

                    if (viewModel.RegardingLead != null && viewModel.RegardingLead.Lead != null && viewModel.RegardingLead.Lead.Id.HasValue)
                    {
                        Common.Models.Activities.ActivityRegardingLead arl = new Common.Models.Activities.ActivityRegardingLead()
                        {
                            Lead = new Common.Models.Leads.Lead()
                            {
                                Id = viewModel.RegardingLead.Lead.Id
                            },
                            Activity = model,
                            Type = new Common.Models.Activities.ActivityRegardingType()
                            {
                                Id = 1
                            }
                        };
                        Data.Activities.ActivityRegardingLead.Create(trans, arl, currentUser);

                        trans.Commit();

                        return RedirectToAction("Details", "Leads", new { Id = viewModel.RegardingLead.Lead.Id });
                    }
                    else if (viewModel.RegardingOpportunity != null && viewModel.RegardingOpportunity.Opportunity != null && viewModel.RegardingOpportunity.Opportunity.Id.HasValue)
                    {
                        Common.Models.Activities.ActivityRegardingOpportunity aro = new Common.Models.Activities.ActivityRegardingOpportunity()
                        {
                            Opportunity = new Common.Models.Opportunities.Opportunity()
                            {
                                Id = viewModel.RegardingOpportunity.Opportunity.Id
                            },
                            Activity = model,
                            Type = new Common.Models.Activities.ActivityRegardingType()
                            {
                                Id = 2
                            }
                        };
                        Data.Activities.ActivityRegardingOpportunity.Create(trans, aro, currentUser);

                        trans.Commit();

                        return RedirectToAction("Details", "Opportunities", new { Id = viewModel.RegardingOpportunity.Opportunity.Id });
                    }
                    else
                        throw new ArgumentException("Unkown regarding entity.");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return RedirectToAction("Create");
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id)
        {
            Common.Models.Account.Users currentUser;
            ViewModels.Activities.ActivityLetterViewModel viewModel = new ViewModels.Activities.ActivityLetterViewModel();
            List<ViewModels.Activities.ActivityDirectionViewModel> directionList = new List<ViewModels.Activities.ActivityDirectionViewModel>();
            List<ViewModels.Activities.ActivityStatusReasonViewModel> statusReasonList = new List<ViewModels.Activities.ActivityStatusReasonViewModel>();
            List<ViewModels.Activities.ActivityPriorityViewModel> priorityList = new List<ViewModels.Activities.ActivityPriorityViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                currentUser = Data.Account.Users.Get(User.Identity.Name, conn, false);

                Common.Models.Activities.ActivityLetter model = Data.Activities.ActivityLetter.Get(id, conn, false);
                model.Direction = Data.Activities.ActivityDirection.Get(model.Direction.Id.Value, conn, false);
                model.Owner = Data.Contacts.Contact.Get(model.Owner.Id.Value, conn, false);
                model.Sender = Data.Contacts.Contact.Get(model.Sender.Id.Value, conn, false);
                model.Recipient = Data.Contacts.Contact.Get(model.Recipient.Id.Value, conn, false);
                model.Priority = Data.Activities.ActivityPriority.Get(model.Priority.Id.Value, conn, false);
                model.StatusReason = Data.Activities.ActivityStatusReason.Get(model.StatusReason.Id.Value, conn, false);
                model.Type = Data.Activities.ActivityType.Get(model.Type.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Activities.ActivityLetterViewModel>(model);
                viewModel.Owner = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Owner);
                viewModel.Sender = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Sender);
                viewModel.Recipient = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Recipient);

                Common.Models.Activities.ActivityRegardingType regtype = Data.Activities.ActivityRegardingType.GetFromActivityId(model.Id.Value, conn, false);

                if (regtype.Title == "Lead")
                {
                    Common.Models.Activities.ActivityRegardingLead ar = Data.Activities.ActivityRegardingLead.GetFromActivityId(model.Id.Value, conn, false);
                    ar.Lead = Data.Leads.Lead.Get(ar.Lead.Id.Value, conn, false);
                    ar.Lead.Contact = Data.Contacts.Contact.Get(ar.Lead.Contact.Id.Value, conn, false);
                    viewModel.RegardingLead = Mapper.Map<ViewModels.Activities.ActivityRegardingLeadViewModel>(ar);
                    viewModel.RegardingLead.Lead = Mapper.Map<ViewModels.Leads.LeadViewModel>(ar.Lead);
                    viewModel.RegardingLead.Lead.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(ar.Lead.Contact);
                    viewModel.RegardingLead.Type = Mapper.Map<ViewModels.Activities.ActivityRegardingTypeViewModel>(regtype);
                }
                else
                {
                    Common.Models.Activities.ActivityRegardingOpportunity ao = Data.Activities.ActivityRegardingOpportunity.GetFromActivityId(model.Id.Value, conn, false);
                    ao.Opportunity = Data.Opportunities.Opportunity.Get(ao.Opportunity.Id.Value, conn, false);
                    ao.Opportunity.Account = Data.Contacts.Contact.Get(ao.Opportunity.Account.Id.Value, conn, false);
                    viewModel.RegardingOpportunity = Mapper.Map<ViewModels.Activities.ActivityRegardingOpportunityViewModel>(ao);
                    viewModel.RegardingOpportunity.Opportunity = Mapper.Map<ViewModels.Opportunities.OpportunityViewModel>(ao.Opportunity);
                    viewModel.RegardingOpportunity.Opportunity.Account = Mapper.Map<ViewModels.Contacts.ContactViewModel>(ao.Opportunity.Account);
                    viewModel.RegardingOpportunity.Type = Mapper.Map<ViewModels.Activities.ActivityRegardingTypeViewModel>(regtype);
                }

                Data.Activities.ActivityDirection.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityDirectionViewModel vm = Mapper.Map<ViewModels.Activities.ActivityDirectionViewModel>(x);
                    directionList.Add(vm);
                });

                Data.Activities.ActivityStatusReason.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityStatusReasonViewModel vm = Mapper.Map<ViewModels.Activities.ActivityStatusReasonViewModel>(x);
                    statusReasonList.Add(vm);
                });

                Data.Activities.ActivityPriority.List(conn, false).ForEach(x =>
                {
                    ViewModels.Activities.ActivityPriorityViewModel vm = Mapper.Map<ViewModels.Activities.ActivityPriorityViewModel>(x);
                    priorityList.Add(vm);
                });
            }

            ViewBag.DirectionList = directionList;
            ViewBag.StatusReasonList = statusReasonList;
            ViewBag.PriorityList = priorityList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id, ViewModels.Activities.ActivityLetterViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Activities.ActivityLetter model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Activities.ActivityLetter>(viewModel);
                    model.Type = Data.Activities.ActivityType.Get(trans, "Letter");

                    if (!model.IsCampaignResponse.HasValue)
                        model.IsCampaignResponse = false;

                    model = Data.Activities.ActivityLetter.Edit(trans, model, currentUser);
                    trans.Commit();

                    if (viewModel.RegardingLead != null && viewModel.RegardingLead.Lead != null && viewModel.RegardingLead.Lead.Id.HasValue)
                    {
                        return RedirectToAction("Details", "Leads", new { Id = viewModel.RegardingLead.Lead.Id });
                    }
                    else if (viewModel.RegardingOpportunity != null && viewModel.RegardingOpportunity.Opportunity != null && viewModel.RegardingOpportunity.Opportunity.Id.HasValue)
                    {
                        return RedirectToAction("Details", "Opportunities", new { Id = viewModel.RegardingOpportunity.Opportunity.Id });
                    }
                    else
                        throw new ArgumentException("Unkown regarding entity.");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return RedirectToAction("Edit", new { id = id });
                }
            }
        }
    }
}