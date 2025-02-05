import { Component, inject, OnInit, output } from '@angular/core';
import { NgForOf } from '@angular/common';
import { UserService } from '../../services/user.service';
import { UserIdWithUsernameDto } from '../../models/userIdWithUsername.dto';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-user-list',
    imports: [
        NgForOf
    ],
    templateUrl: './user-list.component.html',
    styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
    selectedUser = output<number>();

    private readonly userService = inject(UserService);
    private readonly authService = inject(AuthService);
    users: UserIdWithUsernameDto[] = [];

    ngOnInit(): void {
        const currentUserId = this.authService.currentUserId;

        if(!currentUserId) {
            return;
        }

        this.userService.getAllUsers([currentUserId]).subscribe((users) => {
                this.users = users;
            }
        );
    }

    selectUser(userId: number) {
        this.selectedUser.emit(userId);
    }
}