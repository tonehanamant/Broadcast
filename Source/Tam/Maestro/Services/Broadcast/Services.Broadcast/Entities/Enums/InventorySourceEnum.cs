using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum InventorySourceEnum
    {
        [Description("Blank")]
        Blank = 0,

        [Description("OpenMarket")]
        OpenMarket = 1,

        [Description("Assembly")]
        Assembly = 2,

        [Description("TVB")]
        TVB = 3,

        [Description("TTNW")]
        TTNW = 4,

        [Description("CNN")]
        CNN = 5,

        [Description("Sinclair")]
        Sinclair = 6,

        [Description("LilaMax")]
        LilaMax = 7,

        [Description("Ference Media")]
        FerencMedia = 8,

        [Description("O&O")]
        OAndO = 9
    }
}
