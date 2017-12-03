// -----------------------------------------------------------------------
// <copyright file="TimingController.cs" company="Nodine Legal, LLC">
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
    using System.Web.Mvc;
    using AutoMapper;
    using System.Collections.Generic;
    using System.Web.Profile;
    using System.Web.Security;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class TimingController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            Common.Models.Timing.Time model;
            ViewModels.Timing.TimeViewModel viewModel;
            Common.Models.Contacts.Contact contact;
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;
            Common.Models.Timing.TimeCategory timeCategory;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Timing.Time.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Timing.TimeViewModel>(model);

                contact = Data.Contacts.Contact.Get(viewModel.Worker.Id.Value, conn, false);

                viewModel.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                task = Data.Timing.Time.GetRelatedTask(model.Id.Value, conn, false);

                if (model.TimeCategory == null || !model.TimeCategory.Id.HasValue)
                    timeCategory = new Common.Models.Timing.TimeCategory() { Id = 0, Title = "Standard" };
                else
                    timeCategory = Data.Timing.TimeCategory.Get(model.TimeCategory.Id.Value, conn, false);
                viewModel.TimeCategory = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(timeCategory);

                PopulateCoreDetails(viewModel, conn);

                ViewBag.IsFastTime = Data.Timing.Time.IsFastTime(id);

                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            Common.Models.Timing.Time model;
            ViewModels.Timing.TimeViewModel viewModel;
            Common.Models.Contacts.Contact contact;
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            Common.Models.Timing.TimeCategory timeCategory;
            List<Common.Models.Timing.TimeCategory> timeCategoryList;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Timing.Time.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Timing.TimeViewModel>(model);

                contact = Data.Contacts.Contact.Get(viewModel.Worker.Id.Value, conn, false);

                viewModel.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                task = Data.Timing.Time.GetRelatedTask(model.Id.Value, conn, false);

                if (model.TimeCategory == null || !model.TimeCategory.Id.HasValue)
                    timeCategory = new Common.Models.Timing.TimeCategory() { Id = 0, Title = "Standard" };
                else
                    timeCategory = Data.Timing.TimeCategory.Get(model.TimeCategory.Id.Value, conn, false);
                viewModel.TimeCategory = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(timeCategory);

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });

                if (!employeeContactList.Exists(x => x.Id == model.Worker.Id))
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(model.Worker.Id.Value, conn, false)));
                }

                ViewBag.TaskId = task.Id.Value;

                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);

                timeCategoryList = Data.Timing.TimeCategory.List(conn, false);
                timeCategoryList.Insert(0, new Common.Models.Timing.TimeCategory()
                {
                    Id = 0,
                    Title = "Standard"
                });
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;
            ViewBag.EmployeeContactList = employeeContactList;
            ViewBag.TimeCategoryList = timeCategoryList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Timing.TimeViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Timing.Time model;
            List<Common.Models.Timing.TimeCategory> timeCategoryList;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Timing.Time>(viewModel);

                    if (model.Stop.HasValue)
                    {
                        List<Common.Models.Timing.Time> conflicts = 
                            Data.Timing.Time.ListConflictingTimes(trans, model.Start, model.Stop.Value, model.Worker.Id.Value);
                
                        if (conflicts.Count > 1 || 
                            (conflicts.Count == 1 && conflicts[0].Id != id))
                        { // conflict found
                            Common.Models.Contacts.Contact contact;
                            Common.Models.Tasks.Task task;
                            Common.Models.Matters.Matter matter;
                            string errorListString = "";
                            List<ViewModels.Contacts.ContactViewModel> employeeContactList;

                            contact = Data.Contacts.Contact.Get(trans, viewModel.Worker.Id.Value);
                            viewModel.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                            task = Data.Timing.Time.GetRelatedTask(trans, model.Id.Value);

                            foreach (Common.Models.Timing.Time time in conflicts)
                            {
                                time.Worker = Data.Contacts.Contact.Get(time.Worker.Id.Value);
                                errorListString += "<li>" + time.Worker.DisplayName +
                                    "</a> worked from " + time.Start.ToString("M/d/yyyy h:mm tt");

                                if (time.Stop.HasValue)
                                    errorListString += " to " + time.Stop.Value.ToString("M/d/yyyy h:mm tt") +
                                        " [<a href=\"/Timing/Edit/" + time.Id.Value.ToString() + "\">edit</a>]";
                                else
                                    errorListString += " to an unknown time " +
                                        "[<a href=\"/Timing/Edit/" + time.Id.Value.ToString() + "\">edit</a>]";

                                errorListString += "</li>";
                            }

                            ViewBag.ErrorMessage = "Time conflicts with the following other time entries:<ul>" + errorListString + "</ul>";

                            //ModelState.AddModelError(String.Empty, "Time conflicts with other time entries.");

                            matter = Data.Tasks.Task.GetRelatedMatter(trans, task.Id.Value);

                            timeCategoryList = Data.Timing.TimeCategory.List(trans);
                            timeCategoryList.Insert(0, new Common.Models.Timing.TimeCategory()
                            {
                                Id = 0,
                                Title = "Standard"
                            });

                            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();
                            Data.Contacts.Contact.ListEmployeesOnly(trans).ForEach(x =>
                            {
                                employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                            });

                            ViewBag.Task = task;
                            ViewBag.Matter = matter;
                            ViewBag.TimeCategoryList = timeCategoryList;
                            ViewBag.EmployeeContactList = employeeContactList;
                            return View(viewModel);
                        }
                    }

                    model = Data.Timing.Time.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = id });
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        //[Authorize(Roles = "Login, User")]
        //public ActionResult FastTime()
        //{
        //    return View(new ViewModels.Timing.TimeViewModel() { Start = DateTime.Now });
        //}
        
        //[HttpPost]
        //[Authorize(Roles = "Login, User")]
        //public ActionResult FastTime(ViewModels.Timing.TimeViewModel viewModel)
        //{
        //    Common.Models.Account.Users currentUser;
        //    Common.Models.Timing.Time model;

        //    currentUser = Data.Account.Users.Get(User.Identity.Name);

        //    model = Mapper.Map<Common.Models.Timing.Time>(viewModel);

        //    model = Data.Timing.Time.Create(model, currentUser);

        //    return RedirectToAction("Details", new { Id = model.Id });
        //}

        //[Authorize(Roles = "Login, User")]
        //public ActionResult FastTimeList()
        //{
        //    List<ViewModels.Timing.TimeViewModel> list;

        //    list = new List<ViewModels.Timing.TimeViewModel>();

        //    Data.Timing.Time.FastTimeList().ForEach(x =>
        //    {
        //        ViewModels.Timing.TimeViewModel viewModel;
        //        Common.Models.Contacts.Contact worker;

        //        worker = Data.Contacts.Contact.Get(x.Worker.Id.Value);

        //        viewModel = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);
        //        viewModel.WorkerDisplayName = worker.DisplayName;

        //        list.Add(viewModel);
        //    });

        //    return View(list);
        //}

        [Authorize(Roles = "Login, User")]
        public ActionResult DayView(ViewModels.Timing.DayViewModel currentDVM)
        {
            int id;
            DateTime date;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            ViewModels.Timing.DayViewModel dayViewVM = new ViewModels.Timing.DayViewModel();

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            if (RouteData.Values["Id"] != null)
            {
                id = int.Parse((string)RouteData.Values["Id"]);
            }
            else if (currentDVM.Employee != null && currentDVM.Employee.Id.HasValue)
            {
                id = currentDVM.Employee.Id.Value;
            }
            else
            {
                dynamic profile = ProfileBase.Create(Membership.GetUser().UserName);
                if (profile.ContactId != null && !string.IsNullOrEmpty(profile.ContactId))
                    id = int.Parse(profile.ContactId);
                else
                    throw new ArgumentNullException("Must supply an Id or have a ContactId set in profile.");
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                dayViewVM.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                    Data.Contacts.Contact.Get(id, conn, false));

                if (Request["Date"] != null)
                    date = DateTime.Parse(Request["Date"]);
                else
                    date = DateTime.Today;

                Data.Timing.Time.ListForDay(id, date, conn, false).ForEach(x =>
                {
                    ViewModels.Timing.DayViewModel.Item dayVMItem;

                    dayVMItem = new ViewModels.Timing.DayViewModel.Item();

                    dayVMItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    dayVMItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(dayVMItem.Time.Id.Value, conn, false));

                    dayVMItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(dayVMItem.Task.Id.Value, conn, false));

                    dayViewVM.Items.Add(dayVMItem);
                });

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }

            ViewBag.Date = date;
            ViewBag.EmployeeContactList = employeeContactList;
            return View(dayViewVM);
        }
    }
}