import { Component, inject, OnDestroy, OnInit, signal, Signal } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { SignalRService } from '../../services/signal-r.service';
import { DatePipe, NgClass, TitleCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserListComponent } from '../user-list/user-list.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { Message } from '../../models/message.model';
import { UserIdWithUsernameDto } from '../../models/userIdWithUsername.dto';
import { ChatService } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-chat',
    imports: [
        TitleCasePipe,
        DatePipe,
        FormsModule,
        UserListComponent,
        NgClass
    ],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy {
    private readonly signalRService = inject(SignalRService);
    private readonly chatService = inject(ChatService);
    protected readonly authService = inject(AuthService);

    selectedUser: UserIdWithUsernameDto | null = null;

    newMessage = signal('');
    messages: Signal<Message[]> = toSignal(this.chatService.messages$, {initialValue: []});
    connectionState: Signal<HubConnectionState> = toSignal(this.signalRService.connectionState$, {initialValue: HubConnectionState.Disconnected});

    ngOnInit(): void {
        void this.signalRService.startConnection();
    }

    async sendMessage() {
        if (!this.selectedUser || !this.newMessage().trim()) {
            return;
        }

        try {
            await this.signalRService.sendMessage(this.selectedUser.id, this.newMessage().trim());
            this.newMessage.set('');
        } catch (err) {
            console.error('Failed to send message:', err);
            // Show error to user
        }
    }

    selectUser(user: UserIdWithUsernameDto) {
        this.selectedUser = user;
        this.chatService.setCurrentChatPartner(user.id);
    }

    ngOnDestroy(): void {
        this.signalRService.cleanup();
    }

    protected isSent(message: Message) {
        return message.senderId === this.authService.currentUserId;
    }

    protected readonly HubConnectionState = HubConnectionState;
}
