const ytdlp = require('yt-dlp-exec');
const { scrapeDirectVideoUrl } = require('../puppeteer/puppeteerScraper');

function isYouTubeUrl(url) {
    return url.includes('youtube.com') || url.includes('youtu.be');
}

exports.getFormats = async (req, res) => {
    const { url } = req.body;

    if (isYouTubeUrl(url)) {
        try {
            const info = await ytdlp(url, {
                dumpSingleJson: true,
                noWarnings: true,
                preferFreeFormats: true,
                noCheckCertificates: true,
            });

            const formats = info.formats.map(f => ({
                format_id: f.format_id,
                resolution: f.resolution || `${f.width || 0}x${f.height || 0}`,
                filesize: f.filesize || 0,
                ext: f.ext,
                hasAudio: !!f.asr,
                hasVideo: !!f.vcodec,
            }));

            return res.json({
                title: info.title,
                thumbnail: info.thumbnail,
                formats,
                isYouTube: true,
            });
        } catch (err) {
            console.error('yt-dlp failed:', err);
            return res.status(500).json({ error: 'Failed to fetch formats from YouTube' });
        }
    } else {
        try {
            const directUrl = await scrapeDirectVideoUrl(url);
            return res.json({
                title: 'Direct Video',
                thumbnail: null,
                formats: [{
                    format_id: 'default',
                    resolution: 'unknown',
                    filesize: 0,
                    ext: 'mp4',
                    hasAudio: true,
                    hasVideo: true,
                    directUrl
                }],
                isYouTube: false
            });
        } catch (err) {
            console.error('Puppeteer fallback failed:', err);
            return res.status(500).json({ error: 'Non-YouTube video not supported' });
        }
    }
};

exports.downloadMedia = async (req, res) => {
    const { url, format_id, isYouTube, directUrl } = req.body;

    try {
        if (isYouTube) {
            const subprocess = ytdlp.exec(
                [url, '-f', format_id, '-o', '-'],
                { stdout: 'pipe' }
            );

            const info = await ytdlp(url, {
                dumpSingleJson: true
            });

            const title = info.title || 'video';
            const safeTitle = title.replace(/[<>:"/\\|?*\x00-\x1F]/g, '').slice(0, 100);
            res.setHeader('Content-Disposition', `attachment; filename="${safeTitle}.mp4"`);

            subprocess.stdout.pipe(res);
        } else {
            // Stream direct video
            res.setHeader('Content-Disposition', `attachment; filename="video.mp4"`);
            const response = await fetch(directUrl);
            response.body.pipe(res);
        }
    } catch (err) {
        console.error('Download failed:', err);
        res.status(500).json({ error: 'Download failed' });
    }
};