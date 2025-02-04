import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { passwordMatchValidator } from '../../shared/password-matches.directive';
import { NgIf } from '@angular/common';
import { UserService } from '../../services/user.service';
import { catchError, EMPTY } from 'rxjs';
import { HttpStatusCode } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
    selector: 'app-register',
    imports: [
        FormsModule,
        ReactiveFormsModule,
        NgIf
    ],
    templateUrl: './register.component.html',
    styleUrl: './register.component.css'
})
export class RegisterComponent {
    form = new FormGroup({
        username: new FormControl('', [Validators.required]),
        password: new FormControl('', [Validators.required, Validators.minLength(8)]),
        confirmPassword: new FormControl('', [Validators.required])
    }, {validators: passwordMatchValidator()});

    private readonly userService = inject(UserService);
    private readonly snackBar = inject(MatSnackBar);
    private readonly router = inject(Router);

    onSubmit() {
        const val = this.form.value;

        if (val.username && val.password) {
            this.userService.register(val.username, val.password).pipe(
                catchError((err) => {
                    if (err.status === 0) {
                        this.openSnackBar('Server connection problem', 'OK');
                    } else if (err.status === HttpStatusCode.Conflict) {
                        this.openSnackBar('Username already exists', 'OK');
                    }

                    return EMPTY;
                })
            ).subscribe(() => {
                this.openSnackBar('Registered successfully! You may now sign in.', 'OK');
                void this.router.navigate(['/login']);
            });
        }
    }

    private openSnackBar(message: string, action: string) {
        this.snackBar.open(message, action, {
            duration: 2000,
            verticalPosition: 'bottom'
        });
    }
}
