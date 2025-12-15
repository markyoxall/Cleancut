(function(){
    console.log('[blazor-debug] loaded');
    // Check that blazor.server.js is present
    if (!window.Blazor) {
        console.warn('[blazor-debug] window.Blazor not available - blazor.server.js may not be loaded');
    } else {
        console.log('[blazor-debug] window.Blazor present');
    }

    async function probeNegotiate() {
        try {
            const res = await fetch('/_blazor/negotiate', { method: 'POST', credentials: 'include' });
            console.log('[blazor-debug] /_blazor/negotiate status', res.status, res.type, res.headers.get('content-type'));
            if (res.status === 401) {
                console.warn('[blazor-debug] negotiate returned 401 - authentication required or cookie not sent');
            }
            if (res.redirected) {
                console.warn('[blazor-debug] negotiate was redirected to', res.url);
            }
            const txt = await res.text();
            console.log('[blazor-debug] negotiate response (truncated):', txt.slice(0, 200));
        } catch (ex) {
            console.error('[blazor-debug] error probing negotiate', ex);
        }
    }

    // wait a bit so the page has loaded
    setTimeout(() => probeNegotiate(), 500);
})();
