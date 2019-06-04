using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum InventorySourceTypeEnum
    {
        [Description("Open Market")]
        OpenMarket = 1,
        [Description("Barter")]
        Barter = 2,
        [Description("O&O")]
        ProprietaryOAndO = 3,
        [Description("Syndication")]
        Syndication = 4,
        [Description("Diginet")]
        Diginet = 5
    }
}
