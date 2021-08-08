ALTER TABLE `scottishhockeyreference`.`Teams` 
ADD COLUMN `Active` INT NOT NULL DEFAULT 1 AFTER `League_Rank`;
