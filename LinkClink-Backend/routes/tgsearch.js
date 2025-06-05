const express = require('express');
const router = express.Router();
const { searchTelegramChannels } = require('../controllers/tgsearchController');

router.get('/', searchTelegramChannels);

module.exports = router;