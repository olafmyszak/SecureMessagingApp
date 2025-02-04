import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RegisterDto } from '../models/register.dto';
import { environment } from '../../environments/environment';
import { catchError, defer, map, mergeMap, Observable, of } from 'rxjs';
import { CryptoService } from './crypto.service';
import { UserIdWithUsernameDto } from '../models/userIdWithUsername.dto';

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly http = inject(HttpClient);
    private readonly cryptoService = inject(CryptoService);

    register(username: string, password: string): Observable<void> {
        return defer(async () => {
            // Generate and store keys
            const keyPair = await this.cryptoService.generateKeyPair();
            const publicKey = await this.cryptoService.exportPublicKey(keyPair.publicKey);

            // Store private key
            await this.cryptoService.storePrivateKey(keyPair.privateKey);

            const registerDto: RegisterDto = {
                username: username,
                password: password,
                publicKey: publicKey
            };

            return this.http.post<void>(`${environment.baseUrlHttps}/api/user/register`, registerDto);
        }).pipe(
            mergeMap(observable => observable)
        );
    }

    getAllUsers(excludedUserIds?: number[]): Observable<UserIdWithUsernameDto[]> {
        return this.http.get<UserIdWithUsernameDto[]>(`${environment.baseUrlHttps}/api/user`).pipe(
            map((users) => {
                if (excludedUserIds === undefined || excludedUserIds.length === 0) {
                    return users;
                }

                return users.filter(user => !excludedUserIds.includes(user.id));
            }),
            catchError(err => {
                console.log('Error getting all users: ', err);
                return of([]);
            })
        );
    }
}
