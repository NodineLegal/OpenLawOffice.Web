// -----------------------------------------------------------------------
// <copyright file="LeadSourceViewModel.cs" company="Nodine Legal, LLC">
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
    using AutoMapper;
    using Common.Models;

    [MapMe]
    public class LeadSourceViewModel : CoreViewModel
    {
        public int? Id { get; set; }

        public LeadSourceTypeViewModel Type { get; set; }

        public Contacts.ContactViewModel Contact { get; set; }

        public string Title { get; set; }

        public string AdditionalQuestion1 { get; set; }

        public string AdditionalData1 { get; set; }

        public string AdditionalQuestion2 { get; set; }

        public string AdditionalData2 { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Leads.LeadSource, LeadSourceViewModel>()
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
                    return new ViewModels.Leads.LeadSourceTypeViewModel()
                    {
                        Id = db.Type.Id.Value,
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
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.AdditionalQuestion1, opt => opt.MapFrom(src => src.AdditionalQuestion1))
                .ForMember(dst => dst.AdditionalData1, opt => opt.MapFrom(src => src.AdditionalData1))
                .ForMember(dst => dst.AdditionalQuestion2, opt => opt.MapFrom(src => src.AdditionalQuestion2))
                .ForMember(dst => dst.AdditionalData2, opt => opt.MapFrom(src => src.AdditionalData2));

            Mapper.CreateMap<LeadSourceViewModel, Common.Models.Leads.LeadSource>()
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
                    return new ViewModels.Leads.LeadSourceTypeViewModel()
                    {
                        Id = x.Type.Id.Value
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
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.AdditionalQuestion1, opt => opt.MapFrom(src => src.AdditionalQuestion1))
                .ForMember(dst => dst.AdditionalData1, opt => opt.MapFrom(src => src.AdditionalData1))
                .ForMember(dst => dst.AdditionalQuestion2, opt => opt.MapFrom(src => src.AdditionalQuestion2))
                .ForMember(dst => dst.AdditionalData2, opt => opt.MapFrom(src => src.AdditionalData2));
        }
    }
}