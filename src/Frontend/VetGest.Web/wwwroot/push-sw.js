// Web Push handlers shared by the development and published service workers.
// The payload never contains private content: only a generic title/body and an in-app URL.
self.addEventListener('push', event => {
    let data = {};
    try {
        data = event.data ? event.data.json() : {};
    } catch {
        data = {};
    }

    event.waitUntil(self.registration.showNotification(data.title || 'VetGest', {
        body: data.body || 'Você tem uma nova notificação.',
        icon: 'icon-192.png',
        badge: 'icon-192.png',
        tag: 'vetgest-chat',
        renotify: true,
        data: { url: typeof data.url === 'string' ? data.url : '/' }
    }));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();

    // Only same-origin URLs are opened.
    let target = new URL('/', self.location.origin);
    try {
        const requested = new URL((event.notification.data && event.notification.data.url) || '/', self.location.origin);
        if (requested.origin === self.location.origin) {
            target = requested;
        }
    } catch {
        // keep default
    }

    event.waitUntil((async () => {
        const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
        for (const client of windows) {
            if (client.url.startsWith(self.location.origin)) {
                await client.focus();
                if ('navigate' in client) {
                    return client.navigate(target.href);
                }

                return;
            }
        }

        return self.clients.openWindow(target.href);
    })());
});