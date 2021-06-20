import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit {
  model: any = {};
  currentUser$!: Observable<User>;
  isLoggedIn$: Observable<boolean>;
  constructor(private accountsService: AccountService) {
    this.isLoggedIn$ = of(false);
  }

  ngOnInit(): void {
    this.currentUser$ = this.accountsService.currentUser$;
    this.isLoggedIn$ = this.accountsService.isLoggedIn$;
  }

  login() {
    this.accountsService.login(this.model).subscribe(data => {
      console.log(data);
    }, error => {
      console.error(error);
    });
  }

  logout() {
    this.accountsService.logout();
  }
}
