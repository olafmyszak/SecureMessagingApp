import { inject, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Message } from '../models/message.model';
import { MessageService } from './message.service';
import { SignalRService } from './signal-r.service';

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    private readonly messageService = inject(MessageService);
    private readonly signalRService = inject(SignalRService);

    private _messagesSubject = new BehaviorSubject<Message[]>([]);
    messages$ = this._messagesSubject.asObservable();

    constructor() {
        this.signalRService.message$.subscribe({
                next: (message) =>
                    this._messagesSubject.next([...this._messagesSubject.getValue(), message]),
                error:
                    (e) => console.error('Error in SignalR stream: ', e)
            }
        );
    }

    setCurrentChatPartner(userId: number) {
        this.messageService.getAllMessages(userId).subscribe({
            next: (messages) => this._messagesSubject.next(messages),
            error: (e) => console.error('Error loading sent messages: ', e)
        });
    }
}
