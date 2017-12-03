// -----------------------------------------------------------------------
// <copyright file="OpportunityViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.ViewModels.Opportunities
{
    using System;
    using AutoMapper;
    using System.Collections.Generic;
    using Common.Models;

    [MapMe]
    public class OpportunityViewModel : CoreViewModel
    {
        public long? Id { get; set; }

        public Contacts.ContactViewModel Account { get; set; }

        public List<Contacts.ContactViewModel> Contacts { get; set; }

        public OpportunityStageViewModel Stage { get; set; }

        public decimal? Probability { get; set; }

        public decimal? Amount { get; set; }

        public DateTime? Closed { get; set; }

        public Leads.LeadViewModel Lead { get; set; }

        public Matters.MatterViewModel Matter { get; set; }

        public List<Activities.ActivityBaseViewModel> Activities { get; set; }
        public List<Activities.ActivityBaseViewModel> InactiveActivities { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Opportunities.Opportunity, OpportunityViewModel>()
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
                .ForMember(dst => dst.Account, opt => opt.ResolveUsing(db =>
                {
                    if (db.Account == null || !db.Account.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.Account.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Contacts, opt => opt.Ignore())
                .ForMember(dst => dst.Stage, opt => opt.ResolveUsing(db =>
                {
                    if (db.Stage == null || !db.Stage.Id.HasValue) return null;
                    return new ViewModels.Opportunities.OpportunityStageViewModel()
                    {
                        Id = db.Stage.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Probability, opt => opt.MapFrom(src => src.Probability))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.Closed, opt => opt.MapFrom(src => src.Closed))
                .ForMember(dst => dst.Lead, opt => opt.ResolveUsing(db =>
                {
                    if (db.Lead == null || !db.Lead.Id.HasValue) return null;
                    return new ViewModels.Leads.LeadViewModel()
                    {
                        Id = db.Lead.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Matter, opt => opt.ResolveUsing(db =>
                {
                    if (db.Matter == null || !db.Matter.Id.HasValue) return null;
                    return new ViewModels.Matters.MatterViewModel()
                    {
                        Id = db.Matter.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.InactiveActivities, opt => opt.Ignore());

            Mapper.CreateMap<OpportunityViewModel, Common.Models.Opportunities.Opportunity>()
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
                .ForMember(dst => dst.Account, opt => opt.ResolveUsing(x =>
                {
                    if (x.Account == null || !x.Account.Id.HasValue)
                        return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = x.Account.Id.Value
                    };
                }))
                .ForMember(dst => dst.Contacts, opt => opt.Ignore())
                .ForMember(dst => dst.Stage, opt => opt.ResolveUsing(x =>
                {
                    if (x.Stage == null || !x.Stage.Id.HasValue)
                        return null;
                    return new ViewModels.Opportunities.OpportunityStageViewModel()
                    {
                        Id = x.Stage.Id.Value
                    };
                }))
                .ForMember(dst => dst.Probability, opt => opt.MapFrom(src => src.Probability))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.Closed, opt => opt.MapFrom(src => src.Closed))
                .ForMember(dst => dst.Lead, opt => opt.ResolveUsing(x =>
                {
                    if (x.Lead == null || !x.Lead.Id.HasValue)
                        return null;
                    return new ViewModels.Leads.LeadViewModel()
                    {
                        Id = x.Lead.Id.Value
                    };
                }))
                .ForMember(dst => dst.Matter, opt => opt.ResolveUsing(x =>
                {
                    if (x.Matter == null || !x.Matter.Id.HasValue)
                        return null;
                    return new ViewModels.Matters.MatterViewModel()
                    {
                        Id = x.Matter.Id.Value
                    };
                }));
        }
    }
}