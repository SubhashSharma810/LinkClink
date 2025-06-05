const puppeteer = require('puppeteer');

async function scrapeDirectVideoUrl(targetUrl) {
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();

    try {
        await page.goto(targetUrl, { waitUntil: 'networkidle2', timeout: 0 });

        // Wait for a possible video tag
        await page.waitForSelector('video', { timeout: 5000 });

        const videoSrc = await page.$eval('video', video => {
            return video.src || (video.querySelector('source')?.src ?? null);
        });

        if (!videoSrc || videoSrc.startsWith('blob:')) {
            throw new Error('Direct video URL not found or is a blob URL');
        }

        return videoSrc;
    } catch (err) {
        throw new Error(`Puppeteer failed: ${err.message}`);
    } finally {
        await browser.close();
    }
}

module.exports = { scrapeDirectVideoUrl };