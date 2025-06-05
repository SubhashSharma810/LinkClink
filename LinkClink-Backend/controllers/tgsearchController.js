// File: tgsearch-backend/controllers/tgsearchController.js
const puppeteer = require('puppeteer');

exports.searchTelegramChannels = async (req, res) => {
    const query = req.query.q?.toLowerCase()?.trim();
    if (!query) return res.status(400).json({ error: 'Query required' });

    console.log('🔍 Searching Telegram for:', query);

    try {
        const browser = await puppeteer.launch({
            headless: 'new',
            args: ['--no-sandbox', '--disable-setuid-sandbox'] // Optional: safer on some environments
        });

        const page = await browser.newPage();
        await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');

        const googleURL = `https://www.google.com/search?q=site:t.me+${encodeURIComponent(query)}&filter=0`;
        await page.goto(googleURL, { waitUntil: 'domcontentloaded' });

        // Let it render results
        await new Promise(resolve => setTimeout(resolve, 2000));
       

        const results = await page.evaluate(() => {
            const anchors = Array.from(document.querySelectorAll('a[href^="https://t.me/"]'));

            const links = anchors
                .map(a => a.href)
                .filter(href => !href.includes('/s/')) // Exclude post links
                .filter((v, i, a) => a.indexOf(v) === i); // Unique only

            return links.slice(0, 10).map(link => {
                const username = link.split('/').pop();
                return {
                    name: '@' + username,
                    username,
                    link,
                    avatar: `https://t.me/i/userpic/320/${username}.jpg`
                };
            });
        });

        await browser.close();
        res.json(results);
    } catch (err) {
        console.error('❌ Telegram search failed:', err);
        res.status(500).json({ error: 'Telegram search failed' });
    }
};