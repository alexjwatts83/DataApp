import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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
  constructor(
    private accountsService: AccountService,
    private router: Router,
    private toastr: ToastrService) {
    this.isLoggedIn$ = of(false);
  }

  ngOnInit(): void {
    this.currentUser$ = this.accountsService.currentUser$;
    this.isLoggedIn$ = this.accountsService.isLoggedIn$;
  }

  login() {
    this.accountsService.login(this.model).subscribe(data => {
      console.log(data);
      this.router.navigateByUrl('/members');
    }, error => {
      console.error(error);
      this.toastr.error(error.error);
    });
  }

  logout() {
    this.accountsService.logout();
    this.router.navigateByUrl('/');
  }
}
