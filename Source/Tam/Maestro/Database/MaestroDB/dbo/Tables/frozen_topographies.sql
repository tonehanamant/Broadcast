CREATE TABLE [dbo].[frozen_topographies] (
    [media_month_id]  SMALLINT     NOT NULL,
    [id]              INT          NOT NULL,
    [code]            VARCHAR (15) NOT NULL,
    [name]            VARCHAR (63) NOT NULL,
    [topography_type] TINYINT      NOT NULL,
    CONSTRAINT [PK_frozen_topographies] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC)
);

