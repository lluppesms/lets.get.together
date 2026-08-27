CREATE TABLE [Meetings].[User](
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [DisplayName] [nvarchar](200) NOT NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId] ASC)
)
GO

ALTER TABLE [Meetings].[User] ADD CONSTRAINT [DF_User_IsActive] DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [Meetings].[User] ADD CONSTRAINT [DF_User_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

