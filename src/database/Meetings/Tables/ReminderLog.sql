CREATE TABLE [Meetings].[ReminderLog](
    [ReminderLogId] [int] IDENTITY(1,1) NOT NULL,
    [EventId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [Channel] [nvarchar](30) NOT NULL,
    [SentUtc] [datetime2](7) NOT NULL,
    [DeliveryState] [nvarchar](30) NOT NULL,
    [ProviderMessageId] [nvarchar](120) NULL,
    CONSTRAINT [PK_ReminderLog] PRIMARY KEY CLUSTERED ([ReminderLogId] ASC),
    CONSTRAINT [FK_ReminderLog_Event] FOREIGN KEY ([EventId]) REFERENCES [Meetings].[Event] ([EventId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReminderLog_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId])
)
GO

ALTER TABLE [Meetings].[ReminderLog] ADD CONSTRAINT [DF_ReminderLog_Channel] DEFAULT (N'InApp') FOR [Channel]
GO
ALTER TABLE [Meetings].[ReminderLog] ADD CONSTRAINT [DF_ReminderLog_SentUtc] DEFAULT (getutcdate()) FOR [SentUtc]
GO
ALTER TABLE [Meetings].[ReminderLog] ADD CONSTRAINT [DF_ReminderLog_DeliveryState] DEFAULT (N'Queued') FOR [DeliveryState]
GO

CREATE INDEX [IX_ReminderLog_EventId_UserId_SentUtc] ON [Meetings].[ReminderLog] ([EventId], [UserId], [SentUtc])
GO