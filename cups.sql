CREATE TABLE `cups` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) DEFAULT NULL,
  `Hockey_Category_ID` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `FK_cups_hockey_categories_idx` (`Hockey_Category_ID`),
  CONSTRAINT `FK_cups_hockey_categories` FOREIGN KEY (`Hockey_Category_ID`) REFERENCES `hockey_categories` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
