import { Component, OnInit } from '@angular/core';
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
  }

  login() {
    this.accountsService.login(this.model).subscribe(data => {
      console.log(data);
      this.loggedIn = true;
    }, error => {
      console.error(error);
    });
  }
}
