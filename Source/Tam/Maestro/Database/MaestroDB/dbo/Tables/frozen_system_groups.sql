CREATE TABLE [dbo].[frozen_system_groups] (
    [media_month_id] SMALLINT     NOT NULL,
    [id]             INT          NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [flag]           TINYINT      NULL,
    CONSTRAINT [PK_frozen_system_groups] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC)
);

