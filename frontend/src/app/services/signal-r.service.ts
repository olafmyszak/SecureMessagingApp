import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubRoutes } from '../../constants';
import { Observable } from 'rxjs';


@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection: signalR.HubConnection;

    constructor() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(HubRoutes.chatHub)
            .build();
    }

    startConnection(): Observable<void> {
        return new Observable<void>((observer) => {
            this.hubConnection.start()
                .then(() => {
                    console.log('Connection established with SignalR Hub');
                    observer.next();
                    observer.complete();
                })
                .catch((err) => {
                    console.error('Error connecting to SignalR hub: ', err);
                    observer.error(err);
                });
        });
    }

}
