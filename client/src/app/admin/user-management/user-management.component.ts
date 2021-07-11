import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss'],
})
export class UserManagementComponent implements OnInit {
  users!: Partial<User[]> | undefined;
  bsModalRef!: BsModalRef;

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe((users) => {
      this.users = users;
    });
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user: user,
        roles: this.getRolesArrary(user),
      },
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updatedSelectedRoles.subscribe((values: any[]) => {
      // console.log(values);
      const rolesToUdpate = {
        roles: [
          ...values.filter((el) => el.checked === true).map((el) => el.name),
        ],
      };
      if (rolesToUdpate) {
        console.log(rolesToUdpate);
        this.adminService
          .updateUserRoles(user.username, rolesToUdpate.roles)
          .subscribe(() => {
            console.log('update successful');
            user.roles = [...rolesToUdpate.roles];
          });
      }
    });
  }

  private getRolesArrary(user: User) {
    const roles: any[] = [];
    const userRoles = user.roles;
    const availbleRoles: any[] = [
      { name: 'Member', value: 'Member' },
      { name: 'Admin', value: 'Admin' },
      { name: 'Moderator', value: 'Moderator' },
    ];

    availbleRoles.forEach((role) => {
      let isMatch = false;
      for (const userRole of userRoles) {
        if (role.name == userRole) {
          isMatch = true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }

      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }
    });

    return roles;
  }
}
