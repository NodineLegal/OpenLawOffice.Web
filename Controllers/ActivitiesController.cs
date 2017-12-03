// -----------------------------------------------------------------------
// <copyright file="ActivitiesController.cs" company="Nodine Legal, LLC">
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
    public class ActivitiesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(long id)
        {
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Common.Models.Activities.ActivityType type = Data.Activities.ActivityType.GetByActivityId(id, conn, false);

                if (type.Title == "Phone Call")
                    return RedirectToAction("Details", "ActivityPhonecalls", new { id = id });
                else if (type.Title == "Email")
                    return RedirectToAction("Details", "ActivityEmails", new { id = id });
                else if (type.Title == "Letter")
                    return RedirectToAction("Details", "ActivityLetters", new { id = id });
                else if (type.Title == "Task")
                    return RedirectToAction("Details", "ActivityTasks", new { id = id });
                else
                    throw new ArgumentException("Unknown type");
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(long id)
        {
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Common.Models.Activities.ActivityType type = Data.Activities.ActivityType.GetByActivityId(id, conn, false);

                if (type.Title == "Phone Call")
                    return RedirectToAction("Edit", "ActivityPhonecalls", new { id = id });
                else if (type.Title == "Email")
                    return RedirectToAction("Edit", "ActivityEmails", new { id = id });
                else if (type.Title == "Letter")
                    return RedirectToAction("Edit", "ActivityLetters", new { id = id });
                else if (type.Title == "Task")
                    return RedirectToAction("Edit", "ActivityTasks", new { id = id });
                else
                    throw new ArgumentException("Unknown type");
            }
        }
    }
}