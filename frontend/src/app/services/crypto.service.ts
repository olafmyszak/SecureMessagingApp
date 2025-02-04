import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class CryptoService {
    private readonly keyAlgorithm: RsaHashedKeyGenParams = {
        name: 'RSA-OAEP',
        modulusLength: 2048,
        publicExponent: new Uint8Array([0x01, 0x00, 0x01]),
        hash: 'SHA-256'
    };

    private readonly DB_NAME = 'SecureMessaging';
    private readonly STORE_NAME = 'keyStore';
    private readonly PRIVATE_KEY_ID = 'privateKey';
    private db: IDBDatabase | null = null;

    constructor() {
        void this.initDb();
    }

    async generateKeyPair(): Promise<CryptoKeyPair> {
        // Automatically store private key in IndexedDB
        return await crypto.subtle.generateKey(
            this.keyAlgorithm,
            true,
            ['encrypt', 'decrypt']
        );
    }

    async exportPublicKey(publicKey: CryptoKey): Promise<string> {
        const exported = await crypto.subtle.exportKey('spki', publicKey);
        return this.arrayBufferToBase64(exported);
    }

    async importPublicKey(base64Key: string): Promise<CryptoKey> {
        const keyBuffer = this.base64ToArrayBuffer(base64Key);
        return crypto.subtle.importKey(
            'spki',
            keyBuffer,
            this.keyAlgorithm,
            true,
            ['encrypt']
        );
    }

    private arrayBufferToBase64(buffer: ArrayBuffer): string {
        return btoa(String.fromCharCode(...new Uint8Array(buffer)));
    }

    private base64ToArrayBuffer(base64: string): ArrayBuffer {
        const binaryString = atob(base64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes.buffer;
    }

    private async initDb(): Promise<void> {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.DB_NAME, 1);

            request.onerror = () => reject(request.error);

            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };

            request.onupgradeneeded = (event) => {
                const db = (event.target as IDBOpenDBRequest).result;

                if (!db.objectStoreNames.contains(this.STORE_NAME)) {
                    db.createObjectStore(this.STORE_NAME);
                }
            };
        });
    }

    async storePrivateKey(privateKey: CryptoKey): Promise<void> {
        if (!this.db) {
            await this.initDb();
        }

        return new Promise((resolve, reject) => {
            const transaction = this.db!.transaction(this.STORE_NAME, 'readwrite');
            const store = transaction.objectStore(this.STORE_NAME);

            const request = store.put(privateKey, this.PRIVATE_KEY_ID);

            request.onerror = () => reject(request.error);

            request.onsuccess = () => resolve();
        });
    }

    async getPrivateKey(): Promise<CryptoKey> {
        if (!this.db) {
            await this.initDb();
        }

        return new Promise((resolve, reject) => {
            const transaction = this.db!.transaction(this.STORE_NAME, 'readonly');
            const store = transaction.objectStore(this.STORE_NAME);

            const request = store.get(this.PRIVATE_KEY_ID);

            request.onerror = () => reject(request.error);

            request.onsuccess = () => {
                if (!request.result) {
                    reject(new Error('No private key found for this user'));
                    return;
                }
                resolve(request.result);
            };
        });
    }
}
