import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service';
import { catchError, EMPTY } from 'rxjs';
import { HttpStatusCode } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-login',
    imports: [
        ReactiveFormsModule
    ],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css'
})
export class LoginComponent {
    form = new FormGroup({
        username: new FormControl('', [Validators.required]),
        password: new FormControl('', [Validators.required])
    });

    private readonly snackBar = inject(MatSnackBar);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    onSubmit() {
        const val = this.form.value;

        if (val.username && val.password) {
            // console.log(val)
            this.authService.login({username: val.username, password: val.password}).pipe(
                catchError((err) => {
                    if (err.status === 0) {
                        this.openSnackBar('Server connection problem', 'OK');
                    } else if (err.status === HttpStatusCode.Unauthorized) {
                        this.openSnackBar('Invalid username or password', 'OK');
                    }

                    return EMPTY;
                })
            ).subscribe({
                    next: () => void this.router.navigate(['/chat'])
                }
            );
        }
    }

    private openSnackBar(message: string, action: string) {
        this.snackBar.open(message, action, {
            duration: 2000,
            verticalPosition: 'bottom'
        });
    }
}
