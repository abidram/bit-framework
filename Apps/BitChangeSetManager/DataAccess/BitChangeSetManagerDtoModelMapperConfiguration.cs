﻿using AutoMapper;
using BitChangeSetManager.Dto;
using BitChangeSetManager.Model;
using Foundation.Model.Contracts;
using System.Linq;

namespace BitChangeSetManager.DataAccess
{
    public class BitChangeSetManagerDtoModelMapperConfiguration : IDtoModelMapperConfiguration
    {
        public virtual void Configure(IMapperConfigurationExpression mapperConfigExpression)
        {
            int customersCount = 0;

            mapperConfigExpression.CreateMap<ChangeSet, ChangeSetDto>()
                .ForMember(changeSetMember => changeSetMember.IsDeliveredToAll, config => config.MapFrom(changeSet => changeSet.Deliveries.Count() == customersCount));
        }
    }
}
