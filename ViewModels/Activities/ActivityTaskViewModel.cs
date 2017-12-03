// -----------------------------------------------------------------------
// <copyright file="ActivityTaskViewModel.cs" company="Nodine Legal, LLC">
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
    public class ActivityTaskViewModel : ActivityBaseViewModel
    {

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Activities.ActivityTask, ActivityTaskViewModel>()
                .ForMember(dst => dst.IsStub, opt => opt.UseValue(false))
                .ForMember(dst => dst.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dst => dst.Modified, opt => opt.MapFrom(src => src.Modified))
                .ForMember(dst => dst.Disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.CreatedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.ModifiedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(db =>
                {
                    if (db.DisabledBy == null || !db.DisabledBy.PId.HasValue) return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.DisabledBy.PId.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Type, opt => opt.ResolveUsing(db =>
                {
                    if (db.Type == null || !db.Type.Id.HasValue) return null;
                    return new ViewModels.Activities.ActivityTypeViewModel()
                    {
                        Id = db.Type.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsCampaignResponse, opt => opt.MapFrom(src => src.IsCampaignResponse))
                .ForMember(dst => dst.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dst => dst.Body, opt => opt.MapFrom(src => src.Body))
                .ForMember(dst => dst.Owner, opt => opt.ResolveUsing(db =>
                {
                    if (db.Owner == null || !db.Owner.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.Owner.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Priority, opt => opt.ResolveUsing(db =>
                {
                    if (db.Priority == null || !db.Priority.Id.HasValue) return null;
                    return new ViewModels.Activities.ActivityPriorityViewModel()
                    {
                        Id = db.Priority.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Due, opt => opt.MapFrom(src => src.Due))
                .ForMember(dst => dst.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dst => dst.StatusReason, opt => opt.ResolveUsing(db =>
                {
                    if (db.StatusReason == null || !db.StatusReason.Id.HasValue) return null;
                    return new ViewModels.Activities.ActivityStatusReasonViewModel()
                    {
                        Id = db.StatusReason.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dst => dst.RegardingLead, opt => opt.Ignore())
                .ForMember(dst => dst.RegardingOpportunity, opt => opt.Ignore());

            Mapper.CreateMap<ActivityTaskViewModel, Common.Models.Activities.ActivityTask>()
                .ForMember(dst => dst.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dst => dst.Modified, opt => opt.MapFrom(src => src.Modified))
                .ForMember(dst => dst.Disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.CreatedBy.PId
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.ModifiedBy.PId
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.DisabledBy == null || !x.DisabledBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.DisabledBy.PId.Value
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Type, opt => opt.ResolveUsing(x =>
                {
                    if (x.Type == null || !x.Type.Id.HasValue)
                        return null;
                    return new ViewModels.Activities.ActivityTypeViewModel()
                    {
                        Id = x.Type.Id.Value
                    };
                }))
                .ForMember(dst => dst.IsCampaignResponse, opt => opt.MapFrom(src => src.IsCampaignResponse))
                .ForMember(dst => dst.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dst => dst.Body, opt => opt.MapFrom(src => src.Body))
                .ForMember(dst => dst.Owner, opt => opt.ResolveUsing(x =>
                {
                    if (x.Owner == null || !x.Owner.Id.HasValue)
                        return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = x.Owner.Id.Value
                    };
                }))
                .ForMember(dst => dst.Priority, opt => opt.ResolveUsing(x =>
                {
                    if (x.Priority == null || !x.Priority.Id.HasValue)
                        return null;
                    return new ViewModels.Activities.ActivityPriorityViewModel()
                    {
                        Id = x.Priority.Id.Value
                    };
                }))
                .ForMember(dst => dst.Due, opt => opt.MapFrom(src => src.Due))
                .ForMember(dst => dst.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dst => dst.StatusReason, opt => opt.ResolveUsing(x =>
                {
                    if (x.StatusReason == null || !x.StatusReason.Id.HasValue)
                        return null;
                    return new ViewModels.Activities.ActivityStatusReasonViewModel()
                    {
                        Id = x.StatusReason.Id.Value
                    };
                }))
                .ForMember(dst => dst.Duration, opt => opt.MapFrom(src => src.Duration));
        }
    }
}
