﻿using FluentNHibernate.Mapping;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Common.Mappings.CubeDrafting
{
    class CubeDraftResultMap : ClassMap<CubeDraftResult>
    {
        public CubeDraftResultMap()
        {
            Id(x => x.Id);
            Map(x => x.Round);
            Map(x => x.Player1Wins);
            Map(x => x.Player2Wins);
            Map(x => x.Ties);
            Map(x => x.CurrentGame);
            References(x => x.Player1);
            References(x => x.Player2)
                .Nullable();
            References(x => x.Draft);
        }
    }
}
