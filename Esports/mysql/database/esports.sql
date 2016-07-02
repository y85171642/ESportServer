/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50710
Source Host           : localhost:3306
Source Database       : esports

Target Server Type    : MYSQL
Target Server Version : 50710
File Encoding         : 65001

Date: 2016-06-29 00:34:06
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for match_partner
-- ----------------------------
DROP TABLE IF EXISTS `match_partner`;
CREATE TABLE `match_partner` (
  `user_id` int(11) NOT NULL,
  `start_time` datetime DEFAULT NULL,
  `end_time` datetime DEFAULT NULL,
  `lan` decimal(10,0) DEFAULT NULL,
  `lon` decimal(10,0) DEFAULT NULL,
  `groupid` int(11) DEFAULT NULL,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for space_commont
-- ----------------------------
DROP TABLE IF EXISTS `space_commont`;
CREATE TABLE `space_commont` (
  `space_id` int(11) NOT NULL,
  `commont` varchar(255) DEFAULT NULL,
  `stars` int(11) DEFAULT NULL,
  PRIMARY KEY (`space_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for space_info
-- ----------------------------
DROP TABLE IF EXISTS `space_info`;
CREATE TABLE `space_info` (
  `space_id` int(11) NOT NULL,
  `space_name` varchar(255) NOT NULL,
  `space_imgs` varchar(255) DEFAULT NULL,
  `space_address` varchar(255) DEFAULT NULL,
  `space_price` decimal(10,2) DEFAULT NULL,
  `space_phone` varchar(255) DEFAULT NULL,
  `lat` decimal(10,0) DEFAULT NULL,
  `lon` decimal(10,0) DEFAULT NULL,
  `services` int(11) DEFAULT NULL,
  `services_desc` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`space_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for user_group
-- ----------------------------
DROP TABLE IF EXISTS `user_group`;
CREATE TABLE `user_group` (
  `groupid` int(11) NOT NULL,
  `userid` int(11) NOT NULL,
  PRIMARY KEY (`groupid`,`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for user_info
-- ----------------------------
DROP TABLE IF EXISTS `user_info`;
CREATE TABLE `user_info` (
  `user_id` int(11) NOT NULL,
  `nick_name` varchar(255) DEFAULT NULL,
  `head_url` varchar(255) DEFAULT NULL,
  `huanxin_id` int(11) DEFAULT NULL,
  `huanxin_pwd` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for user_reg
-- ----------------------------
DROP TABLE IF EXISTS `user_reg`;
CREATE TABLE `user_reg` (
  `login_account` varchar(255) NOT NULL,
  `login_pwd` varchar(255) DEFAULT NULL,
  `login_type` int(11) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`login_account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for user_sport_level
-- ----------------------------
DROP TABLE IF EXISTS `user_sport_level`;
CREATE TABLE `user_sport_level` (
  `user_id` int(11) NOT NULL,
  `user_level` int(11) DEFAULT NULL,
  `sport_type` int(11) DEFAULT NULL,
  `lat` decimal(10,0) DEFAULT NULL,
  `lon` decimal(10,0) DEFAULT NULL,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
