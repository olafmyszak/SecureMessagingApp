import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState, IHttpConnectionOptions } from '@microsoft/signalr';
import { ChatHubMethod, HubRoute } from '../constants';
import { BehaviorSubject, Subject } from 'rxjs';
import { Message } from '../models/message.model';
import { environment } from '../../environments/environment';


@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private readonly hubConnection: signalR.HubConnection;

    private _messageSubject = new Subject<Message>();
    public message$ = this._messageSubject.asObservable();

    private _connectionStateSubject = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);
    public connectionState$ = this._connectionStateSubject.asObservable();

    private readonly options: IHttpConnectionOptions = {
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

        this._connectionStateSubject.next(HubConnectionState.Connecting);

        await this.sleep(300);

        try {
            await this.hubConnection.start();
            this._connectionStateSubject.next(HubConnectionState.Connected);
        } catch (err) {
            console.error('Error starting hub connection', err);
            this._connectionStateSubject.next(HubConnectionState.Disconnected);
        }
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

    cleanup() {
        if (this.hubConnection) {
            void this.hubConnection.stop();
        }
    }

    private registerHandlers() {
        this.hubConnection.on(ChatHubMethod.ReceiveMessage, (message: Message) => {
            this._messageSubject.next(message);
        });

        this.hubConnection.onreconnecting(() => {
            this._connectionStateSubject.next(HubConnectionState.Reconnecting);
        });

        this.hubConnection.onreconnected(() => {
            this._connectionStateSubject.next(HubConnectionState.Connected);
        });

        this.hubConnection.onclose(() => {
            this._connectionStateSubject.next(HubConnectionState.Disconnected);
        });
    }

    private async sleep(ms: number) {
        await new Promise( resolve => setTimeout(resolve, ms) );
    }

}
