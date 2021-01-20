CREATE TABLE [dbo].[station_contacts] (
    [id]               INT           IDENTITY (1, 1) NOT NULL,
    [name]             VARCHAR (127) NOT NULL,
    [phone]            VARCHAR (63)  NULL,
    [fax]              VARCHAR (63)  NULL,
    [email]            VARCHAR (63)  NULL,
    [type]             TINYINT       NOT NULL,
    [created_by]       VARCHAR (63)  NOT NULL,
    [created_date]     DATETIME      NOT NULL,
    [modified_by]      VARCHAR (63)  NOT NULL,
    [modified_date]    DATETIME      NOT NULL,
    [created_file_id]  INT           NULL,
    [company]          VARCHAR (127) NULL,
    [modified_file_id] INT           NULL,
    [station_id]       INT           NOT NULL,
    CONSTRAINT [PK_station_contacts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_contacts_inventory_files_created] FOREIGN KEY ([created_file_id]) REFERENCES [dbo].[inventory_files] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_station_contacts_inventory_files_modified] FOREIGN KEY ([modified_file_id]) REFERENCES [dbo].[inventory_files] ([id]),
    CONSTRAINT [FK_station_contacts_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_contacts_stations]
    ON [dbo].[station_contacts]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_contacts_inventory_files_modified]
    ON [dbo].[station_contacts]([modified_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_contacts_inventory_files_created]
    ON [dbo].[station_contacts]([created_file_id] ASC);

