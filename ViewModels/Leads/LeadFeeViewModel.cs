// -----------------------------------------------------------------------
// <copyright file="LeadFeeViewModel.cs" company="Nodine Legal, LLC">
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
    using Common.Models;

    [MapMe]
    public class LeadFeeViewModel : CoreViewModel
    {
        public int? Id { get; set; }
        public bool IsEligible { get; set; } = false;
        public decimal? Amount { get; set; }
        public Contacts.ContactViewModel To { get; set; }
        public DateTime? Paid { get; set; }
        public string AdditionalData { get; set; }
        
        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Leads.LeadFee, LeadFeeViewModel>()
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
                //.ForMember(dst => dst.IsEligible, opt => opt.MapFrom(src => src.IsEligible))
                .ForMember(dst => dst.IsEligible, opt => opt.ResolveUsing(db =>
                {
                    if (db.IsEligible == null || !db.IsEligible.HasValue) return false;
                    return db.IsEligible.Value;
                }))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.To, opt => opt.ResolveUsing(db =>
                {
                    if (db.To == null || !db.To.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.To.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Paid, opt => opt.MapFrom(src => src.Paid))
                .ForMember(dst => dst.AdditionalData, opt => opt.MapFrom(src => src.AdditionalData));

            Mapper.CreateMap<LeadFeeViewModel, Common.Models.Leads.LeadFee>()
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
                //.ForMember(dst => dst.IsEligible, opt => opt.MapFrom(src => src.IsEligible))
                .ForMember(dst => dst.IsEligible, opt => opt.ResolveUsing(x =>
                {
                    return x.IsEligible;
                }))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.To, opt => opt.ResolveUsing(x =>
                {
                    if (x.To == null || !x.To.Id.HasValue)
                        return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = x.To.Id.Value
                    };
                }))
                .ForMember(dst => dst.Paid, opt => opt.MapFrom(src => src.Paid))
                .ForMember(dst => dst.AdditionalData, opt => opt.MapFrom(src => src.AdditionalData));
        }
    }
}