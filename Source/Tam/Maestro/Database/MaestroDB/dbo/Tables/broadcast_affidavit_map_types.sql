CREATE TABLE [dbo].[broadcast_affidavit_map_types] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [display]            NVARCHAR (16) NOT NULL,
    [version]            INT           DEFAULT ((1)) NOT NULL,
    [last_modified_by]   INT           NULL,
    [last_modified_date] DATETIME      NOT NULL,
    CONSTRAINT [PK_broadcast_affidavit_map_types] PRIMARY KEY CLUSTERED ([id] ASC)
);

