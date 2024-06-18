
-- YCDB.dbo.Currencies definition

CREATE TABLE YCDB.dbo.Currencies
(
    Id tinyint NOT NULL,
    Code nchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Number nchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Name nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT PK_Currencies_Id PRIMARY KEY (Id)
);


-- YCDB.dbo.Languages definition


CREATE TABLE YCDB.dbo.Languages
(
    Id int NOT NULL,
    NameEng nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NameCode nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NameNative nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT PK_Languages_Id PRIMARY KEY (Id)
);


-- YCDB.dbo.TempPersonalData definition


CREATE TABLE YCDB.dbo.TempPersonalData
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


-- YCDB.dbo.Users definition


CREATE TABLE YCDB.dbo.Users
(
    Id bigint IDENTITY(2523236531105401,1) NOT NULL,
    [Login] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Password nchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Salt nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CreationDate datetime DEFAULT getdate() NOT NULL,
    CONSTRAINT PK_Users_Id PRIMARY KEY (Id),
    CONSTRAINT UK_Users_Login UNIQUE ([Login])
);


-- YCDB.dbo.Balances definition


CREATE TABLE YCDB.dbo.Balances
(
    UserId bigint NOT NULL,
    Cents bigint NOT NULL,
    CONSTRAINT PK_Balances_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_Balances_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES YCDB.dbo.Users(Id)
);


-- YCDB.dbo.OneTimePasswords definition


CREATE TABLE YCDB.dbo.OneTimePasswords
(
    UserId bigint NOT NULL,
    Value nvarchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Expiration datetime2 NOT NULL,
    CONSTRAINT PK_OneTimePasswords_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_OneTimePasswords_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES YCDB.dbo.Users(Id)
);


-- YCDB.dbo.PromiseLimits definition


CREATE TABLE YCDB.dbo.PromiseLimits
(
    UserId bigint NOT NULL,
    Cents bigint NOT NULL,
    CONSTRAINT PK_PromiseLimits_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_PromiseLimits_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES YCDB.dbo.Users(Id)
);


-- YCDB.dbo.PromiseTransactions definition


CREATE TABLE YCDB.dbo.PromiseTransactions
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
    CONSTRAINT FK_PromiseTransactions_ReceiverId_Users_Id FOREIGN KEY (ReceiverId) REFERENCES YCDB.dbo.Users(Id),
    CONSTRAINT FK_PromiseTransactions_SenderId_Users_Id FOREIGN KEY (SenderId) REFERENCES YCDB.dbo.Users(Id)
);
CREATE NONCLUSTERED INDEX IX_PromiseTransactions_ReceiverId ON dbo.PromiseTransactions (  ReceiverId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ]
;
CREATE NONCLUSTERED INDEX IX_PromiseTransactions_SenderId ON dbo.PromiseTransactions (  SenderId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ]
;



-- YCDB.dbo.Rates definition


CREATE TABLE YCDB.dbo.Rates
(
    CurrencyId tinyint NOT NULL,
    AmountFor100 float NOT NULL,
    UpdateDate date NOT NULL,
    CONSTRAINT PK_Rates_CurrencyId PRIMARY KEY (CurrencyId),
    CONSTRAINT FK_Rates_CurrencyId_Currencies_Id FOREIGN KEY (CurrencyId) REFERENCES YCDB.dbo.Currencies(Id) ON DELETE CASCADE ON UPDATE CASCADE
);


-- YCDB.dbo.UserSettings definition


CREATE TABLE YCDB.dbo.UserSettings
(
    UserId bigint NOT NULL,
    LanguageId int NOT NULL,
    CurrencyId tinyint NOT NULL,
    IsDarkTheme bit NOT NULL,
    CONSTRAINT PK_UserSettings_UserId PRIMARY KEY (UserId),
    CONSTRAINT FK_UserSettings_CurrencyId_Currencies_Id FOREIGN KEY (CurrencyId) REFERENCES YCDB.dbo.Currencies(Id),
    CONSTRAINT FK_UserSettings_LanguageId_Languages_Id FOREIGN KEY (LanguageId) REFERENCES YCDB.dbo.Languages(Id),
    CONSTRAINT FK_UserSettings_UserId_Users_Id FOREIGN KEY (UserId) REFERENCES YCDB.dbo.Users(Id)
);