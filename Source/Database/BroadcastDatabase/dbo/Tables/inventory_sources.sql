CREATE TABLE [dbo].[inventory_sources] (
    [id]                    INT          IDENTITY (1, 1) NOT NULL,
    [name]                  VARCHAR (50) NOT NULL,
    [is_active]             BIT          NOT NULL,
    [inventory_source_type] TINYINT      NOT NULL,
    CONSTRAINT [PK_inventory_sources] PRIMARY KEY CLUSTERED ([id] ASC)
);

