// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Nodine Legal, LLC">
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
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AutoMapper;
    using System;
    using System.Web.Profile;
    using System.Web.Security;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class HomeController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index(ViewModels.Home.DashboardViewModel currentDVM)
        {
            int id;
            ViewModels.Home.DashboardViewModel viewModel;
            Common.Models.Contacts.Contact employee;
            Common.Models.Account.Users currentUser;
            List<Common.Models.Settings.TagFilter> tagFilter;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                try
                {
                    Data.Account.Users.List(conn, false);
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "Installation");
                }
                
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
                        id = -1; // Happens on initial setup
                }

                if (id <= 0)
                {
                    employee = null;
                    viewModel = null;
                }
                else
                {
                    employee = Data.Contacts.Contact.Get(id, conn, false);
                    currentUser = Data.Account.Users.Get(User.Identity.Name, conn, false);

                    viewModel = new ViewModels.Home.DashboardViewModel();
                    viewModel.Employee = Mapper.Map<ViewModels.Contacts.ContactViewModel>(employee);

                    viewModel.MyTodoList = new List<Tuple<ViewModels.Matters.MatterViewModel, ViewModels.Tasks.TaskViewModel>>();
                    
                    Data.Tasks.Task.GetTodoListFor(employee, null, null, conn, false).ForEach(x =>
                    {
                        matter = Data.Tasks.Task.GetRelatedMatter(x.Id.Value, conn, false);
                        viewModel.MyTodoList.Add(
                            new Tuple<ViewModels.Matters.MatterViewModel, ViewModels.Tasks.TaskViewModel>(
                            Mapper.Map<ViewModels.Matters.MatterViewModel>(matter),
                            Mapper.Map<ViewModels.Tasks.TaskViewModel>(x)));
                    });

                    viewModel.NotificationList = new List<ViewModels.Notes.NoteNotificationViewModel>();
                    Data.Notes.NoteNotification.ListAllNoteNotificationsForContact(employee.Id.Value, conn, false).ForEach(x =>
                    {
                        ViewModels.Notes.NoteNotificationViewModel nnvm = Mapper.Map<ViewModels.Notes.NoteNotificationViewModel>(x);
                        nnvm.Note = Mapper.Map<ViewModels.Notes.NoteViewModel>(Data.Notes.Note.Get(x.Note.Id.Value, conn, false));
                        viewModel.NotificationList.Add(nnvm);
                    });

                    Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                    {
                        employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                    });

                    viewModel.TasklessActiveMatters = new List<ViewModels.Matters.MatterViewModel>();
                    Data.Matters.Matter.ListMattersWithoutActiveTasks(5, conn, false).ForEach(x =>
                    {
                        viewModel.TasklessActiveMatters.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
                    });

                    viewModel.Activities = new List<Tuple<ViewModels.Activities.ActivityBaseViewModel, ViewModels.Activities.ActivityRegardingBaseViewModel>>();
                    Data.Activities.ActivityTask.GetGeneralActivities(employee.Id.Value, conn, false).ForEach(x =>
                    {
                        Tuple<ViewModels.Activities.ActivityBaseViewModel, ViewModels.Activities.ActivityRegardingBaseViewModel> vm = null;
                        ViewModels.Activities.ActivityBaseViewModel vm1;

                        x.Type = Data.Activities.ActivityType.Get(x.Type.Id.Value, conn, false);

                        vm1 = Mapper.Map<ViewModels.Activities.ActivityBaseViewModel>(x);
                        vm1.Type = Mapper.Map<ViewModels.Activities.ActivityTypeViewModel>(x.Type);

                        Common.Models.Activities.ActivityRegardingType regardingModel = Data.Activities.ActivityRegardingType.GetFromActivityId(x.Id.Value, conn, false);
                        
                        if (regardingModel.Title == "Lead")
                        {
                            Common.Models.Activities.ActivityRegardingLead lm = Data.Activities.ActivityRegardingLead.GetFromActivityId(x.Id.Value, conn, false);
                            vm = new Tuple<ViewModels.Activities.ActivityBaseViewModel, ViewModels.Activities.ActivityRegardingBaseViewModel>(vm1,
                                Mapper.Map<ViewModels.Activities.ActivityRegardingLeadViewModel>(lm));
                        }
                        else if (regardingModel.Title == "Opportunity")
                        {
                            Common.Models.Activities.ActivityRegardingOpportunity om = Data.Activities.ActivityRegardingOpportunity.GetFromActivityId(x.Id.Value, conn, false);
                            vm = new Tuple<ViewModels.Activities.ActivityBaseViewModel, ViewModels.Activities.ActivityRegardingBaseViewModel>(vm1,
                                Mapper.Map<ViewModels.Activities.ActivityRegardingOpportunityViewModel>(om));
                        }

                        viewModel.Activities.Add(vm);
                    });

                    Random rnd = new Random();
                    int activeMatters = Data.Matters.Matter.CountAllActiveMatters(conn, false);
                    int activeMattersWithoutTasks = Data.Matters.Matter.CountAllMattersWithoutActiveTasks(conn, false);
                    ViewBag.MattersWithActiveTasks = activeMatters - activeMattersWithoutTasks;
                    ViewBag.MattersWithoutActiveTasks = activeMattersWithoutTasks;

                   
                }
            }

            ViewBag.EmployeeContactList = employeeContactList;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult About()
        {
            return View();
        }

        private System.Drawing.Color RandomPastelColor(Random rnd)
        {
            int r = (int)(Math.Round(rnd.NextDouble() * 127) + 127);
            int g = (int)(Math.Round(rnd.NextDouble() * 127) + 127);
            int b = (int)(Math.Round(rnd.NextDouble() * 127) + 127);
            return System.Drawing.Color.FromArgb(1, r, g, b);
        }

        private string HexColor(System.Drawing.Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
    }
}