CREATE TABLE `SeasonSnapshot` (
                                  `ID` int NOT NULL AUTO_INCREMENT,
                                  `TeamId` int NOT NULL,
                                  `SeasonId` int NOT NULL,
                                  `LeagueId` int NOT NULL,
                                  `CupId` int DEFAULT NULL,
                                  `LeaguePlacing` int DEFAULT NULL,
                                  `Played` int DEFAULT NULL,
                                  `Won` int DEFAULT NULL,
                                  `Drawn` int DEFAULT NULL,
                                  `Lost` int DEFAULT NULL,
                                  `GoalsFor` int DEFAULT NULL,
                                  `GoalsAgainst` int DEFAULT NULL,
                                  `GoalDifference` int DEFAULT NULL,
                                  `Points` int DEFAULT NULL,
                                  `Sponsor` varchar(200) DEFAULT NULL,
                                  `Rating` int DEFAULT NULL,
                                  PRIMARY KEY (`ID`),
                                  KEY `fk_SeasonSnapshot_Teams_idx` (`TeamId`),
                                  KEY `fk_SeasonSnapshot_Seasons_idx` (`SeasonId`),
                                  KEY `fk_SeasonSnapshot_Leagues_idx` (`LeagueId`),
                                  KEY `fk_SeasonSnapshot_Cups_idx` (`CupId`),
                                  CONSTRAINT `fk_SeasonSnapshot_Cups` FOREIGN KEY (`CupId`) REFERENCES `Cups` (`ID`),
                                  CONSTRAINT `fk_SeasonSnapshot_Leagues` FOREIGN KEY (`LeagueId`) REFERENCES `Leagues` (`ID`),
                                  CONSTRAINT `fk_SeasonSnapshot_Seasons` FOREIGN KEY (`SeasonId`) REFERENCES `Seasons` (`ID`),
                                  CONSTRAINT `fk_SeasonSnapshot_Teams` FOREIGN KEY (`TeamId`) REFERENCES `Teams` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=232 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


USE `scottishhockeyreference`;
DROP procedure IF EXISTS `CreateSeasonSnapshots`;

DELIMITER $$
USE `scottishhockeyreference`$$
CREATE PROCEDURE `CreateSeasonSnapshots` ()
BEGIN
INSERT INTO SeasonSnapshot
	(TeamId,
    SeasonId,
    LeagueId,
    LeaguePlacing,
    Played,
    Won,
    Drawn,
    Lost,
    GoalsFor,
    GoalsAgainst,
    GoalDifference,
    Points,
    Sponsor,
    Rating)
    
SELECT ID,
	1,
    League_ID,
    League_Rank,
    SeasonPlayed,
    SeasonWon,
    SeasonDrawn,
    SeasonLost,
    SeasonGoalsFor,
    SeasonGoalsAgainst,
    SeasonGoalDifference,
    SeasonPoints,
    Sponsor,
    Rating
FROM Teams;
END$$

DELIMITER ;


