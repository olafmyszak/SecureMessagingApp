import { Component, inject, OnInit, output } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserIdWithUsernameDto } from '../../models/userIdWithUsername.dto';
import { AuthService } from '../../services/auth.service';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';

@Component({
    selector: 'app-user-list',
    imports: [
        MatButtonToggleGroup,
        MatButtonToggle
    ],
    templateUrl: './user-list.component.html',
    styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
    selectedUser = output<UserIdWithUsernameDto>();

    private readonly userService = inject(UserService);
    private readonly authService = inject(AuthService);
    users: UserIdWithUsernameDto[] = [];

    ngOnInit(): void {
        const currentUserId = this.authService.currentUserId;

        if (!currentUserId) {
            return;
        } else {
            this.userService.getAllUsers([currentUserId]).subscribe((users) => {
                    this.users = users;
                }
            );
        }
    }

    selectUser(user: UserIdWithUsernameDto) {
        this.selectedUser.emit(user);
    }
}