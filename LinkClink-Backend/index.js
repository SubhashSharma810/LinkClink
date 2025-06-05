const express = require('express');
const cors = require('cors');
const app = express();

const youtubeRoutes = require('./routes/youtube');
const tgSearchRoutes = require('./routes/tgsearch');

app.use(cors());
app.use(express.json());

// Routes
// backend/index.js
app.post('/api/formats', async (req, res) => {
  const { url } = req.body;

  if (url.includes("youtube.com") || url.includes("youtu.be")) {
    return handleYouTubeFormats(req, res);
  } else {
    return handleGenericFormats(req, res); // Puppeteer or yt-dlp fallback
  }
});
app.use('/api/tgsearch', tgSearchRoutes);

const PORT = 5000;
app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});