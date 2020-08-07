using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class InventoryTestData
    {
        public static List<InventorySource> GetInventorySources()
        {
            return new List<InventorySource>
            {
                new InventorySource
                {
                    Id = 1,
                    Name = "Open Market",
                    InventoryType = InventorySourceTypeEnum.OpenMarket
                },
                new InventorySource
                {
                    Id = 3,
                    Name = "TVB",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 4,
                    Name = "TTWN",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 5,
                    Name = "CNN",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 6,
                    Name = "Sinclair",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 7,
                    Name = "LilaMax",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 8,
                    Name = "MLB",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 9,
                    Name = "Ference Media",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 10,
                    Name = "ABC O&O",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 11,
                    Name = "NBC O&O",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 12,
                    Name = "KATZ",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 13,
                    Name = "20th Century Fox (Twentieth Century)",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 14,
                    Name = "CBS Synd",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 15,
                    Name = "NBCU Syn",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 16,
                    Name = "WB Syn",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 17,
                    Name = "Antenna TV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 18,
                    Name = "Bounce",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 19,
                    Name = "BUZZR",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 20,
                    Name = "COZI",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 21,
                    Name = "Escape",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 22,
                    Name = "Grit",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 23,
                    Name = "HITV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 24,
                    Name = "Laff",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 25,
                    Name = "Me TV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                }
            };
        }
    }
}
