import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

const TOKEN_KEY = 'taskflow_token';
const USER_KEY = 'taskflow_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);

    // Signals — reactive state for the current user
    private _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
    private _user = signal<Omit<AuthResponse, 'token'> | null>(
        JSON.parse(localStorage.getItem(USER_KEY) ?? 'null')
    );

    register(dto: RegisterRequest) {
        return this.http
            .post<AuthResponse>(`${environment.apiUrl}/auth/register`, dto)
            .pipe(tap(res => this.persist(res)));
    }

    login(dto: LoginRequest) {
        return this.http
            .post<AuthResponse>(`${environment.apiUrl}/auth/login`, dto)
            .pipe(tap(res => this.persist(res)));
    }

    logout() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        this._token.set(null);
        this._user.set(null);
        this.router.navigate(['/login']);
    }

    private persist(res: AuthResponse) {
        const { token, ...user } = res;
        localStorage.setItem(TOKEN_KEY, token);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
        this._token.set(token);
        this._user.set(user);
    }
}