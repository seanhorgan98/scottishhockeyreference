CREATE TABLE `leagues` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `Hockey_Category_ID` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `Hockey_Category_ID_idx` (`Hockey_Category_ID`),
  CONSTRAINT `FK_leagues_hockey_categories` FOREIGN KEY (`Hockey_Category_ID`) REFERENCES `hockey_categories` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=39 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
