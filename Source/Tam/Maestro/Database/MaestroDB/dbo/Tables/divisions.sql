CREATE TABLE [dbo].[divisions] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [code]           VARCHAR (15)  NULL,
    [name]           VARCHAR (255) NULL,
    [mvpd]           INT           NOT NULL,
    [effective_date] DATETIME      NULL,
    [is_active]      BIT           DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

