CREATE TABLE [dbo].[nti_universe_headers] (
    [id]           INT          IDENTITY (1, 1) NOT NULL,
    [created_date] DATETIME     NOT NULL,
    [created_by]   VARCHAR (63) NOT NULL,
    [year]         INT          NOT NULL,
    [month]        INT          NOT NULL,
    [week_number]  INT          NOT NULL,
    CONSTRAINT [PK_nti_universe_headers] PRIMARY KEY CLUSTERED ([id] ASC)
);

