CREATE TABLE [dbo].[inventory_source_logos] (
    [id]                  INT             IDENTITY (1, 1) NOT NULL,
    [inventory_source_id] INT             NOT NULL,
    [created_by]          VARCHAR (63)    NOT NULL,
    [created_date]        DATETIME        NOT NULL,
    [file_name]           VARCHAR (127)   NOT NULL,
    [file_content]        VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_inventory_source_logos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_source_logos_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_source_logos_inventory_sources]
    ON [dbo].[inventory_source_logos]([inventory_source_id] ASC);

