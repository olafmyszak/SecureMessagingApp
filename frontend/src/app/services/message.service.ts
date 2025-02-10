import { inject, Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { Message } from '../models/message.model';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class MessageService {
    private readonly http = inject(HttpClient);
    private readonly authService = inject(AuthService)

    getAllMessages(recipientId: number): Observable<Message[]> {
        return this.http.get<Message[]>(`${environment.baseUrlHttps}/api/message/history/${recipientId}`).pipe(
            catchError(err => {
                const currentUserId = this.authService.currentUserId;

                if (currentUserId == null) {
                    console.error(`Failed to GET message history with ${recipientId} because current user is null`, err);
                } else {
                    console.error(`Failed to GET message history with ${recipientId} from ${this.authService.currentUserId}`, err);
                }

                return of([]);
            })
        );
    }
}
