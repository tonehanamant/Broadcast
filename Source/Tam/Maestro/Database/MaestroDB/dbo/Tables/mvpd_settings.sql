CREATE TABLE [dbo].[mvpd_settings] (
    [mvpd_business_id]      INT NOT NULL,
    [no_cents_in_spot_rate] BIT NOT NULL,
    CONSTRAINT [PK_mvpd_settings] PRIMARY KEY CLUSTERED ([mvpd_business_id] ASC)
);

