CREATE TABLE [dbo].[cluster_rate_cards] (
    [id]                      INT           IDENTITY (1, 1) NOT NULL,
    [topography_id]           INT           NOT NULL,
    [start_date]              DATETIME      NOT NULL,
    [end_date]                DATETIME      NULL,
    [name]                    VARCHAR (127) NOT NULL,
    [date_created]            DATETIME      NOT NULL,
    [date_last_modified]      DATETIME      NOT NULL,
    [date_approved]           DATETIME      NULL,
    [approved_by_employee_id] INT           NULL,
    CONSTRAINT [PK_cluster_rate_cards] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_rate_cards_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);

