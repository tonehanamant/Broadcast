﻿CREATE TABLE [dbo].[inventory_files] (
    [id]                  INT           IDENTITY (1, 1) NOT NULL,
    [name]                VARCHAR (127) NOT NULL,
    [identifier]          VARCHAR (127) NULL,
    [file_hash]           VARCHAR (63)  NOT NULL,
    [created_by]          VARCHAR (63)  NOT NULL,
    [created_date]        DATETIME      NOT NULL,
    [status]              TINYINT       NOT NULL,
    [inventory_source_id] INT           NOT NULL,
    [rows_processed]      INT           NULL,
    [effective_date]      DATETIME      NULL,
    [end_date]            DATETIME      NULL,
    CONSTRAINT [PK_inventory_files] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_files_inventory_source] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_files_inventory_source]
    ON [dbo].[inventory_files]([inventory_source_id] ASC);
