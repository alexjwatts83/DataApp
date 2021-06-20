import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from './models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Almighty Push';
  constructor(private accountService: AccountService) {
  }

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser() {
    let user: User = JSON.parse(localStorage.getItem('user') || '{}');
    this.accountService.setCurrentUser(user);
  }

}
