// -----------------------------------------------------------------------
// <copyright file="TasksController.cs" company="Nodine Legal, LLC">
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
    using System.Configuration;
    using System.Web.Profile;
    using System.Web.Security;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class TasksController : BaseController
    {
        //[HttpGet]
        //[Authorize(Roles = "Login, User")]
        //public ActionResult ListChildrenJqGrid(long? id)
        //{
        //    List<ViewModels.Tasks.TaskViewModel> modelList;
        //    ViewModels.JqGridObject jqObject;
        //    List<object> anonList;
        //    int level = 0;

        //    if (id == null)
        //    {
        //        // jqGrid uses nodeid by default
        //        if (!string.IsNullOrEmpty(Request["nodeid"]))
        //            id = long.Parse(Request["nodeid"]);
        //    }

        //    anonList = new List<object>();

        //    if (!string.IsNullOrEmpty(Request["n_level"]))
        //        level = int.Parse(Request["n_level"]) + 1;

        //    if (!id.HasValue)
        //    {
        //        string matterid = Request["MatterId"];
        //        if (string.IsNullOrEmpty(matterid))
        //            modelList = GetList();
        //        else
        //            modelList = GetListForMatter(Guid.Parse(matterid));
        //    }
        //    else
        //    {
        //        modelList = GetChildrenList(id.Value);
        //    }

        //    modelList.ForEach(x =>
        //    {
        //        if (x.IsGroupingTask)
        //        {
        //            // isLeaf = false
        //            anonList.Add(new
        //            {
        //                Id = x.Id,
        //                Title = x.Title,
        //                Type = x.Type,
        //                DueDate = x.DueDate,
        //                Description = x.Description,
        //                level = level,
        //                isLeaf = false,
        //                expanded = false
        //            });
        //        }
        //        else
        //        {
        //            // isLeaf = true
        //            anonList.Add(new
        //            {
        //                Id = x.Id,
        //                Title = x.Title,
        //                Type = x.Type,
        //                DueDate = x.DueDate,
        //                Description = x.Description,
        //                level = level,
        //                isLeaf = true,
        //                expanded = false
        //            });
        //        }
        //    });

        //    jqObject = new ViewModels.JqGridObject()
        //    {
        //        TotalPages = 1,
        //        CurrentPage = 1,
        //        TotalRecords = modelList.Count,
        //        Rows = anonList.ToArray()
        //    };

        //    return Json(jqObject, JsonRequestBehavior.AllowGet);
        //}

        public static List<ViewModels.Tasks.TaskViewModel> GetChildrenList(long id, IDbConnection conn)
        {
            List<ViewModels.Tasks.TaskViewModel> viewModelList;

            viewModelList = new List<ViewModels.Tasks.TaskViewModel>();
            ViewModels.Tasks.TaskViewModel viewModel;

            Data.Tasks.Task.ListChildren(id, null, conn, false).ForEach(x =>
            {
                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(x);

                if (viewModel.IsGroupingTask)
                {
                    if (Data.Tasks.Task.GetTaskForWhichIAmTheSequentialPredecessor(x.Id.Value) != null)
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

                viewModelList.Add(viewModel);
            });

            return viewModelList;
        }

        public static List<ViewModels.Tasks.TaskViewModel> GetListForMatter(Guid matterid, bool? active, IDbConnection conn)
        {
            List<ViewModels.Tasks.TaskViewModel> viewModelList;
            ViewModels.Tasks.TaskViewModel viewModel;

            viewModelList = new List<ViewModels.Tasks.TaskViewModel>();

            Data.Tasks.Task.ListForMatter(matterid, active, conn, false).ForEach(x =>
            {
                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(x);

                if (viewModel.IsGroupingTask)
                {
                    if (Data.Tasks.Task.GetTaskForWhichIAmTheSequentialPredecessor(x.Id.Value, conn, false)
                        != null)
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

                viewModelList.Add(viewModel);
            });

            return viewModelList;
        }

        public static List<ViewModels.Tasks.TaskViewModel> GetList(IDbConnection conn)
        {
            List<ViewModels.Tasks.TaskViewModel> viewModelList;
            ViewModels.Tasks.TaskViewModel viewModel;

            viewModelList = new List<ViewModels.Tasks.TaskViewModel>();

            Data.Tasks.Task.List(conn, false).ForEach(x =>
            {
                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(x);

                if (viewModel.IsGroupingTask)
                {
                    if (Data.Tasks.Task.GetTaskForWhichIAmTheSequentialPredecessor(x.Id.Value, conn, false)
                        != null)
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

                viewModelList.Add(viewModel);
            });

            return viewModelList;
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(long id)
        {
            ViewModels.Tasks.TaskViewModel viewModel;
            Common.Models.Tasks.Task model;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.Task.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model);

                viewModel.Notes = new List<ViewModels.Notes.NoteViewModel>();
                Data.Notes.NoteTask.ListForTask(id, conn, false).ForEach(x =>
                {
                    viewModel.Notes.Add(Mapper.Map<ViewModels.Notes.NoteViewModel>(x));
                });

                viewModel.Times = new List<ViewModels.Timing.TimeViewModel>();
                Data.Timing.Time.ListForTask(id, conn, false).ForEach(x =>
                {
                    x.Worker = Data.Contacts.Contact.Get(x.Worker.Id.Value, conn, false);
                    ViewModels.Timing.TimeViewModel vm = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);
                    vm.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(x.Worker);
                    viewModel.Times.Add(vm);
                });

                PopulateCoreDetails(viewModel, conn);

                if (model.Parent != null && model.Parent.Id.HasValue)
                {
                    model.Parent = Data.Tasks.Task.Get(model.Parent.Id.Value, conn, false);
                    viewModel.Parent = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Parent);
                }

                if (model.SequentialPredecessor != null && model.SequentialPredecessor.Id.HasValue)
                {
                    model.SequentialPredecessor = Data.Tasks.Task.Get(model.SequentialPredecessor.Id.Value, conn, false);
                    viewModel.SequentialPredecessor = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.SequentialPredecessor);
                }

                matter = Data.Tasks.Task.GetRelatedMatter(model.Id.Value, conn, false);
            }
            
            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id)
        {
            ViewModels.Tasks.TaskViewModel viewModel;
            Common.Models.Tasks.Task model;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.Task.Get(id, conn, false);
                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model);

                if (model.Parent != null && model.Parent.Id.HasValue)
                {
                    model.Parent = Data.Tasks.Task.Get(model.Parent.Id.Value, conn, false);
                    viewModel.Parent = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Parent);
                }

                if (model.SequentialPredecessor != null && model.SequentialPredecessor.Id.HasValue)
                {
                    model.SequentialPredecessor = Data.Tasks.Task.Get(model.SequentialPredecessor.Id.Value, conn, false);
                    viewModel.SequentialPredecessor = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.SequentialPredecessor);
                }

                matter = Data.Tasks.Task.GetRelatedMatter(model.Id.Value, conn, false);
            }

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id, ViewModels.Tasks.TaskViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.Task model;
            Common.Models.Matters.Matter matterModel;
            Common.Models.Tasks.Task currentModel;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.Task>(viewModel);

                    matterModel = Data.Tasks.Task.GetRelatedMatter(trans, id);

                    currentModel = Data.Tasks.Task.Get(trans, id);

                    if (model.Parent != null && model.Parent.Id.HasValue)
                    {
                        if (model.Parent.Id.Value == model.Id.Value)
                        {
                            //  Task is trying to set itself as its parent
                            ModelState.AddModelError("Parent.Id", "Parent cannot be the task itself.");

                            ViewBag.MatterId = matterModel.Id.Value;
                            ViewBag.Matter = matterModel.Title;

                            return View(viewModel);
                        }
                    }

                    model.Description = new Ganss.XSS.HtmlSanitizer().Sanitize(model.Description);
                    model = Data.Tasks.Task.Edit(trans, model, currentUser);

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

        [Authorize(Roles = "Login, User")]
        public ActionResult Close(long id)
        {
            return Close(id, null);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult CloseWithNewTask(long id)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.Task model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Data.Tasks.Task.Get(trans, id);
                    model.Active = false;
                    model.ActualEnd = DateTime.Now;

                    model = Data.Tasks.Task.Edit(trans, model, currentUser);
                    Common.Models.Matters.Matter matter = Data.Tasks.Task.GetRelatedMatter(trans, id);

                    trans.Commit();
                    return RedirectToAction("Create", "Tasks", new { MatterId = matter.Id });
                }
                catch
                {
                    trans.Rollback();
                    return Close(id);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Close(long id, ViewModels.Tasks.TaskViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.Task model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Data.Tasks.Task.Get(trans, id);
                    model.Active = false;
                    model.ActualEnd = DateTime.Now;

                    model = Data.Tasks.Task.Edit(trans, model, currentUser);

                    if (!string.IsNullOrEmpty(Request["NewTask"]) && Request["NewTask"] == "on")
                    { // not empty & "on"
                        Common.Models.Matters.Matter matter = Data.Tasks.Task.GetRelatedMatter(trans, id);
                        return RedirectToAction("Create", "Tasks", new { MatterId = matter.Id });
                    }

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
        public ActionResult Create()
        {
            int contactId = -1;
            ViewModels.Tasks.CreateTaskViewModel viewModel;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Account.UsersViewModel> userList;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            Newtonsoft.Json.Linq.JArray taskTemplates;

            userList = new List<ViewModels.Account.UsersViewModel>();
            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();
            
            dynamic profile = ProfileBase.Create(Membership.GetUser().UserName);
            if (profile.ContactId != null && !string.IsNullOrEmpty(profile.ContactId))
                contactId = int.Parse(profile.ContactId);

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

                viewModel = new ViewModels.Tasks.CreateTaskViewModel();
                viewModel.TaskTemplates = new List<ViewModels.Tasks.TaskTemplateViewModel>();
                taskTemplates = new Newtonsoft.Json.Linq.JArray();
                Data.Tasks.TaskTemplate.List(conn, false).ForEach(x =>
                {
                    viewModel.TaskTemplates.Add(Mapper.Map<ViewModels.Tasks.TaskTemplateViewModel>(x));
                    Newtonsoft.Json.Linq.JObject template = new Newtonsoft.Json.Linq.JObject();
                    template.Add(new Newtonsoft.Json.Linq.JProperty("Id", x.Id.Value));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("TaskTemplateTitle", x.TaskTemplateTitle));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("Title", x.Title));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("Description", x.Description));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("ProjectedStart", DTProp(x.ProjectedStart)));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("DueDate", DTProp(x.DueDate)));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("ProjectedEnd", DTProp(x.ProjectedEnd)));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("ActualEnd", DTProp(x.ActualEnd)));
                    template.Add(new Newtonsoft.Json.Linq.JProperty("Active", x.Active));
                    taskTemplates.Add(template);
                });

                if (contactId > 0)
                {
                    viewModel.TaskContact = new ViewModels.Tasks.TaskAssignedContactViewModel()
                    {
                        Contact = new ViewModels.Contacts.ContactViewModel()
                        {
                            Id = contactId
                        }
                    };
                }

                matter = Data.Matters.Matter.Get(Guid.Parse(Request["MatterId"]), conn, false);
            }

            ViewBag.Matter = matter;
            ViewBag.UserList = userList;
            ViewBag.EmployeeContactList = employeeContactList;
            ViewBag.TemplateJson = taskTemplates.ToString();

            return View(new ViewModels.Tasks.CreateTaskViewModel()
            {
                TaskTemplates = viewModel.TaskTemplates,
                Task = new ViewModels.Tasks.TaskViewModel()
                {
                    DueDate = DateTime.Today.AddHours(17),
                    Active = true
                },         
                TaskContact = new ViewModels.Tasks.TaskAssignedContactViewModel()
                {
                    AssignmentType = ViewModels.AssignmentTypeViewModel.Direct,
                    Contact = viewModel.TaskContact.Contact
                }
            });
        }

        private string DTProp(string val)
        {
            if (string.IsNullOrEmpty(val)) return null;

            if (val.Contains("[NOW]"))
                val = val.Replace("[NOW]", DateTime.Now.ToString("M/d/yyyy h:mm tt"));
            if (val.Contains("[DATE]"))
                val = val.Replace("[DATE]", DateTime.Now.ToString("M/d/yyyy"));
            if (val.Contains("[DATE+"))
            {
                int num = -1;
                try
                {
                    // abc[DATE+12]asd
                    string a = val.Substring(0, val.IndexOf("[DATE+"));
                    // a=abc
                    string b = val.Substring(val.IndexOf("[DATE+") + 6);
                    // b=12]asd
                    string c = b.Substring(0, b.IndexOf("]"));
                    // c=12
                    string d = b.Substring(b.IndexOf("]") + 1);
                    // d=asd
                    num = int.Parse(c);
                    val = a + DateTime.Now.AddDays(num).ToString("M/d/yyyy") + d;
                }
                catch
                {
                    num = -1;
                    val = "Invalid Format";
                }
            }

            return val;
        }

        [ValidateInput(false)]
        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Tasks.CreateTaskViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.Task model;
            Guid matterid = Guid.Empty;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.Task>(viewModel.Task);
                    model.Description = new Ganss.XSS.HtmlSanitizer().Sanitize(model.Description);

                    matterid = Guid.Parse(Request["MatterId"]);
                    
                    model = Data.Tasks.Task.Create(trans, model, currentUser);

                    Data.Tasks.Task.RelateMatter(trans, model, matterid, currentUser);
                    Data.Tasks.TaskAssignedContact.Create(trans, new Common.Models.Tasks.TaskAssignedContact()
                    {
                        Task = model,
                        Contact = new Common.Models.Contacts.Contact() { Id = viewModel.TaskContact.Contact.Id },
                        AssignmentType = (Common.Models.Tasks.AssignmentType)(int)viewModel.TaskContact.AssignmentType
                    }, currentUser);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    return Create();
                }
            }

            return RedirectToAction("Details", new { Id = model.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult PhoneCall(Guid id)
        {
            string phoneCallWith = null;
            string title;
            Common.Models.Matters.Matter matter;
            Common.Models.Account.Users currentUser;
            List<Common.Models.Timing.TimeCategory> timeCategoryList;
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                currentUser = Data.Account.Users.Get(User.Identity.Name, conn, false);
                matter = Data.Matters.Matter.Get(id, conn, false);

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });

                timeCategoryList = Data.Timing.TimeCategory.List(conn, false);
                timeCategoryList.Insert(0, new Common.Models.Timing.TimeCategory()
                {
                    Id = 0,
                    Title = "Standard"
                });
            }

            if (Request["func"] == "client")
                phoneCallWith = "client";
            else if (Request["func"] == "opposingcounsel")
                phoneCallWith = "opposing counsel";
            else if (Request["func"] == "court")
                phoneCallWith = "court";

            if (string.IsNullOrEmpty(phoneCallWith))
            {
                title = "Phone call";
            }
            else
            {
                title = "Phone call with " + phoneCallWith;
            }

            ViewBag.Matter = matter;
            ViewBag.TimeCategoryList = timeCategoryList;

            return View(new ViewModels.Tasks.PhoneCallViewModel()
            {
                MakeTime = true,
                MakeNote = true,
                TimeCategory = new ViewModels.Timing.TimeCategoryViewModel() { Id = 0 },
                Start = DateTime.Now,
                Stop = DateTime.Now.AddMinutes(6),
                Billable = true,
                Title = title,
                TimeDetails = title,
                EmployeeContactList = employeeContactList
            });
        }

        [ValidateInput(false)]
        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult PhoneCall(Guid id, ViewModels.Tasks.PhoneCallViewModel viewModel)
        {
            int contactId;
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter matter;

            dynamic profile = ProfileBase.Create(Membership.GetUser().UserName);
            if (profile.ContactId == null && string.IsNullOrEmpty(profile.ContactId))
                throw new Exception("Profile.ContactId not configured.");

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    contactId = int.Parse(profile.ContactId);
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    matter = Data.Matters.Matter.Get(trans, id);

                    // Task
                    Common.Models.Tasks.Task task = new Common.Models.Tasks.Task()
                    {
                        Active = true,
                        DueDate = DateTime.Now,
                        Title = viewModel.Title,
                        Description = new Ganss.XSS.HtmlSanitizer().Sanitize(viewModel.TaskAndNoteDetails)
                    };

                    // TaskAssignedContact
                    Common.Models.Tasks.TaskAssignedContact taskAssignedContact = new Common.Models.Tasks.TaskAssignedContact()
                    {
                        Contact = new Common.Models.Contacts.Contact() { Id = contactId },
                        Task = task,
                        AssignmentType = Common.Models.Tasks.AssignmentType.Direct
                    };

                    // Time
                    Common.Models.Timing.Time time = new Common.Models.Timing.Time()
                    {
                        Billable = viewModel.Billable,
                        Details = viewModel.TimeDetails,
                        Start = viewModel.Start,
                        Stop = viewModel.Stop,
                        TimeCategory = new Common.Models.Timing.TimeCategory() { Id = viewModel.TimeCategory.Id.Value },
                        Worker = new Common.Models.Contacts.Contact() { Id = contactId }
                    };

                    // Note
                    Common.Models.Notes.Note note = new Common.Models.Notes.Note()
                    {
                        Body = viewModel.TaskAndNoteDetails,
                        Timestamp = DateTime.Now,
                        Title = viewModel.Title
                    };


                    task = Data.Tasks.Task.Create(trans, task, currentUser);
                    Data.Tasks.Task.RelateMatter(trans, task, matter.Id.Value, currentUser);
                    Data.Tasks.TaskAssignedContact.Create(trans, taskAssignedContact, currentUser);

                    if (viewModel.MakeTime)
                    {
                        time = Data.Timing.Time.Create(trans, time, currentUser);
                        Data.Timing.Time.RelateTask(trans, time, task.Id.Value, currentUser);
                    }

                    if (viewModel.MakeNote)
                    {
                        note = Data.Notes.Note.Create(trans, note, currentUser);
                        Data.Notes.Note.RelateTask(trans, note, task.Id.Value, currentUser);

                        if (viewModel.NotifyContactIds != null)
                        {
                            foreach (string x in viewModel.NotifyContactIds)
                            {
                                Data.Notes.NoteNotification.Create(trans, new Common.Models.Notes.NoteNotification()
                                {
                                    Contact = new Common.Models.Contacts.Contact() { Id = int.Parse(x) },
                                    Note = note,
                                    Cleared = null
                                }, currentUser);
                            };
                        }
                    }

                    trans.Commit();

                    return RedirectToAction("Details", "Tasks", new { Id = task.Id });
                }
                catch
                {
                    trans.Rollback();
                    return PhoneCall(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult TodoListForAll()
        {
            DateTime? start = null;
            DateTime? stop = null;
            List<dynamic> jsonList;

            if (Request["start"] != null)
                start = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["start"]));
            if (Request["stop"] != null)
                stop = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["stop"]));

            jsonList = new List<dynamic>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Tasks.Task.GetTodoListForAll(start, stop, conn, false).ForEach(x =>
                {
                    if (x.DueDate.HasValue)
                    {
                        jsonList.Add(new
                        {
                            id = x.Id.Value,
                            title = x.Title,
                            allDay = true,
                            start = Common.Utilities.DateTimeToUnixTimestamp(x.DueDate.Value.ToLocalTime()),
                            description = x.Description
                        });
                    }
                });
            }

            return Json(jsonList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult TodoListForUser(Guid? id)
        {
            DateTime? start = null;
            DateTime? stop = null;
            List<dynamic> jsonList;
            Common.Models.Account.Users user;
            List<Common.Models.Tasks.Task> taskList;
            List<Common.Models.Settings.TagFilter> tagFilter;
            if (Request["start"] != null)
                start = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["start"]));
            if (Request["stop"] != null)
                stop = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["stop"]));

            if (!id.HasValue)
            {
                if (string.IsNullOrEmpty(Request["UserPId"]))
                    return null;
                else
                    id = Guid.Parse(Request["UserPId"]);
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                user = Data.Account.Users.Get(id.Value, conn, false);

                tagFilter = Data.Settings.UserTaskSettings.ListTagFiltersFor(user, conn, false);

                jsonList = new List<dynamic>();

                if (Request["ContactId"] == null || string.IsNullOrEmpty(Request["ContactId"]))
                    taskList = Data.Tasks.Task.GetTodoListFor(user, start, stop, conn, false);
                else
                {
                    int contactId = int.Parse(Request["ContactId"]);
                    taskList = Data.Tasks.Task.GetTodoListFor(user,
                        new Common.Models.Contacts.Contact() { Id = contactId }, 
                        start, stop, conn, false);
                }

                taskList.ForEach(x =>
                {
                    if (x.DueDate.HasValue)
                    {
                        jsonList.Add(new
                        {
                            id = x.Id.Value,
                            title = x.Title,
                            allDay = true,
                            start = Common.Utilities.DateTimeToUnixTimestamp(x.DueDate.Value.ToLocalTime()),
                            description = x.Description
                        });
                    }
                });
            }

            return Json(jsonList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult TodoListForContact(int? id)
        {
            DateTime? start = null;
            DateTime? stop = null;
            List<dynamic> jsonList;
            Common.Models.Contacts.Contact contact;
            List<Common.Models.Settings.TagFilter> tagFilters;
            if (Request["start"] != null)
                start = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["start"]));
            if (Request["stop"] != null)
                stop = Common.Utilities.UnixTimeStampToDateTime(double.Parse(Request["stop"]));

            if (!id.HasValue)
            {
                if (string.IsNullOrEmpty(Request["ContactId"]))
                    return null;
                else
                    id = int.Parse(Request["ContactId"]);
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                contact = Data.Contacts.Contact.Get(id.Value, conn, false);

                jsonList = new List<dynamic>();

                Data.Tasks.Task.GetTodoListFor(contact, start, stop, conn, false).ForEach(x =>
                {
                    if (x.DueDate.HasValue)
                    {
                        jsonList.Add(new
                        {
                            id = x.Id.Value,
                            title = x.Title,
                            allDay = true,
                            start = Common.Utilities.DateTimeToUnixTimestamp(x.DueDate.Value.ToLocalTime()),
                            description = x.Description
                        });
                    }
                });
            }

            return Json(jsonList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Time(long id)
        {
            List<ViewModels.Timing.TimeViewModel> viewModelList;
            ViewModels.Timing.TimeViewModel viewModel;
            Common.Models.Contacts.Contact contact;
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;

            viewModelList = new List<ViewModels.Timing.TimeViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Timing.Time.ListForTask(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Timing.TimeViewModel>(x);

                    contact = Data.Contacts.Contact.Get(viewModel.Worker.Id.Value, conn, false);

                    viewModel.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);
                    viewModel.WorkerDisplayName = viewModel.Worker.DisplayName;

                    viewModelList.Add(viewModel);
                });

                task = Data.Tasks.Task.Get(id, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(id, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Contacts(long id)
        {
            List<ViewModels.Tasks.TaskAssignedContactViewModel> viewModelList;
            ViewModels.Tasks.TaskAssignedContactViewModel viewModel;
            Common.Models.Contacts.Contact contact;
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;

            viewModelList = new List<ViewModels.Tasks.TaskAssignedContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Tasks.TaskAssignedContact.ListForTask(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Tasks.TaskAssignedContactViewModel>(x);

                    contact = Data.Contacts.Contact.Get(x.Contact.Id.Value, conn, false);

                    viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

                    viewModelList.Add(viewModel);
                });

                task = Data.Tasks.Task.Get(id, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(id, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Tags(long id)
        {
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Tasks.TaskTagViewModel> viewModelList;

            viewModelList = new List<ViewModels.Tasks.TaskTagViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Tasks.TaskTag.ListForTask(id, conn, false).ForEach(x =>
                {
                    viewModelList.Add(Mapper.Map<ViewModels.Tasks.TaskTagViewModel>(x));
                });

                task = Data.Tasks.Task.Get(id, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(id, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Notes(long id)
        {
            Common.Models.Tasks.Task task;
            Common.Models.Matters.Matter matter;
            List<ViewModels.Notes.NoteViewModel> viewModelList;
            ViewModels.Notes.NoteViewModel viewModel;

            viewModelList = new List<ViewModels.Notes.NoteViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Notes.Note.ListForTask(id, conn, false).ForEach(x =>
                {
                    viewModel = Mapper.Map<ViewModels.Notes.NoteViewModel>(x);

                    viewModelList.Add(viewModel);
                });

                task = Data.Tasks.Task.Get(id, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(id, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(viewModelList);
        }
    }
}