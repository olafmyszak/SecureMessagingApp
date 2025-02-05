import { Component, inject, OnDestroy, OnInit, Signal } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { ChatService } from '../../services/chat.service';
import { DatePipe, NgForOf, TitleCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserListComponent } from '../user-list/user-list.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { Message } from '../../models/message.model';

@Component({
    selector: 'app-chat',
    imports: [
        TitleCasePipe,
        DatePipe,
        FormsModule,
        NgForOf,
        UserListComponent
    ],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy {
    newMessage = '';
    selectedUserId: number = -1;

    private readonly chatService = inject(ChatService);

    connectionState: Signal<HubConnectionState> = toSignal(this.chatService.connectionState$, {initialValue: HubConnectionState.Disconnected});
    messages: Signal<Message[]> = toSignal(this.chatService.messages$, {initialValue: []});

    ngOnInit(): void {
        void this.chatService.startConnection();
    }

    async sendMessage() {
        if (!this.selectedUserId || !this.newMessage.trim()) {
            return;
        }

        try {
            await this.chatService.sendMessage(this.selectedUserId, this.newMessage.trim());
            this.newMessage = '';
        } catch (err) {
            console.error('Failed to send message:', err);
            // Show error to user
        }
    }

    selectUser(id: number) {
        this.selectedUserId = id;
    }

    ngOnDestroy(): void {
        this.chatService.cleanup();
    }

    protected readonly HubConnectionState = HubConnectionState;
}
