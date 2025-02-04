export interface Message {
    id: number;
    encryptedContent: string;
    timestamp: Date;
    senderId: number;
    recipientId: number;
}