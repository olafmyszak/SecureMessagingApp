import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState, IHttpConnectionOptions } from '@microsoft/signalr';
import { ChatHubMethod, HubRoute } from '../constants';
import { BehaviorSubject } from 'rxjs';
import { Message } from '../models/message.model';
import { environment } from '../../environments/environment';


@Injectable({
    providedIn: 'root'
})
export class ChatService {
    private readonly hubConnection: signalR.HubConnection;

    private _messages = new BehaviorSubject<Message[]>([]);
    public messages$ = this._messages.asObservable();

    private _connectionState = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);
    public connectionState$ = this._connectionState.asObservable();

    options: IHttpConnectionOptions = {
        accessTokenFactory: () => {
            const token = localStorage.getItem(environment.access_token);

            if (!token) {
                return '';
            }

            return token;
        }
    };


    constructor() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.baseUrlHttps}/${HubRoute.ChatHub}`, this.options)
            .build();
    }

    async startConnection() {
        this.registerHandlers();

        try {
            await this.hubConnection.start();
            this._connectionState.next(HubConnectionState.Connected);
        } catch (err) {
            console.error('Error starting hub connection', err);
            this._connectionState.next(HubConnectionState.Disconnected);
        }

        this.hubConnection.onreconnected(() => {
            this._connectionState.next(HubConnectionState.Connected);
        });

        this.hubConnection.onclose(() => {
            this._connectionState.next(HubConnectionState.Disconnected);
        });
    }

    async sendMessage(recipientId: number, content: string) {
        if (!this.hubConnection) {
            throw new Error('Connection not initialized');
        }

        try {
            await this.hubConnection.invoke(ChatHubMethod.SendMessage, recipientId, content);
        } catch (err) {
            console.error('Error sending message:', err);
            throw err;
        }
    }

    async joinConversation(recipientId: number) {
        await this.hubConnection.invoke(ChatHubMethod.JoinConversation, recipientId);
    }

    async leaveConversation(recipientId: number) {
        await this.hubConnection.invoke(ChatHubMethod.LeaveConversation, recipientId);
    }

    get connectionId(): string | null {
        return this.hubConnection.connectionId;
    }

    async reconnect(): Promise<void> {
        if (this.hubConnection.state === HubConnectionState.Disconnected) {
            await this.hubConnection.start();
        }
    }

    cleanup() {
        if (this.hubConnection) {
            void this.hubConnection.stop();
        }
    }

    private registerHandlers() {
        this.hubConnection.on(ChatHubMethod.ReceiveMessage, (message: Message) => {
            this._messages.next([...this._messages.value, message]);
        });
    }

}
