const express = require('express');
const router = express.Router();
const { getFormats, downloadMedia } = require('../controllers/youtubeController');

router.post('/formats', getFormats);
router.post('/download', downloadMedia); // if defined

module.exports = router;