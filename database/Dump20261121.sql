-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: grooming_salon
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `appointments`
--

DROP TABLE IF EXISTS `appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `appointments` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int unsigned NOT NULL,
  `pet_id` int unsigned NOT NULL,
  `groomer_id` int unsigned NOT NULL,
  `service_id` int unsigned NOT NULL,
  `start_datetime` datetime NOT NULL,
  `end_datetime` datetime NOT NULL,
  `status` enum('booked','confirmed','completed','cancelled','no_show') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'booked',
  `comment` text COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_app_client` (`user_id`),
  KEY `idx_app_pet` (`pet_id`),
  KEY `idx_app_groomer` (`groomer_id`),
  KEY `idx_app_service` (`service_id`),
  KEY `idx_app_start` (`start_datetime`),
  KEY `idx_app_status` (`status`),
  CONSTRAINT `fk_app_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_app_pet` FOREIGN KEY (`pet_id`) REFERENCES `pets` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_app_service` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_app_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_app_range` CHECK ((`start_datetime` < `end_datetime`))
) ENGINE=InnoDB AUTO_INCREMENT=151 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `appointments`
--

LOCK TABLES `appointments` WRITE;
/*!40000 ALTER TABLE `appointments` DISABLE KEYS */;
INSERT INTO `appointments` VALUES (4,5,4,1,1,'2026-11-19 09:00:00','2026-11-19 11:00:00','completed','Повторное посещение','2026-11-17 11:02:14'),(6,5,4,1,2,'2026-11-23 11:30:00','2026-11-23 12:00:00','cancelled','Клиент отменил запись','2026-11-20 12:32:05'),(7,5,4,1,1,'2026-11-23 09:30:00','2026-11-23 11:30:00','cancelled','','2026-11-20 20:20:23'),(8,5,4,1,2,'2026-11-04 09:30:00','2026-11-04 10:00:00','completed','Стрижка когтей','2026-11-01 10:00:00'),(9,9,7,1,1,'2026-11-02 09:00:00','2026-11-02 11:00:00','completed','Первое посещение','2026-10-30 10:00:00'),(10,9,7,2,3,'2026-11-04 10:00:00','2026-11-04 11:00:00','completed','Повторное мытьё','2026-11-01 11:00:00'),(11,10,8,2,3,'2026-11-02 10:00:00','2026-11-02 11:00:00','completed','Мытьё и сушка','2026-10-30 11:00:00'),(12,10,8,1,7,'2026-11-05 13:00:00','2026-11-05 14:30:00','completed','SPA-процедура','2026-11-02 12:00:00'),(13,11,9,1,3,'2026-11-03 11:00:00','2026-11-03 12:00:00','completed','Гигиенический уход','2026-10-31 12:00:00'),(14,11,9,2,3,'2026-11-05 14:00:00','2026-11-05 15:00:00','completed','Мытьё и сушка','2026-11-02 13:00:00'),(15,12,10,2,1,'2026-11-03 12:00:00','2026-11-03 14:00:00','completed','Комплексный уход','2026-10-31 13:00:00'),(23,5,4,2,3,'2026-11-28 09:00:00','2026-11-28 10:00:00','booked','','2026-11-21 09:26:58'),(24,10,8,1,1,'2026-11-30 09:00:00','2026-11-30 11:00:00','completed','Историческая запись','2026-11-23 09:00:00'),(25,11,9,1,3,'2026-11-30 13:00:00','2026-11-30 14:00:00','completed','Историческая запись','2026-11-23 13:00:00'),(26,9,7,1,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(27,10,8,1,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(28,5,4,1,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(29,9,7,1,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(30,12,10,1,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(31,5,4,1,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(32,11,9,2,1,'2026-11-29 09:00:00','2026-11-29 11:00:00','completed','Историческая запись','2026-11-22 09:00:00'),(33,12,10,2,3,'2026-11-29 13:00:00','2026-11-29 14:00:00','completed','Историческая запись','2026-11-22 13:00:00'),(34,10,8,2,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(35,11,9,2,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(36,9,7,2,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(37,10,8,2,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(38,5,4,2,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(39,9,7,2,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(40,12,10,3,1,'2026-11-30 09:00:00','2026-11-30 11:00:00','completed','Историческая запись','2026-11-23 09:00:00'),(41,5,4,3,3,'2026-11-30 13:00:00','2026-11-30 14:00:00','completed','Историческая запись','2026-11-23 13:00:00'),(42,11,9,3,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(43,12,10,3,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(44,10,8,3,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(45,11,9,3,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(46,9,7,3,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(47,10,8,3,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(48,5,4,3,1,'2026-11-03 09:00:00','2026-11-03 11:00:00','completed','Историческая запись','2026-10-27 09:00:00'),(49,9,7,3,3,'2026-11-05 13:00:00','2026-11-05 14:00:00','completed','Историческая запись','2026-10-29 13:00:00'),(50,5,4,4,1,'2026-11-29 09:00:00','2026-11-29 11:00:00','completed','Историческая запись','2026-11-22 09:00:00'),(51,9,7,4,3,'2026-11-29 13:00:00','2026-11-29 14:00:00','completed','Историческая запись','2026-11-22 13:00:00'),(52,12,10,4,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(53,5,4,4,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(54,11,9,4,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(55,12,10,4,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(56,10,8,4,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(57,11,9,4,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(58,9,7,4,1,'2026-11-03 09:00:00','2026-11-03 11:00:00','completed','Историческая запись','2026-10-27 09:00:00'),(59,10,8,4,3,'2026-11-05 13:00:00','2026-11-05 14:00:00','completed','Историческая запись','2026-10-29 13:00:00'),(60,9,7,5,1,'2026-11-30 09:00:00','2026-11-30 11:00:00','completed','Историческая запись','2026-11-23 09:00:00'),(61,10,8,5,3,'2026-11-30 13:00:00','2026-11-30 14:00:00','completed','Историческая запись','2026-11-23 13:00:00'),(62,5,4,5,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(63,9,7,5,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(64,12,10,5,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(65,5,4,5,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(66,11,9,5,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(67,12,10,5,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(68,10,8,5,1,'2026-11-03 09:00:00','2026-11-03 11:00:00','completed','Историческая запись','2026-10-27 09:00:00'),(69,11,9,5,3,'2026-11-05 13:00:00','2026-11-05 14:00:00','completed','Историческая запись','2026-10-29 13:00:00'),(70,10,8,6,1,'2026-11-29 09:00:00','2026-11-29 11:00:00','completed','Историческая запись','2026-11-22 09:00:00'),(71,11,9,6,3,'2026-11-29 13:00:00','2026-11-29 14:00:00','completed','Историческая запись','2026-11-22 13:00:00'),(72,9,7,6,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(73,10,8,6,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(74,5,4,6,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(75,9,7,6,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(76,12,10,6,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(77,5,4,6,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(78,11,9,6,1,'2026-11-03 09:00:00','2026-11-03 11:00:00','completed','Историческая запись','2026-10-27 09:00:00'),(79,12,10,6,3,'2026-11-05 13:00:00','2026-11-05 14:00:00','completed','Историческая запись','2026-10-29 13:00:00'),(80,11,9,7,1,'2026-11-30 09:00:00','2026-11-30 11:00:00','completed','Историческая запись','2026-11-23 09:00:00'),(81,12,10,7,3,'2026-11-30 13:00:00','2026-11-30 14:00:00','completed','Историческая запись','2026-11-23 13:00:00'),(82,10,8,7,1,'2026-11-25 09:00:00','2026-11-25 11:00:00','completed','Историческая запись','2026-11-18 09:00:00'),(83,11,9,7,3,'2026-11-27 13:00:00','2026-11-27 14:00:00','completed','Историческая запись','2026-11-20 13:00:00'),(84,9,7,7,1,'2026-11-17 09:00:00','2026-11-17 11:00:00','completed','Историческая запись','2026-11-10 09:00:00'),(85,10,8,7,3,'2026-11-19 13:00:00','2026-11-19 14:00:00','completed','Историческая запись','2026-11-12 13:00:00'),(86,5,4,7,1,'2026-11-10 09:00:00','2026-11-10 11:00:00','completed','Историческая запись','2026-11-03 09:00:00'),(87,9,7,7,3,'2026-11-12 13:00:00','2026-11-12 14:00:00','completed','Историческая запись','2026-11-05 13:00:00'),(88,12,10,7,1,'2026-11-03 09:00:00','2026-11-03 11:00:00','completed','Историческая запись','2026-10-27 09:00:00'),(89,5,4,7,3,'2026-11-05 13:00:00','2026-11-05 14:00:00','completed','Историческая запись','2026-10-29 13:00:00');
/*!40000 ALTER TABLE `appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groomer_portfolio`
--

DROP TABLE IF EXISTS `groomer_portfolio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groomer_portfolio` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `groomer_id` int unsigned NOT NULL,
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_portfolio_groomer` (`groomer_id`),
  CONSTRAINT `fk_portfolio_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groomer_portfolio`
--

LOCK TABLES `groomer_portfolio` WRITE;
/*!40000 ALTER TABLE `groomer_portfolio` DISABLE KEYS */;
INSERT INTO `groomer_portfolio` VALUES (5,1,'/uploads/portfolio/fa3b67a38e0d4885ab0a23dbc521aca6.jpg','2026-11-21 08:43:43'),(6,1,'/uploads/portfolio/850662857d9c49898bc3503480c6232e.jpg','2026-11-21 08:43:51'),(7,1,'/uploads/portfolio/b2d2cf53b12145f6aaf232ffddb372e0.jpg','2026-11-21 08:43:58'),(8,2,'/uploads/portfolio/a079bbea1da84fd984a2b04a811d6bd7.jpg','2026-11-21 08:44:09'),(9,2,'/uploads/portfolio/34c2f997e1914e3f8de2c0d654dc6c58.jpg','2026-11-21 08:44:17'),(10,2,'/uploads/portfolio/d99164cdda9f4ec0aca76f4544012a7d.jpg','2026-11-21 08:44:37'),(11,3,'/uploads/portfolio/136f9023dd2c41a781a448dd0f1d18d7.jpg','2026-11-21 08:44:45'),(12,3,'/uploads/portfolio/563f99f2620d455f9a02dbcd4ae90144.jpg','2026-11-21 08:44:55'),(13,3,'/uploads/portfolio/fd8dda9f5a1740babba1235ff3631863.jpg','2026-11-21 08:45:01'),(14,4,'/uploads/portfolio/8fb70499041b4cf78da3f02b6ae6de61.jpg','2026-11-21 08:45:27'),(15,4,'/uploads/portfolio/7a385a66e2bd4647ab3cd06cae74950f.jpg','2026-11-21 08:45:33'),(16,4,'/uploads/portfolio/2101e62cefe24027ae23fb4251320e34.jpg','2026-11-21 08:45:42'),(17,5,'/uploads/portfolio/b72f7ba8f3c44c0e878ba58ec657fe3e.jpg','2026-11-21 08:47:22'),(18,5,'/uploads/portfolio/47586e3b371d427d8703cdc271d9eb2f.jpg','2026-11-21 08:47:30'),(19,6,'/uploads/portfolio/1f990d1e7b9f4318aeb2fe9cc2dd679d.jpg','2026-11-21 08:47:43'),(20,7,'/uploads/portfolio/9941d6cbc03b46ebacaaf6a84feb9d53.jpg','2026-11-21 08:47:51');
/*!40000 ALTER TABLE `groomer_portfolio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groomer_schedule`
--

DROP TABLE IF EXISTS `groomer_schedule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groomer_schedule` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `groomer_id` int unsigned NOT NULL,
  `day_of_week` tinyint NOT NULL COMMENT '1=Пн ... 7=Вс',
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `valid_from` date NOT NULL,
  `valid_to` date DEFAULT NULL,
  `slot_duration_minutes` smallint unsigned NOT NULL DEFAULT '60',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `idx_schedule_groomer` (`groomer_id`),
  KEY `idx_schedule_day` (`day_of_week`),
  CONSTRAINT `fk_schedule_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_schedule_day` CHECK ((`day_of_week` between 1 and 7)),
  CONSTRAINT `chk_schedule_time` CHECK ((`start_time` < `end_time`))
) ENGINE=InnoDB AUTO_INCREMENT=168 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groomer_schedule`
--

LOCK TABLES `groomer_schedule` WRITE;
/*!40000 ALTER TABLE `groomer_schedule` DISABLE KEYS */;
INSERT INTO `groomer_schedule` VALUES (1,1,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(2,1,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(3,1,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(4,1,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(5,1,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(6,2,1,'10:00:00','19:00:00','2026-11-01','2026-11-22',30,1),(7,2,2,'10:00:00','19:00:00','2026-11-01','2026-11-22',30,1),(8,2,3,'10:00:00','19:00:00','2026-11-01','2026-11-22',30,1),(9,2,4,'10:00:00','19:00:00','2026-11-01','2026-11-22',30,1),(10,2,5,'10:00:00','19:00:00','2026-11-01','2026-11-22',30,1),(11,1,5,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(12,1,4,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(13,1,3,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(14,1,2,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(15,1,1,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(16,2,7,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(17,2,6,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(18,2,5,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(19,2,4,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(20,2,3,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(21,3,5,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(22,3,4,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(23,3,3,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(24,3,2,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(25,3,1,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(26,4,7,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(27,4,6,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(28,4,5,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(29,4,4,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(30,4,3,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(31,5,5,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(32,5,4,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(33,5,3,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(34,5,2,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(35,5,1,'09:00:00','13:00:00','2026-11-23',NULL,30,1),(36,6,7,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(37,6,6,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(38,6,5,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(39,6,4,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(40,6,3,'09:00:00','14:00:00','2026-11-23',NULL,30,1),(41,7,5,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(42,7,4,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(43,7,3,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(44,7,2,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(45,7,1,'09:00:00','12:00:00','2026-11-23',NULL,30,1),(74,1,5,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(75,1,4,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(76,1,3,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(77,1,2,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(78,1,1,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(79,2,7,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(80,2,6,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(81,2,5,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(82,2,4,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(83,2,3,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(84,3,5,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(85,3,4,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(86,3,3,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(87,3,2,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(88,3,1,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(89,4,7,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(90,4,6,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(91,4,5,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(92,4,4,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(93,4,3,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(94,5,5,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(95,5,4,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(96,5,3,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(97,5,2,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(98,5,1,'14:00:00','18:00:00','2026-11-23',NULL,30,1),(99,6,7,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(100,6,6,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(101,6,5,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(102,6,4,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(103,6,3,'15:00:00','18:00:00','2026-11-23',NULL,30,1),(104,7,5,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(105,7,4,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(106,7,3,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(107,7,2,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(108,7,1,'13:00:00','18:00:00','2026-11-23',NULL,30,1),(137,7,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(138,6,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(139,5,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(140,4,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(141,3,1,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(142,7,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(143,6,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(144,5,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(145,4,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(146,3,2,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(147,7,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(148,6,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(149,5,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(150,4,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(151,3,3,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(152,7,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(153,6,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(154,5,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(155,4,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(156,3,4,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(157,7,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(158,6,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(159,5,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(160,4,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1),(161,3,5,'09:00:00','18:00:00','2026-11-01','2026-11-22',30,1);
/*!40000 ALTER TABLE `groomer_schedule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groomer_time_off`
--

DROP TABLE IF EXISTS `groomer_time_off`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groomer_time_off` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `groomer_id` int unsigned NOT NULL,
  `start_datetime` datetime NOT NULL,
  `end_datetime` datetime NOT NULL,
  `type` enum('vacation','sick_leave','other') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'other',
  `comment` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`),
  KEY `idx_timeoff_groomer` (`groomer_id`),
  CONSTRAINT `fk_timeoff_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_timeoff_range` CHECK ((`start_datetime` < `end_datetime`))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groomer_time_off`
--

LOCK TABLES `groomer_time_off` WRITE;
/*!40000 ALTER TABLE `groomer_time_off` DISABLE KEYS */;
INSERT INTO `groomer_time_off` VALUES (1,2,'2026-11-21 00:00:00','2026-11-23 23:59:59','vacation','Плановый отпуск');
/*!40000 ALTER TABLE `groomer_time_off` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groomers`
--

DROP TABLE IF EXISTS `groomers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groomers` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `password_hash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `age` tinyint unsigned DEFAULT NULL,
  `education` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `experience_years` smallint unsigned DEFAULT NULL,
  `specialization` varchar(150) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_groomers_email` (`email`),
  KEY `idx_groomers_is_active` (`is_active`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groomers`
--

LOCK TABLES `groomers` WRITE;
/*!40000 ALTER TABLE `groomers` DISABLE KEYS */;
INSERT INTO `groomers` VALUES (1,'Мария Иванова','maria@gmail.com','+79994444444','AQAAAAIAAYagAAAAEOvNU3GPvnkk2NJTVFHQtyDEsmF4yzy2D2XIfmZnBMQbUJP2vCTvy/N3ZinHsxXxwg==',28,'Курсы груминга',5,'Стрижка собак',1,'2026-11-09 14:49:53','/uploads/groomers/e864cf8741f34336ab41a8d50d9bc12e.png'),(2,'Алексей Волков','alex@example.com','+79995555555','test_hash_5',32,'Профессиональная школа груминга',8,'Комплексный уход',1,'2026-11-09 14:49:53','/uploads/groomers/0cccfd3289244e15a2797dad4d0ce725.png'),(3,'Екатерина Смирнова','ekaterina.smirnova@salon.ru','+7-900-111-22-33','bfeb5815724b037a9b7cbe5e3b44d20ee7995b3a6397815ad2f08dc18b2668cf',29,'Курсы профессионального груминга',5,'Стрижка собак мелких пород',1,'2026-11-10 12:09:39','/uploads/groomers/c16a9f5e782143abaf901941cafc735f.png'),(4,'Александр Петров','alex.petrov@salon.ru','+7-900-222-33-44','4839468acd350318b9cdc0da8a8e0f4cbf9b8af91edaf5cc082437d7118b87d3',34,'Академия груминга',8,'Стрижка и тримминг',1,'2026-11-10 12:09:39','/uploads/groomers/fbae67c6fa9446e6ab013a989ca1b72f.png'),(5,'Ольга Васильева','olga.vasilieva@salon.ru','+7-900-333-44-55','65a6f8e96a53c1eb210eb4591146c2daaeb9a4fb71748f65f1903430e1aa33b9',26,'Школа зоокосметологии',3,'Купание и уход за шерстью',1,'2026-11-10 12:09:39','/uploads/groomers/3d771a30b578434ea6ccf208276b5e5e.png'),(6,'Дмитрий Козлов','dmitry.kozlov@salon.ru','+7-900-444-55-66','f743f0b00e63829e17310df182853ba22e572bd206c7fe6d114b43c5c698f44d',31,'Профессиональная школа груминга',6,'Комплексный уход за собаками',1,'2026-11-10 12:09:39','/uploads/groomers/f7a2b62e8d8a49bb98b82f70127cfe6d.png'),(7,'Наталья Соколова','natalia.sokolova@salon.ru','+7-900-555-66-77','95516a731d60cf483c6f89ffe00e7fc5f77df77edfa92b1d33a35e1096874828',27,'Курсы ветеринарной косметологии',4,'Уход за когтями и гигиенические процедуры',1,'2026-11-10 12:09:39','/uploads/groomers/e4b44ef42e5b4f26b0ab4b38ce4b9f54.png');
/*!40000 ALTER TABLE `groomers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `leave_requests`
--

DROP TABLE IF EXISTS `leave_requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_requests` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `groomer_id` int unsigned NOT NULL,
  `start_date` date NOT NULL,
  `end_date` date NOT NULL,
  `reason` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `status` enum('pending','approved','rejected') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_leave_groomer` (`groomer_id`),
  KEY `idx_leave_status` (`status`),
  CONSTRAINT `fk_leave_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_leave_dates` CHECK ((`start_date` <= `end_date`))
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `leave_requests`
--

LOCK TABLES `leave_requests` WRITE;
/*!40000 ALTER TABLE `leave_requests` DISABLE KEYS */;
/*!40000 ALTER TABLE `leave_requests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notifications` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int unsigned DEFAULT NULL,
  `groomer_id` int unsigned DEFAULT NULL,
  `title` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `message` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `type` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_read` tinyint(1) NOT NULL DEFAULT '0',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_notifications_user` (`user_id`),
  KEY `idx_notifications_groomer` (`groomer_id`),
  KEY `idx_notifications_created` (`created_at`),
  CONSTRAINT `fk_notifications_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_notifications_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
INSERT INTO `notifications` VALUES (1,NULL,1,'Новая запись','Бакс — Стрижка когтей, 23.11.2026 11:30','appointment_created',1,'2026-11-20 12:32:05'),(2,5,NULL,'Запись отменена','Ваша запись на 23.11.2026 11:30 отменена.','appointment_cancelled',1,'2026-11-20 18:18:03'),(3,NULL,1,'Запись отменена','Запись на 23.11.2026 11:30 отменена.','appointment_cancelled',1,'2026-11-20 18:18:03'),(4,NULL,1,'Новый отзыв','Клиент оставил отзыв. Оценка: 2/5.','review_created',1,'2026-11-20 19:37:46'),(5,NULL,1,'Новая запись','Бакс — Комплексный уход, 23.11.2026 09:30','appointment_created',1,'2026-11-20 20:20:24'),(6,NULL,2,'Новая запись','Бакс — Мытьё и сушка, 28.11.2026 09:00','appointment_created',0,'2026-11-21 09:26:58'),(7,NULL,1,'Запись отменена','Клиент отменил запись на 23.11.2026 09:30.','appointment_cancelled',0,'2026-11-21 09:27:15');
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pets`
--

DROP TABLE IF EXISTS `pets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pets` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int unsigned NOT NULL,
  `name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `age` tinyint unsigned DEFAULT NULL,
  `breed` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `size` enum('small','medium','large') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_pets_client` (`user_id`),
  CONSTRAINT `fk_pets_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pets`
--

LOCK TABLES `pets` WRITE;
/*!40000 ALTER TABLE `pets` DISABLE KEYS */;
INSERT INTO `pets` VALUES (4,5,'Бакс',5,'Британская короткошёрстная','medium','Спокойный, боится фена','2026-11-17 10:16:27','/uploads/pets/ea85f03e013b4c65bab6b97f03822d9b.png'),(7,9,'Рекс',4,'Лабрадор','large','Спокойный, дружелюбный','2026-10-30 12:00:00',NULL),(8,10,'Боня',3,'Шпиц','small','Активная, любит внимание','2026-10-30 12:00:00',NULL),(9,11,'Чарли',2,'Йоркширский терьер','small','Нуждается в регулярной стрижке','2026-10-30 12:00:00',NULL),(10,12,'Луна',5,'Пудель','medium','Хорошо переносит процедуры','2026-10-30 12:00:00',NULL);
/*!40000 ALTER TABLE `pets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reviews`
--

DROP TABLE IF EXISTS `reviews`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reviews` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int unsigned NOT NULL,
  `groomer_id` int unsigned NOT NULL,
  `appointment_id` int unsigned DEFAULT NULL,
  `rating` tinyint unsigned NOT NULL,
  `text` text COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'approved',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_review_per_appointment` (`appointment_id`),
  KEY `idx_rev_client` (`user_id`),
  KEY `idx_rev_groomer` (`groomer_id`),
  KEY `idx_rev_appointment` (`appointment_id`),
  CONSTRAINT `fk_rev_appointment` FOREIGN KEY (`appointment_id`) REFERENCES `appointments` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_rev_groomer` FOREIGN KEY (`groomer_id`) REFERENCES `groomers` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_rev_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_rev_rating` CHECK ((`rating` between 1 and 5))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reviews`
--

LOCK TABLES `reviews` WRITE;
/*!40000 ALTER TABLE `reviews` DISABLE KEYS */;
INSERT INTO `reviews` VALUES (3,5,1,4,5,'Отличная работа!','2026-11-18 10:33:02','approved');
/*!40000 ALTER TABLE `reviews` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `services`
--

DROP TABLE IF EXISTS `services`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `services` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `price` decimal(10,2) NOT NULL,
  `duration_minutes` smallint unsigned NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_services_is_active` (`is_active`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `services`
--

LOCK TABLES `services` WRITE;
/*!40000 ALTER TABLE `services` DISABLE KEYS */;
INSERT INTO `services` VALUES (1,'Комплексный уход','Мытьё, сушка, стрижка и расчёсывание',2500.00,120,1,'2026-11-09 14:49:53','/uploads/services/bf650e6f41c1402cbf29ea880cde2e0a.jpg'),(2,'Стрижка когтей','Подстригание когтей',500.00,30,1,'2026-11-09 14:49:53','/uploads/services/6b00a4ac815849b4a92f73b755c88ddd.jpg'),(3,'Мытьё и сушка','Мытьё шерсти и сушка феном',1200.00,60,1,'2026-11-09 14:49:53','/uploads/services/bc16875a78714a3db9e029f2152eda53.jpg'),(5,'Экспресс-линька','Удаление подшёрстка и вычёсывание шерсти',1800.00,60,1,'2026-11-10 12:22:00','/uploads/services/6b23aa48e1bc4bf48f7da932d413c455.jpg'),(7,'SPA-уход','Комплексный уход с использованием косметических средств для животных',2500.00,90,1,'2026-11-10 12:22:00','/uploads/services/50ba0d1b59a24add9605baaa235007ea.jpg'),(8,'Комплексный уход за шерстью','Мытьё, сушка, расчёсывание и уход за шерстью питомца',2000.00,75,1,'2026-11-10 12:23:18','/uploads/services/60533f7c9e7f4db1949850a82b185e95.jpg');
/*!40000 ALTER TABLE `services` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `password_hash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_admin` tinyint(1) NOT NULL DEFAULT '0',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `is_banned` tinyint(1) NOT NULL DEFAULT '0',
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_email` (`email`),
  KEY `idx_users_is_admin` (`is_admin`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (5,'Анастасия Кузнецова','test@gmail.com','+779515225981','AQAAAAIAAYagAAAAECW7kQGFbxX3T2CoLT8GZvAVVOVKPph4lLi5odPqteC9Xj7SrmfT+edfjsBlE3Butg==',0,'2026-11-10 08:52:44',0,NULL),(8,'Администратор','admin2@example.com','+79991112233','AQAAAAIAAYagAAAAEId5ZQnu15BS3WlmFPzhqH08tYLhF3AMfqHGESBLlScEX4giuj9GuqreUgL+73yN0g==',1,'2026-11-18 12:00:39',0,NULL),(9,'Елена Морозова','elena.morozova@gmail.com','+79991234501','AQAAAAIAAYagAAAAEHYqsTylzNHwgzPQO2r/zH4URbzeTMie3d6cL0eB6ZhGYcyLFpiP8MPcUHZevHlyqA==',0,'2026-11-21 09:05:26',0,NULL),(10,'Дмитрий Соколов','d.sokolov@gmail.com','+79991234501','AQAAAAIAAYagAAAAEKfa7zG0Q3PmyykapL1Z+Wbrx7Bp7QTwTwYtvniX25SETJVKVJEqAjeHGO91heQWjA==',0,'2026-11-21 09:06:03',0,NULL),(11,'Марина Волкова','marina.volkova@gmail.com','+79991234503','AQAAAAIAAYagAAAAEIpaGG1V+HhUY1vhruCUKNF8/DFJVdJ572+f8yflM6LKmsI1hFrnqksItdV6n/Gdjw==',0,'2026-11-21 09:06:40',0,NULL),(12,'Артём Кузнецов','artem.kuznetsov@gmail.com','+79991234504','AQAAAAIAAYagAAAAEE5eRhxtgVLDmfvwgWr4LNNMmmLbl9xL7/56WhWuNjisjE9MzKdmV1fDSfTntn8ZmQ==',0,'2026-11-21 09:08:25',0,NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-21 12:37:26
