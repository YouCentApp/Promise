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


CREATE TABLE PersonalData (
	UserId bigint NOT NULL,
	Email nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Tel nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Secret nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	EmailHash nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TelHash nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	SecretHash nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Salt nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	EmailMasked nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TelMasked nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CONSTRAINT PK_PersonalData_UserId PRIMARY KEY (UserId)
);

ALTER TABLE PersonalData ADD CONSTRAINT FK_PersonalData_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES YCDB.dbo.Users(Id);


-- AccessRestore definition


CREATE TABLE AccessRestore (
	UserId bigint NOT NULL,
	UseSecretTryNumber int NOT NULL,
	UseSecretTryDate datetime NULL,
	UseEmailTryNumber int NOT NULL,
	UseEmailTryDate datetime NULL,
	UseTelTryNumber int NOT NULL,
	UseTelTryDate datetime NULL,
	CONSTRAINT PK_AccessRestore_UserId PRIMARY KEY (UserId)
);

ALTER TABLE AccessRestore ADD CONSTRAINT FK_AccessRestore_UserId_User_Id FOREIGN KEY (UserId) REFERENCES Users(Id);