// -----------------------------------------------------------------------
// <copyright file="ActivityRegardingLeadViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.ViewModels.Activities
{
    using Common.Models;
    using AutoMapper;

    [MapMe]
    public class ActivityRegardingLeadViewModel : ActivityRegardingBaseViewModel
    {
        public Leads.LeadViewModel Lead { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Activities.ActivityRegardingLead, ActivityRegardingLeadViewModel>()
                .ForMember(dst => dst.IsStub, opt => opt.UseValue(false))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Type, opt => opt.ResolveUsing(db =>
                {
                    if (db.Type == null || !db.Type.Id.HasValue) return null;
                    return new ViewModels.Activities.ActivityRegardingTypeViewModel()
                    {
                        Id = db.Type.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Activity, opt => opt.ResolveUsing(db =>
                {
                    if (db.Activity == null || !db.Activity.Id.HasValue) return null;

                    Common.Models.Activities.ActivityType type = Data.Activities.ActivityType.GetByActivityId(db.Activity.Id.Value);

                    if (type.Title == "Phone Call")
                        return new ViewModels.Activities.ActivityPhonecallViewModel()
                        {
                            Id = db.Activity.Id,
                            IsStub = true
                        };
                    else if (type.Title == "Email")
                        return new ViewModels.Activities.ActivityEmailViewModel()
                        {
                            Id = db.Activity.Id,
                            IsStub = true
                        };
                    else if (type.Title == "Letter")
                        return new ViewModels.Activities.ActivityLetterViewModel()
                        {
                            Id = db.Activity.Id,
                            IsStub = true
                        };
                    else if (type.Title == "Task")
                        return new ViewModels.Activities.ActivityTaskViewModel()
                        {
                            Id = db.Activity.Id,
                            IsStub = true
                        };
                    else
                        throw new System.InvalidOperationException("db.Activity.Type of unknown value");
                }))
                .ForMember(dst => dst.Lead, opt => opt.ResolveUsing(db =>
                {
                    if (db.Lead == null || !db.Lead.Id.HasValue) return null;
                    return new ViewModels.Leads.LeadViewModel()
                    {
                        Id = db.Lead.Id.Value,
                        IsStub = true
                    };
                }));

            Mapper.CreateMap<ActivityRegardingLeadViewModel, Common.Models.Activities.ActivityRegardingLead>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Type, opt => opt.ResolveUsing(x =>
                {
                    if (x.Type == null || !x.Type.Id.HasValue)
                        return null;
                    return new ViewModels.Activities.ActivityRegardingTypeViewModel()
                    {
                        Id = x.Type.Id.Value
                    };
                }))
                .ForMember(dst => dst.Activity, opt => opt.ResolveUsing(db =>
                {
                    if (db.Activity == null || !db.Activity.Id.HasValue) return null;

                    // ActivityTask has all properties of base and no more
                    Common.Models.Activities.ActivityTask activity = Data.Activities.ActivityTask.Get(db.Activity.Id.Value);

                    if (activity.Type.Title == "Phone Call")
                        return new Common.Models.Activities.ActivityPhonecall()
                        {
                            Id = activity.Id,
                            IsStub = true
                        };
                    else if (activity.Type.Title == "Email")
                        return new Common.Models.Activities.ActivityEmail()
                        {
                            Id = activity.Id,
                            IsStub = true
                        };
                    else if (activity.Type.Title == "Letter")
                        return new Common.Models.Activities.ActivityLetter()
                        {
                            Id = activity.Id,
                            IsStub = true
                        };
                    else if (activity.Type.Title == "Task")
                        return new Common.Models.Activities.ActivityTask()
                        {
                            Id = activity.Id,
                            IsStub = true
                        };
                    else
                        throw new System.InvalidOperationException("db.Activity.Type of unknown value");
                }))
                .ForMember(dst => dst.Lead, opt => opt.ResolveUsing(x =>
                {
                    if (x.Lead == null || !x.Lead.Id.HasValue)
                        return null;
                    return new ViewModels.Leads.LeadViewModel()
                    {
                        Id = x.Lead.Id.Value
                    };
                }));
        }
    }
}
