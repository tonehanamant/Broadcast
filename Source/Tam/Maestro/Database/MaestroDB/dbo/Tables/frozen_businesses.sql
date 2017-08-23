CREATE TABLE [dbo].[frozen_businesses] (
    [media_month_id] SMALLINT     NOT NULL,
    [id]             INT          NOT NULL,
    [code]           VARCHAR (15) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [type]           VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_frozen_business] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC)
);

