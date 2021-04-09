CREATE TABLE `locations` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  `Associated_Team_ID` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `Associated_Team_ID_idx` (`Associated_Team_ID`),
  CONSTRAINT `FK_locations_teams` FOREIGN KEY (`Associated_Team_ID`) REFERENCES `teams` (`League_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
