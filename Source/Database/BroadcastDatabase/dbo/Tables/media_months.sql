CREATE TABLE [dbo].[media_months] (
    [id]          INT          IDENTITY (1, 1) NOT NULL,
    [year]        INT          NOT NULL,
    [month]       INT          NOT NULL,
    [media_month] VARCHAR (15) NOT NULL,
    [start_date]  DATETIME     NOT NULL,
    [end_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_media_months] PRIMARY KEY CLUSTERED ([id] ASC)
);

