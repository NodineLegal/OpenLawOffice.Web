// -----------------------------------------------------------------------
// <copyright file="TaskViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.ViewModels.Tasks
{
    using System;
    using AutoMapper;
    using OpenLawOffice.Common.Models;
    using System.Collections.Generic;
    
    public class PhoneCallViewModel : CoreViewModel
    {
        public Timing.TimeCategoryViewModel TimeCategory { get; set; }

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public bool Billable { get; set; }

        public string Title { get; set; }

        public string TimeDetails { get; set; }

        public bool MakeTime { get; set; }

        public bool MakeNote { get; set; }

        public string TaskAndNoteDetails { get; set; }

        public string[] NotifyContactIds { get; set; }

        public List<ViewModels.Contacts.ContactViewModel> EmployeeContactList { get; set; }
    }
}