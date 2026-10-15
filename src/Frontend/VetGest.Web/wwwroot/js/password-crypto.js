// Encrypts a password with the API's RSA public key so it never appears in a request body.
// This is an extra layer on top of TLS, not a replacement for it.

const MAX_ENVELOPE_BYTES = 318; // RSA-3072 OAEP-SHA256 payload limit.

function base64ToBytes(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
}

function bytesToBase64(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

export function isSupported() {
    return typeof crypto !== 'undefined' && typeof crypto.subtle !== 'undefined';
}

export async function encryptPassword(publicKeyBase64, password, timestampMs) {
    if (!isSupported()) {
        // Only reachable on a non-localhost plain-HTTP origin, where Web Crypto is unavailable.
        throw new Error('INSECURE_CONTEXT');
    }

    const key = await crypto.subtle.importKey(
        'spki',
        base64ToBytes(publicKeyBase64),
        { name: 'RSA-OAEP', hash: 'SHA-256' },
        false,
        ['encrypt']);

    const envelope = new TextEncoder().encode(JSON.stringify({ p: password, t: timestampMs }));
    if (envelope.length > MAX_ENVELOPE_BYTES) {
        throw new Error('PASSWORD_TOO_LONG');
    }

    const ciphertext = await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, key, envelope);
    return bytesToBase64(ciphertext);
}
