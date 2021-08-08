CREATE TABLE `Schema_Changes` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `MajorReleaseNumber` varchar(2) NOT NULL,
  `MinorReleaseNumber` varchar(2) NOT NULL,
  `PointReleaseNumber` varchar(4) NOT NULL,
  `ScriptName` varchar(50) NOT NULL,
  `DateApplied` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Most_Recent_Date` (
  `MostRecentDate` datetime NOT NULL,
  `ID` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`MostRecentDate`),
  UNIQUE KEY `MostRecentDay_UNIQUE` (`MostRecentDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Categories` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Category_ID_UNIQUE` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Cups` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) DEFAULT NULL,
  `Hockey_Category_ID` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `FK_cups_hockey_categories_idx` (`Hockey_Category_ID`),
  CONSTRAINT `FK_cups_hockey_categories` FOREIGN KEY (`Hockey_Category_ID`) REFERENCES `Categories` (`ID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Leagues` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `Hockey_Category_ID` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `Hockey_Category_ID_idx` (`Hockey_Category_ID`),
  CONSTRAINT `FK_leagues_hockey_categories` FOREIGN KEY (`Hockey_Category_ID`) REFERENCES `Categories` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Seasons` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Teams` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Teamname` varchar(255) DEFAULT NULL,
  `League_ID` int NOT NULL,
  `Sponsor` varchar(255) DEFAULT NULL,
  `Movement` int NOT NULL DEFAULT '0',
  `Rating` int DEFAULT NULL,
  `SeasonDrawn` int NOT NULL DEFAULT '0',
  `SeasonGoalDifference` int NOT NULL DEFAULT '0',
  `SeasonGoalsAgainst` int NOT NULL DEFAULT '0',
  `SeasonGoalsFor` int NOT NULL DEFAULT '0',
  `SeasonLost` int NOT NULL DEFAULT '0',
  `SeasonPlayed` int NOT NULL DEFAULT '0',
  `SeasonPoints` int NOT NULL DEFAULT '0',
  `SeasonWon` int NOT NULL DEFAULT '0',
  `TotalDrawn` int NOT NULL DEFAULT '0',
  `TotalGoalDifference` int NOT NULL DEFAULT '0',
  `TotalGoalsAgainst` int NOT NULL DEFAULT '0',
  `TotalGoalsFor` int NOT NULL DEFAULT '0',
  `TotalLost` int NOT NULL DEFAULT '0',
  `TotalPlayed` int NOT NULL DEFAULT '0',
  `TotalPoints` int NOT NULL DEFAULT '0',
  `TotalWon` int NOT NULL DEFAULT '0',
  `Hockey_Category_ID` int NOT NULL,
  `League_Rank` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Id_UNIQUE` (`ID`),
  KEY `League_ID_idx` (`League_ID`),
  KEY `Hockey_Category_ID_idx` (`Hockey_Category_ID`),
  CONSTRAINT `FK_teams_hockey_category` FOREIGN KEY (`Hockey_Category_ID`) REFERENCES `Categories` (`ID`),
  CONSTRAINT `FK_teams_leagues` FOREIGN KEY (`League_ID`) REFERENCES `Leagues` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=3064 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `Fixtures` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `Team_1_ID` int NOT NULL,
  `Team_2_ID` int NOT NULL,
  `Location` varchar(50) DEFAULT NULL,
  `Season_ID` int NOT NULL,
  `League_ID` int DEFAULT NULL,
  `Cup_ID` int DEFAULT NULL,
  `Team_1_Score` int DEFAULT NULL,
  `Team_2_Score` int DEFAULT NULL,
  `Team_1_Elo_Change` int DEFAULT NULL,
  `Hockey_Category_ID` int DEFAULT NULL,
  `Team_2_Elo_Change` int DEFAULT NULL,
  PRIMARY KEY (`Date`,`Team_1_ID`,`Team_2_ID`),
  UNIQUE KEY `fixture_id_UNIQUE` (`ID`),
  KEY `FK_fixtures_teams_1_idx` (`Team_1_ID`),
  KEY `FK_fixtures_teams_2_idx` (`Team_2_ID`),
  KEY `FK_fixtures_seasons_idx` (`Season_ID`),
  KEY `FK_fixtures_cups_idx` (`Cup_ID`),
  KEY `FK_fixtures_leagues_idx` (`League_ID`),
  CONSTRAINT `FK_fixtures_cups` FOREIGN KEY (`Cup_ID`) REFERENCES `Cups` (`ID`),
  CONSTRAINT `FK_fixtures_leagues` FOREIGN KEY (`League_ID`) REFERENCES `Leagues` (`ID`),
  CONSTRAINT `FK_fixtures_seasons` FOREIGN KEY (`Season_ID`) REFERENCES `Seasons` (`ID`),
  CONSTRAINT `FK_fixtures_teams_1` FOREIGN KEY (`Team_1_ID`) REFERENCES `Teams` (`ID`),
  CONSTRAINT `FK_fixtures_teams_2` FOREIGN KEY (`Team_2_ID`) REFERENCES `Teams` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=1577 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
