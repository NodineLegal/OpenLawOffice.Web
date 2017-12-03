// -----------------------------------------------------------------------
// <copyright file="LeadViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.ViewModels.Leads
{
    using System;
    using AutoMapper;
    using System.Collections.Generic;
    using Common.Models;

    [MapMe]
    public class LeadViewModel : CoreViewModel
    {
        public long? Id { get; set; }

        public LeadStatusViewModel Status { get; set; }

        public Contacts.ContactViewModel Contact { get; set; }

        public LeadSourceViewModel Source { get; set; }

        public LeadFeeViewModel Fee { get; set; }

        public DateTime? Closed { get; set; }

        public string Details { get; set; }

        public Opportunities.OpportunityViewModel Opportunity { get; set; }
        public List<Activities.ActivityBaseViewModel> Activities { get; set; }
        public List<Activities.ActivityBaseViewModel> InactiveActivities { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Leads.Lead, LeadViewModel>()
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
                .ForMember(dst => dst.Status, opt => opt.ResolveUsing(db =>
                {
                    if (db.Status == null || !db.Status.Id.HasValue) return null;
                    return new ViewModels.Leads.LeadStatusViewModel()
                    {
                        Id = db.Status.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Contact, opt => opt.ResolveUsing(db =>
                {
                    if (db.Contact == null || !db.Contact.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.Contact.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Source, opt => opt.ResolveUsing(db =>
                {
                    if (db.Source == null || !db.Source.Id.HasValue) return null;
                    return new ViewModels.Leads.LeadSourceViewModel()
                    {
                        Id = db.Source.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Fee, opt => opt.ResolveUsing(db =>
                {
                    if (db.Fee == null || !db.Fee.Id.HasValue) return null;
                    return new ViewModels.Leads.LeadFeeViewModel()
                    {
                        Id = db.Fee.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Closed, opt => opt.MapFrom(src => src.Closed))
                .ForMember(dst => dst.Details, opt => opt.MapFrom(src => src.Details))
                .ForMember(dst => dst.Opportunity, opt => opt.Ignore())
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.InactiveActivities, opt => opt.Ignore());

            Mapper.CreateMap<LeadViewModel, Common.Models.Leads.Lead>()
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
                .ForMember(dst => dst.Status, opt => opt.ResolveUsing(x =>
                {
                    if (x.Status == null || !x.Status.Id.HasValue)
                        return null;
                    return new ViewModels.Leads.LeadStatusViewModel()
                    {
                        Id = x.Status.Id.Value
                    };
                }))
                .ForMember(dst => dst.Contact, opt => opt.ResolveUsing(x =>
                {
                    if (x.Contact == null || !x.Contact.Id.HasValue)
                        return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = x.Contact.Id.Value
                    };
                }))
                .ForMember(dst => dst.Source, opt => opt.ResolveUsing(x =>
                {
                    if (x.Source == null || !x.Source.Id.HasValue)
                        return null;
                    return new ViewModels.Leads.LeadSourceViewModel()
                    {
                        Id = x.Source.Id.Value
                    };
                }))
                .ForMember(dst => dst.Fee, opt => opt.ResolveUsing(x =>
                {
                    if (x.Fee == null || !x.Fee.Id.HasValue)
                        return null;
                    return new ViewModels.Leads.LeadFeeViewModel()
                    {
                        Id = x.Fee.Id.Value
                    };
                }))
                .ForMember(dst => dst.Closed, opt => opt.MapFrom(src => src.Closed))
                .ForMember(dst => dst.Details, opt => opt.MapFrom(src => src.Details));
        }
    }
}