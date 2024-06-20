SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Currencies definition

CREATE TABLE Currencies
(
    Id tinyint NOT NULL,
    Code nchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Number nchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Name nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT PK_Currencies_Id PRIMARY KEY (Id)
);


-- Languages definition


CREATE TABLE Languages
(
    Id int NOT NULL,
    NameEng nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NameCode nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NameNative nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT PK_Languages_Id PRIMARY KEY (Id)
);


-- TempPersonalData definition


CREATE TABLE TempPersonalData
(
    UserId bigint NOT NULL,
    FirstName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    LastName nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Email nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Tel nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    WhatsApp nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Viber nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Telegram nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    IsEmailNews bit NOT NULL,
    IsMesNews bit NOT NULL,
    SecretWord nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    IsAgreePSPD bit NOT NULL,
    Promocode nvarchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    SubmitDate datetime DEFAULT getdate() NOT NULL,
    CONSTRAINT PK_TempPersonalData_UserId PRIMARY KEY (UserId)
);


-- Users definition


CREATE TABLE Users
(
    Id bigint IDENTITY(2523236531105401,1) NOT NULL,
    [Login] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Password nchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Salt nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CreationDate datetime DEFAULT getdate() NOT NULL,
    CONSTRAINT PK_Users_Id PRIMARY KEY (Id),
    CONSTRAINT UK_Users_Login UNIQUE ([Login])
);


-- Balances definition


CREATE TABLE Balances
(
    UserId bigint NOT NULL,
    Cents bigint NOT NULL,
    CONSTRAINT PK_Balances_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_Balances_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- OneTimePasswords definition


CREATE TABLE OneTimePasswords
(
    UserId bigint NOT NULL,
    Value nvarchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Expiration datetime2 NOT NULL,
    CONSTRAINT PK_OneTimePasswords_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_OneTimePasswords_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- PromiseLimits definition


CREATE TABLE PromiseLimits
(
    UserId bigint NOT NULL,
    Cents bigint NOT NULL,
    CONSTRAINT PK_PromiseLimits_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_PromiseLimits_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- PromiseTransactions definition


CREATE TABLE PromiseTransactions
(
    Id bigint IDENTITY(1,1) NOT NULL,
    SenderId bigint NOT NULL,
    ReceiverId bigint NOT NULL,
    Cents int NOT NULL,
    [Date] datetime DEFAULT getutcdate() NOT NULL,
    Hash nchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    IsBlockchain bit NOT NULL,
    Memo nvarchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT PK_PromiseTransactions_Id PRIMARY KEY (Id),
    CONSTRAINT FK_PromiseTransactions_ReceiverId_Users_Id FOREIGN KEY (ReceiverId) REFERENCES Users(Id),
    CONSTRAINT FK_PromiseTransactions_SenderId_Users_Id FOREIGN KEY (SenderId) REFERENCES Users(Id)
);
CREATE NONCLUSTERED INDEX IX_PromiseTransactions_ReceiverId ON dbo.PromiseTransactions (  ReceiverId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ]
;
CREATE NONCLUSTERED INDEX IX_PromiseTransactions_SenderId ON dbo.PromiseTransactions (  SenderId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ]
;



-- Rates definition


CREATE TABLE Rates
(
    CurrencyId tinyint NOT NULL,
    AmountFor100 float NOT NULL,
    UpdateDate date NOT NULL,
    CONSTRAINT PK_Rates_CurrencyId PRIMARY KEY (CurrencyId),
    CONSTRAINT FK_Rates_CurrencyId_Currencies_Id FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id) ON DELETE CASCADE ON UPDATE CASCADE
);


-- UserSettings definition


CREATE TABLE UserSettings
(
    UserId bigint NOT NULL,
    LanguageId int NOT NULL,
    CurrencyId tinyint NOT NULL,
    IsDarkTheme bit NOT NULL,
    CONSTRAINT PK_UserSettings_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_UserSettings_CurrencyId_Currencies_Id FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_UserSettings_LanguageId_Languages_Id FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT FK_UserSettings_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES Users(Id)
);



-- PersonalData definition

CREATE TABLE [dbo].[UserSettings]
(
    [UserId] [bigint] NOT NULL,
    [LanguageId] [int] NOT NULL,
    [CurrencyId] [tinyint] NOT NULL,
    [IsDarkTheme] [bit] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserSettings] ADD  CONSTRAINT [PK_UserSettings_UserId] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserSettings]  WITH CHECK ADD  CONSTRAINT [FK_UserSettings_CurrencyId_Currencies_Id] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[Currencies] ([Id])
GO
ALTER TABLE [dbo].[UserSettings] CHECK CONSTRAINT [FK_UserSettings_CurrencyId_Currencies_Id]
GO
ALTER TABLE [dbo].[UserSettings]  WITH CHECK ADD  CONSTRAINT [FK_UserSettings_LanguageId_Languages_Id] FOREIGN KEY([LanguageId])
REFERENCES [dbo].[Languages] ([Id])
GO
ALTER TABLE [dbo].[UserSettings] CHECK CONSTRAINT [FK_UserSettings_LanguageId_Languages_Id]
GO
ALTER TABLE [dbo].[UserSettings]  WITH CHECK ADD  CONSTRAINT [FK_UserSettings_UserId_Users_Id] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserSettings] CHECK CONSTRAINT [FK_UserSettings_UserId_Users_Id]
GO
