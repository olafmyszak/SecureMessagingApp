import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { SignInDto } from '../models/signIn.dto';
import { JwtResponse } from '../models/jwtResponse.model';
import { JwtHelperService } from '@auth0/angular-jwt';
import { JwtPayload } from 'jsonwebtoken';

const jwtHelperService = new JwtHelperService();

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly http = inject(HttpClient);

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

    logout() {
        localStorage.removeItem(environment.access_token);
    }

    get isAuthenticated(): boolean {
        const token = localStorage.getItem(environment.access_token);

        if (!token) {
            return false;
        }

        const b = !jwtHelperService.isTokenExpired(token);
        console.log(b);
        return b;
    }

    get currentUserId(): number | null {
        if (!this.isAuthenticated) {
            return null;
        }

        const token: string = localStorage.getItem(environment.access_token)!;

        const decodedToken: JwtPayload | null = jwtHelperService.decodeToken(token);

        if (!decodedToken) {
            return null;
        }

        const id: string | undefined = decodedToken.sub;

        if (id === undefined) {
            console.error('Error getting user id from JWT');
            return null;
        }

        return +id;
    }
}
