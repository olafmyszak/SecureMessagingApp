import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment.dev';
import { SignInDto } from '../models/signInDto';
import { JwtResponse } from '../models/jwtResponse';

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    http = inject(HttpClient);

    login(signInDto: SignInDto): Observable<JwtResponse> {
        return this.http.post<JwtResponse>(`${environment.baseUrlHttps}/api/auth/login`, signInDto).pipe(
            tap({
                next: res => {
                    localStorage.setItem(environment.access_token, res.accessToken);
                },
                error: err => {
                    console.error(err);
                    console.error(`Login failed: ${err.message}`);
                }
            })
        );
    }
}
