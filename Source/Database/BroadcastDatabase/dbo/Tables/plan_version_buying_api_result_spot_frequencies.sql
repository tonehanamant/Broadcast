CREATE TABLE [dbo].[plan_version_buying_api_result_spot_frequencies] (
    [id]                                     INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_buying_api_result_spot_id] INT             NOT NULL,
    [spot_length_id]                         INT             NOT NULL,
    [cost]                                   DECIMAL (19, 4) NOT NULL,
    [spots]                                  INT             NOT NULL,
    [impressions]                            FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_plan_version_buying_api_result_spot_frequencies] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_buying_api_result_spot_frequencies_plan_version_buying_api_result_spots] FOREIGN KEY ([plan_version_buying_api_result_spot_id]) REFERENCES [dbo].[plan_version_buying_api_result_spots] ([id]),
    CONSTRAINT [FK_plan_version_buying_api_result_spot_frequencies_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spot_frequencies_spot_lengths]
    ON [dbo].[plan_version_buying_api_result_spot_frequencies]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spot_frequencies_plan_version_buying_api_result_spots]
    ON [dbo].[plan_version_buying_api_result_spot_frequencies]([plan_version_buying_api_result_spot_id] ASC);

