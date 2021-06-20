import { Component, OnInit } from '@angular/core';
import { User } from '../models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit {
  model: any = {};
  loggedIn: boolean;
  constructor(private accountsService: AccountService) {
    this.loggedIn = false;
  }

  ngOnInit(): void {
    this.getCurrentUser();
  }

  login() {
    this.accountsService.login(this.model).subscribe(data => {
      console.log(data);
      this.loggedIn = true;
    }, error => {
      console.error(error);
    });
  }

  logout() {
    this.accountsService.logout();
    this.loggedIn = false;
  }

  getCurrentUser() {
    this.accountsService.currentUserSource$.subscribe((user: User) => {
      // because user can be an an empty object we test if its not null
      // and if the user name is set
      console.log({user: user, getCurrentUser: true});
      this.loggedIn = !!user && !!user.username;
    },
    err => {
      console.error(err);
    });
  }
}
