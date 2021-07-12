import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from './models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Almighty Push';
  constructor(private accountService: AccountService, private presence: PresenceService) {
  }

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser() {
    let userJson = localStorage.getItem('user');
    if(userJson) {
      let user: User = JSON.parse(userJson);
      // should not need to do this check but will anyway.
      if(user){
        this.accountService.setCurrentUser(user);
        this.presence.createHubConnection(user);
      }
    }
  }
}
