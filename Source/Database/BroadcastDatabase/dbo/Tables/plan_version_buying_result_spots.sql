CREATE TABLE [dbo].[plan_version_buying_result_spots] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_buying_result_id] INT             NOT NULL,
    [program_name]                  VARCHAR (255)   NOT NULL,
    [genre]                         VARCHAR (500)   NOT NULL,
    [avg_impressions]               FLOAT (53)      NOT NULL,
    [avg_cpm]                       DECIMAL (19, 4) NOT NULL,
    [station_count]                 INT             NOT NULL,
    [market_count]                  INT             NOT NULL,
    [percentage_of_buy]             FLOAT (53)      NOT NULL,
    [budget]                        DECIMAL (19, 4) NOT NULL,
    [spots]                         INT             NOT NULL,
    [impressions]                   FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_plan_version_buying_result_spots] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_buying_result_spots_plan_version_buying_results] FOREIGN KEY ([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_result_spots_plan_version_buying_results]
    ON [dbo].[plan_version_buying_result_spots]([plan_version_buying_result_id] ASC);

