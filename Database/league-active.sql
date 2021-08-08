ALTER TABLE `scottishhockeyreference`.`Leagues` 
ADD COLUMN `Active` INT NOT NULL DEFAULT 1 AFTER `Tier`;
