// -----------------------------------------------------------------------
// <copyright file="ContactsController.cs" company="Nodine Legal, LLC">
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
    public class ContactsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Contacts.ContactViewModel> viewModelList = null;
            string contactFilter;

            viewModelList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                if (!string.IsNullOrEmpty(contactFilter = Request["contactFilter"]))
                {
                    Data.Contacts.Contact.List(contactFilter, conn, false).ForEach(x =>
                    {
                        viewModelList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                    });
                }
                else
                {
                    Data.Contacts.Contact.List(conn, false).ForEach(x =>
                    {
                        viewModelList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                    });
                }
            }

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult ListDisplayNameOnly()
        {
            string term;
            List<Common.Models.Contacts.Contact> list;

            term = Request["term"];

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                list = Data.Contacts.Contact.List(term.Trim(), conn, false);
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            Common.Models.Contacts.Contact contact = null;
            ViewModels.Contacts.ContactViewModel viewModel;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                contact = OpenLawOffice.Data.Contacts.Contact.Get(id, conn, false);
                if (contact.BillingRate != null && contact.BillingRate.Id.HasValue)
                    contact.BillingRate = Data.Billing.BillingRate.Get(contact.BillingRate.Id.Value, conn, false);
                else
                    contact.BillingRate = null;

                if (contact == null)
                    return View("InvalidRequest");

                viewModel = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);
                if (contact.BillingRate != null)
                    viewModel.BillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(contact.BillingRate);

                PopulateCoreDetails(viewModel, conn);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;

            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Billing.BillingRate.List(conn, false).ForEach(x =>
                {
                    ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                    vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                    billingRateList.Add(vm);
                });
            }

            ViewBag.BillingRateList = billingRateList;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Contacts.ContactViewModel viewModel)
        {
            string errorListString = "";
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
            Common.Models.Account.Users currentUser = null;
            Common.Models.Contacts.Contact model;
            List<Common.Models.Contacts.Contact> possibleDuplicateList;


            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Contacts.Contact>(viewModel);

                    if (Request["OverrideConflict"] != "True")
                    {
                        possibleDuplicateList = Data.Contacts.Contact.ListPossibleDuplicates(trans, model);

                        if (possibleDuplicateList.Count > 0)
                        {
                            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

                            possibleDuplicateList.ForEach(x =>
                            {
                                errorListString += "<li><a href=\"/Contacts/Details/" + x.Id.Value + "\">" + x.DisplayName + "</a> [<a href=\"/Contacts/Edit/" + x.Id.Value + "\">edit</a>]</li>";
                            });

                            Data.Billing.BillingRate.List(trans).ForEach(x =>
                            {
                                ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                                vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                                billingRateList.Add(vm);
                            });

                            ViewBag.ErrorMessage = "Contact possibly conflicts with the following existing contacts:<ul>" + errorListString + "</ul>Click Save again to create the contact anyway.";
                            ViewBag.OverrideConflict = "True";
                            ViewBag.BillingRateList = billingRateList;
                            return View(viewModel);
                        }
                    }
            
                    model = Data.Contacts.Contact.Create(trans, model, currentUser);

                    trans.Commit();
                    return RedirectToAction("Details", new { Id = model.Id.Value });
                }
                catch
                {
                    trans.Rollback();
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            Common.Models.Contacts.Contact model = null;
            ViewModels.Contacts.ContactViewModel viewModel;
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;

            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Contacts.Contact.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model);

                if (model.BillingRate != null && model.BillingRate.Id.HasValue)
                {
                    model.BillingRate = Data.Billing.BillingRate.Get(model.BillingRate.Id.Value, conn, false);
                    viewModel.BillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(model.BillingRate);
                }

                Data.Billing.BillingRate.List(conn, false).ForEach(x =>
                {
                    ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                    vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                    billingRateList.Add(vm);
                });
            }

            ViewBag.BillingRateList = billingRateList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Contacts.ContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser = null;
            Common.Models.Contacts.Contact model, oldModel;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    // When modifying a user, if they have a billing rate then if they are being modified from an employee to not an employee (terminated)
                    // we need to make sure and retain that billing rate - needs to also retain the rate for future edits - basically, once there is a billing rate
                    // there will always be a billing rate

                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Contacts.Contact>(viewModel);

                    oldModel = Data.Contacts.Contact.Get(trans, id);
                    if (oldModel.BillingRate != null && oldModel.BillingRate.Id.HasValue)
                        model.BillingRate = oldModel.BillingRate;

                    model = Data.Contacts.Contact.Edit(trans, model, currentUser);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Conflicts(int id)
        {
            List<Common.Models.Matters.Matter> matterList = null;
            Common.Models.Contacts.Contact contact;
            List<Tuple<Common.Models.Matters.Matter, Common.Models.Matters.MatterContact, Common.Models.Contacts.Contact>> matterRelationshipList;
            ViewModels.Contacts.ConflictViewModel viewModel = new ViewModels.Contacts.ConflictViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                contact = Data.Contacts.Contact.Get(id, conn, false);

                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                viewModel.Matters = new List<ViewModels.Contacts.ConflictViewModel.MatterRelationship>();

                matterList = Data.Contacts.Contact.ListMattersForContact(id, conn, false);

                foreach (var x in matterList)
                {
                    ViewModels.Contacts.ConflictViewModel.MatterRelationship mr = new ViewModels.Contacts.ConflictViewModel.MatterRelationship();

                    mr.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(x);

                    mr.MatterContacts = new List<ViewModels.Matters.MatterContactViewModel>();

                    matterRelationshipList = Data.Contacts.Contact.ListMatterRelationshipsForContact(id, x.Id.Value, conn, false);

                    matterRelationshipList.ForEach(y =>
                    {
                        ViewModels.Matters.MatterContactViewModel mc = Mapper.Map<ViewModels.Matters.MatterContactViewModel>(y.Item2);
                        mc.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(y.Item3);
                        mc.Matter = mr.Matter;
                        mr.MatterContacts.Add(mc);
                    });

                    viewModel.Matters.Add(mr);
                };
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Matters(int? id)
        {
            List<ViewModels.Matters.MatterViewModel> list;
            int contactId;

            if (id.HasValue)
                contactId = id.Value;
            else
                contactId = int.Parse(Request["ContactId"]);

            list = new List<ViewModels.Matters.MatterViewModel>();
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                ViewBag.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(contactId));
                Data.Matters.Matter.ListAllMattersForContact(contactId, conn, false).ForEach(x =>
                {
                    list.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
                });
            }

            return View(list);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Tasks(int? id)
        {
            ViewModels.Tasks.TaskViewModel viewModel;
            List<ViewModels.Tasks.TaskViewModel> list;
            int contactId;

            if (id.HasValue)
                contactId = id.Value;
            else
                contactId = int.Parse(Request["ContactId"]);

            list = new List<ViewModels.Tasks.TaskViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                ViewBag.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(contactId));
                Data.Tasks.Task.ListAllTasksForContact(contactId, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(x);

                    if (viewModel.IsGroupingTask)
                    {
                        if (Data.Tasks.Task.GetTaskForWhichIAmTheSequentialPredecessor(x.Id.Value, conn, false) != null)
                            viewModel.Type = "Sequential Group";
                        else
                            viewModel.Type = "Group";
                    }
                    else
                    {
                        if (x.SequentialPredecessor != null)
                            viewModel.Type = "Sequential";
                        else
                            viewModel.Type = "Standard";
                    }
                    list.Add(viewModel);
                });
            }

            return View(list);
        }
    }
}