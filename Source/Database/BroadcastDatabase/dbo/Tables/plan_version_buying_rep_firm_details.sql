CREATE TABLE [dbo].[plan_version_buying_rep_firm_details] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_buying_result_id] INT             NOT NULL,
    [rep_firm_name]                 NVARCHAR (100)  NOT NULL,
    [markets]                       INT             NOT NULL,
    [stations]                      INT             NOT NULL,
    [spots]                         INT             NOT NULL,
    [impressions]                   FLOAT (53)      NOT NULL,
    [cpm]                           DECIMAL (19, 4) NOT NULL,
    [budget]                        DECIMAL (19, 4) NOT NULL,
    [impressions_percentage]        FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_plan_version_buying_rep_firm_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_buying_rep_firm_details_plan_version_buying_results] FOREIGN KEY ([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_rep_firm_details_plan_version_buying_results]
    ON [dbo].[plan_version_buying_rep_firm_details]([plan_version_buying_result_id] ASC);

