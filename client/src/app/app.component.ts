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
  users: any;
  restApiUrl: string;
  constructor(private httpClient: HttpClient, private accountService: AccountService) {
    this.restApiUrl = 'https://localhost:5001/api/users';
  }

  ngOnInit(): void {
    this.loadUsers();
    this.setCurrentUser();
  }

  setCurrentUser() {
    let user: User = JSON.parse(localStorage.getItem('user') || '{}');
    this.accountService.setCurrentUser(user);
  }

  loadUsers() {
    this.httpClient.get(this.restApiUrl).subscribe((data : any) => {
      this.users = data;
    });
    console.log({users: this.users});
  }
}
