// Browser side of Web Push. All state stays in the browser; the server only receives the subscription.
window.vgPush = (() => {
    const supported = () =>
        'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;

    const toKey = base64 => {
        const padding = '='.repeat((4 - (base64.length % 4)) % 4);
        const raw = atob((base64 + padding).replace(/-/g, '+').replace(/_/g, '/'));
        return Uint8Array.from(raw, ch => ch.charCodeAt(0));
    };

    const registration = () => navigator.serviceWorker.ready;

    return {
        isSupported: () => supported(),
        permission: () => (supported() ? Notification.permission : 'unsupported'),

        currentEndpoint: async () => {
            if (!supported()) return null;
            const subscription = await (await registration()).pushManager.getSubscription();
            return subscription ? subscription.endpoint : null;
        },

        subscribe: async publicKey => {
            if (!supported()) return null;
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') return null;

            const reg = await registration();
            const subscription = (await reg.pushManager.getSubscription())
                || await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: toKey(publicKey) });
            const json = subscription.toJSON();
            return { endpoint: json.endpoint, p256dh: json.keys.p256dh, auth: json.keys.auth };
        },

        unsubscribe: async () => {
            if (!supported()) return null;
            const subscription = await (await registration()).pushManager.getSubscription();
            if (!subscription) return null;
            const endpoint = subscription.endpoint;
            await subscription.unsubscribe();
            return endpoint;
        }
    };
})();